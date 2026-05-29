using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class UgPrintedUpload
{
    public int Id { get; set; }

    public int? FacultyCode { get; set; }

    public string? CollegeCode { get; set; }

    public string? DocumentPath { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? Ipaddress { get; set; }

    public string? ReferenceId { get; set; }

    public string? EofficeNo { get; set; }
}
