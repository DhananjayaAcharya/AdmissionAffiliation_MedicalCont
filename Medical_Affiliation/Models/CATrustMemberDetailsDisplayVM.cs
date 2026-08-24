namespace Medical_Affiliation.Models
{
    public class CATrustMemberDetailsDisplayVM
    {
        public List<CATrustMemberDisplayRowVM> Members { get; set; } = new();

        public bool HasRegisteredTrustMemberDetails { get; set; }
    }

    public class CATrustMemberDisplayRowVM
    {
        public int SlNo { get; set; }
        public string? TrustMemberName { get; set; }
        public string? Designation { get; set; }
        public string? Qualification { get; set; }
        public string? MobileNumber { get; set; }
        public int? Age { get; set; }
        public DateOnly? JoiningDate { get; set; }
    }
}