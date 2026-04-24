namespace Medical_Affiliation.Models
{
    //public class CA_Med_LibraryMainVM
    //{
    //    public LibraryGeneralVM General { get; set; } = new();
    //    public List<LibraryItemsVM> Items { get; set; } = new();
    //    public LibraryBuildingVM Building { get; set; } = new();
    //    public List<LibTechnicalProcessVM> TechnicalProcess { get; set; } = new();
    //    public List<LibraryEquipmentsVM> Equipments { get; set; } = new();
    //    public LibraryFinanceVM Finance { get; set; } = new();
    //}

    public class CA_Med_LibraryMainVM
    {
        public CA_Medi_LibraryGeneralVM General { get; set; } = new();
        public List<CA_Medi_LibraryItemsVM> Items { get; set; } = new();
        public CA_Medi_LibraryBuildingVM Building { get; set; } = new();
        public List<CA_Medi_TechnicalProcessVM> TechnicalProcess { get; set; } = new();
        public List<CA_Medi_LibraryEquipmentsVM> Equipments { get; set; } = new();
        public CA_Medi_LibraryFinanceVM Finance { get; set; } = new();
        public List<string> ExistingCourseLevels
        { get; set; } = new();

        public string? BinderyValue
        { get; set; }
    }
}
