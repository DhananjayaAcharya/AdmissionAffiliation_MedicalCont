using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using System.Data;

namespace Medical_Affiliation.Controllers
{
    public class FellowshipController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FellowshipController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET
        [HttpGet]
        public IActionResult FellowshipMedical_Details()
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var pageVm = new FellowshipMedicalPageVm();

            pageVm.Form.FacultyCode = HttpContext.Session.GetString("FacultyCode") ?? string.Empty;
            pageVm.Form.CollegeCode = HttpContext.Session.GetString("CollegeCode") ?? string.Empty;

            ViewBag.CollegeName = HttpContext.Session.GetString("CollegeName") ?? string.Empty;

            ViewBag.CourseList = _context.CollegeCourseIntakeDetails
                .Where(e => e.FacultyCode.ToString() == facultyCode && e.CollegeCode == collegeCode)
                .OrderBy(c => c.CourseName)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseName,
                    Text = c.CourseName
                })
                .ToList();

            // Load latest records (TOP 1000)
            pageVm.ExistingRecords = _context.FellowShipMedicals
                        .Where(e => e.FacultyCode == facultyCode && e.Collegecode == collegeCode)
                .OrderByDescending(f => f.Id)         // adjust key as per your entity
                .Take(1000)
                .ToList();

            return View(pageVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FellowshipMedical_Details(
            FellowshipMedicalPageVm pageVm,
            IFormFile? ssLC_Doc,
            IFormFile? kmc_Doc,
            IFormFile? experience_Letter_Doc,
            IFormFile? appointmentLetter_Doc)
        {
            var vm = pageVm.Form;

            if (ModelState.IsValid)
            {
                vm.FacultyCode = HttpContext.Session.GetString("FacultyCode") ?? string.Empty;
                vm.CollegeCode = HttpContext.Session.GetString("CollegeCode") ?? string.Empty;

                var generatedFellowshipCode = GenerateFellowshipCode(vm.Course);

                var entity = new FellowShipMedical
                {
                    FacultyCode = vm.FacultyCode,
                    Collegecode = vm.CollegeCode,
                    StudentName = vm.StudentName,
                    Dob = vm.DOB,
                    Dateofjoining = vm.Dateofjoining,
                    AdmissionOpeningDate = vm.Admission_openingDate,
                    EndingDate = vm.EndingDate,
                    Course = vm.Course,
                    KmcCertificateNumber = vm.KMC_CertificateNumber,
                    PrincipalName = vm.principal_name,
                    PrincipalDeclaration = vm.principal_Declaration ? "Yes" : "No",
                    FellowshipCode = generatedFellowshipCode,
                    SslcDoc = GetBytes(ssLC_Doc),
                    KmcDoc = GetBytes(kmc_Doc),
                    ExperienceLetterDoc = GetBytes(experience_Letter_Doc),
                    AppointmentLetterDoc = GetBytes(appointmentLetter_Doc)
                };

                _context.FellowShipMedicals.Add(entity);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Record saved successfully!";
                return RedirectToAction(nameof(FellowshipMedical_Details));
            }

            ViewBag.CollegeName = HttpContext.Session.GetString("CollegeName") ?? string.Empty;
            ViewBag.CourseList = _context.MstCourses
                .OrderBy(c => c.CourseName)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseName,
                    Text = c.CourseName
                })
                .ToList();

            // Reload list when validation fails
            pageVm.ExistingRecords = _context.FellowShipMedicals
                .OrderByDescending(f => f.Id)
                .Take(1000)
                .ToList();

            return View(pageVm);
        }


        private static byte[]? GetBytes(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            using var stream = new MemoryStream();
            file.CopyTo(stream);
            return stream.ToArray();
        }

        private string GenerateFellowshipCode(string course)
        {
            // Year prefix (26 as per requirement)
            var yearPart = "26";

            // Get CollegeCode from session (for current user/college)
            var collegeCode = HttpContext.Session.GetString("CollegeCode") ?? string.Empty;

            // Normalize college code (remove spaces, upper)
            collegeCode = collegeCode.Trim().Replace(" ", string.Empty).ToUpperInvariant();

            // First 4 characters of course, uppercased, padded if short
            var coursePart = (course ?? string.Empty)
                .Trim()
                .Replace(" ", string.Empty);

            if (coursePart.Length >= 4)
                coursePart = coursePart.Substring(0, 4).ToUpperInvariant();
            else
                coursePart = coursePart.ToUpperInvariant().PadRight(4, 'X'); // PadRight keeps original if length >= target[web:103]

            // Prefix = year + college + course
            var prefix = yearPart + collegeCode + coursePart;

            // Get latest sequence for this prefix and increment
            var lastCode = _context.FellowShipMedicals
                .Where(f => f.FellowshipCode.StartsWith(prefix))
                .OrderByDescending(f => f.FellowshipCode)
                .Select(f => f.FellowshipCode)
                .FirstOrDefault();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastCode) && lastCode.Length >= prefix.Length + 4)
            {
                var numPart = lastCode.Substring(prefix.Length, 4);
                if (int.TryParse(numPart, out var parsed))
                {
                    nextNumber = parsed + 1;
                }
            }

            // Format as 4-digit series: 0001, 0002, ...
            var seriesPart = nextNumber.ToString("0000"); // leading zeros[web:103]

            return prefix + seriesPart;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFellowshipMedical(int id)
        {
            var entity = await _context.FellowShipMedicals.FindAsync(id);
            if (entity != null)
            {
                _context.FellowShipMedicals.Remove(entity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Record deleted successfully!";
            }
            return RedirectToAction(nameof(FellowshipMedical_Details));
        }


        public async Task<IActionResult> DownloadDocument(int id, string type)
        {
            var entity = await _context.FellowShipMedicals.FindAsync(id);
            if (entity == null) return NotFound();

            byte[]? data = null;
            string fileName = "document";

            switch (type)
            {
                case "sslc":
                    data = entity.SslcDoc;
                    fileName = "SSLC.pdf";
                    break;
                case "kmc":
                    data = entity.KmcDoc;
                    fileName = "KMC.pdf";
                    break;
                case "exp":
                    data = entity.ExperienceLetterDoc;
                    fileName = "ExperienceLetter.pdf";
                    break;
                case "appt":
                    data = entity.AppointmentLetterDoc;
                    fileName = "AppointmentLetter.pdf";
                    break;
            }

            if (data == null) return NotFound();

            // Adjust content type/filename as needed for real file types.[web:90][web:92]
            return File(data, "application/octet-stream", fileName);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "DirectorAuth", Roles = "Director")]
        public async Task<IActionResult> AdminDashboard_Fellowship()
        {
            var rows = await (from f in _context.FellowShipMedicals
                              join c in _context.AffiliationCollegeMasters
                                  on f.Collegecode equals c.CollegeCode into cj
                              from c in cj.DefaultIfEmpty()
                              orderby f.Id descending
                              select new FellowshipDashboardRowVm
                              {
                                  Id = f.Id,
                                  CollegeCode = f.Collegecode,
                                  CollegeName = (c != null && c.CollegeName != null) ? (c.CollegeName + (string.IsNullOrWhiteSpace(c.CollegeTown) ? "" : ", " + c.CollegeTown)) : (f.Collegecode ?? string.Empty),
                                  StudentName = f.StudentName ?? string.Empty,
                                  DOB = f.Dob ?? DateOnly.MinValue,
                                  Dateofjoining = f.Dateofjoining ?? DateOnly.MinValue,
                                  Admission_openingDate = f.AdmissionOpeningDate ?? DateOnly.MinValue,
                                  EndingDate = f.EndingDate ?? DateOnly.MinValue,
                                  Course = f.Course,
                                  KMC_CertificateNumber = f.KmcCertificateNumber,
                                  principal_name = f.PrincipalName,
                                  principal_Declaration = f.PrincipalDeclaration,
                                  FellowshipCode = f.FellowshipCode,
                                  ApprovalStatus = f.ApprovalStatus ?? "Pending",
                                  ApprovalRemark = f.ApprovalRemark,
                                  HasSSLC = f.SslcDoc != null && f.SslcDoc.Length > 0,
                                  HasKMC = f.KmcDoc != null && f.KmcDoc.Length > 0,
                                  HasExperience = f.ExperienceLetterDoc != null && f.ExperienceLetterDoc.Length > 0,
                                  HasAppointment = f.AppointmentLetterDoc != null && f.AppointmentLetterDoc.Length > 0
                              }).ToListAsync();

            return View(new FellowshipDashboardListVm { Items = rows });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "DirectorAuth", Roles = "Director")]
        public async Task<IActionResult> AdminDashboard_Fellowship(FellowshipDashboardListVm model, string? submit)
        {
            // If no model posted, reload GET view
            if (model?.Items == null || model.Items.Count == 0)
            {
                return RedirectToAction(nameof(AdminDashboard_Fellowship));
            }

            var ids = model.Items.Select(i => i.Id).ToList();
            var entities = await _context.FellowShipMedicals
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            foreach (var row in model.Items)
            {
                var entity = entities.FirstOrDefault(e => e.Id == row.Id);
                if (entity == null) continue;

                // Update approval status and remark (section officer decision stored here)
                entity.ApprovalStatus = row.ApprovalStatus;
                entity.ApprovalRemark = row.ApprovalStatus == "Rejected"
                    ? row.ApprovalRemark
                    : null;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Approvals updated successfully.";

            // Redirect to GET to refresh and avoid repost
            return RedirectToAction(nameof(AdminDashboard_Fellowship));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(FellowshipDashboardListVm vm)
        {
            if (vm.Items == null || vm.Items.Count == 0)
            {
                TempData["Error"] = "No records to update.";
                return RedirectToAction(nameof(AdminDashboard_Fellowship));
            }

            var ids = vm.Items.Select(i => i.Id).ToList();

            // Load entities in a single query and map by Id for fast lookup
            var entities = await _context.FellowShipMedicals
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            foreach (var row in vm.Items)
            {
                if (!entities.TryGetValue(row.Id, out var entity))
                    continue;

                // Update approval status and remark
                entity.ApprovalStatus = row.ApprovalStatus;
                entity.ApprovalRemark = string.Equals(row.ApprovalStatus, "Rejected", StringComparison.OrdinalIgnoreCase)
                    ? row.ApprovalRemark
                    : null;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Approvals updated successfully.";

            // Redirect back to the dashboard GET to reload and avoid double-post on refresh
            return RedirectToAction(nameof(AdminDashboard_Fellowship));
        }
        [HttpGet]
        public async Task<IActionResult> Download(int id, string doc)
        {
            var e = await _context.FellowShipMedicals.FirstOrDefaultAsync(x => x.Id == id);
            if (e == null) return NotFound();

            byte[]? bytes = null;
            string fileName;
            const string contentType = "application/pdf"; // adjust if needed

            switch (doc)
            {
                case "SSLC":
                    bytes = e.SslcDoc;
                    fileName = $"SSLC_{id}.pdf";
                    break;
                case "KMC":
                    bytes = e.KmcDoc;
                    fileName = $"KMC_{id}.pdf";
                    break;
                case "EXP":
                    bytes = e.ExperienceLetterDoc;
                    fileName = $"Experience_{id}.pdf";
                    break;
                case "APPT":
                    bytes = e.AppointmentLetterDoc;
                    fileName = $"Appointment_{id}.pdf";
                    break;
                default:
                    return BadRequest();
            }

            if (bytes == null || bytes.Length == 0)
                return NotFound();

            return File(bytes, contentType, fileName);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("SectionOfficerAuth");
            await HttpContext.SignOutAsync("DirectorAuth");
            HttpContext.Session.Clear();
            return RedirectToAction("AdminLogin");
        }
        [HttpGet]
        public async Task<IActionResult> Fellowship_DirectorDashboard()
        {
            var rows = await _context.FellowShipMedicals
                .Where(x => x.ApprovalStatus == "Approved")
                .OrderByDescending(x => x.Id)
                .Select(x => new FellowshipDashboardRowVm
                {
                    Id = x.Id,
                    StudentName = x.StudentName,
                    DOB = (DateOnly)x.Dob,
                    Dateofjoining = (DateOnly)x.Dateofjoining,
                    Admission_openingDate = (DateOnly)x.AdmissionOpeningDate,
                    EndingDate = (DateOnly)x.EndingDate,
                    Course = x.Course,
                    KMC_CertificateNumber = x.KmcCertificateNumber,
                    principal_name = x.PrincipalName,
                    principal_Declaration = x.PrincipalDeclaration,
                    FellowshipCode = x.FellowshipCode,
                    ApprovalStatus = x.ApprovalStatus,
                    ApprovalRemark = x.ApprovalRemark,
                    HasSSLC = x.SslcDoc != null && x.SslcDoc.Length > 0,
                    HasKMC = x.KmcDoc != null && x.KmcDoc.Length > 0,
                    HasExperience = x.ExperienceLetterDoc != null && x.ExperienceLetterDoc.Length > 0,
                    HasAppointment = x.AppointmentLetterDoc != null && x.AppointmentLetterDoc.Length > 0
                })
                .ToListAsync();

            return View("Fellowship_DirectorDashboard", new FellowshipDashboardListVm { Items = rows });
        }

        [HttpGet]
        public async Task<IActionResult> viewDoc(int id, string doc)
        {
            var e = await _context.FellowShipMedicals.FirstOrDefaultAsync(x => x.Id == id);
            if (e == null) return NotFound();

            byte[]? bytes = null;
            string fileName;
            const string contentType = "application/pdf";

            switch (doc)
            {
                case "SSLC":
                    bytes = e.SslcDoc;
                    fileName = $"SSLC_{id}.pdf";
                    break;
                case "KMC":
                    bytes = e.KmcDoc;
                    fileName = $"KMC_{id}.pdf";
                    break;
                case "EXP":
                    bytes = e.ExperienceLetterDoc;
                    fileName = $"Experience_{id}.pdf";
                    break;
                case "APPT":
                    bytes = e.AppointmentLetterDoc;
                    fileName = $"Appointment_{id}.pdf";
                    break;
                default:
                    return BadRequest();
            }

            if (bytes == null || bytes.Length == 0)
                return NotFound();

            return File(bytes, contentType, fileName);
        }

        [HttpGet]
        public async Task<IActionResult> DirectorDashboard_Fellowship()
        {
            var rows = await (from f in _context.FellowShipMedicals
                              join c in _context.AffiliationCollegeMasters
                                  on f.Collegecode equals c.CollegeCode into cj
                              from c in cj.DefaultIfEmpty()
                              orderby f.Id descending
                              select new FellowshipDirectorDashboardRowVm
                              {
                                  Id = f.Id,
                                  CollegeCode = f.Collegecode,
                                  CollegeName = (c != null && c.CollegeName != null) ? (c.CollegeName + (string.IsNullOrWhiteSpace(c.CollegeTown) ? "" : ", " + c.CollegeTown)) : f.Collegecode ?? string.Empty,
                                  StudentName = f.StudentName ?? string.Empty,
                                  DOB = f.Dob ?? DateOnly.MinValue,
                                  Dateofjoining = f.Dateofjoining ?? DateOnly.MinValue,
                                  Admission_openingDate = f.AdmissionOpeningDate ?? DateOnly.MinValue,
                                  EndingDate = f.EndingDate ?? DateOnly.MinValue,
                                  Course = f.Course ?? string.Empty,
                                  KMC_CertificateNumber = f.KmcCertificateNumber,
                                  principal_name = f.PrincipalName,
                                  principal_Declaration = f.PrincipalDeclaration,
                                  FellowshipCode = f.FellowshipCode,
                                  SectionOfficerApproval = f.ApprovalStatus,
                                  SectionOfficerRemark = f.ApprovalRemark,
                                  // now use CLR properties directly
                                  DirectorApprovalStatus = f.DrApprovalStatus,
                                  DirectorApprovalRemark = f.DrApprovalRemark,
                                  HasSSLC = f.SslcDoc != null && f.SslcDoc.Length > 0,
                                  HasKMC = f.KmcDoc != null && f.KmcDoc.Length > 0,
                                  HasExperience = f.ExperienceLetterDoc != null && f.ExperienceLetterDoc.Length > 0,
                                  HasAppointment = f.AppointmentLetterDoc != null && f.AppointmentLetterDoc.Length > 0
                              }).ToListAsync();

            return View("DirectorDashboard_Fellowship", new FellowshipDirectorDashboardListVm { Items = rows });
        }

        // POST: save director approvals
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DirectorDashboard_Fellowship(FellowshipDirectorDashboardListVm vm)
        {
            if (vm?.Items == null || vm.Items.Count == 0)
            {
                TempData["Error"] = "No records submitted.";
                return RedirectToAction(nameof(DirectorDashboard_Fellowship));
            }

            var ids = vm.Items.Select(i => i.Id).ToList();
            var entities = await _context.FellowShipMedicals
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            foreach (var row in vm.Items)
            {
                var entity = entities.FirstOrDefault(e => e.Id == row.Id);
                if (entity == null) continue;

                // write director decision to CLR properties
                entity.DrApprovalStatus = string.IsNullOrWhiteSpace(row.DirectorApprovalStatus) ? null : row.DirectorApprovalStatus;
                entity.DrApprovalRemark = string.IsNullOrWhiteSpace(row.DirectorApprovalRemark) ? null : row.DirectorApprovalRemark;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Director decisions saved.";
            return RedirectToAction(nameof(DirectorDashboard_Fellowship));
        }



        public async Task<IActionResult> AcceptedRejectedDashboard()
        {
            // Get session scope - restrict results to the currently signed-in college/faculty
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            // If session values not present, return empty view with message (avoid showing all records)
            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
            {
                TempData["Error"] = "Session expired or college/faculty not set. Please login again.";
                return View("AcceptedRejectedDashboard", new FellowshipAcceptedRejectedListVm { Items = new List<FellowshipAcceptedRejectedRowVm>() });
            }

            var list = new List<FellowshipAcceptedRejectedRowVm>();

            // raw SQL - select only the requested columns (top 1000) and filter by session college/faculty
            var sql = @"
        SELECT TOP (1000)
            Id,
            StudentName,
            DOB,
            Dateofjoining,
            Admission_openingDate,
            EndingDate,
            Course,
            KMC_CertificateNumber,
            principal_name,
            principal_Declaration,
            FellowshipCode,
            SSLC_Doc,
            KMC_Doc,
            Experience_Letter_Doc,
            AppointmentLetter_Doc,
            DR_ApprovalStatus,
            DR_ApprovalRemark
        FROM FellowShip_Medical
        WHERE FacultyCode = @faculty AND Collegecode = @college
        ORDER BY Id DESC";

            var conn = _context.Database.GetDbConnection();
            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;

                var pFaculty = cmd.CreateParameter();
                pFaculty.ParameterName = "@faculty";
                pFaculty.Value = facultyCode;
                cmd.Parameters.Add(pFaculty);

                var pCollege = cmd.CreateParameter();
                pCollege.ParameterName = "@college";
                pCollege.Value = collegeCode;
                cmd.Parameters.Add(pCollege);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var row = new FellowshipAcceptedRejectedRowVm
                    {
                        Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                        StudentName = reader["StudentName"] != DBNull.Value ? reader["StudentName"].ToString() : null,
                        DOB = reader["DOB"] != DBNull.Value ? DateOnly.FromDateTime(Convert.ToDateTime(reader["DOB"])) : DateOnly.MinValue,
                        Dateofjoining = reader["Dateofjoining"] != DBNull.Value ? DateOnly.FromDateTime(Convert.ToDateTime(reader["Dateofjoining"])) : DateOnly.MinValue,
                        Admission_openingDate = reader["Admission_openingDate"] != DBNull.Value ? DateOnly.FromDateTime(Convert.ToDateTime(reader["Admission_openingDate"])) : DateOnly.MinValue,
                        EndingDate = reader["EndingDate"] != DBNull.Value ? DateOnly.FromDateTime(Convert.ToDateTime(reader["EndingDate"])) : DateOnly.MinValue,
                        Course = reader["Course"] != DBNull.Value ? reader["Course"].ToString() : null,
                        KMC_CertificateNumber = reader["KMC_CertificateNumber"] != DBNull.Value ? reader["KMC_CertificateNumber"].ToString() : null,
                        principal_name = reader["principal_name"] != DBNull.Value ? reader["principal_name"].ToString() : null,
                        principal_Declaration = reader["principal_Declaration"] != DBNull.Value ? reader["principal_Declaration"].ToString() : null,
                        FellowshipCode = reader["FellowshipCode"] != DBNull.Value ? reader["FellowshipCode"].ToString() : null,
                        // documents presence
                        HasSSLC = reader["SSLC_Doc"] != DBNull.Value && ((byte[])reader["SSLC_Doc"]).Length > 0,
                        HasKMC = reader["KMC_Doc"] != DBNull.Value && ((byte[])reader["KMC_Doc"]).Length > 0,
                        HasExperience = reader["Experience_Letter_Doc"] != DBNull.Value && ((byte[])reader["Experience_Letter_Doc"]).Length > 0,
                        HasAppointment = reader["AppointmentLetter_Doc"] != DBNull.Value && ((byte[])reader["AppointmentLetter_Doc"]).Length > 0,
                        DR_ApprovalStatus = reader["DR_ApprovalStatus"] != DBNull.Value ? reader["DR_ApprovalStatus"].ToString() : null,
                        DR_ApprovalRemark = reader["DR_ApprovalRemark"] != DBNull.Value ? reader["DR_ApprovalRemark"].ToString() : null
                    };

                    list.Add(row);
                }
            }
            finally
            {
                await conn.CloseAsync();
            }

            var vm = new FellowshipAcceptedRejectedListVm { Items = list };
            return View("AcceptedRejectedDashboard", vm);
        }

    }

}
