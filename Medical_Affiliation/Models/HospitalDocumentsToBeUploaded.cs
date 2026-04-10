using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class HospitalDocumentsToBeUploaded
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string HospitalName { get; set; } = null!;

    public int HospitalDetailsId { get; set; }

    public int DocumentId { get; set; }

    public string DocumentName { get; set; } = null!;

    public string CertificateNumber { get; set; } = null!;

    public byte[] HospitalDocumentFile { get; set; } = null!;

    public virtual HospitalDetailsForAffiliation HospitalDetails { get; set; } = null!;
}
