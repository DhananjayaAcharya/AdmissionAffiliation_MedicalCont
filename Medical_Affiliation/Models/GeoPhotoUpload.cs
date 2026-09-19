using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class GeoPhotoUpload
{
    public long PhotoId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public int CategoryId { get; set; }

    public byte ImageSlotNo { get; set; }

    public string ImagePath { get; set; } = null!;

    public string? OriginalFileName { get; set; }

    public string? ContentType { get; set; }

    public int? FileSizeKb { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public decimal? AccuracyMeters { get; set; }

    public DateTime? CapturedOn { get; set; }

    public string? DeviceInfo { get; set; }

    public string? UploadedBy { get; set; }

    public string? UploadedIp { get; set; }

    public DateTime UploadedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsActive { get; set; }

    public virtual GeoPhotoCategoryMaster Category { get; set; } = null!;
}
