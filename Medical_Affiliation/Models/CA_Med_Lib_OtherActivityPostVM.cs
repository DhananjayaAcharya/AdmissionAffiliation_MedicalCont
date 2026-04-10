namespace Medical_Affiliation.Models
{
    public class CA_Med_Lib_OtherActivityPostVM
    {
        public int? Id { get; set; } // null = add, value = edit
        public int ActivityId { get; set; }
        public string DepartmentCode { get; set; } = "";
        public IFormFile ActivityPdf { get; set; } = null!;
    }
}
