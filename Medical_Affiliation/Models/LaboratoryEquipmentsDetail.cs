using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LaboratoryEquipmentsDetail
{
    public int LabEqId { get; set; }

    public string RegistrationNumber { get; set; } = null!;

    public int? NoAvailable { get; set; }

    public string? CourseCode { get; set; }

    public int? SubjectId { get; set; }

    public string? CreatedUser { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedUser { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public string? Ipaddress { get; set; }

    public int? IsAvailable { get; set; }
}
