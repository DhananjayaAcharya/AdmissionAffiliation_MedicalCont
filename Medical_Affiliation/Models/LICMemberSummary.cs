namespace Medical_Affiliation.Models
{
    public class LICMemberSummary
    {
        public string MemberName { get; set; }
        public string PhoneNumber { get; set; }   // ← add this
        public List<DateOnly> InspectionDates { get; set; }
        public decimal TotalClaim { get; set; }
        public bool DateMismatchFlag { get; set; }
    }
}