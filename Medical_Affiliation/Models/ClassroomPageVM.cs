namespace Medical_Affiliation.Models
{
    public class ClassroomPageVM
    {
        public string FacultyCode { get; set; }

        
        public List<ClassroomDetailVM> ClassroomDetails { get; set; }

        public List<LaboratoryDetailVM> LaboratoryDetails { get; set; }
    }


    public class ClassroomDetailVM
    {
       
        public string CourseCode { get; set; }
        public string CourseName { get; set; }

        public int RequiredClassrooms { get; set; }
        public int RequiredSize { get; set; }

        // User input
        public int? AvailableClassrooms { get; set; }
        public int? AvailableSize { get; set; }

        // CALCULATED FIELDS
        public int DeficiencyClassrooms =>
            RequiredClassrooms - (AvailableClassrooms ?? 0);

        public int RequiredTotalSize =>
            RequiredClassrooms * RequiredSize;

        public int DeficiencySize =>
            RequiredTotalSize - (AvailableSize ?? 0);
    }


    public class LaboratoryDetailVM
    {
        public int LabID { get; set; }
        public string LaboratoryName { get; set; }

        public int RequiredLabs { get; set; }
        public int RequiredSize { get; set; }

        public int Intakeid { get; set; }

        public int? AvailableSize { get; set; }
        public int? AvailableMannequin { get; set; }

        public int Deficiency =>
        (RequiredSize - (AvailableSize ?? 0)) > 0
         ? RequiredSize - (AvailableSize ?? 0)
         : 0;
    }
}
