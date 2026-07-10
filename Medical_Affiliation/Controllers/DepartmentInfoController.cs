using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class DepartmentInfoController : BaseController
    {
        protected readonly ApplicationDbContext _context;
        public DepartmentInfoController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IActionResult> DepartmentInformation()
        {
            var facultyCode = Convert.ToInt32(FacultyCode);

            var vm = new DepartmentPageVm();

            vm.DepartmentSelection.Departments = await _context.DepartmentMasters
                .Where(x => x.FacultyCode == facultyCode)
                .OrderBy(x => x.DepartmentName)
                .Select(x => new SelectListItem
                {
                    Value = x.DepartmentCode,
                    Text = x.DepartmentName
                })
                .ToListAsync();

            return View(vm);
        }
    }
}
