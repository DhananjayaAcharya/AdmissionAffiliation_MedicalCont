using System.Text.RegularExpressions;
using Medical_Affiliation.DATA;
using GeoPhotoModule.Models;
using GeoPhotoModule.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GeoPhotoModule.Controllers
{
    [Authorize(AuthenticationSchemes = AuthScheme)]
    public class GeoPhotoController : Controller
    {
        // ---- adjust these three to match your project ---------------------
        private const string AuthScheme = "CollegeAuth";
        private const string SessionCollegeKey = "CollegeCode";
        private const string SessionFacultyKey = "FacultyCode";
        // -------------------------------------------------------------------

        private const long MaxFileBytes = 5 * 1024 * 1024;   // 5 MB per image
        private static readonly Regex SafeCode = new(@"^[A-Za-z0-9_\-]{1,20}$", RegexOptions.Compiled);

        private readonly IGeoPhotoRepository _repo;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<GeoPhotoController> _log;

        public GeoPhotoController(
            IGeoPhotoRepository repo,
            ApplicationDbContext context,
            IWebHostEnvironment env,
            ILogger<GeoPhotoController> log)
        {
            _repo = repo;
            _context = context;
            _env = env;
            _log = log;
        }

        /// <summary>Photos are stored outside wwwroot and streamed only through the authorized Image action.</summary>
        private string StorageRoot => Path.Combine(
            Directory.Exists(@"E:\") ? @"E:\Affiliation_Medical" : @"D:\Affiliation_Medical",
            "GeoPhotos");

        // ==================================================================
        // GET /GeoPhoto  -> upload page
        // ==================================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!TryGetCodes(out var college, out var faculty))
                return SessionExpired();

            var vm = new GeoPhotoPageVm
            {
                CollegeCode = college,
                FacultyCode = faculty,
                Categories = await _repo.GetPageAsync(college, faculty)
            };

            return View(vm);
        }

        // ==================================================================
        // POST /GeoPhoto/Upload  -> save one image (called via fetch)
        // ==================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxFileBytes + 1_048_576)]
        public async Task<IActionResult> Upload([FromForm] GeoPhotoUploadRequest req)
        {
            if (!TryGetCodes(out var college, out var faculty))
                return Unauthorized(Fail("Session expired. Please log in again."));

            if (!ModelState.IsValid)
                return BadRequest(Fail("Invalid data. GPS location and a valid slot are required."));

            if (req.Latitude is null || req.Longitude is null ||
                (req.Latitude == 0 && req.Longitude == 0))
                return BadRequest(Fail("A valid GPS location is required."));

            var file = req.Photo;
            if (file is null || file.Length == 0)
                return BadRequest(Fail("Please choose a photo."));

            if (file.Length > MaxFileBytes)
                return BadRequest(Fail("Photo must be 5 MB or smaller."));

            // ---- verify the real file type from its first bytes (not the file name) ----
            string ext, contentType;
            await using (var s = file.OpenReadStream())
            {
                var header = new byte[8];
                int read = await s.ReadAsync(header, 0, header.Length);

                if (read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                {
                    ext = ".jpg"; contentType = "image/jpeg";
                }
                else if (read == 8 &&
                         header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                         header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                {
                    ext = ".png"; contentType = "image/png";
                }
                else
                {
                    return BadRequest(Fail("Only JPG or PNG images are allowed."));
                }
            }

            // ---- save the file to disk (server-generated name) ----
            string relPath = $"{college}/{faculty}/{req.CategoryId}_{req.SlotNo}_{Guid.NewGuid():N}{ext}";
            string? fullPath = ResolveSafe(relPath);
            if (fullPath is null)
                return BadRequest(Fail("Invalid file location."));

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            await using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
            {
                await file.CopyToAsync(fs);
            }

            try
            {
                // remember the previous file of this slot so it can be removed after a successful replace
                string? oldPath = await _repo.GetSlotPathAsync(college, faculty, req.CategoryId, req.SlotNo);

                var ua = Request.Headers.UserAgent.ToString();
                var by = User.Identity?.Name ?? college;

                var (photoId, _) = await _repo.SaveAsync(new GeoPhotoSaveDto
                {
                    CollegeCode = college,
                    FacultyCode = faculty,
                    CategoryId = req.CategoryId,
                    ImageSlotNo = req.SlotNo,
                    ImagePath = relPath,
                    OriginalFileName = Trunc(Path.GetFileName(file.FileName), 255),
                    ContentType = contentType,
                    FileSizeKB = (int)Math.Ceiling(file.Length / 1024.0),
                    Latitude = Math.Round(req.Latitude.Value, 6),
                    Longitude = Math.Round(req.Longitude.Value, 6),
                    AccuracyMeters = req.AccuracyMeters.HasValue ? Math.Round(req.AccuracyMeters.Value, 2) : null,
                    CapturedOn = req.CapturedOn,
                    DeviceInfo = Trunc(ua, 200),
                    UploadedBy = Trunc(by, 100),
                    UploadedIP = Trunc(HttpContext.Connection.RemoteIpAddress?.ToString(), 45)
                });

                if (!string.IsNullOrEmpty(oldPath) && !string.Equals(oldPath, relPath, StringComparison.OrdinalIgnoreCase))
                    TryDeleteFile(oldPath);

                await UpdateGeoPhotoProgressAsync(college, faculty);

                return Ok(new
                {
                    success = true,
                    photoId,
                    url = Url.Action(nameof(Image), new { id = photoId }),
                    latitude = Math.Round(req.Latitude.Value, 6),
                    longitude = Math.Round(req.Longitude.Value, 6),
                    accuracyMeters = req.AccuracyMeters,
                    message = "Uploaded"
                });
            }
            catch (SqlException ex) when (ex.Number == 50001 || ex.Number == 50002)
            {
                TryDeleteFile(relPath);
                return BadRequest(Fail(ex.Message));
            }
            catch (Exception ex)
            {
                TryDeleteFile(relPath);
                _log.LogError(ex, "GeoPhoto upload failed for college {College} faculty {Faculty}", college, faculty);
                return StatusCode(500, Fail("Upload failed. Please try again."));
            }
        }

        // ==================================================================
        // GET /GeoPhoto/Image/{id}  -> stream an image (owner only)
        // ==================================================================
        [HttpGet]
        public async Task<IActionResult> Image(long id)
        {
            if (!TryGetCodes(out var college, out var faculty))
                return Unauthorized();

            var info = await _repo.GetPhotoAsync(id);
            if (info is null ||
                !string.Equals(info.CollegeCode, college, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(info.FacultyCode, faculty, StringComparison.OrdinalIgnoreCase))
                return NotFound();

            string? full = ResolveSafe(info.ImagePath);
            if (full is null || !System.IO.File.Exists(full))
                return NotFound();

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Cache-Control"] = "private, max-age=300";
            return PhysicalFile(full, info.ContentType ?? "image/jpeg");
        }

        // ==================================================================
        // POST /GeoPhoto/Delete  -> remove one image
        // ==================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            if (!TryGetCodes(out var college, out var faculty))
                return Unauthorized(Fail("Session expired. Please log in again."));

            try
            {
                string? path = await _repo.DeleteAsync(id, college, faculty);
                if (path is null)
                    return NotFound(Fail("Photo not found."));

                TryDeleteFile(path);
                await UpdateGeoPhotoProgressAsync(college, faculty);
                return Ok(new { success = true, message = "Deleted" });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "GeoPhoto delete failed for photo {PhotoId}", id);
                return StatusCode(500, Fail("Delete failed. Please try again."));
            }
        }

        // ==================================================================
        // Helpers
        // ==================================================================

        /// <summary>Reads college/faculty from session and makes sure they are safe to use in a path.</summary>
        private bool TryGetCodes(out string college, out string faculty)
        {
            college = HttpContext.Session.GetString(SessionCollegeKey) ?? "";
            faculty = HttpContext.Session.GetString(SessionFacultyKey) ?? "";
            return SafeCode.IsMatch(college) && SafeCode.IsMatch(faculty);
        }

        /// <summary>Change this if you want to redirect to your own login page instead.</summary>
        private IActionResult SessionExpired() => Challenge(AuthScheme);

        /// <summary>Combines a relative path with the storage root and blocks path traversal.</summary>
        private string? ResolveSafe(string relPath)
        {
            string root = Path.GetFullPath(StorageRoot);
            string full = Path.GetFullPath(Path.Combine(root, relPath));
            return full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ? full : null;
        }

        private void TryDeleteFile(string relPath)
        {
            try
            {
                string? full = ResolveSafe(relPath);
                if (full != null && System.IO.File.Exists(full))
                    System.IO.File.Delete(full);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Could not delete GeoPhoto file {Path}", relPath);
            }
        }

        private async Task UpdateGeoPhotoProgressAsync(string collegeCode, string facultyCode)
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel")
                ?? HttpContext.Session.GetString("SelectedCourseLevel")
                ?? HttpContext.Session.GetString("SelectedLevel");

            if (string.IsNullOrWhiteSpace(courseLevel))
                return;

            courseLevel = courseLevel.Trim().ToUpperInvariant();
            var categories = await _repo.GetPageAsync(collegeCode, facultyCode);
            var allRequiredPhotosUploaded = categories.Count > 0
                && categories.All(category => category.UploadedCount >= category.RequiredImages);

            var progress = await _context.CaProgresses.FirstOrDefaultAsync(x =>
                x.CollegeCode == collegeCode
                && x.CourseLevel == courseLevel
                && x.StepKey == "GeoPhoto");

            if (progress == null)
            {
                progress = new Medical_Affiliation.Models.CaProgress
                {
                    CollegeCode = collegeCode,
                    CourseLevel = courseLevel,
                    StepKey = "GeoPhoto"
                };
                _context.CaProgresses.Add(progress);
            }

            progress.IsCompleted = allRequiredPhotosUploaded;
            progress.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        private static string? Trunc(string? value, int max) =>
            string.IsNullOrEmpty(value) ? value : (value.Length <= max ? value : value[..max]);

        private static object Fail(string message) => new { success = false, message };
    }
}