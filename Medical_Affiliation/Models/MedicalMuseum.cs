using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedicalMuseum
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public bool SeparateAnatomyMuseumAvailable { get; set; }

    public bool PathologyForensicSharedMuseum { get; set; }

    public bool PharmMicroCommSharedMuseum { get; set; }

    public int SeatingCapacityPerMuseum { get; set; }

    public decimal SeatingAreaAvailableSqm { get; set; }

    public decimal SeatingAreaRequiredSqm { get; set; }

    public decimal SeatingAreaDeficiencySqm { get; set; }

    public bool MuseumsHaveAv { get; set; }

    public bool MuseumsHaveInternet { get; set; }

    public bool MuseumsDigitallyLinked { get; set; }

    public bool MuseumsHaveRacksShelves { get; set; }

    public bool MuseumsHaveRadiologyDisplay { get; set; }

    public bool TeachingTimeSharingProgrammed { get; set; }

    public string? CollegeCode { get; set; }

    public string? CourseLevel { get; set; }
}
