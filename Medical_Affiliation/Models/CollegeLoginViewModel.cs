using Microsoft.AspNetCore.Mvc.Rendering;

public class CollegeLoginViewModel
{
    public string CollegeId { get; set; }
    public int FacultyId { get; set; }
    public string Password { get; set; }
    public string CaptchaCode { get; set; }
    public bool ShowNodalOfficer {  get; set; }
    public bool ShowIntakeDetails { get; set; }
    public bool ShowRepository { get; set; }

    // For Dropdowns
    public List<SelectListItem> Colleges { get; set; }
    public List<SelectListItem> Faculties { get; set; }
}
public class AdminLoginViewModel
{
    public string UserName { get; set; }
    public string Password { get; set; }
    //public string CaptchaString { get; set; }
    //public string CaptchaInput { get; set; }
    public IEnumerable<SelectListItem>? Faculties { get; set; }
    public IEnumerable<SelectListItem>? Colleges { get; set; }
    //[Required]
    //public string FacultyId { get; set; }

    //[Required]
    //public string CollegeId { get; set; }
}


public class DashboardViewModel
{
    public string CollegeName { get; set; }
    public string FacultyName { get; set; }
}

