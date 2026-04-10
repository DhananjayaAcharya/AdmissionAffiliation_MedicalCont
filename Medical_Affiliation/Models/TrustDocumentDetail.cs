using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TrustDocumentDetail
{
    public int? FacultyCode { get; set; }

    public int DocumentId { get; set; }

    public string? DocumentName { get; set; }

    public string? FileName { get; set; }

    public byte[]? DocumentFile { get; set; }

    public DateTime? UploadedOn { get; set; }
}
