using Medical_Affiliation.DATA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using static Medical_Affiliation.Controllers.NursingContinuesAffiliationController;

public class SidebarViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SidebarViewComponent(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Get faculty code from session
        var facultyCode = _httpContextAccessor.HttpContext.Session.GetString("FacultyCode");

        ViewBag.ShowButtons = false;

        // Get dropdown data from DB
        var typeOfAffiliationList = await _context.TypeOfAffiliations
            .Select(t => new SelectListItem
            {
                Value = t.TypeId.ToString(),
                Text = t.TypeDescription
            }).ToListAsync();

        var model = new SidebarViewModel
        {
            FacultyCode = facultyCode,
            TypeOfAffiliationList = typeOfAffiliationList
        };

        return View(model);
    }
}

//public class SidebarViewModel 
//{
//    public string? FacultyCode { get; set; }
//    public List<SelectListItem> TypeOfAffiliationList { get; set; } = new List<SelectListItem>();
//}

public class AffiliationViewModel
{
    public int SelectedAffiliationId { get; set; }
    public IEnumerable<SelectListItem> TypeOfAffiliations { get; set; }
}

public class SidebarViewModel : AffiliationViewModel
{
    public string? FacultyCode { get; set; }
    public List<SelectListItem> TypeOfAffiliationList { get; set; } = new List<SelectListItem>();
}
