namespace Medical_Affiliation.Models
{
    public class DentalChairVm
    {
        public int CourseCode { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseLevel { get; set;  } = string.Empty;
        public int SeatSlab { get; set; }

        public int HospitalDetailsId { get; set; }
        public string SeatSlabId { get; set; } = string.Empty;

        public int ChairsRequired { get; set; }

        public int ChairsExisting { get; set; }
    }
}
