using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class UGPgIntakeDetailsService : IUGPgIntakeDetailsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public UGPgIntakeDetailsService(
            ApplicationDbContext context,
            IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<UGPgIntakeDetailsPreviewViewModel?>
            GetUgCourseDetailsAsync()
        {
            var facultyCode = _userContext.FacultyId.ToString();
            var collegeCode = _userContext.CollegeCode;

            if (string.IsNullOrWhiteSpace(facultyCode) ||
                string.IsNullOrWhiteSpace(collegeCode))
            {
                return null;
            }

            // =====================================================
            // Get UG and PG course details
            // =====================================================

            var courseDetails = await (
                from detail in _context.AffiliationCourseDetails.AsNoTracking()
                join course in _context.MstCourses.AsNoTracking()
                    on detail.CourseId equals course.CoursePrefix
                where detail.Facultycode == facultyCode
                   && detail.Collegecode == collegeCode
                   && (course.CourseLevel == "UG" ||
                       course.CourseLevel == "PG")
                select new
                {
                    Detail = detail,
                    Course = course
                }
            ).ToListAsync();

            if (!courseDetails.Any())
                return null;

            // =====================================================
            // Map UG
            // =====================================================

            var ugEntity = courseDetails
                .FirstOrDefault(x =>
                    x.Course.CourseLevel == "UG");

            // =====================================================
            // Map PG
            // =====================================================

            var pgEntity = courseDetails
                .FirstOrDefault(x =>
                    x.Course.CourseLevel == "PG");

            // =====================================================
            // Build Preview Model
            // =====================================================

            return new UGPgIntakeDetailsPreviewViewModel
            {
                UgCourseDetails = ugEntity != null
                    ? MapCourseDetails(
                        ugEntity.Detail,
                        ugEntity.Course)
                    : null,

                PgCourseDetails = pgEntity != null
                    ? MapCourseDetails(
                        pgEntity.Detail,
                        pgEntity.Course)
                    : null
            };
        }

        // =========================================================
        // Common mapper for UG / PG
        // =========================================================

        private AffiliationCourseDetailsViewModel MapCourseDetails(
            AffiliationCourseDetail entity,
            MstCourse course)
        {
            return new AffiliationCourseDetailsViewModel
            {
                Id = entity.Id,

                Facultycode = entity.Facultycode,
                Collegecode = entity.Collegecode,

                CourseId = entity.CourseId,

                courseName = !string.IsNullOrWhiteSpace(entity.CourseName)
                    ? entity.CourseName
                    : course.CourseName,

                // =================================================
                // A. Present Intake
                // =================================================

                IntakeDuring_2025_26 =
                    entity.IntakeDuring202526 ?? "",

                // =================================================
                // B. Previous Details
                // =================================================

                Intake_slab =
                    entity.IntakeSlab ?? "",

                Typeofpermission =
                    entity.Typeofpermission ?? "",

                yearofLOP =
                    entity.YearofLop,

                // =================================================
                // C. Permission / Recognition
                // =================================================

                SanctionedIntake_Permission =
                    entity.SanctionedIntakePermission ?? "",

                DateOfLOP_Renewal_GOIMCI =
                    entity.DateOfLoprenewalGoimci ?? "",

                DateOfLOP_Renewal_DCIKSDC =
                    entity.DateOfLoprenewalDciksdc ?? "",

                dateofrecognition =
                    entity.Dateofrecognition ?? "",

                // =================================================
                // D. EC & FC
                // =================================================

                yearofObtainingECandFC =
                    entity.YearofObtainingEcandFc,

                sannctionedIntake_EC_FC =
                    entity.SannctionedIntakeEcFc ?? "",

                // =================================================
                // E. Last Affiliation
                // =================================================

                YearOfLastAffiliation_RGUHS =
                    entity.YearOfLastAffiliationRguhs ?? "",

                SanctionedIntake_LastAffiliation =
                    entity.SanctionedIntakeLastAffiliation ?? "",

                // =================================================
                // F. Previous LIC Inspection
                // =================================================

                DateOfPreviousLICInspection =
                    entity.DateOfPreviousLicinspection,

                ActionTakenOnDeficiencies =
                    entity.ActionTakenOnDeficiencies ?? "",

                // =================================================
                // Files
                // =================================================

                HasGOKOrder =
                    !string.IsNullOrEmpty(entity.GokorderPath),

                HasLastAffiliationFile =
                    !string.IsNullOrEmpty(
                        entity.LastAffiliationRguhsfilePath),

                HasPreviousNotificationFile =
                    !string.IsNullOrEmpty(
                        entity.PreviousNotificationFilesPath)
            };
        }
    }
}