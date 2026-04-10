namespace Medical_Affiliation.Models
{
    public class LICMemberReportViewModel
    {
        public string MemberName { get; set; }
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }

        public string TypeOfMember { get; set; }
        public string PhoneNumber { get; set; }

        public List<DateOnly> InspectionDates { get; set; }

        public List<LicclaimDetail> Claims { get; set; }

        public decimal TotalClaimAmount { get; set; }

        public decimal? TravelCost { get; set; }
        public decimal? DACost { get; set; }
        public decimal? LCACost { get; set; }
        public decimal? CollegeCost { get; set; }
        public decimal? AirFareCost { get; set; }
        public decimal? AirRoadCost { get; set; }

        public string? LicApprovalFileName { get; set; }
    }

}