using System.Text.Json;
using Medical_Affiliation.Models;
using Microsoft.Extensions.Options;

namespace Medical_Affiliation.Services
{
    public interface IWhatsAppService
    {
        Task<WhatsAppSendResult> SendMessageAsync(
            string phoneNumber,
            string message);

        Task<WhatsAppSendResult> SendTemplateAsync(
            string phoneNumber,
            string templateId);

        Task<WhatsAppSendResult> SendFileAsync(
            string phoneNumber,
            string mediaUrl,
            string mediaType,
            string mediaName,
            string? captionText = null);
    }

    public class WhatsAppSendResult
    {
        public bool Success { get; set; }
        public string? WaMessageId { get; set; }
        public string? Message { get; set; }
    }

    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly WhatsAppSettings _settings;

        public WhatsAppService(HttpClient httpClient, IOptions<WhatsAppSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<WhatsAppSendResult> SendMessageAsync(
            string phoneNumber,
            string message)
        {
            var normalizedPhone = NormalizeIndianMobile(phoneNumber);

            if (string.IsNullOrWhiteSpace(normalizedPhone))
                return new WhatsAppSendResult { Success = false, Message = "Invalid or missing phone number." };

            if (string.IsNullOrWhiteSpace(_settings.MessageApiBaseUrl))
                return new WhatsAppSendResult { Success = false, Message = "WhatsApp message API URL is not configured." };

            if (string.IsNullOrWhiteSpace(_settings.ApiToken))
                return new WhatsAppSendResult { Success = false, Message = "WhatsApp API token is not configured." };

            if (string.IsNullOrWhiteSpace(_settings.PhoneNumberId))
                return new WhatsAppSendResult { Success = false, Message = "WhatsApp phone number ID is not configured." };

            var form = new Dictionary<string, string>
            {
                ["apiToken"] = _settings.ApiToken,
                ["phone_number_id"] = _settings.PhoneNumberId,
                ["message"] = message,
                ["phone_number"] = normalizedPhone
            };

            try
            {
                using var content = new FormUrlEncodedContent(form);
                var response = await _httpClient.PostAsync(_settings.MessageApiBaseUrl, content);
                var body = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[WhatsApp] Message API status: {(int)response.StatusCode} {response.StatusCode}");
                Console.WriteLine($"[WhatsApp] Message API response: {body}");

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                var status = root.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null;
                var responseMessage = root.TryGetProperty("message", out var messageEl) ? messageEl.GetString() : null;
                var waMessageId = root.TryGetProperty("wa_message_id", out var idEl) ? idEl.GetString() : null;
                
                return new WhatsAppSendResult
                {
                    Success = response.IsSuccessStatusCode && status == "1",
                    WaMessageId = waMessageId,
                    Message = responseMessage ?? body
                };
            }
            catch (Exception ex)
            {
                return new WhatsAppSendResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<WhatsAppSendResult> SendTemplateAsync(
            string phoneNumber,
            string templateId)
        {
            var normalizedPhone = NormalizeIndianMobile(phoneNumber);

            if (string.IsNullOrWhiteSpace(normalizedPhone))
                return new WhatsAppSendResult { Success = false, Message = "Invalid or missing phone number." };

            if (string.IsNullOrWhiteSpace(_settings.TemplateApiBaseUrl))
                return new WhatsAppSendResult { Success = false, Message = "WhatsApp template API URL is not configured." };

            if (string.IsNullOrWhiteSpace(templateId))
                return new WhatsAppSendResult { Success = false, Message = "WhatsApp template ID is not configured." };

            if (string.IsNullOrWhiteSpace(_settings.ApiToken))
                return new WhatsAppSendResult { Success = false, Message = "WhatsApp API token is not configured." };

            if (string.IsNullOrWhiteSpace(_settings.PhoneNumberId))
                return new WhatsAppSendResult { Success = false, Message = "WhatsApp phone number ID is not configured." };

            var form = new Dictionary<string, string>
            {
                ["apiToken"] = _settings.ApiToken,
                ["phone_number_id"] = _settings.PhoneNumberId,
                ["phone_number"] = normalizedPhone,
                ["template_id"] = templateId
            };
             
            try
            {
                using var content = new FormUrlEncodedContent(form);
                var response = await _httpClient.PostAsync(_settings.TemplateApiBaseUrl, content);
                var body = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[WhatsApp] Template API status: {(int)response.StatusCode} {response.StatusCode}");
                Console.WriteLine($"[WhatsApp] Template API response: {body}");

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                var status = root.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null;
                var responseMessage = root.TryGetProperty("message", out var messageEl) ? messageEl.GetString() : null;
                var waMessageId = root.TryGetProperty("wa_message_id", out var idEl) ? idEl.GetString() : null;

                return new WhatsAppSendResult
                {
                    Success = response.IsSuccessStatusCode && status == "1",
                    WaMessageId = waMessageId,
                    Message = responseMessage ?? body
                };
            }
            catch (Exception ex)
            {
                return new WhatsAppSendResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<WhatsAppSendResult> SendFileAsync(
            string phoneNumber,
            string mediaUrl,
            string mediaType,
            string mediaName,
            string? captionText = null)
        {
            var normalizedPhone = NormalizeIndianMobile(phoneNumber);

            if (string.IsNullOrWhiteSpace(normalizedPhone))
            {
                Console.WriteLine($"[WhatsApp] Failed to normalize phone number: {phoneNumber}");
                return new WhatsAppSendResult { Success = false, Message = "Invalid or missing phone number." };
            }

            if (string.IsNullOrWhiteSpace(_settings.ApiBaseUrl))
            {
                Console.WriteLine($"[WhatsApp] ApiBaseUrl is empty in WhatsAppSettings");
                return new WhatsAppSendResult { Success = false, Message = "WhatsApp API base URL is not configured." };
            }

            if (string.IsNullOrWhiteSpace(_settings.ApiToken))
            {
                Console.WriteLine($"[WhatsApp] ApiToken is empty in WhatsAppSettings");
                return new WhatsAppSendResult { Success = false, Message = "WhatsApp API token is not configured." };
            }

            if (string.IsNullOrWhiteSpace(_settings.PhoneNumberId))
            {
                Console.WriteLine($"[WhatsApp] PhoneNumberId is empty in WhatsAppSettings — this is REQUIRED for WhatsApp to work. Please add your WhatsApp phone number ID to appsettings.json.");
                return new WhatsAppSendResult { Success = false, Message = "WhatsApp phone number ID is not configured. Please contact your administrator." };
            }

            var form = new Dictionary<string, string>
            {
                ["apiToken"] = _settings.ApiToken,
                ["phone_number_id"] = _settings.PhoneNumberId,
                ["phone_number"] = normalizedPhone,
                ["media_url"] = mediaUrl,
                ["media_type"] = mediaType,
                ["media_name"] = mediaName
            };

            if (!string.IsNullOrWhiteSpace(captionText))
            {
                form["media_caption_text"] = captionText;
            }

            Console.WriteLine($"[WhatsApp] Sending {mediaType} to {normalizedPhone}: {mediaName}");

            try
            {
                using var content = new FormUrlEncodedContent(form);
                Console.WriteLine($"[WhatsApp] API URL: {_settings.ApiBaseUrl}");
                Console.WriteLine($"[WhatsApp] Request params: phone={normalizedPhone}, media_url={mediaUrl}, media_type={mediaType}");
                
                var response = await _httpClient.PostAsync(_settings.ApiBaseUrl, content);
                var body = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[WhatsApp] API Response Status: {response.StatusCode}");
                Console.WriteLine($"[WhatsApp] API Response Body: {body}");

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                var status = root.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null;
                var message = root.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : null;
                var waMessageId = root.TryGetProperty("wa_message_id", out var idEl) ? idEl.GetString() : null;

                Console.WriteLine($"[WhatsApp] Parsed response: status={status}, message={message}, waMessageId={waMessageId}");

                return new WhatsAppSendResult
                {
                    Success = status == "1",
                    WaMessageId = waMessageId,
                    Message = message ?? body
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WhatsApp] Exception sending to {normalizedPhone}: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"[WhatsApp] Stack trace: {ex.StackTrace}");
                return new WhatsAppSendResult { Success = false, Message = ex.Message };
            }
        }

        // API requires: "Must start with country code and only numeric characters are allowed."
        // Assumes 10-digit Indian mobile numbers stored without country code — prepends "91".
        // Adjust this if PrincipalMobileNumber is sometimes stored with a country code already.
        private static string NormalizeIndianMobile(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                Console.WriteLine($"[WhatsApp] NormalizeIndianMobile: Input is null/empty");
                return string.Empty;
            }

            // Remove all non-digit characters
            var digits = new string(raw.Where(char.IsDigit).ToArray());
            Console.WriteLine($"[WhatsApp] NormalizeIndianMobile: Raw input='{raw}', Extracted digits='{digits}', Length={digits.Length}");

            // If already has country code (12 digits), keep as is
            if (digits.Length == 12 && digits.StartsWith("91"))
            {
                Console.WriteLine($"[WhatsApp] Phone already has 91 prefix: {digits}");
                return digits;
            }

            // If 10 digits, prepend 91
            if (digits.Length == 10)
            {
                digits = "91" + digits;
                Console.WriteLine($"[WhatsApp] Added 91 prefix: {digits}");
                return digits;
            }

            // If already has country code prefix but in different format
            if (digits.Length > 10)
            {
                Console.WriteLine($"[WhatsApp] Phone number is {digits.Length} digits (not standard 10 or 12), using as-is: {digits}");
                return digits;
            }

            Console.WriteLine($"[WhatsApp] WARNING: Phone number '{raw}' has invalid length {digits.Length}");
            return digits;
        }
    }
}