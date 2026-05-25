using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class SmallGroupTeaching
{
    public int Id { get; set; }

    public int AnnualMbbsIntake { get; set; }

    public int SmallGroupBatchSize { get; set; }

    public bool TeachingAreasSharedAllDepts { get; set; }

    public bool AvInAllTeachingAreas { get; set; }

    public bool InternetInAllTeachingAreas { get; set; }

    public bool DigitalLinkAllTeachingAreas { get; set; }

    public int SmallGroupStudents { get; set; }

    public decimal RequiredAreaSqm { get; set; }

    public decimal AvailableAreaSqm { get; set; }

    public decimal AreaDeficiencySqm { get; set; }

    public bool RoomsSharedByAllDepts { get; set; }

    public bool AppropriateAreaEachSpecialty { get; set; }

    public bool ConnectedToLectureHalls { get; set; }

    public bool InternetInTeachingRooms { get; set; }

    public string? FacultyCode { get; set; }

    public string? CollegeCode { get; set; }

    public bool? IsMinimumLandAvailable { get; set; }

    public string? LandDetailsIfYes { get; set; }

    public bool? HasPurchasePlanIfNo { get; set; }

    public bool? HasBudgetProvisionIfNo { get; set; }

    public bool? HasFutureExpansionSpace { get; set; }

    public string? LandRecordsPath { get; set; }

    public bool? IsBuildingAsPerCouncilNorms { get; set; }

    public string? LandOwnershipType { get; set; }

    public string? BuildingOwnershipType { get; set; }

    public decimal? FloorAreaSqFt { get; set; }

    public int? NumberOfBlocks { get; set; }

    public int? NumberOfFloors { get; set; }

    public int? YearOfConstruction { get; set; }

    public string? ApprovedBuildingPlanPath { get; set; }

    public string? CourseLevel { get; set; }

    public string? LandRecordsFilePath { get; set; }

    public string? ApprovedBuildingPlanFilePath { get; set; }
}
