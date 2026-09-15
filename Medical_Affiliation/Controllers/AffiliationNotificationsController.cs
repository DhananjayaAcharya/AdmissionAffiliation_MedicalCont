using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class AffiliationNotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AffiliationNotificationsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // NOTIFICATION PAGE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            var facultyCodeString =
                HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrWhiteSpace(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(facultyCodeString, out int facultyCode))
            {
                return RedirectToAction("Login", "Account");
            }

            // -----------------------------------------------------
            // Get notifications applicable to this college
            // -----------------------------------------------------

            var notifications = await
                (
                    from n in _context.AffiliationNotifications.AsNoTracking()

                    join r in _context.AffiliationNotificationReads
                        .AsNoTracking()
                        .Where(x => x.CollegeCode == collegeCode)
                        on n.NotificationId equals r.NotificationId
                        into readGroup

                    from r in readGroup.DefaultIfEmpty()

                    where
                        n.IsActive
                        &&
                        n.FacultyCode == facultyCode
                        &&
                        (
                            n.NotificationType == "General"
                            ||
                            (
                                n.NotificationType == "Specific"
                                &&
                                n.CollegeCode == collegeCode
                            )
                        )

                    orderby n.CreatedDate descending

                    select new AffiliationNotificationVM
                    {
                        NotificationId = n.NotificationId,

                        FacultyCode = n.FacultyCode,

                        NotificationType = n.NotificationType,

                        CollegeCode = n.CollegeCode,

                        FileName = n.FileName,

                        FilePath = n.FilePath,

                        IsActive = n.IsActive,

                        CreatedDate = n.CreatedDate,

                        IsRead = r != null && r.IsRead,

                        FacultyName = "Medical"
                    }
                )
                .ToListAsync();

            ViewBag.CollegeCode = collegeCode;
            ViewBag.FacultyCode = facultyCode;

            ViewBag.UnreadCount =
                notifications.Count(x => !x.IsRead);

            return View(notifications);
        }


        // =========================================================
        // MARK AS READ
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrWhiteSpace(collegeCode))
            {
                return Unauthorized();
            }

            var notification =
                await _context.AffiliationNotifications
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.NotificationId == id &&
                        x.IsActive);

            if (notification == null)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // Security:
            // Make sure this notification actually belongs
            // to this college/faculty.
            // -----------------------------------------------------

            var facultyCodeString =
                HttpContext.Session.GetString("FacultyCode");

            if (!int.TryParse(facultyCodeString, out int facultyCode))
            {
                return Unauthorized();
            }

            if (notification.FacultyCode != facultyCode)
            {
                return Forbid();
            }

            if (
                notification.NotificationType == "Specific"
                &&
                notification.CollegeCode != collegeCode
            )
            {
                return Forbid();
            }

            // -----------------------------------------------------
            // Check existing read record
            // -----------------------------------------------------

            var readRecord =
                await _context.AffiliationNotificationReads
                    .FirstOrDefaultAsync(x =>
                        x.NotificationId == id &&
                        x.CollegeCode == collegeCode);

            if (readRecord == null)
            {
                readRecord = new AffiliationNotificationRead
                {
                    NotificationId = id,
                    CollegeCode = collegeCode,
                    IsRead = true,
                    ReadDate = DateTime.Now,
                    CreatedDate = DateTime.Now
                };

                _context.AffiliationNotificationReads
                    .Add(readRecord);
            }
            else
            {
                readRecord.IsRead = true;
                readRecord.ReadDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                notificationId = id
            });
        }


        // =========================================================
        // GET UNREAD COUNT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            var facultyCodeString =
                HttpContext.Session.GetString("FacultyCode");

            if (
                string.IsNullOrWhiteSpace(collegeCode)
                ||
                !int.TryParse(
                    facultyCodeString,
                    out int facultyCode)
            )
            {
                return Json(new
                {
                    success = false,
                    count = 0
                });
            }

            var unreadCount = await
                (
                    from n in _context.AffiliationNotifications.AsNoTracking()

                    join r in _context.AffiliationNotificationReads
                        .AsNoTracking()
                        .Where(x => x.CollegeCode == collegeCode)
                        on n.NotificationId equals r.NotificationId
                        into readGroup

                    from r in readGroup.DefaultIfEmpty()

                    where
                        n.IsActive
                        &&
                        n.FacultyCode == facultyCode
                        &&
                        (
                            n.NotificationType == "General"
                            ||
                            (
                                n.NotificationType == "Specific"
                                &&
                                n.CollegeCode == collegeCode
                            )
                        )
                        &&
                        (
                            r == null
                            ||
                            !r.IsRead
                        )

                    select n.NotificationId
                )
                .CountAsync();

            return Json(new
            {
                success = true,
                count = unreadCount
            });
        }


        // =========================================================
        // MARK AS READ + VIEW PDF
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> ViewPdf(int id)
        {
            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            var facultyCodeString =
                HttpContext.Session.GetString("FacultyCode");

            if (
                string.IsNullOrWhiteSpace(collegeCode)
                ||
                !int.TryParse(
                    facultyCodeString,
                    out int facultyCode)
            )
            {
                return Unauthorized();
            }

            // -----------------------------------------------------
            // Get notification
            // -----------------------------------------------------

            var notification =
                await _context.AffiliationNotifications
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.NotificationId == id &&
                        x.IsActive);

            if (notification == null)
            {
                return NotFound(
                    "Notification not found.");
            }

            // -----------------------------------------------------
            // Faculty security
            // -----------------------------------------------------

            if (notification.FacultyCode != facultyCode)
            {
                return Forbid();
            }

            // -----------------------------------------------------
            // Specific notification security
            // -----------------------------------------------------

            if (
                notification.NotificationType == "Specific"
                &&
                notification.CollegeCode != collegeCode
            )
            {
                return Forbid();
            }

            // -----------------------------------------------------
            // Mark as read before opening PDF
            // -----------------------------------------------------

            var readRecord =
                await _context.AffiliationNotificationReads
                    .FirstOrDefaultAsync(x =>
                        x.NotificationId == id &&
                        x.CollegeCode == collegeCode);

            if (readRecord == null)
            {
                readRecord = new AffiliationNotificationRead
                {
                    NotificationId = id,
                    CollegeCode = collegeCode,
                    IsRead = true,
                    ReadDate = DateTime.Now,
                    CreatedDate = DateTime.Now
                };

                _context.AffiliationNotificationReads
                    .Add(readRecord);
            }
            else if (!readRecord.IsRead)
            {
                readRecord.IsRead = true;
                readRecord.ReadDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Extract filename
            // -----------------------------------------------------

            var fileName =
                Path.GetFileName(
                    notification.FilePath
                        .Replace('\\', '/'));

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound(
                    "Invalid notification file.");
            }

            // -----------------------------------------------------
            // Medical notification folder
            // -----------------------------------------------------

            var baseFolder =
                @"D:\AffiliationAdmin\AffiliationNotifications";

            var physicalFilePath =
                Path.Combine(
                    baseFolder,
                    "Medical",
                    fileName);

            // -----------------------------------------------------
            // Verify file exists
            // -----------------------------------------------------

            if (!System.IO.File.Exists(
                    physicalFilePath))
            {
                return NotFound(
                    $"Notification PDF not found: {fileName}");
            }

            return PhysicalFile(
                physicalFilePath,
                "application/pdf",
                enableRangeProcessing: true);
        }
    }
}