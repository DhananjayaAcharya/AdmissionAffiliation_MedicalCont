using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class GeoPhotoCategoryMaster
{
    public int CategoryId { get; set; }

    public string CategoryCode { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public byte RequiredImages { get; set; }

    public byte DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<GeoPhotoUpload> GeoPhotoUploads { get; set; } = new List<GeoPhotoUpload>();
}
