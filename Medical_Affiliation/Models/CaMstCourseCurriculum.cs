using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMstCourseCurriculum
{
    public int CurriculumId { get; set; }

    public string CurriculumName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<CaCourseCurriculum> CaCourseCurricula { get; set; } = new List<CaCourseCurriculum>();
}
