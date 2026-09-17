using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationNotificationRead
{
    public int NotificationReadId { get; set; }

    public int NotificationId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime? ReadDate { get; set; }

    public DateTime CreatedDate { get; set; }
}
