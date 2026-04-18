using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffHostelDetail
{
    public int HostelDetailsId { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string HostelType { get; set; } = null!;

    public string BuiltUpAreaSqFt { get; set; } = null!;

    public bool HasSeparateHostel { get; set; }

    public bool SeparateProvisionMaleFemale { get; set; }

    public string TotalFemaleStudents { get; set; } = null!;

    public string TotalFemaleRooms { get; set; } = null!;

    public string TotalMaleStudents { get; set; } = null!;

    public string TotalMaleRooms { get; set; } = null!;

    public string? PossessionProofPath { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? CreatedBy { get; set; }

    public bool? CommonRoomMen { get; set; }

    public bool? CommonRoomWomen { get; set; }

    public string? AnyOtherFacility { get; set; }

    public string? HostelFacilityDetails { get; set; }

    public int? HostelMenCount { get; set; }

    public int? HostelWomenCount { get; set; }

    public string? OwnOrRented { get; set; }

    public decimal? SpacePerStudent { get; set; }

    public bool? SleepingFurniture { get; set; }

    public bool? SanitaryBathing { get; set; }

    public bool? DiningHall { get; set; }

    public bool? HostelCommonRoom { get; set; }

    public bool? VisitorsRoom { get; set; }

    public bool? KitchenPantry { get; set; }

    public bool? WardenOffice { get; set; }

    public bool? ReceptionCounter { get; set; }

    public bool? GamesRecreation { get; set; }

    public bool? MedicalFacilities { get; set; }

    public string? CourseLevel { get; set; }
}
