using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedicalSkillsLaboratory
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public int AnnualMbbsIntake { get; set; }

    public decimal TotalAreaAvailableSqm { get; set; }

    public decimal TotalAreaRequiredSqm { get; set; }

    public decimal TotalAreaDeficiencySqm { get; set; }

    public bool SixWeeksTrainingCompletedBeforeClinical { get; set; }

    public int NumberOfExaminationRooms { get; set; }

    public bool HasMinFourExamRooms { get; set; }

    public bool HasDemoRoomSmallGroups { get; set; }

    public bool HasDebriefArea { get; set; }

    public bool HasFacultyCoordinatorRoom { get; set; }

    public bool HasSupportStaffRoom { get; set; }

    public bool HasStorageForMannequins { get; set; }

    public bool HasVideoRecordingFacility { get; set; }

    public int NumberOfSkillStations { get; set; }

    public bool HasGroupAndIndividualStations { get; set; }

    public bool HasRequiredTrainersAndMannequins { get; set; }

    public bool HasDedicatedTechnicalOfficer { get; set; }

    public bool HasAdequateSupportStaff { get; set; }

    public bool TeachingAreasHaveAv { get; set; }

    public bool TeachingAreasHaveInternet { get; set; }

    public bool SkillsLabEnabledForElearning { get; set; }

    public string? CollegeCode { get; set; }
}
