using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedicalAdministrativePhysicalFacility
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string? CourseLevel { get; set; }

    public decimal? PrincipalChamberAreaSqFt { get; set; }

    public decimal? OfficeRoomAreaSqFt { get; set; }

    public decimal? StaffRoomsAreaSqFt { get; set; }

    public decimal? LectureHallsAreaSqFt { get; set; }

    public decimal? LaboratoriesAreaSqFt { get; set; }

    public decimal? SeminarHallAreaSqFt { get; set; }

    public decimal? AuditoriumAreaSqFt { get; set; }

    public decimal? MuseumAreaSqFt { get; set; }

    public bool? ExaminationHallAvailable { get; set; }

    public int? WorkshopStaffCount { get; set; }

    public string? WorkshopEquipmentDetails { get; set; }

    public string? WorkshopScopeOfWork { get; set; }

    public decimal? AnimalHouseAreaSqFt { get; set; }

    public int? AnimalHouseStaffCount { get; set; }

    public string? AnimalTypes { get; set; }

    public decimal? CommitteeRoomsAreaSqFt { get; set; }

    public bool? CommonRoomMenAvailable { get; set; }

    public bool? CommonRoomWomenAvailable { get; set; }

    public bool? StudentHostelAvailable { get; set; }

    public bool? StaffQuartersPrincipal { get; set; }

    public bool? StaffQuartersOtherStaff { get; set; }

    public bool? StaffQuartersTeachingAncillary { get; set; }

    public bool? RegisteredUnderAnatomyAct { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? AnimalHouseAvailable { get; set; }
}
