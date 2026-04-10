using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class NodalOfficerAdminViewModel
    {
        public int Id { get; set; }

        // Officer Details
        public string FacultyCode { get; set; }
        public string FacultyName { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string NodalOfficerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailId { get; set; }

        public bool IsEditable { get; set; }

        // Initiatives Handling
        public List<InitiativeItem> AvailableInitiatives { get; set; } = new();
        public List<string> SelectedInitiativeIds { get; set; } = new();
    }

    public class InitiativeItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public bool CanEdit { get; set; }   // <-- flag for edit

    }

    public class NodalOfficerReportData
    {
        public List<SelectListItem>? FacultyList { get; set; }
        public List<SelectListItem>? CollegeList { get; set; }
        public string SelectedFaculty { get; set; }
        public string SelectedCollege { get; set; }
        public List<NodalOfficerReportRow>? NodalOfficerReport { get; set; }
        public List<SelectListItem>? InitiativeList { get; set; }

        public string SelectedInitiative { get; set; }


    }
    public class NodalOfficerReportRow
    {
        public int SlNo { get; set; }
        public string CollegeName { get; set; }
        public string NodalOfficerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailId { get; set; }
        public string Initiative { get; set; }
        public string FacultyName { get; set; }
    }

    public class NodalOfficerReportViewModel
    {
        public int SlNo { get; set; }
        public string CollegeName { get; set; }
        public string Initiative { get; set; }
        public string NodalOfficerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailId { get; set; }

        // Dropdown filters
        public string SelectedFacultyCode { get; set; }
        public string SelectedCollegeCode { get; set; }
        public string SelectedFaculty { get; set; }   // <-- Add this
        public string SelectedCollege { get; set; }  // <-- Add this
        public List<SelectListItem> Faculties { get; set; }
        public List<SelectListItem> Colleges { get; set; }

        public List<NodalOfficerReportRow> NodalOfficerReport { get; set; }
    }
    public class NodalOfficerReportRow1
    {
        public string CollegeCode { get; set; }
        public int SlNo { get; set; }
        public string CollegeName { get; set; }
        public string NodalOfficerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailId { get; set; }
        public string Initiative { get; set; }
        public string FacultyName { get; set; }
    }

    public class NodalOfficerReportData1
    {
        public SelectList? FacultyList { get; set; }
        public SelectList? CollegeList { get; set; }
        public string SelectedFaculty { get; set; }
        public string SelectedCollege { get; set; }
        public List<NodalOfficerReportRow1>? NodalOfficerReport { get; set; }

    }
}
