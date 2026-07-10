namespace Medical_Affiliation.Models
{
    public class DepartmentPageVm
    {
        public DepartmentSelectionVm DepartmentSelection { get; set; } = new();
        public AffiliationDepartmentInformation DepartmentInformation { get; set; } = new();
        public List<AffiliationDepartmentUnitDetail> UnitDetails { get; set; } = new();
        public List<AffiliationDepartmentIcudetail> ICUDetails { get; set; } = new();
    }
}
