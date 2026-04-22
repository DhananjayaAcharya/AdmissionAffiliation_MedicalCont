using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMedStaffParticularsOtherTemp
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string? TeachersUpdatedInEms { get; set; }

    public string? ExaminerDetailsAttached { get; set; }

    public string? ServiceRegisterMaintained { get; set; }

    public string? AcquittanceRegisterMaintained { get; set; }

    public string? ExaminerDetailsPdfName { get; set; }

    public string? AebaslastThreeMonthsPdfName { get; set; }

    public string? AebasinspectionDayPdfName { get; set; }

    public string? ProvidentFundPdfName { get; set; }

    public string? EsipdfName { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? TeachersUpdatedPdfName { get; set; }

    public string? CourseLevel { get; set; }

    public string? ExaminerDetailsPdfPath { get; set; }

    public string? AebaslastThreeMonthsPdfPath { get; set; }

    public string? AebasinspectionDayPdfPath { get; set; }

    public string? ProvidentFundPdfPath { get; set; }

    public string? EsipdfPath { get; set; }

    public string? TeachersUpdatedPdfPath { get; set; }
}
