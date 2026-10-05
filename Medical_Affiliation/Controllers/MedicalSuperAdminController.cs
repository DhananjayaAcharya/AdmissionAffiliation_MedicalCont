using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Medical_Affiliation.Controllers
{
    public class MedicalSuperAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string MEDICAL_FACULTY_CODE = "1";

        public MedicalSuperAdminController(
        ApplicationDbContext context,
        IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        private string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            return input.Replace(".", "").Trim().ToLower();
        }

        public async Task<IActionResult> Dashboard(
     string districtId = "",
     string talukId = "",
     string search = "")
        {
            districtId = (districtId ?? "").Trim();
            talukId = (talukId ?? "").Trim();
            search = (search ?? "").Trim();

            var vm = new MedicalDashboardViewModel
            {
                SearchTerm = search,
                SelectedDistrict = districtId,
                SelectedTaluk = talukId
            };

            vm.Districts = await _context.DistrictMasters
                .AsNoTracking()
                .OrderBy(d => d.DistrictName)
                .Select(d => new DistrictItem
                {
                    DistrictID = d.DistrictId,
                    DistrictName = d.DistrictName
                })
                .ToListAsync();

            var collegeList = await _context.AffiliationCollegeMasters
                .AsNoTracking()
                .Where(c =>
                    c.FacultyCode == MEDICAL_FACULTY_CODE &&
                    (c.Status == true || c.Status == null))
                .OrderBy(c => c.CollegeName)
                .Select(c => new
                {
                    c.CollegeCode,
                    c.CollegeName,
                    c.CollegeTown,
                    c.DistrictId,
                    c.TalukId
                })
                .ToListAsync();

            if (!collegeList.Any())
            {
                vm.Colleges = new List<CollegeListItem>();
                vm.Stats = await BuildStatsAsync();
                return View(vm);
            }

            var collegeCodes = collegeList
                .Select(c => c.CollegeCode)
                .ToHashSet();

            var allIntakes = await _context.CollegeCourseIntakeDetails
                .AsNoTracking()
                .Where(i => collegeCodes.Contains(i.CollegeCode))
                .Select(i => new
                {
                    i.CollegeCode,
                    i.CourseCode,
                    i.CourseName,
                    i.PresentIntake
                })
                .ToListAsync();


            var levelLookup = await _context.MstCourses
                .AsNoTracking()
                .Where(x => x.FacultyCode == 1)
                .GroupBy(x => x.CourseCode.ToString())
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.First().CourseLevel);

            var ugTotals = new Dictionary<string, int>();
            var pgTotals = new Dictionary<string, int>();

            foreach (var intake in allIntakes)
            {
                if (string.IsNullOrWhiteSpace(intake.CourseCode))
                    continue;

                if (levelLookup.TryGetValue(
                        intake.CourseCode,
                        out string level))
                {
                    if (level == "UG")
                    {
                        ugTotals[intake.CollegeCode] =
                            ugTotals.GetValueOrDefault(intake.CollegeCode)
                            + (intake.PresentIntake ?? 0);
                    }
                    else if (level == "PG")
                    {
                        pgTotals[intake.CollegeCode] =
                            pgTotals.GetValueOrDefault(intake.CollegeCode)
                            + (intake.PresentIntake ?? 0);
                    }
                }
            }

            var districtMap = await _context.DistrictMasters
                .AsNoTracking()
                .ToDictionaryAsync(
                    d => d.DistrictId,
                    d => d.DistrictName);

            var talukMap = await _context.TalukMasters
                .AsNoTracking()
                .ToDictionaryAsync(
                    t => t.TalukId,
                    t => t.TalukName);

            vm.Colleges = collegeList.Select(c => new CollegeListItem
            {
                CollegeCode = c.CollegeCode ?? "",
                CollegeName = c.CollegeName ?? "",
                CollegeTown = c.CollegeTown ?? "",
                DistrictId = c.DistrictId?.Trim() ?? "",
                DistrictName =
                    districtMap.TryGetValue(
                        c.DistrictId?.Trim() ?? "",
                        out var dn)
                        ? dn
                        : "",

                TalukId = c.TalukId?.Trim() ?? "",
                TalukName =
                    talukMap.TryGetValue(
                        c.TalukId?.Trim() ?? "",
                        out var tn)
                        ? tn
                        : "",

                UGSeats =
                    ugTotals.TryGetValue(
                        c.CollegeCode,
                        out var ug)
                        ? ug
                        : 0,

                PGSeats =
                    pgTotals.TryGetValue(
                        c.CollegeCode,
                        out var pg)
                        ? pg
                        : 0
            }).ToList();

            vm.Stats = await BuildStatsAsync();

            return View(vm);
        }

        [HttpGet]
        public async Task<JsonResult> GetTaluks(string districtId)
        {
            if (string.IsNullOrWhiteSpace(districtId)) return Json(new TalukResult { Success = false });
            var taluks = await _context.TalukMasters
                .Where(t => EF.Functions.Like(t.DistrictId, districtId.Trim() + "%"))
                .OrderBy(t => t.TalukName)
                .Select(t => new TalukItem { TalukID = t.TalukId, TalukName = t.TalukName })
                .ToListAsync();
            return Json(new TalukResult { Success = true, Taluks = taluks }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null });
        }

        [HttpGet]
        public async Task<JsonResult> GetCollegeDetail(string collegeCode)
        {
            if (string.IsNullOrWhiteSpace(collegeCode))
                return Json(new CollegeDetailResult
                {
                    Success = false
                });

            var cleanCode = collegeCode.Trim();

            var college = await _context.AffiliationCollegeMasters
                .AsNoTracking()
                .Where(c =>
                    c.CollegeCode == cleanCode &&
                    (c.Status == true || c.Status == null))
                .Select(c => new
                {
                    c.CollegeCode,
                    c.CollegeName,
                    c.CollegeTown
                })
                .FirstOrDefaultAsync();

            if (college == null)
            {
                return Json(new CollegeDetailResult
                {
                    Success = false,
                    Message = "Not found"
                });
            }

            var intakeDetails = await _context.CollegeCourseIntakeDetails
                .AsNoTracking()
                .Where(i => i.CollegeCode == cleanCode)
                .ToListAsync();

            var levelLookup = await _context.MstCourses
             .AsNoTracking()
             .Where(x => x.FacultyCode == 1)
             .GroupBy(x => x.CourseCode.ToString())
             .ToDictionaryAsync(
         g => g.Key,
         g => g.First().CourseLevel);

            var ugCourses = new List<CourseRow>();
            var pgCourses = new List<CourseRow>();

            foreach (var item in intakeDetails)
            {
                if (string.IsNullOrWhiteSpace(item.CourseCode))
                    continue;

                if (levelLookup.TryGetValue(
                    item.CourseCode,
                    out string level))
                {
                    var row = new CourseRow
                    {
                        CourseName = item.CourseName,
                        CourseLevel = level,
                        PresentIntake = item.PresentIntake ?? 0
                    };

                    if (level == "UG")
                        ugCourses.Add(row);
                    else if (level == "PG")
                        pgCourses.Add(row);
                }
            }

            return Json(new CollegeDetailResult
            {
                Success = true,
                Data = new CollegeDetailData
                {
                    CollegeCode = college.CollegeCode,
                    CollegeName = college.CollegeName,
                    CollegeTown = college.CollegeTown ?? "",
                    TotalCourses = ugCourses.Count + pgCourses.Count,
                    TotalIntake = ugCourses.Sum(x => x.PresentIntake) +
                      pgCourses.Sum(x => x.PresentIntake),
                    UGIntake = ugCourses.Sum(x => x.PresentIntake),
                    PGIntake = pgCourses.Sum(x => x.PresentIntake),
                    UGCourses = ugCourses,
                    PGCourses = pgCourses
                }
            },
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });
        }

        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [HttpGet]
        public async Task<IActionResult> SubmissionRecords(string? collegeCode)
        {
            var cleanCode = collegeCode?.Trim();
            if (string.IsNullOrWhiteSpace(cleanCode))
            return RedirectToAction(nameof(Dashboard));

            var college = await _context.AffiliationCollegeMasters
                .AsNoTracking()
                .Where(c => c.CollegeCode == cleanCode && c.FacultyCode == MEDICAL_FACULTY_CODE)
                .Select(c => new { c.CollegeCode, c.CollegeName, c.CollegeTown })
                .FirstOrDefaultAsync();

            if (college == null)
                return NotFound();

            var sealedDocuments = await _context.AffiliationSealedSignedReuploads
                .AsNoTracking()
                .Where(x => x.CollegeCode == cleanCode && x.FacultyCode == MEDICAL_FACULTY_CODE)
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new
                {
                    x.Id,
                    x.TypeOfApplication,
                    x.CreatedOn,
                    x.ReuploadedDoc
                })
                .ToListAsync();

            var paymentDocuments = await _context.PaymentAffiliationDocuments
                .AsNoTracking()
                .Where(x => x.CollegeCode == cleanCode && x.FacultyCode == 1)
                .OrderByDescending(x => x.PaymentDate ?? x.CreatedOn)
                .Select(x => new
                {
                    x.PaymentDocumentId,
                    x.CourseLevel,
                    x.AffiliationTypeId,
                    x.TransactionId,
                    x.PaymentAmount,
                    x.PaymentDate,
                    x.CreatedOn,
                    x.ScreenshotFileName,
                    x.ScreenshotFilePath,
                    x.PublicAccessToken
                })
                .ToListAsync();

            var paymentDocumentIds = paymentDocuments.Select(x => x.PaymentDocumentId).ToList();
            var receipts = await _context.PaymentReceipts
                .AsNoTracking()
                .Where(x => paymentDocumentIds.Contains(x.PaymentDocumentId))
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new
                {
                    x.ReceiptId,
                    x.PaymentDocumentId,
                    x.FilePath,
                    x.PublicAccessToken,
                    x.CreatedOn
                })
                .ToListAsync();

            var affiliationTypeIds = paymentDocuments.Select(x => x.AffiliationTypeId).Distinct().ToList();
            var affiliationTypes = await _context.MstAffiliationTypes
                .AsNoTracking()
                .Where(x => x.FacultyCode == MEDICAL_FACULTY_CODE && affiliationTypeIds.Contains(x.AffiliationTypeId))
                .ToDictionaryAsync(x => x.AffiliationTypeId, x => x.AffiliationCategory);

            var model = new MedicalSubmissionRecordsViewModel
            {
                CollegeCode = college.CollegeCode,
                CollegeName = college.CollegeName ?? "",
                CollegeTown = college.CollegeTown ?? "",
                SealedSignedDocuments = sealedDocuments.Select(x => new SealedSignedDocumentRecord
                {
                    Id = x.Id,
                    ApplicationType = x.TypeOfApplication ?? "Not specified",
                    SubmittedOn = x.CreatedOn,
                    FileAvailable = !string.IsNullOrWhiteSpace(x.ReuploadedDoc) && System.IO.File.Exists(x.ReuploadedDoc)
                }).ToList(),
                Payments = paymentDocuments.Select(x => new MedicalPaymentRecord
                {
                    PaymentDocumentId = x.PaymentDocumentId,
                    CourseLevel = x.CourseLevel,
                    AffiliationType = affiliationTypes.GetValueOrDefault(x.AffiliationTypeId, $"Application type {x.AffiliationTypeId}"),
                    TransactionId = x.TransactionId,
                    Amount = x.PaymentAmount,
                    PaymentDate = x.PaymentDate,
                    RecordedOn = x.CreatedOn,
                    ScreenshotFileName = x.ScreenshotFileName,
                    ScreenshotAvailable = !string.IsNullOrWhiteSpace(x.ScreenshotFilePath) && System.IO.File.Exists(x.ScreenshotFilePath),
                    ScreenshotToken = x.PublicAccessToken,
                    Receipts = receipts
                        .Where(r => r.PaymentDocumentId == x.PaymentDocumentId)
                        .Select(r => new MedicalPaymentReceiptRecord
                        {
                            ReceiptId = r.ReceiptId,
                            CreatedOn = r.CreatedOn,
                            FileAvailable = !string.IsNullOrWhiteSpace(r.FilePath) && System.IO.File.Exists(r.FilePath),
                            PublicAccessToken = r.PublicAccessToken
                        })
                        .ToList()
                }).ToList()
            };

            return View(model);
        }

        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [HttpGet]
        public async Task<IActionResult> ViewSealedSignedDocument(int id)
        {
            var filePath = await _context.AffiliationSealedSignedReuploads
                .AsNoTracking()
                .Where(x => x.Id == id && x.FacultyCode == MEDICAL_FACULTY_CODE)
                .Select(x => x.ReuploadedDoc)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
                return NotFound("Document not found.");

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
                contentType = "application/octet-stream";

            return PhysicalFile(filePath, contentType);
        }

        private async Task<DashboardStats> BuildStatsAsync()
        {
            return await _cache.GetOrCreateAsync(
                "MedicalDashboardStats",
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(10);

                    var activeColleges = await _context
                        .AffiliationCollegeMasters
                        .AsNoTracking()
                        .Where(c =>
                            c.FacultyCode == MEDICAL_FACULTY_CODE &&
                            (c.Status == true || c.Status == null))
                        .Select(c => new
                        {
                            c.CollegeCode,
                            c.DistrictId
                        })
                        .ToListAsync();

                    int totalColleges = activeColleges.Count;

                    int districtsCovered =
                        activeColleges
                        .Select(x => x.DistrictId)
                        .Distinct()
                        .Count();

                    var activeCodes =
                        activeColleges
                        .Select(x => x.CollegeCode)
                        .ToHashSet();

                    var lookup = await _context.MstCourses
                          .AsNoTracking()
                          .Where(x => x.FacultyCode == 1)
                          .GroupBy(x => x.CourseCode.ToString())
                          .ToDictionaryAsync(
                          g => g.Key,
                          g => g.First().CourseLevel);

                    var intakes = await _context
                        .CollegeCourseIntakeDetails
                        .AsNoTracking()
                        .Where(i => activeCodes.Contains(i.CollegeCode))
                        .Select(i => new
                        {
                            i.CourseCode,
                            i.CourseName,
                            i.PresentIntake
                        })
                        .ToListAsync();

                    int totalUG = 0;
                    int totalPG = 0;

                    foreach (var intake in intakes)
                    {
                        if (string.IsNullOrWhiteSpace(intake.CourseCode))
                            continue;

                        if (lookup.TryGetValue(
                                intake.CourseCode,
                                out string level))
                        {
                            if (level == "UG")
                                totalUG += intake.PresentIntake ?? 0;

                            else if (level == "PG")
                                totalPG += intake.PresentIntake ?? 0;
                        }
                    }

                    return new DashboardStats
                    {
                        TotalColleges = totalColleges,
                        TotalUGSeats = totalUG,
                        TotalPGSeats = totalPG,
                        DistrictsCovered = districtsCovered
                    };
                });
        }
    }
}
