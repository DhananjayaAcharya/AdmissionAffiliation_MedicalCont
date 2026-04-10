using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstHospitalDocument
{
    public int Id { get; set; }

    public string DocumentName { get; set; } = null!;

    public int FacultyCode { get; set; }

    public string CertificateNo { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
