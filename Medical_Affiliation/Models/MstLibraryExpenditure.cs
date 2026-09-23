using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstLibraryExpenditure
{
    public int LibraryExpenditureId { get; set; }

    public int FacultyId { get; set; }

    public int TypeId { get; set; }

    public string ItemName { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual Faculty Faculty { get; set; } = null!;

    public virtual ICollection<LibraryExpenditure> LibraryExpenditures { get; set; } = new List<LibraryExpenditure>();

    public virtual TypeOfAffiliation Type { get; set; } = null!;
}
