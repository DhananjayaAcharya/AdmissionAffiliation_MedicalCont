using Medical_Affiliation.Models;
using Medical_Affiliation.Services;
using Microsoft.AspNetCore.Mvc;
using static Medical_Affiliation.Services.LicTadaService;

namespace Medical_Affiliation.Controllers
{
    public class LicTadaController : Controller
    {
        private readonly LicTadaService _svc;
        public LicTadaController(LicTadaService svc) => _svc = svc;

        // ══════════════════════════════════════════ ROLE HELPERS

        private string? GetRole()
        {
            var role = HttpContext.Session.GetString("FinanceRole");
            if (!string.IsNullOrEmpty(role)) return role;

            if (HttpContext.Session.GetString("IsFinance") == "true") return "Finance";
            if (HttpContext.Session.GetString("IsCaseWorker") == "true") return "CaseWorker";
            if (HttpContext.Session.GetString("IsAO") == "true") return "AO";
            if (HttpContext.Session.GetString("IsCashier") == "true") return "Cashier";
            return null;
        }

        private string GetUserName()
            => HttpContext.Session.GetString("UserName")
            ?? User.Identity?.Name
            ?? "User";

        private string? GetFacultyCode()
            => HttpContext.Session.GetString("FacultyCode")
            ?? HttpContext.Session.GetString("FacultyId");

        private string GetDesignationLabel()
        {
            return HttpContext.Session.GetString("FinanceDesignation") switch
            {
                "FO" => "Finance Officer",
                "CW" => "Case Worker",
                "AO" => "Account Officer",
                "CS" => "Cashier",
                _ => GetRole() ?? "User"
            };
        }

        private bool IsRole(string required)
            => GetRole() == required;

        private IActionResult RedirectByRole()
        {
            return GetRole() switch
            {
                "Finance" => RedirectToAction("FO"),
                "CaseWorker" => RedirectToAction("CW"),
                "AO" => RedirectToAction("AO"),
                "Cashier" => RedirectToAction("Cashier"),
                _ => RedirectToAction("AdminLogin", "Admin")
            };
        }

        private void SetViewBagRole()
        {
            ViewBag.Role = GetRole();
            ViewBag.UserName = GetUserName();
            ViewBag.DesignationLabel = GetDesignationLabel();
            ViewBag.Faculty = GetFacultyCode();
        }

        // ══════════════════════════════════════════ SHARED DROPDOWNS

        private LicTadaDashboardVM BuildVM() => new LicTadaDashboardVM
        {
            AcademicYears = _svc.GetYears(),
            Faculties = _svc.GetFaculties(),
            Colleges = _svc.GetColleges()
        };

        // ══════════════════════════════════════════ ENTRY POINT

        public IActionResult Dashboard()
        {
            if (GetRole() == null)
                return RedirectToAction("AdminLogin", "Admin");
            return RedirectByRole();
        }

        // ══════════════════════════════════════════ FINANCE OFFICER

        public IActionResult FO()
        {
            if (!IsRole("Finance")) return RedirectByRole();
            SetViewBagRole();
            return View(BuildVM());
        }

        [HttpPost]
        public IActionResult FOFreshGrid(string? year, string? faculty, string? college)
        {
            if (!IsRole("Finance"))
                return Content("<div class='lt-empty'>⚠️ Not authorized.</div>");
            return PartialView("_GridFOFresh",
                _svc.GetFOFreshRecords(year, faculty, college));
        }

        [HttpPost]
        public IActionResult FOFinalGrid(string? year, string? faculty, string? college)
        {
            if (!IsRole("Finance"))
                return Content("<div class='lt-empty'>⚠️ Not authorized.</div>");
            return PartialView("_GridFOFinal",
                _svc.GetFOFinalApprovalRecords(year, faculty, college));
        }

        [HttpPost]
        public IActionResult ForwardToCW(int id, string remarks)
        {
            if (!IsRole("Finance"))
                return Json(new { success = false, message = "Not authorized." });
            bool ok = _svc.ForwardToCW(id, remarks ?? "");
            return Json(new { success = ok });
        }

        [HttpPost]
        public IActionResult FO2Action(int id, string action, string remarks)
        {
            if (!IsRole("Finance"))
                return Json(new { success = false, message = "Not authorized." });
            bool ok = action == "Approved"
                ? _svc.FO2Approve(id, remarks ?? "")
                : _svc.FO2Reject(id, remarks ?? "");
            return Json(new { success = ok });
        }

        // ══════════════════════════════════════════ CASE WORKER

        public IActionResult CW()
        {
            if (!IsRole("CaseWorker")) return RedirectByRole();
            SetViewBagRole();
            return View(BuildVM());
        }

        [HttpPost]
        public IActionResult CWGrid(string? year, string? faculty, string? college)
        {
            if (!IsRole("CaseWorker"))
                return Content("<div class='lt-empty'>⚠️ Not authorized.</div>");
            return PartialView("_GridCW",
                _svc.GetCWRecords(year, faculty, college));
        }

        // ── SaveKm — shared by CW and AO ────────────────────────────────────
        [HttpPost]
        public IActionResult SaveKm(int id, decimal km, decimal rkm)
        {
            if (!IsRole("CaseWorker") && !IsRole("AO") && !IsRole("Finance"))
                return Json(new { success = false, message = "Not authorized." });
            bool ok = _svc.SaveKm(id, km, rkm);
            return Json(new { success = ok });
        }

        [HttpPost]
        public IActionResult CWVerify(int id, string remarks)
        {
            if (!IsRole("CaseWorker"))
                return Json(new { success = false, message = "Not authorized." });
            bool ok = _svc.CWVerify(id, remarks ?? "");
            return Json(new { success = ok });
        }

        [HttpPost]
        public IActionResult CWReject(int id, string remarks)
        {
            if (!IsRole("CaseWorker"))
                return Json(new { success = false, message = "Not authorized." });
            bool ok = _svc.CWReject(id, remarks ?? "");
            return Json(new { success = ok });
        }

        // ══════════════════════════════════════════ AO / SP

        public IActionResult AO()
        {
            if (!IsRole("AO")) return RedirectByRole();
            SetViewBagRole();
            return View(BuildVM());
        }

        [HttpPost]
        public IActionResult AOGrid(string? year, string? faculty, string? college)
        {
            if (!IsRole("AO"))
                return Content("<div class='lt-empty'>⚠️ Not authorized.</div>");
            return PartialView("_GridAO",
                _svc.GetAORecords(year, faculty, college));
        }

        // ── AOAction — single endpoint for Approve + Reject ─────────────────
        [HttpPost]
        public IActionResult AOAction(int id, string action, string remarks)
        {
            if (!IsRole("AO"))
                return Json(new { success = false, message = "Not authorized." });
            bool ok = action == "Approved"
                ? _svc.AOApprove(id, remarks ?? "")
                : _svc.AOReject(id, remarks ?? "");
            return Json(new { success = ok });
        }

        // ══════════════════════════════════════════ CASHIER

        public IActionResult Cashier()
        {
            if (!IsRole("Cashier")) return RedirectByRole();
            SetViewBagRole();
            return View(BuildVM());
        }

        [HttpPost]
        public IActionResult CashierGrid(string? year, string? faculty, string? college)
        {
            if (!IsRole("Cashier"))
                return Content("<div class='lt-empty'>⚠️ Not authorized.</div>");
            return PartialView("_GridCashier",
                _svc.GetCashierRecords(year, faculty, college));
        }

        [HttpPost]
        public IActionResult CashierPay(int id, string remarks)
        {
            if (!IsRole("Cashier"))
                return Json(new { success = false, message = "Not authorized." });
            bool ok = _svc.CashierPay(id, remarks ?? "");
            return Json(new { success = ok });
        }

        // ══════════════════════════════════════════ SHARED: Member modal

        [HttpGet]
        public IActionResult Member(string phone, string name, string mode = "view")
        {
            if (GetRole() == null)
                return Content("<div class='lt-empty'>Session expired. Please log in again.</div>");

            var m = _svc.GetMember(phone);
            if (m.Id == 0)
                return Content("<div class='lt-empty'>Member not found.</div>");

            ViewBag.MemberName = name ?? "";
            ViewBag.Mode = mode;
            return PartialView("_TadaMember", m);
        }
        [HttpGet]
        public IActionResult BankDetails(string phone)
        {
            var role = GetRole();
            Console.WriteLine($"[BankDetails] phone='{phone}' role='{role}'");

            if (role == null)
                return Json(new { found = false, error = "no_session" });

            if (string.IsNullOrWhiteSpace(phone))
                return Json(new { found = false, error = "no_phone" });

            var b = _svc.GetBankDetails(phone);

            Console.WriteLine($"[BankDetails] found={b.Found} acct='{b.AccountNumber}'");

            return Json(new
            {
                found = b.Found,
                accountHolderName = b.AccountHolderName,
                accountNumber = b.AccountNumber,
                ifscCode = b.IfscCode,
                bankName = b.BankName,
                branchName = b.BranchName,
                panNumber = b.PanNumber,
                aadhaarNumber = b.AadhaarNumber,
                modeOfTravel = b.ModeOfTravel
            });
        }

        [HttpPost]
        public IActionResult CashierPaidGrid(string? year, string? faculty, string? college)
        {
            if (!IsRole("Cashier"))
                return Content("<div class='lt-empty'>⚠️ Not authorized.</div>");
            return PartialView("_GridCashierPaid",
                _svc.GetCashierPaidRecords(year, faculty, college));
        }

        [HttpPost]
        public IActionResult CashierPayAll(string collegeCode, string academicYear, string remarks)
        {
            if (!IsRole("Cashier"))
                return Json(new { success = false, message = "Not authorized." });
            bool ok = _svc.CashierPayAll(collegeCode, academicYear, remarks ?? "");
            return Json(new { success = ok });
        }
    }
}