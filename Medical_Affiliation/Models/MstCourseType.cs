using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstCourseType
{
    public int CourseTypeId { get; set; }

    public string CourseTypeName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsUg { get; set; }

    public bool IsPg { get; set; }

    public bool IsSs { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
