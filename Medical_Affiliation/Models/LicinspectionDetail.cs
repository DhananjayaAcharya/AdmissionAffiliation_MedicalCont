using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LicinspectionDetail
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? PhoneNumber { get; set; }

    public string? TypeofMember { get; set; }

    public string? CollegeName { get; set; }

    public DateOnly? DateOfInspection { get; set; }

    public bool? IsCompleted { get; set; }

    public string? SelectedCollegeCode { get; set; }

    public byte[]? AttendenceDoc { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? Faculty { get; set; }

    public int? FacultyId { get; set; }
}
