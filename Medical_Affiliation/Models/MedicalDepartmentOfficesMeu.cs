using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedicalDepartmentOfficesMeu
{
    public int Id { get; set; }

    public bool HasHodRoomWithOfficeAndRecords { get; set; }

    public bool HasRoomsForFacultyAndResidents { get; set; }

    public bool FacultyRoomsHaveCommunicationComputerInternet { get; set; }

    public bool HasRoomsForNonTeachingStaff { get; set; }

    public bool HasMedicalEducationUnit { get; set; }

    public decimal? MedicalEducationUnitAreaSqm { get; set; }

    public bool? MedicalEducationUnitHasAudioVisual { get; set; }

    public bool? MedicalEducationUnitHasInternet { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? MeuCoordinatorName { get; set; }

    public string? MeuCoordinatorDesignationDepartment { get; set; }

    public string? MeuCoordinatorPhone { get; set; }

    public string? MeuCoordinatorEmail { get; set; }

    public string? MeuMembersListDescription { get; set; }

    public string? MeuActivitiesLastAcademicYear { get; set; }

    public byte[]? MeuMembersListFile { get; set; }

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public string? CourseLevel { get; set; }

    public bool? HasDentalEducationUnit { get; set; }

    public decimal? DentalEducationUnitAreaSqm { get; set; }

    public bool? DentalEducationUnitHasAudioVisual { get; set; }

    public bool? DentalEducationUnitHasInternet { get; set; }

    public string? MeuMembersListFilePath { get; set; }

    public string? DeuCoordinatorName { get; set; }

    public string? DeuCoordinatorDesignationDepartment { get; set; }

    public string? DeuCoordinatorPhone { get; set; }

    public string? DeuCoordinatorEmail { get; set; }

    public string? DeuMembersListDescription { get; set; }

    public string? DeuActivitiesLastAcademicYear { get; set; }

    public string? DeuMembersListFilePath { get; set; }
}
