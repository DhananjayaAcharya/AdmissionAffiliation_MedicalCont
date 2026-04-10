using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class DepartmentOfficesMeuViewModel : IValidatableObject
    {
        public string? CourseLevel { get; set; }

        [Required]
        public bool? HasHodRoomWithOfficeAndRecords { get; set; }

        [Required]
        public bool? HasRoomsForFacultyAndResidents { get; set; }

        [Required]
        public bool? FacultyRoomsHaveCommunicationComputerInternet { get; set; }

        [Required]
        public bool? HasRoomsForNonTeachingStaff { get; set; }

        [Required]
        public bool? HasMedicalEducationUnit { get; set; }

        public decimal? MedicalEducationUnitAreaSqm { get; set; }

        public bool? MedicalEducationUnitHasAudioVisual { get; set; }
        public bool? MedicalEducationUnitHasInternet { get; set; }

        public string? MeuCoordinatorName { get; set; }
        public string? MeuCoordinatorDesignationDepartment { get; set; }
        public string? MeuCoordinatorPhone { get; set; }

        [EmailAddress]
        public string? MeuCoordinatorEmail { get; set; }

        public string? MeuActivitiesLastAcademicYear { get; set; }

        public IFormFile? MeuMembersListFile { get; set; }
        public bool HasMeuMembersListFile { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HasMedicalEducationUnit == true)
            {
                if (!MedicalEducationUnitAreaSqm.HasValue)
                    yield return new ValidationResult("MEU Area is required.",
                        new[] { nameof(MedicalEducationUnitAreaSqm) });

                else if (MedicalEducationUnitAreaSqm < 150)
                    yield return new ValidationResult("Minimum area should be 150 Sq.m.",
                        new[] { nameof(MedicalEducationUnitAreaSqm) });

                if (MedicalEducationUnitHasAudioVisual == null)
                    yield return new ValidationResult("Select Audio-Visual facilities.",
                        new[] { nameof(MedicalEducationUnitHasAudioVisual) });

                if (MedicalEducationUnitHasInternet == null)
                    yield return new ValidationResult("Select Internet facilities.",
                        new[] { nameof(MedicalEducationUnitHasInternet) });

                if (string.IsNullOrWhiteSpace(MeuCoordinatorName))
                    yield return new ValidationResult("Coordinator name is required.",
                        new[] { nameof(MeuCoordinatorName) });

                if (string.IsNullOrWhiteSpace(MeuCoordinatorDesignationDepartment))
                    yield return new ValidationResult("Designation & department required.",
                        new[] { nameof(MeuCoordinatorDesignationDepartment) });

                if (string.IsNullOrWhiteSpace(MeuCoordinatorPhone))
                    yield return new ValidationResult("Phone is required.",
                        new[] { nameof(MeuCoordinatorPhone) });

                if (string.IsNullOrWhiteSpace(MeuCoordinatorEmail))
                    yield return new ValidationResult("Email is required.",
                        new[] { nameof(MeuCoordinatorEmail) });

                if (MeuMembersListFile == null && !HasMeuMembersListFile)
                    yield return new ValidationResult("Upload MEU Members List PDF.",
                        new[] { nameof(MeuMembersListFile) });

                if (string.IsNullOrWhiteSpace(MeuActivitiesLastAcademicYear))
                    yield return new ValidationResult("Activities are required.",
                        new[] { nameof(MeuActivitiesLastAcademicYear) });
            }
        }
    }


}
