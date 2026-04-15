namespace Medical_Affiliation.Models
{
    public class AffiliationPaymentViewModel
    {
        public int Id { get; set; }
        public string CollegeCode { get; set; }
        public int FacultyCode { get; set; }
        public int AffiliationTypeId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string TransactionReferenceNo { get; set; }
        public string? SupportingDocument { get; set; }
        public IFormFile File { get; set; }
    }
}
