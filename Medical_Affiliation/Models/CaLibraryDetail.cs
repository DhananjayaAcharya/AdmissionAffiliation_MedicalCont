using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaLibraryDetail
{
    public int Id { get; set; }

    public string? RegistrationNo { get; set; }

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public int? TotalNursingBooks { get; set; }

    public int? TotalNursingJournals { get; set; }

    public bool? InternetFacility { get; set; }

    public int? TotalThesis { get; set; }

    public int? TotalEbooks { get; set; }

    public int? BooksPurchasedLastYear { get; set; }

    public decimal? TotalBudget { get; set; }

    public bool? IndependentBuilding { get; set; }

    public int? TotalFloorAreaSqFt { get; set; }
}
