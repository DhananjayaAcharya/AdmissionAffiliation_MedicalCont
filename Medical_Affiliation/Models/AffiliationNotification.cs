using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationNotification
{
    public int NotificationId { get; set; }

    public int FacultyCode { get; set; }

    public string NotificationType { get; set; } = null!;

    public string? CollegeCode { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
