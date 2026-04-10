using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
   

    public class SkillsLabViewModel
    {
        // Basic
        [Required(ErrorMessage = "Annual MBBS intake is required.")]
        [Range(100, 250, ErrorMessage = "Intake must be between 100 and 250.")]
        public int AnnualMbbsIntake { get; set; } // 100/150/200/250

        // Area
        [Required(ErrorMessage = "Available skills lab area is required.")]
        [Range(typeof(decimal), "1", "100000", ErrorMessage = "Enter a valid available area.")]
        public decimal TotalAreaAvailableSqm { get; set; }

        [Required(ErrorMessage = "Required skills lab area must be calculated.")]
        [Range(typeof(decimal), "1", "100000", ErrorMessage = "Enter a valid required area.")]
        public decimal TotalAreaRequiredSqm { get; set; } // 600 or 800

        [Required(ErrorMessage = "Area deficiency must be calculated.")]
        [Range(typeof(decimal), "0", "100000", ErrorMessage = "Enter a valid deficiency value.")]
        public decimal TotalAreaDeficiencySqm { get; set; }

        // Mandatory 6‑week training
        [Required(ErrorMessage = "Please select YES or NO.")]
        public bool? SixWeeksTrainingCompletedBeforeClinical { get; set; }

        // (a) Minimum 4 rooms for exam
        [Required(ErrorMessage = "Number of examination rooms is required.")]
        [Range(0, 100, ErrorMessage = "Enter a valid number of examination rooms.")]
        public int NumberOfExaminationRooms { get; set; }

        [Required(ErrorMessage = "Please confirm if minimum 4 examination rooms are available.")]
        public bool? HasMinFourExamRooms { get; set; }

        // (b) Demo room for small groups
        [Required(ErrorMessage = "Please specify if demo room is available.")]
        public bool? HasDemoRoomSmallGroups { get; set; }

        // (c) Debrief / review area
        [Required(ErrorMessage = "Please specify if debrief area is available.")]
        public bool? HasDebriefArea { get; set; }

        // (d) Rooms for coordinator and staff
        [Required(ErrorMessage = "Please specify if coordinator room is available.")]
        public bool? HasFacultyCoordinatorRoom { get; set; }

        [Required(ErrorMessage = "Please specify if support staff rooms are available.")]
        public bool? HasSupportStaffRoom { get; set; }

        // (e) Storage for mannequins/equipment
        [Required(ErrorMessage = "Please specify if storage for mannequins/equipment is available.")]
        public bool? HasStorageForMannequins { get; set; }

        // (f) Video recording & review facility
        [Required(ErrorMessage = "Please specify if video recording facility is available.")]
        public bool? HasVideoRecordingFacility { get; set; }

        // (g) Stations for practicing skills
        [Required(ErrorMessage = "Number of skill practice stations is required.")]
        [Range(0, 500, ErrorMessage = "Enter a valid number of skill practice stations.")]
        public int NumberOfSkillStations { get; set; }

        [Required(ErrorMessage = "Please specify if individual and group stations are available.")]
        public bool? HasGroupAndIndividualStations { get; set; }

        // (h) Trainers / mannequins as per CBME
        [Required(ErrorMessage = "Please specify if required trainers/mannequins are available.")]
        public bool? HasRequiredTrainersAndMannequins { get; set; }

        // (i) Technical officer & support staff
        [Required(ErrorMessage = "Please specify if a dedicated technical officer is posted.")]
        public bool? HasDedicatedTechnicalOfficer { get; set; }

        [Required(ErrorMessage = "Please specify if adequate support staff is available.")]
        public bool? HasAdequateSupportStaff { get; set; }

        // (j) AV / Internet / e‑learning
        [Required(ErrorMessage = "Please specify if AV facilities are available.")]
        public bool? TeachingAreasHaveAV { get; set; }

        [Required(ErrorMessage = "Please specify if internet facility is available.")]
        public bool? TeachingAreasHaveInternet { get; set; }

        [Required(ErrorMessage = "Please specify if skills lab is enabled for e‑learning.")]
        public bool? SkillsLabEnabledForELearning { get; set; }
    }


}
