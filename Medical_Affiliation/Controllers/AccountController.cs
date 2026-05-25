using Medical_Affiliation.DATA;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;

        }
        private void ReloadLoginView(string errorMessage)
        {
            ViewBag.Faculties = _context.Faculties.ToList();
            ViewBag.Error = errorMessage;

            var newCaptcha = new Random().Next(1000, 9999).ToString();
            HttpContext.Session.SetString("Captcha", newCaptcha);
            ViewBag.Captcha = newCaptcha;
        }

    }
}
