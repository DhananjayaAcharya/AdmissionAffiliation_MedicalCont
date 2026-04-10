using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMedStaffParticularsOther
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string? RegistrationNo { get; set; }

    public string? SubFacultyCode { get; set; }

    public string? TeachersUpdatedInEms { get; set; }

    public string? ExaminerDetailsAttached { get; set; }

    public string? ExaminerDetailsPdfName { get; set; }

    public byte[]? ExaminerDetailsPdf { get; set; }

    public string? AebaslastThreeMonthsPdfName { get; set; }

    public byte[]? AebaslastThreeMonthsPdf { get; set; }

    public string? AebasinspectionDayPdfName { get; set; }

    public byte[]? AebasinspectionDayPdf { get; set; }

    public string? ServiceRegisterMaintained { get; set; }

    public string? AcquittanceRegisterMaintained { get; set; }

    public string? ProvidentFundPdfName { get; set; }

    public byte[]? ProvidentFundPdf { get; set; }

    public string? EsipdfName { get; set; }

    public byte[]? Esipdf { get; set; }

    public string? CourseLevel { get; set; }

    public byte[]? TeachersUpdatedPdf { get; set; }

    public string? TeachersUpdatedPdfName { get; set; }
}
