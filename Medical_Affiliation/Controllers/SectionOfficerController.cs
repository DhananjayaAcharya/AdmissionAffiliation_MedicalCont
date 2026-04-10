using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing;
using   Medical_Affiliation.DATA;
using Admission_Affiliation.Models;


namespace Admission_Affiliation.Controllers
{
    public class SectionOfficerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SectionOfficerController> _logger;


        public SectionOfficerController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult SOLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SOLogin(AHSSOloginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.TblRguhsFacultyUsers
                .FirstOrDefaultAsync(e => e.UserName == model.username);

            if (user == null)
            {
                ModelState.AddModelError("username", "Username not found.");
                return View(model);
            }

            bool isPasswordValid = false;

            // Detect if hash is BCrypt
            if (!string.IsNullOrEmpty(user.PasswordHash) &&
                (user.PasswordHash.StartsWith("$2a$") ||
                 user.PasswordHash.StartsWith("$2b$") ||
                 user.PasswordHash.StartsWith("$2y$")))
            {
                try
                {
                    string storedHash = user.PasswordHash.Replace("$2y$", "$2a$"); // normalize
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(model.password, storedHash);
                }
                catch
                {
                    isPasswordValid = false; // any parse errors treated as invalid
                }
            }
            else
            {
                // Fall back to ASP.NET Core PasswordHasher
                var passwordHasher = new PasswordHasher<TblRguhsFacultyUser>();
                var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.password);
                isPasswordValid = result == PasswordVerificationResult.Success ||
                                  result == PasswordVerificationResult.SuccessRehashNeeded;
            }

            if (!isPasswordValid)
            {
                ModelState.AddModelError("password", "Incorrect password.");
                return View(model);
            }


            // Capture IP and Browser info
            var userIP = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, "SectionOfficer"),
                new Claim("FacultyId", user.Faculty.ToString()),
                new Claim("UserIP", userIP ?? string.Empty),
                new Claim("UserAgent", userAgent ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, "SectionOfficerAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("SectionOfficerAuth", principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                });

            HttpContext.Session.SetString("FacultyId", user.Faculty.ToString());

            return RedirectToAction("SODashboard", "SectionOfficer");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("SectionOfficerAuth"); // 👈 specify scheme
            HttpContext.Session.Clear();
            return RedirectToAction("UniversityLogin", "Admin");
        }


        [Authorize(AuthenticationSchemes = "SectionOfficerAuth", Roles = "SectionOfficer")]
        public IActionResult SODashboard()
        {
            // Read FacultyId directly from claims
            var facultyId = User.FindFirst("FacultyId")?.Value;
            ViewBag.isAhsSoLogin = Convert.ToInt32(facultyId) == 9;

            if (string.IsNullOrEmpty(facultyId))
            {
                return RedirectToAction("SOLogin", "SectionOfficer");
            }

            ViewBag.FacultyId = facultyId;
            return View();
        }



        //public async Task<IActionResult> SOIntakeDetails(AHSSOupdateIntakeDetails model)
        //{
        //    var soDetails =

        //}

        [Authorize(AuthenticationSchemes = "SectionOfficerAuth, AdminAuth")]
        [HttpGet("SOIntakeDetails")]
        public async Task<IActionResult> SOIntakeDetails(int facultycode, string collegeCode)
        {
            var model = new AHSSOupdateIntakeDetails();

            // ---------- FACULTIES ----------
            var facultiesQuery = _context.Faculties.Where(f => f.Status == "Active");


            if (facultycode <= 98 && facultycode != 0)
            {
                facultiesQuery = facultiesQuery.Where(f => f.FacultyId == facultycode);
                
            }

            var faculties = await facultiesQuery
                .Select(f => new SelectListItem
                {
                    Value = f.FacultyId.ToString(),
                    Text = f.FacultyName,
                    Selected = (f.FacultyId == facultycode)
                })
                .ToListAsync();

            model.Faculties = faculties;

            // ---------- COLLEGES ----------
            if (facultycode != 0)
            {
                var colleges = await _context.AffiliationCollegeMasters
                    .Where(c => c.FacultyCode == facultycode.ToString())
                    .Select(c => new SelectListItem
                    {
                        Value = c.CollegeCode,
                        Text = c.CollegeName,
                        Selected = (c.CollegeCode == collegeCode)
                    })
                    .ToListAsync();

                // Add "All Colleges" option
                colleges.Insert(0, new SelectListItem
                {
                    Value = "all",
                    Text = "All Colleges",
                    Selected = (collegeCode == "all")
                }); 

                // Add "Select College" option
                colleges.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "Select College",
                    Selected = string.IsNullOrEmpty(collegeCode)
                });

                model.Colleges = colleges;
            }

            // ---------- COURSE DETAILS ----------
            model.CollegeCode = collegeCode;
            model.CollegeName = await _context.AffiliationCollegeMasters
                .Where(e => e.CollegeCode == collegeCode)
                .Select(e => e.CollegeName)
                .FirstOrDefaultAsync();

            if (facultycode != 0)
            {
                var query = _context.CollegeCourseIntakeDetails
                    .Where(e => e.FacultyCode == facultycode);

                if (!string.IsNullOrEmpty(collegeCode) && collegeCode != "all")
                {
                    // ✅ Specific college
                    query = query.Where(e => e.CollegeCode == collegeCode);

                    var document = await _context.CollegeCourseIntakeDetails
                        .FirstOrDefaultAsync(e => e.CollegeCode == collegeCode);

                    var result = await (
                        from e in query
                        join f in _context.Faculties on e.FacultyCode equals f.FacultyId
                        join c in _context.MstCourses on e.CourseCode equals c.CourseCode.ToString()
                        select new AHScourseIntakeViewModel
                        {
                            CollegeCode = e.CollegeCode,
                            CollegeName = e.CollegeName,
                            FacultyCode = e.FacultyCode.ToString(),
                            FacultyName = f.FacultyName,
                            CourseCode = e.CourseCode,
                            CourseName = c.CoursePrefix+" "+ e.CourseName ,
                            CourseLevel = c.CourseLevel,
                            //CoursePrefix = c.CoursePrefix,
                            SanctionedIntake = e.ExistingIntake,
                            notificationBytes = null,
                            isNotificationDoc = false
                        }
                    ).ToListAsync();

                    model.allCoursesBytes = document?.DocumentAffiliation;
                    model.isRguhsNotification = document?.DocumentAffiliation != null;
                    model.AHScourseIntakeViewModel = result;
                }
                else if (collegeCode == "all")
                {
                    // ✅ All colleges
                    model.AHScourseIntakeViewModel = await query
                        .Select(e => new AHScourseIntakeViewModel
                        {
                            CollegeCode = e.CollegeCode,
                            CollegeName = e.CollegeName,
                            FacultyCode = e.FacultyCode.ToString(),
                            FacultyName = _context.Faculties
                                .Where(f => f.FacultyId == e.FacultyCode)
                                .Select(f => f.FacultyName)
                                .FirstOrDefault(),
                            CourseCode = null,
                            CourseName = e.CourseName,
                            SanctionedIntake = e.ExistingIntake,
                            notificationBytes = null,
                            isNotificationDoc = false
                        })
                        .ToListAsync();
                }
                else
                {
                    // ✅ No college selected
                    model.AHScourseIntakeViewModel = new List<AHScourseIntakeViewModel>();
                }
            }

            // ---------- UPLOADED COLLEGES ----------
            model.FacultyCode = facultycode.ToString();
            model.UploadedColleges = await _context.CollegeCourseIntakeDetails
                .Where(f => f.FacultyCode.ToString() == model.FacultyCode && f.DocumentAffiliation != null)
                .ToListAsync();

            return View(model);
        }


        [Authorize(AuthenticationSchemes = "SectionOfficerAuth", Roles = "SectionOfficer")]
        [ValidateAntiForgeryToken]
        [HttpPost("SOIntakeDetails")]
        public async Task<IActionResult> SOIntakeDetails(AHSSOupdateIntakeDetails model, List<IFormFile> AllDocuments)
        {

            const long maxFileSize = 2 * 1024 * 1024; // 2 MB
            if (AllDocuments != null && AllDocuments.Count > 0)
            {
                byte[] fileBytes;
                if (AllDocuments.Count == 1)
                {
                    using var ms = new MemoryStream();
                    await AllDocuments[0].CopyToAsync(ms);
                    fileBytes = ms.ToArray();
                }
                else
                {
                    using var msZip = new MemoryStream();
                    using (var archive = new ZipArchive(msZip, ZipArchiveMode.Create, true))
                    {
                        foreach (var file in AllDocuments)
                        {
                            if(file.Length > maxFileSize)
                            {
                                TempData["ErrorNotification"] = $"File {file.FileName} exceeds 2 MB limit.";
                                return RedirectToAction("SOIntakeDetails", new { facultycode = Convert.ToInt32(model.FacultyCode.Trim()), collegeCode = model.CollegeCode });
                            }
                            var zipEntry = archive.CreateEntry(file.FileName);
                            using var entryStream = zipEntry.Open();
                            await file.CopyToAsync(entryStream);
                        }
                    }
                    fileBytes = msZip.ToArray();
                }
                var college = await _context.CollegeCourseIntakeDetails.FirstOrDefaultAsync(e=>e.CollegeCode==model.CollegeCode);
                if (college != null)
                {
                    college.DocumentAffiliation = fileBytes;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["CollegeNotFound"] = "College Not Found";
                }

            }
            TempData["SuccessNotification"] = "Notification Added Successfully";
            return RedirectToAction("SOIntakeDetails", new { facultycode = Convert.ToInt32(model.FacultyCode.Trim()), collegeCode = model.CollegeCode});
        }

        //[HttpGet]
        //public async Task<IActionResult> getCollegesForSectionOfficer(int facultyCode)
        //{
        //    var colleges = await _context.AffiliationCollegeMasters
        //        .Where(c => c.FacultyCode == facultyCode.ToString())
        //        .Select(c => new
        //        {
        //            value = c.CollegeCode,
        //            text = c.CollegeName,
        //        })
        //        .ToListAsync();

        //    // Add "Select College" option at the top
        //    colleges.Insert(0, new { value = "", text = "Select College" });

        //    // Add "All Colleges" option after Select College
        //    colleges.Insert(1, new { value = "all", text = "All Colleges" });

        //    return Json(colleges);
        //}


        public IActionResult ViewDocument1(string collegeCode)
        {
            var college = _context.CollegeCourseIntakeDetails
                .FirstOrDefault(c => c.CollegeCode== collegeCode && c.DocumentAffiliation != null);

            if (college == null)
                return NotFound();

            byte[] fileBytes = college.DocumentAffiliation;

            // Check if it's already a byte array
            if (IsZipFile(fileBytes))
            {
                return File(fileBytes, "application/zip", "Documents.zip");
            }
            else
            {
                // Single PDF → serve directly
                return File(fileBytes, "application/pdf");
            }
        }

        private bool IsZipFile(byte[] fileBytes)
        {
            // ZIP files start with "PK" (0x50, 0x4B)
            return fileBytes.Length > 4 &&
                   fileBytes[0] == 0x50 &&
                   fileBytes[1] == 0x4B;
        }


        public async Task<IActionResult> UpdateDistrictsAndTaluksForColleges()
        {
            var facultyId = HttpContext.Session.GetString("FacultyId");

            var model = new UpdateDistrictsAndTaluksForCollegesViewModel
            {
                Faculties = await _context.Faculties.ToListAsync(),
                DistrictMasters = await _context.DistrictMasters.ToListAsync(),
                TalukMasters = await _context.TalukMasters.ToListAsync(),
                SelectedFaculty = facultyId ?? "",
                affiliationCollegeMasters = new List<SelectListItem>(),
                SelectedCollegeIds = new List<string>()
            };

            if (!string.IsNullOrEmpty(facultyId))
            {
                model.affiliationCollegeMasters = await _context.AffiliationCollegeMasters
                    .Where(c => c.FacultyCode == facultyId)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CollegeCode,
                        Text = c.CollegeName
                    })
                    .ToListAsync();
            }

            return View(model);
        }


        [HttpGet]
        public JsonResult GetTaluksByDistrict(string districtId)
        {
            var taluks = _context.TalukMasters
                         .Where(t => t.DistrictId == districtId)
                         .Select(t => new { t.TalukId, t.TalukName })
                         .ToList();
            return Json(taluks);
        }

        [HttpGet]
        public JsonResult GetCollegeByFaculty(string facultyId)
        {
            var colleges = _context.AffiliationCollegeMasters
                          .Where(c => c.FacultyCode == facultyId && !string.IsNullOrEmpty(c.DistrictId) && !string.IsNullOrEmpty(c.TalukId))
                          .Select(c => new { c.CollegeCode, c.CollegeName })
                          .ToList();
            return Json(colleges);
        }


        public async Task<IActionResult> SOMobileAndPasswordUpdate()
        {
            var facultyIdStr = HttpContext.Session.GetString("FacultyId");
            ViewBag.facultyId = facultyIdStr;
            ViewBag.Controller = Convert.ToInt32(facultyIdStr) > 98 ? "Admin" : "SectionOfficer";
            ViewBag.Dashboard = Convert.ToInt32(facultyIdStr) > 98 ? "AdminDashboard" : "SODashboard";
            var vm = new CollegeDetailsUpdateViewModel
            {
                FacultyList = await _context.Faculties
                    .Select(f => new SelectListItem
                    {
                        Value = f.FacultyId.ToString(),
                        Text = f.FacultyName
                    })
                    .ToListAsync(),

            };

            return View(vm);
        }


        [HttpGet]
        public async Task<IActionResult> GetCollegesForSoMobileUpdate(int facultyId)
        {
            var facultyIdStr = HttpContext.Session.GetString("FacultyId");
            ViewBag.facultyId = facultyIdStr;
            ViewBag.Controller = Convert.ToInt32(facultyIdStr) > 98 ? "Admin" : "SectionOfficer";
            ViewBag.Dashboard = Convert.ToInt32(facultyIdStr) > 98 ? "AdminDashboard" : "SODashboard";
            var colleges = await _context.AffiliationCollegeMasters
                .Where(c => c.FacultyCode == facultyId.ToString())
                .Select(c => new
                {
                    c.CollegeCode,
                    c.CollegeName,
                    c.PrincipalMobileNumber,
                    c.Password
                })
                .ToListAsync();

            return Json(colleges);
        }


        [HttpPost]
        [RequestFormLimits(ValueCountLimit = 100000)]
        public async Task<IActionResult> UpdateCollegesBulk(CollegeDetailsUpdateViewModel vm)
        {
            try
            {
                if (vm == null)
                {
                    return Json(new { success = false, message = "Invalid request. Data is missing." });
                }

                if (vm.CollegeList == null || !vm.CollegeList.Any())
                {
                    return Json(new { success = false, message = "No data received." });
                }

                // Collect valid codes
                var codes = vm.CollegeList
                    .Where(x => !string.IsNullOrWhiteSpace(x.CollegeCode))
                    .Select(x => x.CollegeCode.Trim())
                    .ToList();

                if (!codes.Any())
                {
                    return Json(new { success = false, message = "No valid college codes provided." });
                }

                // Fetch colleges in one go
                var colleges = await _context.AffiliationCollegeMasters
                    .Where(c => c.CollegeCode != null && codes.Contains(c.CollegeCode.Trim()))
                    .ToListAsync();

                var passwordHasher = new PasswordHasher<AffiliationCollegeMaster>();
                int updatedCount = 0;
                int skippedNulls = 0;

                foreach (var update in vm.CollegeList.Where(u => !string.IsNullOrWhiteSpace(u.CollegeCode)))
                {
                    var code = update.CollegeCode.Trim();

                    var college = colleges.FirstOrDefault(c =>
                        !string.IsNullOrWhiteSpace(c.CollegeCode) &&
                        c.CollegeCode.Trim() == code);

                    if (college == null)
                    {
                        continue;
                    }

                    var mobile = update.MobileNumber?.Trim();
                    if (string.IsNullOrEmpty(mobile))
                    {
                        skippedNulls++;
                        continue;
                    }

                    college.PrincipalMobileNumber = mobile;
                    college.Password = mobile;
                    college.HashedPassword = passwordHasher.HashPassword(college, mobile);
                    updatedCount++;
                }

                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = updatedCount > 0
                        ? $"{updatedCount} college(s) updated successfully!"
                        : "No changes detected."
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "An unexpected error occurred while processing your request."
                });
            }
        }

        public async Task<IActionResult> GetCollegesByFacultyForSOUpdate()
        {
            // Get faculty code from session   
            var facultyCode = HttpContext.Session.GetString("FacultyId");

            int facultyId = 0;
            if (!string.IsNullOrEmpty(facultyCode))
                int.TryParse(facultyCode, out facultyId);

            var vm = new SectionOfficerCourseWiseRguhsIntakeViewModel
            {
                // Faculties: either all or just the session faculty
                faculties = string.IsNullOrEmpty(facultyCode)
                    ? await _context.Faculties.ToListAsync()
                    : await _context.Faculties
                        .Where(f => f.FacultyId == facultyId)
                        .ToListAsync(),

                // Colleges: distinct per CollegeCode, showing CollegeName + Town
                collegeCourseIntakeDetails = string.IsNullOrEmpty(facultyCode)
                    ? new List<CollegeCourseIntakeDetail>()
                    : await (
                        from cc in _context.CollegeCourseIntakeDetails
                        join acm in _context.AffiliationCollegeMasters
                            on cc.CollegeCode equals acm.CollegeCode
                        where cc.FacultyCode == facultyId
                        select new
                        {
                            cc.CollegeCode,
                            cc.CollegeName,
                            acm.CollegeTown
                        }
                    )
                    .GroupBy(c => c.CollegeCode)
                    .Select(g => new CollegeCourseIntakeDetail
                    {
                        CollegeCode = g.Key,
                        CollegeName = g.First().CollegeName + ", " + g.First().CollegeTown
                    })
                    .ToListAsync(),


            rguhsIntakeChangeAndApprovals = null,

                SelectedFacultyCode = facultyCode // for default selection
            };

            return View(vm);
        }



        public async Task<IActionResult> FetchCollegesByFaculty(string facultyId)
        {
            var colleges = await _context.CollegeCourseIntakeDetails
                .Where(x => x.FacultyCode.ToString() == facultyId)
                .GroupBy(x => new { x.CollegeCode, x.CollegeName })
                .Select(g => new {
                    value = g.Key.CollegeCode,
                    text = g.Key.CollegeName
                })
                .OrderBy(x => x.text)
                .ToListAsync();

            return Json(colleges);
        }

        public async Task<IActionResult> GetCoursesByCollegeForSOUpdate(string collegeCode)
        {
            var list =
               (from course in _context.CollegeCourseIntakeDetails
                join rg in _context.RguhsIntakeChangeAndApprovals
                on new { course.CollegeCode, course.CourseCode }
                equals new { rg.CollegeCode, rg.CourseCode }
                into leftJoin
                from rguhs in leftJoin.DefaultIfEmpty()
                where course.CollegeCode == collegeCode
                select new
                {
                    course.CourseCode,
                    course.CourseName,
                    RguhsExistingIntake = course.ExistingIntake,
                    RguhsNewIntake = rguhs.RguhsIntake,
                    rguhs.RemarksForIntakeChange
                }).ToList();

            return Json(list);
        }


        //[HttpPost]
        //[RequestFormLimits(ValueCountLimit = 100000)]
        //[Consumes("application/json")]
        //public async Task<IActionResult> SOUpdateSaveNewRguhsIntake([FromBody] List<IntakeUpdateVM> model)
        //{
        //    // Critical null/empty checks FIRST
        //    if (model == null)
        //    {
        //        return BadRequest(new
        //        {
        //            success = false,
        //            message = "Invalid request: Model is null. Ensure Content-Type: application/json and valid JSON array."
        //        });
        //    }

        //    if (!model.Any())
        //    {
        //        return BadRequest(new
        //        {
        //            success = false,
        //            message = "Invalid request: Empty model list."
        //        });
        //    }

        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        var processedCount = 0;
        //        var errors = new List<string>();
        //        var successRecords = new List<object>();

        //        foreach (var item in model)
        //        {
        //            // Validate individual item
        //            if (string.IsNullOrWhiteSpace(item.CollegeCode) ||
        //                string.IsNullOrWhiteSpace(item.CourseCode) ||
        //                item.NewIntake <= 0 ||
        //                string.IsNullOrWhiteSpace(item.Reason))
        //            {
        //                errors.Add($"Invalid data: CollegeCode='{item.CollegeCode}', CourseCode='{item.CourseCode}'");
        //                continue;
        //            }

        //            // Check RguhsIntakeChangeAndApprovals table
        //            var existingChangeRequest = await _context.RguhsIntakeChangeAndApprovals
        //                .FirstOrDefaultAsync(x => x.CollegeCode == item.CollegeCode &&
        //                                        x.CourseCode == item.CourseCode);

        //            // Check CollegeCourseIntakeDetails table
        //            var collegeCourse = await _context.CollegeCourseIntakeDetails
        //                .FirstOrDefaultAsync(x => x.CollegeCode == item.CollegeCode &&
        //                                        x.CourseCode == item.CourseCode);

        //            if (collegeCourse == null)
        //            {
        //                errors.Add($"CollegeCourseIntakeDetails not found: CollegeCode='{item.CollegeCode}', CourseCode='{item.CourseCode}'");
        //                continue;
        //            }

        //            if (existingChangeRequest == null)
        //            {
        //                // CREATE NEW: Change request + Update live table
        //                var changeRequest = new RguhsIntakeChangeAndApproval
        //                {
        //                    CollegeCode = item.CollegeCode,
        //                    CourseCode = item.CourseCode,
        //                    RguhsIntake = item.NewIntake,
        //                    ApprovalByAdmin = 0, // Pending approval
        //                    RemarksForIntakeChange = item.Reason,
        //                };
        //                _context.RguhsIntakeChangeAndApprovals.Add(changeRequest);
        //            }
        //            else
        //            {
        //                // UPDATE: Existing change request (reset to pending)
        //                existingChangeRequest.RguhsIntake = item.NewIntake;
        //                existingChangeRequest.ApprovalByAdmin = 0;
        //                existingChangeRequest.RemarksForIntakeChange = item.Reason;
        //            }

        //            // ALWAYS update live CollegeCourseIntakeDetails table
        //            collegeCourse.RguhsIntake202526 = item.NewIntake.ToString();
        //            _context.Entry(collegeCourse).State = EntityState.Modified;

        //            successRecords.Add(new
        //            {
        //                CollegeCode = item.CollegeCode,
        //                CourseCode = item.CourseCode,
        //                NewIntake = item.NewIntake
        //            });
        //            processedCount++;
        //        }

        //        // Save all changes in single transaction
        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        if (errors.Any())
        //        {
        //            return Ok(new
        //            {
        //                success = true,
        //                message = $"Processed {processedCount} records successfully. {errors.Count} errors encountered.",
        //                processedCount,
        //                successRecords,
        //                errors
        //            });
        //        }

        //        return Ok(new
        //        {
        //            success = true,
        //            message = $"Successfully processed {processedCount} intake updates. Awaiting admin approval.",
        //            processedCount,
        //            successRecords
        //        });
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        await transaction.RollbackAsync();
        //        return BadRequest(new
        //        {
        //            success = false,
        //            message = "Database update failed",
        //            error = dbEx.InnerException?.Message ?? dbEx.Message
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return StatusCode(500, new
        //        {
        //            success = false,
        //            message = "Internal server error occurred",
        //            error = ex.Message
        //        });
        //    }
        //}

        ///////////////////////////////////////////////////////////////
        ///

        [HttpGet]
        public IActionResult YearWiseReport()
        {
            var model = new YearWiseReportViewModel
            {
                SelectedYear = "2025-26",
                YearList = new List<string> { "2025-26", "2026-27" },
                FacultyList = GetFacultyList(),
                CollegeList = new List<SelectListItem>()
            };

            return View(model);
        }

        //[HttpGet]
        //public JsonResult GetYearWiseData(string year, int? facultyId, string collegeCode)
        //{
        //    var rows = _context.CollegeCourseIntakeDetails.AsQueryable();

        //    if (!string.IsNullOrEmpty(year))
        //    {
        //        if (year == "2025-26")
        //        {
        //            rows = rows.Where(x => x.ExistingIntake != null); // or your condition
        //        }
        //        else
        //        {
        //            rows = rows.Where(x => x.RguhsIntake202526 != null); // or your condition
        //        }
        //    }

        //    if (facultyId.HasValue)
        //        rows = rows.Where(x => x.FacultyCode == facultyId.Value);

        //    if (!string.IsNullOrEmpty(collegeCode))
        //        rows = rows.Where(x => x.CollegeCode == collegeCode);

        //    var data = rows
        //        .OrderBy(x => x.CollegeName)
        //        .ThenBy(x => x.CourseName)
        //        .Select(x => new
        //        {
        //            x.CollegeName,
        //            x.CollegeAddress,
        //            x.CourseName,
        //            RguhsIntake_2026_27 = x.RguhsIntake202526,
        //            CollegeIntake_2026_27 = x.CollegeIntake202526,
        //            ExistingIntake_2025_26 = x.ExistingIntake,
        //            PresentIntake_2025_26 = x.PresentIntake
        //        })
        //        .ToList();

        //    return Json(data);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult YearWiseReport(YearWiseReportViewModel model)
        //{
        //    // Rebuild dropdowns
        //    model.YearList = new List<string> { "2025-26", "2026-27" };
        //    model.FacultyList = GetFacultyList();

        //    // Build College dropdown only if a faculty is selected
        //    if (model.SelectedFacultyId.HasValue)
        //    {
        //        model.CollegeList = GetCollegeList(model.SelectedFacultyId);
        //    }
        //    else
        //    {
        //        model.CollegeList = new List<SelectListItem>();
        //    }

        //    // Fetch filtered rows
        //    model.Records = GetRows(model.SelectedYear, model.SelectedFacultyId, model.SelectedCollegeId);

        //    return View(model);
        //}


        private List<SelectListItem> GetFacultyList()
        {
            return _context.Faculties
                .Where(e => e.FacultyId == 1)
                .OrderBy(f => f.FacultyName)
                .Select(f => new SelectListItem
                {
                    Value = f.FacultyId.ToString(),
                    Text = f.FacultyName
                }).ToList();
        }

        private List<SelectListItem> GetCollegeList(int? facultyId)
        {
            if (!facultyId.HasValue)
                return new List<SelectListItem>();

            return _context.AffiliationCollegeMasters
                .Where(c => c.FacultyCode == "1") // use int comparison
                .OrderBy(c => c.CollegeName)    
                .Select(c => new SelectListItem
                {
                    Value = c.CollegeCode,
                    Text = c.CollegeName
                }).ToList();
        }

        [HttpGet]
        public JsonResult GetCollegesByFaculty(int facultyId)
        {
            var colleges = _context.AffiliationCollegeMasters
                .Where(c => c.FacultyCode == facultyId.ToString())
                .OrderBy(c => c.CollegeName)
                .Select(c => new { value = c.CollegeCode, text = c.CollegeName })
                .ToList();

            return Json(colleges);
        }

        //private List<YearWiseRowDto> GetRows(string year, int? facultyId, int? collegeId)
        //{
        //    var q = _context.CollegeCourseIntakeDetails.AsQueryable();

        //    if (facultyId.HasValue)
        //        q = q.Where(x => x.FacultyCode == facultyId.Value);

        //    if (collegeId.HasValue)
        //        q = q.Where(x => x.CollegeCode == collegeId.Value.ToString());

        //    var data = q.OrderBy(x => x.CollegeName)
        //                .ThenBy(x => x.CourseName)
        //                .ToList(); // bring to memory

        //    if (year == "2025-26")
        //    {
        //        return data.Select(x => new YearWiseRowDto
        //        {
        //            CollegeName = x.CollegeName ?? "",
        //            CollegeAddress = x.CollegeAddress ?? "",
        //            CourseName = x.CourseName ?? "",
        //            ExistingIntake_2025_26 = x.ExistingIntake?.ToString() ?? "",
        //            PresentIntake_2025_26 = x.PresentIntake?.ToString() ?? ""
        //        }).ToList();
        //    }
        //    else if (year == "2026-27")
        //    {
        //        return data.Select(x => new YearWiseRowDto
        //        {
        //            CollegeName = x.CollegeName ?? "",
        //            CollegeAddress = x.CollegeAddress ?? "",
        //            CourseName = x.CourseName ?? "",
        //            RguhsIntake_2026_27 = x.RguhsIntake202526 ?? "0",
        //            CollegeIntake_2026_27 = x.CollegeIntake202526 ?? "0"
        //        }).ToList();
        //    }

        //    return new List<YearWiseRowDto>();
        //}


    }
}
