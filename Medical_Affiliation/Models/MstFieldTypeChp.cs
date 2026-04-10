using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstFieldTypeChp
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string FieldType { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
