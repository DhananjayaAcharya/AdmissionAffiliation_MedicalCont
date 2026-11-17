using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Medical_Affiliation.Services
{
    // -------------------------------------------------------------------------
    // appsettings.json section "Easebuzz" (keep BearerToken in user-secrets /
    // environment variables, never in source control):
    //
    // "Easebuzz": {
    //   "BaseUrl": "https://14.139.159.12:447",
    //   "PaymentEndpoint": "/api/EaseBuzzPayments/finance/payment",
    //   "BearerToken": "",
    //   "ClientCode": "Affiliation_Fees",
    //   "AccountCode": "",
    //   "TypeOfService": "",
    //   "AllowUntrustedCertificate": false
    // }
    // -------------------------------------------------------------------------
    public sealed class EasebuzzSettings
    {
        public const string SectionName = "Easebuzz";

        public string BaseUrl { get; set; } = string.Empty;
        public string PaymentEndpoint { get; set; } = "/api/EaseBuzzPayments/finance/payment";
        public string BearerToken { get; set; } = string.Empty;
        public string ClientCode { get; set; } = "Affiliation_Fees";
        public string AccountCode { get; set; } = string.Empty;
        public string TypeOfService { get; set; } = string.Empty;

        // The gateway host is an IP with (probably) a self-signed certificate.
        // Enable ONLY in development; in production install the proper cert chain.
        public bool AllowUntrustedCertificate { get; set; }
    }

    public sealed class EasebuzzPaymentRequest
    {
        [JsonPropertyName("clientCode")] public string ClientCode { get; set; } = string.Empty;
        [JsonPropertyName("courseCode")] public string CourseCode { get; set; } = string.Empty;
        [JsonPropertyName("facultyCode")] public string FacultyCode { get; set; } = string.Empty;
        [JsonPropertyName("collegeCode")] public string CollegeCode { get; set; } = string.Empty;
        [JsonPropertyName("accountCode")] public string AccountCode { get; set; } = string.Empty;
        [JsonPropertyName("typeOfService")] public string TypeOfService { get; set; } = string.Empty;
        [JsonPropertyName("firstName")] public string FirstName { get; set; } = string.Empty;
        [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
        [JsonPropertyName("mobile")] public string Mobile { get; set; } = string.Empty;
        [JsonPropertyName("amount")] public decimal Amount { get; set; }
        [JsonPropertyName("additionalInfo")] public string? AdditionalInfo { get; set; }
    }

    public sealed record EasebuzzInitiateResult(bool Success, string? RedirectUrl, string Message, string? RawResponse);

    public interface IEasebuzzPaymentService
    {
        string ClientCode { get; }
        string AccountCode { get; }
        string TypeOfService { get; }

        Task<EasebuzzInitiateResult> InitiateAsync(EasebuzzPaymentRequest request, CancellationToken ct = default);
    }

    public sealed class EasebuzzPaymentService : IEasebuzzPaymentService
    {
        private static readonly string[] UrlPropertyNames =
        {
            "paymentUrl", "payment_url", "paymentLink", "payment_link",
            "redirectUrl", "redirect_url", "checkoutUrl", "url", "link"
        };

        private readonly HttpClient _http;
        private readonly EasebuzzSettings _settings;
        private readonly ILogger<EasebuzzPaymentService> _logger;

        public EasebuzzPaymentService(
            HttpClient http,
            IOptions<EasebuzzSettings> options,
            ILogger<EasebuzzPaymentService> logger)
        {
            _http = http;
            _settings = options.Value;
            _logger = logger;
        }

        public string ClientCode => _settings.ClientCode;
        public string AccountCode => _settings.AccountCode;
        public string TypeOfService => _settings.TypeOfService;

        public async Task<EasebuzzInitiateResult> InitiateAsync(EasebuzzPaymentRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.BaseUrl) || string.IsNullOrWhiteSpace(_settings.BearerToken))
            {
                _logger.LogError("Easebuzz settings are incomplete (BaseUrl / BearerToken).");
                return new EasebuzzInitiateResult(false, null, "Online payment is not configured. Please contact support.", null);
            }

            var url = new Uri(new Uri(_settings.BaseUrl.TrimEnd('/') + "/"), _settings.PaymentEndpoint.TrimStart('/'));

            using var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(request)
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.BearerToken);
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                using var response = await _http.SendAsync(message, ct);
                var body = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Easebuzz initiate failed. HTTP {Status}. Body: {Body}", (int)response.StatusCode, body);
                    return new EasebuzzInitiateResult(false, null,
                        $"The payment gateway rejected the request (HTTP {(int)response.StatusCode}).", body);
                }

                var redirectUrl = TryFindPaymentUrl(body);
                if (redirectUrl == null)
                {
                    _logger.LogWarning("Easebuzz responded OK but no payment URL was found. Body: {Body}", body);
                    return new EasebuzzInitiateResult(false, null,
                        "The payment gateway did not return a payment link.", body);
                }

                return new EasebuzzInitiateResult(true, redirectUrl, "OK", body);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "Easebuzz request timed out.");
                return new EasebuzzInitiateResult(false, null, "The payment gateway timed out. Please try again.", null);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Easebuzz request failed.");
                return new EasebuzzInitiateResult(false, null, "Could not reach the payment gateway. Please try again.", null);
            }
        }

        /// <summary>
        /// The gateway's response shape isn't documented in the request, so this
        /// looks for a well-known URL property anywhere in the JSON (data.paymentUrl,
        /// url, link, ...), then falls back to any http(s) string, or a bare URL body.
        /// Replace with a strongly typed DTO once the response schema is confirmed.
        /// </summary>
        private static string? TryFindPaymentUrl(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;

            var trimmed = body.Trim().Trim('"');
            if (IsHttpUrl(trimmed)) return trimmed;

            try
            {
                using var doc = JsonDocument.Parse(body);
                return Find(doc.RootElement, preferNamed: true) ?? Find(doc.RootElement, preferNamed: false);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string? Find(JsonElement element, bool preferNamed)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.String)
                        {
                            var value = prop.Value.GetString();
                            var named = Array.Exists(UrlPropertyNames,
                                n => string.Equals(n, prop.Name, StringComparison.OrdinalIgnoreCase));
                            if (IsHttpUrl(value) && (named || !preferNamed)) return value;
                        }
                        else
                        {
                            var nested = Find(prop.Value, preferNamed);
                            if (nested != null) return nested;
                        }
                    }
                    break;
                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        var nested = Find(item, preferNamed);
                        if (nested != null) return nested;
                    }
                    break;
            }
            return null;
        }

        private static bool IsHttpUrl(string? value) =>
            Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }
}