using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class HospitalDocumentDetail
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public int DocumentId { get; set; }

    public int DocumentName { get; set; }

    public byte[] DocumentFile { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
