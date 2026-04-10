using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaAcademicMatter
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string? RegistrationNumber { get; set; }

    public byte[]? AcademicPerformanceFile { get; set; }

    public string? AcademicPerformanceFileName { get; set; }

    public string? TheoryClassesRatio { get; set; }

    public string? PracticalClassesRatio { get; set; }

    public byte[]? CourseCurriculumFile { get; set; }

    public string? CourseCurriculumFileName { get; set; }

    public string? YearOfStarting { get; set; }

    public string? NatureOfActivities { get; set; }

    public byte[]? CeuMembersFile { get; set; }

    public string? CeuMembersFileName { get; set; }

    public byte[]? CeuProgramsFile { get; set; }

    public string? CeuProgramsFileName { get; set; }

    public string? PublicationsLast3Years { get; set; }

    public string? ResearchProjectsPgstudents { get; set; }

    public byte[]? IndexedJournalsFile { get; set; }

    public string? IndexedJournalsFileName { get; set; }

    public byte[]? FundedStaffListFile { get; set; }

    public string? FundedStaffListFileName { get; set; }

    public byte[]? AcademicCommitteeFile { get; set; }

    public string? AcademicCommitteeFileName { get; set; }

    public byte[]? AntiRaggingCommitteeFile { get; set; }

    public string? AntiRaggingCommitteeFileName { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
