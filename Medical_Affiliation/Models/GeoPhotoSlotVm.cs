using System.ComponentModel.DataAnnotations;

namespace GeoPhotoModule.Models
{
    /// <summary>One upload box (slot) on the page.</summary>
    public class GeoPhotoSlotVm
    {
        public int CategoryId { get; set; }
        public int SlotNo { get; set; }

        public long? PhotoId { get; set; }
        public string? ImagePath { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? AccuracyMeters { get; set; }
        public DateTime? CapturedOn { get; set; }
        public DateTime? UploadedOn { get; set; }

        public bool HasPhoto => PhotoId.HasValue;
    }

    /// <summary>A photo category (College Building, ICU, Labs ...) with its slots.</summary>
    public class GeoPhotoCategoryVm
    {
        public int CategoryId { get; set; }
        public string CategoryCode { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public int RequiredImages { get; set; }
        public int DisplayOrder { get; set; }
        public List<GeoPhotoSlotVm> Slots { get; set; } = new();

        public int UploadedCount => Slots.Count(s => s.HasPhoto);
    }

    /// <summary>Model for the Index page.</summary>
    public class GeoPhotoPageVm
    {
        public string CollegeCode { get; set; } = "";
        public string FacultyCode { get; set; } = "";
        public List<GeoPhotoCategoryVm> Categories { get; set; } = new();

        public int TotalRequired => Categories.Sum(c => c.RequiredImages);
        public int TotalUploaded => Categories.Sum(c => c.UploadedCount);
    }

    /// <summary>Posted by the page (multipart/form-data) for each image.</summary>
    public class GeoPhotoUploadRequest
    {
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Range(1, 20)]
        public byte SlotNo { get; set; }

        [Required, Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Required, Range(-180, 180)]
        public decimal? Longitude { get; set; }

        [Range(0, 999999)]
        public decimal? AccuracyMeters { get; set; }

        public DateTime? CapturedOn { get; set; }

        public IFormFile? Photo { get; set; }
    }

    /// <summary>Data passed to usp_GeoPhoto_Save.</summary>
    public class GeoPhotoSaveDto
    {
        public string CollegeCode { get; set; } = "";
        public string FacultyCode { get; set; } = "";
        public int CategoryId { get; set; }
        public byte ImageSlotNo { get; set; }
        public string ImagePath { get; set; } = "";
        public string? OriginalFileName { get; set; }
        public string? ContentType { get; set; }
        public int? FileSizeKB { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? AccuracyMeters { get; set; }
        public DateTime? CapturedOn { get; set; }
        public string? DeviceInfo { get; set; }
        public string? UploadedBy { get; set; }
        public string? UploadedIP { get; set; }
    }

    /// <summary>Minimal row used to stream an image back securely.</summary>
    public class GeoPhotoFileInfo
    {
        public long PhotoId { get; set; }
        public string CollegeCode { get; set; } = "";
        public string FacultyCode { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public string? ContentType { get; set; }
    }
}