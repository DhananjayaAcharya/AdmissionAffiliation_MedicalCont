using System.Collections.Generic;

namespace Medical_Affiliation.Models
{
    public class CA_Med_Lib_DetailsAndItemsVM
    {
        public CA_Medi_LibraryGeneralVM General { get; set; } = new();
        public List<CA_Medi_LibraryItemsVM> Items { get; set; } = new();
    }
}