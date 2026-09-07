namespace Medical_Affiliation.Models
{
    public class DentalPaymentSubmitViewModel
    {
        public int AffiliationTypeId { get; set; }

        public string? TransactionId { get; set; }

        public decimal AmountPaid { get; set; }

        public IFormFile? TransactionReceipt { get; set; }
    }
}
