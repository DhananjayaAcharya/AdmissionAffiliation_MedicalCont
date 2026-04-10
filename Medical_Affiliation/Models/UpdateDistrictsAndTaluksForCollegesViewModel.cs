using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class UpdateDistrictsAndTaluksForCollegesViewModel
    {
        public string SelectedDistrict { get; set; }
        public string SelectedTaluk { get; set; }
        public string SelectedFaculty { get; set; }
        public List<string> SelectedCollegeIds { get; set; }

        public List<TalukMaster> TalukMasters { get; set; }
        public List<DistrictMaster> DistrictMasters { get; set; }
        public List<Faculty> Faculties { get; set; }
        public List<SelectListItem> affiliationCollegeMasters { get; set; }
    }


}
