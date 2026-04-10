using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AhsinstitutionDetail
{
    public int Id { get; set; }

    public string? TypeOfInstitution { get; set; }

    public string? NameOfInstitution { get; set; }

    public string? AddressOfInstitution { get; set; }

    public string? CityTownVillage { get; set; }

    public string? District { get; set; }

    public string? Taluk { get; set; }

    public int? PinCode { get; set; }

    public int? Stdcode { get; set; }

    public long? MobileNumber { get; set; }

    public long? LandLineNumber { get; set; }

    public string? Fax { get; set; }

    public string? EmailId { get; set; }

    public string? Website { get; set; }

    public int? YearOfEstablishment { get; set; }

    public string? SurveyOrPidno { get; set; }

    public string? RuralInstitute { get; set; }

    public string? MinorityInstitute { get; set; }

    public string? NameOfInstitutionHead { get; set; }

    public string? AddressOfInstitutionHead { get; set; }

    public string? AuthorityName { get; set; }

    public string? CollegeStatus { get; set; }

    public byte[]? GoKorderFile { get; set; }

    public int? CollegeEstablishedYear { get; set; }

    public string? CollegeCode { get; set; }
}
