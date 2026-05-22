using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class UgFacultyDetail
{
    public int Id { get; set; }

    public int? FacultyCode { get; set; }

    public string? CollegeCode { get; set; }

    public string? DepartmentCode { get; set; }

    public string? NameOftheFaculty { get; set; }

    public string? DesignationCode { get; set; }

    public string? Dob { get; set; }

    public string? DateOfAppointment { get; set; }

    public string? AadhaarNo { get; set; }

    public string? Panno { get; set; }

    public string MobileNo { get; set; } = null!;

    public string? EmailId { get; set; }

    public string? StateCouncilRegNo { get; set; }

    public string? AebasattendId { get; set; }

    public string? ProfessionalQualification { get; set; }

    public string? NatureOfEmployment { get; set; }

    public string? TeachingExpInYrs { get; set; }

    public string? PhotoFilePath { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? Ipaddress { get; set; }

    public bool? IsDeclared { get; set; }

    public string? PrincipalName { get; set; }

    public bool? PrintedCopyUploaded { get; set; }

    public byte[] RowTimestamp { get; set; } = null!;
}
