namespace Medical_Affiliation.Models
{
    public class NotificationSummaryVM
    {
        public int UnreadCount { get; set; }

        public List<AffiliationNotificationVM> Notifications { get; set; }
            = new List<AffiliationNotificationVM>();
    }
    public class AffiliationNotificationVM
    {
        public int NotificationId { get; set; }

        public int FacultyCode { get; set; }

        public string NotificationType { get; set; } = string.Empty;

        public string? CollegeCode { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsRead { get; set; }

        public bool IsNew => !IsRead;

        public string FacultyName { get; set; } = "Medical";
    }
}
