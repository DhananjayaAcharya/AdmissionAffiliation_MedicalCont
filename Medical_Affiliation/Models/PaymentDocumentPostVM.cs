namespace Medical_Affiliation.Models
{
    public class PaymentDocumentPostVM
    {
        public string CollegeCode { get; set; }
        public int FacultyCode { get; set; }
        public string CourseLevel { get; set; }
        public int AffiliationTypeId { get; set; }

        public string TransactionId { get; set; }
        public decimal? PaymentAmount { get; set; }
        public DateTime? PaymentDate { get; set; }

        public IFormFile? ScreenshotFile { get; set; }

        // JSON snapshot of the calculation results the user saw on screen
        // (matched courses, fee lines, grand total, affiliation type name),
        // built client-side from the same Model the PaymentCalculation view
        // already rendered. Used to populate the WhatsApp receipt PDF without
        // needing to re-run the fee calculation server-side.
        public string? CalculationSnapshotJson { get; set; }
    }

    public class PaymentDocumentViewVM
    {
        public int PaymentDocumentId { get; set; }
        public string CollegeCode { get; set; }
        public int FacultyCode { get; set; }
        public string CourseLevel { get; set; }
        public int AffiliationTypeId { get; set; }
        public string TransactionId { get; set; }
        public decimal? PaymentAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? ScreenshotFileName { get; set; }
        public bool HasScreenshot { get; set; }
    }
}