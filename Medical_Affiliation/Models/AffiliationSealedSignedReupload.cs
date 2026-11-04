using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationSealedSignedReupload
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string TypeOfApplication { get; set; } = null!;

    public string ReuploadedDoc { get; set; } = null!;

    public DateTime CreatedOn { get; set; }
}
