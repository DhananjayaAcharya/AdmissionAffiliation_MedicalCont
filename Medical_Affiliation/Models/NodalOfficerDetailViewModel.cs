using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{

    public class NodalOfficersList
    {
        public List<NodalOfficerDetailViewModel> NodalOfficerDetail { get; set; }
        public NodalOfficerDetailViewModel NewOfficer { get; set; } // for modal
        public List<InitiativeMaster> MasterInitiatives { get; set; }

        public List<InitiativeMaster> FreeInitiatives { get; set; }
        public NodalOfficerEditViewModel? EditModal { get; set; }

    }




    public class NodalOfficerDetailViewModel
    {
        public string? FacultyCode { get; set; }

        public int Id {  get; set; }

        public string? FacultyName { get; set; }

        public string? CollegeName { get; set; }

        public string? CollegeCode { get; set; }

        public string? NodalOfficerName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? EmailId { get; set; }

        public string DeclarationAccepted { get; set; }
        public List<SelectListItem> FacultyList {  get; set; }
        public List<SelectListItem> CollegeList { get; set; }
        public List<InitiativeCheckboxViewModel> InitiativeList { get; set; }
    }

    public class InitiativeCheckboxViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public bool IsDisabled { get; set; }

        public string DebugInfo { get; set; }

        public bool IsEditEnabled { get; set; }
    }

    public class UpdatePassword
    {
        public string CollegeCode { get; set; }
        public string UpdatedPassword { get; set; }
    }
}
