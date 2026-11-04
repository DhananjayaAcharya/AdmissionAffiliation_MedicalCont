using Microsoft.AspNetCore.Mvc;

namespace Medical_Affiliation.Controllers
{
    public class DentalAdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
