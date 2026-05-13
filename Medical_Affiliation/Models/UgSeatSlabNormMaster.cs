using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class UgSeatSlabNormMaster
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public int SeatSlab { get; set; }

    public decimal LandTier2Acres { get; set; }

    public decimal LandOtherAreaAcres { get; set; }

    public int CollegeBuiltupAreaSqm { get; set; }

    public int LectureHallCount { get; set; }

    public int LectureHallAreaSqm { get; set; }

    public int LectureHallCapacity { get; set; }

    public int ExaminationHallAreaSqm { get; set; }

    public int LibraryAreaSqm { get; set; }

    public int DentalHospitalAreaSqm { get; set; }

    public DateTime? CreatedOn { get; set; }

    public bool RequiresMuseumAndDemoRooms { get; set; }

    public bool RequiresDepartmentalAreas { get; set; }

    public bool RequiresPreclinicalSkillLabs { get; set; }

    public bool RequiresFutureExpansion { get; set; }

    public decimal? MaximumCollegeHospitalDistanceKm { get; set; }
}
