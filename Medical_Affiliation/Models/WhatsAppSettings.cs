namespace Medical_Affiliation.Models
{
    public class WhatsAppSettings
    {
        public string ApiBaseUrl { get; set; } = string.Empty;
        public string MessageApiBaseUrl { get; set; } = string.Empty;
        public string TemplateApiBaseUrl { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public string ApiToken { get; set; } = string.Empty;
        public string PhoneNumberId { get; set; } = string.Empty;
    }
}