namespace Medical_Affiliation.Models
{
    public class DocumentViewerViewModel
    {
        public string? DocumentUrl { get; set; }

        public string? DocumentName { get; set; }

        public string? DocumentType { get; set; }

        public bool AllowDownload { get; set; } = true;

        public bool AllowPrint { get; set; } = true;
    }
}