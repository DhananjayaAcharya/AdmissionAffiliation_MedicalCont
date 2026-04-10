using Microsoft.AspNetCore.Mvc;
using Medical_Affiliation.DATA;

namespace Medical_Affiliation.Controllers
{
    public class RepositoryAdminReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RepositoryAdminReportController(ApplicationDbContext context) 
        {
            _context = context;
        }


    }
}
