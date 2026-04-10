using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class IntakeDetail
{
    public int Id { get; set; }

    public string Degree { get; set; } = null!;

    public string? Course { get; set; }

    public DateOnly Lopdate { get; set; }

    public int NumberOfSeats { get; set; }

    public DateOnly PermittedYear { get; set; }

    public DateOnly RecognizedYear { get; set; }

    public string? FreshOrContinuation { get; set; }

    public int? InstitutionId { get; set; }

    public virtual InstitutionDetail? Institution { get; set; }
}
