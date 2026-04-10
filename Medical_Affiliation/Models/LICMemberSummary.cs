namespace Medical_Affiliation.Models
{
    public class LICMemberSummary
    {
        public string MemberName { get; set; }

        public List<DateOnly> InspectionDates { get; set; }

        public decimal TotalClaim { get; set; }

        public bool DateMismatchFlag { get; set; }
    }
}