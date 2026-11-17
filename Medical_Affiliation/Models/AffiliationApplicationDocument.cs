using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationApplicationDocument
{
    public long DocumentId { get; set; }

    public Guid DocumentGuid { get; set; }

    public int TypeId { get; set; }

    public string DocumentTitle { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string FileExtension { get; set; } = null!;

    public string StorageDrive { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string? UploadedBy { get; set; }

    public DateTime UploadedOn { get; set; }

    public bool IsActive { get; set; }

    public virtual TypeOfAffiliation Type { get; set; } = null!;
}
