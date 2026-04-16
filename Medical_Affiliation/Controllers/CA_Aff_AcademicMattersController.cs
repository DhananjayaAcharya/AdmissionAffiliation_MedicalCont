using Medical_Affiliation.Controllers;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Medical_Affiliation.Controllers
{
    public class CA_Aff_AcademicMattersController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private static readonly int?[] _yearIds = { 1, 2, 3, 4 }; // 1st, 2nd, 3rd, Final

        public CA_Aff_AcademicMattersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult AcademicMatters()
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            string collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "M001";
            int facultyId = HttpContext.Session.GetInt32("FacultyId") ?? 1;
            int affiliationType = HttpContext.Session.GetInt32("AffiliationType") ?? 2;

            var model = new CA_Aff_AcademicMattersViewModel
            {
                CollegeCode = collegeCode,
                FacultyId = facultyId,
                AffiliationType = affiliationType,
                CourseLevel = courseLevel
            };


            //&&
            //x.CurriculumId == curriculumId);
            var savedCurriculums = _context.CaCourseCurricula
                .Where(x => x.CollegeCode == model.CollegeCode &&
                            x.FacultyId == model.FacultyId &&
                            x.CourseLevel == model.CourseLevel &&
                            x.AffiliationType == model.AffiliationType)
                .ToList();

            //model.CourseCurriculums = savedCurriculums.Select(x => new CourseCurriculumViewModel
            //{
            //    CurriculumId = x.CurriculumId,
            //    CurriculumDetails = x.CurriculumDetails,
            //    CurriculumPdfPath = x.CurriculumPdfPath
            //}).ToList();



            //if (pdf == null || pdf.CurriculumPdf == null)
            //{
            //    return Content("No PDF uploaded for this item.");
            //    // OR: return NotFound("No PDF uploaded");
            //}

            //return File(
            //    pdf.CurriculumPdf,
            //    "application/pdf",
            //    pdf.PdfFileName ?? "Curriculum.pdf"
            //);

            // Load academic performance rows for this college/faculty/affiliation
            var academics = _context.CaAcademicPerformances
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyId == facultyId &&
                    x.CourseLevel == courseLevel &&
                    x.AffiliationType == affiliationType &&
                    _yearIds.Any(y => y == x.YearOfStudyId))
                .ToList();

            // Year master
            var yearMaster = _context.CaMstYearOfStudies
                .Where(y => _yearIds.Contains(y.YearOfStudyId))
                .ToDictionary(y => y.YearOfStudyId, y => y.YearName);

            var academicRows = new List<AcademicPerformanceViewModel>();
            foreach (var y in _yearIds)
            {
                var existing = academics.FirstOrDefault(a => a.YearOfStudyId == y);
                if (existing != null)
                {

                    academicRows.Add(new AcademicPerformanceViewModel
                    {
                        AcademicPerformanceId = existing.AcademicPerformanceId,
                        YearOfStudyId = existing.YearOfStudyId,
                        YearName = yearMaster.ContainsKey(existing.YearOfStudyId.Value)
                            ? yearMaster[existing.YearOfStudyId.Value]
                            : "",
                        RegularStudents = existing.RegularStudents ?? 0,
                        RepeaterStudents = existing.RepeaterStudents ?? 0,
                        NumberOfStudentsPassed = existing.NumberOfStudentsPassed ?? 0,
                        PassPercentage = existing.PassPercentage ?? 0,
                        FirstClassCount = existing.FirstClassCount ?? 0,
                        DistinctionCount = existing.DistinctionCount ?? 0,
                        Remarks = existing.Remarks
                    });

                }
                else
                {
                    academicRows.Add(new AcademicPerformanceViewModel
                    {
                        AcademicPerformanceId = 0,
                        YearOfStudyId = y,
                        YearName = yearMaster.ContainsKey(y.Value) ? yearMaster[y.Value] : "",
                        RegularStudents = null,
                        RepeaterStudents = null,
                        NumberOfStudentsPassed = null,
                        PassPercentage = 0,
                        FirstClassCount = null,
                        DistinctionCount = null,
                        Remarks = string.Empty
                    });
                }
            }
            model.AcademicRows = academicRows;

            // Course curriculum

            // ================= COURSE CURRICULUM (LIST) =================
            var curriculumMasters = _context.CaMstCourseCurricula
                .OrderBy(c => c.CurriculumId)
                .ToList();

            //var savedCurriculums = _context.CaCourseCurricula
            //    .Where(x => x.CollegeCode == collegeCode
            //                && x.FacultyId == facultyId
            //                && x.AffiliationType == affiliationType)
            //    .ToList();

            model.CourseCurriculums = curriculumMasters.Select(m =>
            {
                var saved = savedCurriculums.FirstOrDefault(s => s.CurriculumId == m.CurriculumId);

                return new CourseCurriculumViewModel
                {
                    CurriculumId = m.CurriculumId,
                    CurriculumName = m.CurriculumName,
                    CurriculumDetails = saved?.CurriculumDetails,
                    HasPdf = saved != null
                             && saved.CurriculumPdf != null
                             && saved.CurriculumPdf.Length > 0

                };
            }).ToList();


            ViewBag.CurriculumMasters = curriculumMasters;

            // Examination schemes: build master list and merge saved values
            var savedSchemes = _context.CaExaminationSchemes
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyId == facultyId &&
                            x.CourseLevel == courseLevel &&
                            x.AffiliationType == affiliationType)
                .ToList();

            var schemes = _context.CaMstExaminationSchemes
                .OrderBy(s => s.SchemeId)
                .ToList();

            model.ExaminationSchemess = schemes
                .Select(m =>
                {
                    var saved = savedSchemes.FirstOrDefault(s => s.SchemeId == m.SchemeId);
                    return new ExaminationSchemeRowViewModel
                    {
                        SchemeId = m.SchemeId,
                        SchemeCode = m.SchemeCode,
                        NumberOfStudents = saved != null ? saved.NumberOfStudents : null
                    };
                })
                .ToList();

            // Student register records (multiple)
            var registerMasters = _context.CaMstRegisterRecords
                                         .OrderBy(r => r.RegisterRecordId)
                                         .ToList();

            var savedRegisters = _context.CaStudentRegisterRecords
                    .Where(x => x.CollegeCode == collegeCode &&
                                x.FacultyId == facultyId &&
                                x.CourseLevel == courseLevel &&
                                x.AffiliationType == affiliationType)
                    .ToList();

            model.StudentRegisterRecords = registerMasters
                     .Select(m =>
                     {
                         var saved = savedRegisters
                             .FirstOrDefault(s => s.RegisterRecordId == m.RegisterRecordId);

                         return new StudentRegisterRecordViewModel
                         {
                             RegisterRecordId = m.RegisterRecordId,
                             RegisterName = m.RegisterName,

                             // ✅ FIX
                             IsMaintained = saved?.IsMaintained
                         };
                     })
                     .ToList();


            // ViewBag Masters
            ViewBag.YearList = _context.CaMstYearOfStudies
                .Where(y => _yearIds.Contains(y.YearOfStudyId))
                .OrderBy(y => y.YearOfStudyId)
                .ToList();

            ViewBag.RegisterRecords = _context.CaMstRegisterRecords
                .OrderBy(r => r.RegisterRecordId)
                .ToList();

            //        ViewBag.CurriculumMasters = _context.CaMstCourseCurricula
            //.OrderBy(c => c.CurriculumId)     // or CurriculumName if you have that column
            //.ToList();

            return View("AcademicMatters", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AcademicMatters(CA_Aff_AcademicMattersViewModel model)
        {

            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            if (model == null)
                return RedirectToAction(nameof(AcademicMatters));

            // ModelState debug: capture any binding errors to show in the view
            if (!ModelState.IsValid)
            {
                var allErrors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage + (e.Exception != null ? " (" + e.Exception.Message + ")" : "")));
                ModelState.AddModelError("", "Validation failed: " + allErrors);

                LoadAcademicMattersMasters(model);
                return View("AcademicMatters", model);
            }

            // Compute pass%
            foreach (var row in model.AcademicRows)
            {
                int total = (int)(row.RegularStudents + row.RepeaterStudents);
                row.PassPercentage = total == 0
                    ? 0
                    : Math.Round((decimal)row.NumberOfStudentsPassed * 100 / total, 2);
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // Academic rows: remove and re-add to keep simple
                foreach (var posted in model.AcademicRows)
                {
                    var old = _context.CaAcademicPerformances.Where(x =>
                        x.CollegeCode == model.CollegeCode &&
                        x.FacultyId == model.FacultyId &&
                        x.CourseLevel == courseLevel &&
                        x.AffiliationType == model.AffiliationType &&
                        x.YearOfStudyId == posted.YearOfStudyId);

                    _context.CaAcademicPerformances.RemoveRange(old);

                    var entity = new CaAcademicPerformance
                    {
                        CollegeCode = model.CollegeCode,
                        FacultyId = model.FacultyId ?? 0,
                        CourseLevel = courseLevel,
                        AffiliationType = model.AffiliationType ?? 0,
                        YearOfStudyId = posted.YearOfStudyId,
                        RegularStudents = posted.RegularStudents,
                        RepeaterStudents = posted.RepeaterStudents,
                        NumberOfStudentsPassed = posted.NumberOfStudentsPassed,
                        PassPercentage = posted.PassPercentage,
                        FirstClassCount = posted.FirstClassCount,
                        DistinctionCount = posted.DistinctionCount,
                        Remarks = posted.Remarks,
                        CreatedOn = DateTime.Now
                    };
                    _context.CaAcademicPerformances.Add(entity);
                }


                // ================= COURSE CURRICULUM (INSERT OR UPDATE) =================
                if (model.CourseCurriculums != null)
                {
                    foreach (var row in model.CourseCurriculums)
                    {
                        var existing = _context.CaCourseCurricula.FirstOrDefault(x =>
                            x.CollegeCode == model.CollegeCode &&
                            x.FacultyId == model.FacultyId &&
                            x.CourseLevel == courseLevel &&
                            x.AffiliationType == model.AffiliationType &&
                            x.CurriculumId == row.CurriculumId
                        );

                        if (existing == null)
                        {
                            // INSERT
                            existing = new CaCourseCurriculum
                            {
                                CollegeCode = model.CollegeCode,
                                FacultyId = model.FacultyId ?? 0,
                                CourseLevel = courseLevel,
                                AffiliationType = model.AffiliationType ?? 0,
                                CurriculumId = (int)row.CurriculumId,
                                CreatedOn = DateTime.Now
                            };

                            _context.CaCourseCurricula.Add(existing);
                        }

                        // UPDATE details
                        existing.CurriculumDetails = row.CurriculumDetails;

                        // -------------------- PDF Handling (DB ONLY) --------------------
                        if (row.CurriculumPdfFiles != null && row.CurriculumPdfFiles.Any())
                        {
                            var file = row.CurriculumPdfFiles.First();

                            using var ms = new MemoryStream();
                            file.CopyTo(ms);

                            existing.CurriculumPdf = ms.ToArray();
                            existing.PdfFileName = file.FileName;
                        }




                    }
                    _context.SaveChanges();
                }







                // Examination schemes: robust binding handling
                // The view renders ExaminationSchemess (master rows) and posts those values.
                // But to be defensive, accept either ExaminationSchemess or ExaminationSchemes posted.
                var postedSchemes = new List<ExaminationSchemeRowViewModel>();

                if (model.ExaminationSchemess != null && model.ExaminationSchemess.Count > 0)
                {
                    postedSchemes = model.ExaminationSchemess;
                }
                else if (model.ExaminationSchemes != null && model.ExaminationSchemes.Count > 0)
                {
                    // map ExaminationSchemeViewModel -> ExaminationSchemeRowViewModel
                    postedSchemes = model.ExaminationSchemes.Select(s => new ExaminationSchemeRowViewModel
                    {
                        SchemeId = s.SchemeId ?? 0,
                        NumberOfStudents = s.NumberOfStudents
                    }).ToList();
                }

                var oldSchemes = _context.CaExaminationSchemes.Where(x =>
                    x.CollegeCode == model.CollegeCode &&
                    x.FacultyId == model.FacultyId &&
                    x.CourseLevel == courseLevel &&
                    x.AffiliationType == model.AffiliationType).ToList();

                _context.CaExaminationSchemes.RemoveRange(oldSchemes);

                if (postedSchemes != null)
                {
                    foreach (var scheme in postedSchemes)
                    {
                        if (!scheme.NumberOfStudents.HasValue) ;

                        _context.CaExaminationSchemes.Add(new CaExaminationScheme
                        {
                            CollegeCode = model.CollegeCode,
                            FacultyId = model.FacultyId ?? 0,
                            CourseLevel = courseLevel,
                            AffiliationType = model.AffiliationType ?? 0,
                            SchemeId = (int)scheme.SchemeId,
                            NumberOfStudents = scheme.NumberOfStudents.Value,
                            CreatedOn = DateTime.Now
                        });
                    }
                }

                // Student Register Records (multiple)
                var oldRegisters = _context.CaStudentRegisterRecords.Where(x =>
                    x.CollegeCode == model.CollegeCode &&
                    x.FacultyId == model.FacultyId &&
                    x.CourseLevel == courseLevel &&
                    x.AffiliationType == model.AffiliationType).ToList();
                _context.CaStudentRegisterRecords.RemoveRange(oldRegisters);

                if (model.StudentRegisterRecords != null)
                {
                    foreach (var rec in model.StudentRegisterRecords)
                    {
                        // Only add if RegisterRecordId is present (defensive)
                        if (!rec.RegisterRecordId.HasValue) continue;

                        _context.CaStudentRegisterRecords.Add(new CaStudentRegisterRecord
                        {
                            CollegeCode = model.CollegeCode,
                            FacultyId = model.FacultyId ?? 0,
                            CourseLevel = courseLevel,
                            AffiliationType = model.AffiliationType ?? 0,
                            RegisterRecordId = rec.RegisterRecordId ?? 0,
                            IsMaintained = rec.IsMaintained,


                            CreatedOn = DateTime.Now
                        });
                    }
                }

                _context.SaveChanges();
                transaction.Commit();

                TempData["Success"] = "Academic matters saved successfully.";
                return RedirectToAction(nameof(AcademicMatters));
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
                LoadAcademicMattersMasters(model);
                return View("AcademicMatters", model);
            }
        }

        private void LoadAcademicMattersMasters(CA_Aff_AcademicMattersViewModel model)
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            ViewBag.YearList = _context.CaMstYearOfStudies
                .Where(y => _yearIds.Contains(y.YearOfStudyId))
                .ToList();

            ViewBag.RegisterRecords = _context.CaMstRegisterRecords

                .ToList();

            ViewBag.CurriculumMasters = _context.CaMstCourseCurricula
    .OrderBy(c => c.CurriculumId)
    .ToList();


            // Ensure ExaminationSchemess is always populated with master rows (merge saved counts when available)
            var schemes = _context.CaMstExaminationSchemes
                .OrderBy(s => s.SchemeId)
                .ToList();

            var savedSchemes = new List<CaExaminationScheme>();
            if (!string.IsNullOrEmpty(model.CollegeCode) && model.FacultyId.HasValue)
            {
                savedSchemes = _context.CaExaminationSchemes
                    .Where(x => x.CollegeCode == model.CollegeCode
                                && x.FacultyId == model.FacultyId &&
                                x.CourseLevel == courseLevel
                                && x.AffiliationType == model.AffiliationType)
                    .ToList();
            }

            model.ExaminationSchemess = schemes.Select(s =>
            {
                var saved = savedSchemes.FirstOrDefault(x => x.SchemeId == s.SchemeId);
                return new ExaminationSchemeRowViewModel
                {
                    SchemeId = s.SchemeId,
                    SchemeCode = s.SchemeCode,
                    NumberOfStudents = saved != null ? saved.NumberOfStudents : null
                };
            }).ToList();

            // Student Register Records – rebuild for validation failure
            var registerMasters = _context.CaMstRegisterRecords
                .OrderBy(r => r.RegisterRecordId)
                .ToList();

            var savedRegisters = new List<CaStudentRegisterRecord>();
            if (!string.IsNullOrEmpty(model.CollegeCode) && model.FacultyId.HasValue)
            {
                savedRegisters = _context.CaStudentRegisterRecords
                    .Where(x => x.CollegeCode == model.CollegeCode
                                && x.FacultyId == model.FacultyId &&
                                x.CourseLevel == courseLevel
                                && x.AffiliationType == model.AffiliationType)
                    .ToList();
            }
            var postedRegisters = model.StudentRegisterRecords ?? new List<StudentRegisterRecordViewModel>();

            model.StudentRegisterRecords = registerMasters
                .Select(m =>
                {
                    var posted = postedRegisters
                        .FirstOrDefault(p => p.RegisterRecordId == m.RegisterRecordId);

                    var saved = savedRegisters
                        .FirstOrDefault(s => s.RegisterRecordId == m.RegisterRecordId);

                    return new StudentRegisterRecordViewModel
                    {
                        RegisterRecordId = m.RegisterRecordId,
                        IsMaintained = posted.IsMaintained
                    };
                })
                .ToList();




        }


        [HttpGet]
        public IActionResult ViewCurriculumPdf(int curriculumId)
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            string collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "M001";
            int facultyId = HttpContext.Session.GetInt32("FacultyId") ?? 1;        // 🔑 FIX
            int affiliationType = HttpContext.Session.GetInt32("AffiliationType") ?? 2; // 🔑 FIX

            // Primary lookup (same as list page)
            var record = _context.CaCourseCurricula.FirstOrDefault(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyId == facultyId &&
                x.AffiliationType == affiliationType &&
                x.CourseLevel == courseLevel &&
                x.CurriculumId == curriculumId);

            // Fallback (in case session mismatch)
            if (record == null)
            {
                record = _context.CaCourseCurricula
                    .FirstOrDefault(x => x.CurriculumId == curriculumId);
            }

            if (record == null || record.CurriculumPdf == null || record.CurriculumPdf.Length == 0)
                return NotFound("PDF not found in database.");

            Response.Headers["Content-Disposition"] =
                $"inline; filename=\"{record.PdfFileName ?? "Curriculum.pdf"}\"";

            return File(record.CurriculumPdf, "application/pdf");
        }



    }
}