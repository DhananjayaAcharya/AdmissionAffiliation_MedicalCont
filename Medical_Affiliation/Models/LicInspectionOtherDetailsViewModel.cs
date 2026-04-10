using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class LicInspectionOtherDetailsViewModel
    {
        public string SenateCode { get; set; }
        public string SenetMemberName { get; set; }
        public List<CollegeGroupVM> GroupedColleges { get; set; } = new();
        public HashSet<string> CompletedSet { get; set; } = new();
        public List<OtherDetails> OtherDetailsList { get; set; } = new();

        // This is what the form posts
        public List<OtherDetails> PendingList { get; set; } = new();
    }
    public class CollegeGroupVM
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string CollegePlace { get; set; }
        public string SenetMember { get; set; }
        public string SenetMemberPhNo { get; set; }
        public DateOnly? DateOfInspection { get; set; }
        public List<ACMemberVM> ACMembers { get; set; } = new();
        public List<SubjectExpertiseVM> SubjectExpertiseMembers { get; set; } = new();
    }

    public class ClaimsAmountvm
    {
        public int TotalCost { get; set; }
        public string CollegeCode { set; get; }
        public string PhoneNumber { get; set; }
        public string ModeOfTransport { get; set; }
        public string FromPlace { get; set; }
        public string ToPlace { get; set; }
        public string KiloMeters { get; set; }
        public string ReturnFromPlace { get; set; }
        public string ReturnToPlace { get; set; }
        public string ReturnKilometers { get; set; }
        public List<string> AssignedColleges { get; set; } = new();
        public int NoOfDays { get; set; }
        public decimal Airfare { get; set; }

        public decimal TravelCost { get; set; }
        public decimal DACost { get; set; }
        public decimal LCACost { get; set; }
        public decimal CollegeCost { get; set; }

    }

    public class LicCollegeList
    {
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
    }

    public class LicInspectionMemberVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
    }

    public class SenateMemberVM : LicInspectionMemberVM
    {

    }

    public class ACMemberVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNo { get; set; }
        public decimal InspectionAmount { get; set; }
    }

    public class SubjectExpertiseVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNo { get; set; }
        public string RevisedOrder { get; set; }
        public decimal InspectionAmount { get; set; }

    }


    public class InspectionDetails
    {
        public int Id { get; set; }

        public string SelectedMemberCode { get; set; }
        public string SelectedMemberName { get; set; }

        [ValidateNever]
        public string MemberType { get; set; }

        [Required]
        public DateOnly? DateOfInspection { get; set; }
        public bool IsAttended { get; set; }

        public string collegeCode { get; set; }
        public string collegeName { get; set; }

    }

    public class OtherDetails : InspectionDetails
    {
        public string SenateCode { get; set; }
        public string Phonenumberselected { get; set; }
    }

    public class PendingInspection : InspectionDetails { }

}