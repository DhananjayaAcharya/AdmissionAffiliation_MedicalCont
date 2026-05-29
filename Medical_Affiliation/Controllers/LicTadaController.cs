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

        // ══════════════════════════════════════════ BUILDVM OVERLOADS

        // Single-role: CW, AO, Cashier
        private LicTadaDashboardVM BuildVM(string role) => new LicTadaDashboardVM
        {
            AcademicYears = _svc.GetYears(role),
            Faculties = _svc.GetFaculties(role),
            Colleges = _svc.GetColleges(role),
            FinalAcademicYears = new(),
            FinalFaculties = new(),
            FinalColleges = new(),
            PaidAcademicYears = _svc.GetYears("Cashier_Paid"),
            PaidFaculties = _svc.GetFaculties("Cashier_Paid"),
            PaidColleges = _svc.GetColleges("Cashier_Paid")
        };

        // Dual-role: FO only (Fresh tab + Final tab)
        private LicTadaDashboardVM BuildVM(string role1, string role2) => new LicTadaDashboardVM
        {
            // Fresh tab
            AcademicYears = _svc.GetYears(role1),
            Faculties = _svc.GetFaculties(role1),
            Colleges = _svc.GetColleges(role1),
            // Final tab
            FinalAcademicYears = _svc.GetYears(role2),
            FinalFaculties = _svc.GetFaculties(role2),
            FinalColleges = _svc.GetColleges(role2),
            // Paid tab
            PaidAcademicYears = _svc.GetYears("Cashier_Paid"),
            PaidFaculties = _svc.GetFaculties("Cashier_Paid"),
            PaidColleges = _svc.GetColleges("Cashier_Paid")
        };

        // IEqualityComparer to deduplicate by Value
        private class DDComparer : IEqualityComparer<LicTadaDDItem>
        {
            public bool Equals(LicTadaDDItem? x, LicTadaDDItem? y)
                => string.Equals(x?.Value, y?.Value, StringComparison.OrdinalIgnoreCase);
            public int GetHashCode(LicTadaDDItem obj)
                => (obj.Value ?? "").ToLowerInvariant().GetHashCode();
        }

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
            return View(BuildVM("FO_Fresh", "FO_Final"));
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
        public IActionResult ForwardToCW(int id, string remarks, string routedTo)
        {
            if (!IsRole("Finance"))
                return Json(new { success = false, message = "Not authorized." });
            bool ok = _svc.ForwardToCW(id, remarks ?? "", routedTo ?? "");
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

        [HttpPost]
        public IActionResult FO2Approve(int id, string remarks)
        {
            if (!IsRole("Finance"))
                return Json(new { success = false, message = "Not authorized." });
            bool ok = _svc.FO2Approve(id, remarks ?? "");
            return Json(new { success = ok });
        }

        [HttpPost]
        public IActionResult FO2Reject(int id, string remarks)
        {
            if (!IsRole("Finance"))
                return Json(new { success = false, message = "Not authorized." });
            bool ok = _svc.FO2Reject(id, remarks ?? "");
            return Json(new { success = ok });
        }

        // ══════════════════════════════════════════ CASE WORKER

        public IActionResult CW()
        {
            if (!IsRole("CaseWorker")) return RedirectByRole();
            SetViewBagRole();
            return View(BuildVM("CaseWorker"));
        }

        [HttpPost]
        public IActionResult CWGrid(string? year, string? faculty, string? college)
        {
            if (!IsRole("CaseWorker"))
                return Content("<div class='lt-empty'>⚠️ Not authorized.</div>");
            return PartialView("_GridCW",
                _svc.GetCWRecords(year, faculty, college));
        }

        [HttpPost]
        public IActionResult SaveKm(int id, decimal km, decimal rkm,
                              decimal? da = null,
                              decimal? lca = null,
                              decimal? airRoad = null,
                              decimal? airFare = null)
        {
            if (!IsRole("CaseWorker") && !IsRole("AO") && !IsRole("Finance"))
                return Json(new { success = false, message = "Not authorized." });

            var editedBy = HttpContext.Session.GetString("UserName") ?? "";
            var editorDesignation = HttpContext.Session.GetString("Designation") ?? "";
            string stage = IsRole("CaseWorker") ? "CaseWorker"
                                  : IsRole("AO") ? "AO"
                                  : "FO2";

            bool ok = _svc.SaveKm(id, km, rkm, da, lca, airRoad, airFare,
                                  editedBy, editorDesignation, stage);
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
            return View(BuildVM("AO"));
        }

        [HttpPost]
        public IActionResult AOGrid(string? year, string? faculty, string? college)
        {
            if (!IsRole("AO"))
                return Content("<div class='lt-empty'>⚠️ Not authorized.</div>");
            return PartialView("_GridAO",
                _svc.GetAORecords(year, faculty, college));
        }

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
            return View(BuildVM("Cashier"));
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

        [HttpPost]
        public IActionResult CashierPaidGrid(string? year, string? faculty, string? college)
        {
            // AFTER
            if (!IsRole("Cashier"))
                return Content("<div class='lt-empty'>⚠️ Not authorized.</div>");
            return PartialView("_GridCashierPaid",
                _svc.GetCashierPaidRecords(year, faculty, college));
        }

        [HttpPost]
        public IActionResult AllPaidGrid(string? year, string? faculty, string? college)
        {
            // AFTER
            if (!IsRole("Finance") && !IsRole("CaseWorker") && !IsRole("AO"))
                return Content("<div class='lt-empty'>⚠️ Not authorized.</div>");
            return PartialView("_AllPaidGrid",
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

        [HttpPost]
        public IActionResult CashierPayMultiple(string ids, string remarks)
        {
            if (!IsRole("Cashier"))
                return Json(new { success = false, message = "Not authorized." });

            if (string.IsNullOrWhiteSpace(ids))
                return Json(new { success = false, message = "No IDs provided." });

            var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => int.TryParse(s.Trim(), out int n) ? n : 0)
                            .Where(n => n > 0)
                            .ToList();

            if (!idList.Any())
                return Json(new { success = false, message = "No valid IDs." });

            int paid = 0, failed = 0;
            foreach (var id in idList)
            {
                bool ok = _svc.CashierPay(id, remarks ?? "");
                if (ok) paid++; else failed++;
            }
            return Json(new { success = paid > 0, paid, failed });
        }

        // ══════════════════════════════════════════ SHARED

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
            if (role == null)
                return Json(new { found = false, error = "no_session" });
            if (string.IsNullOrWhiteSpace(phone))
                return Json(new { found = false, error = "no_phone" });

            var b = _svc.GetBankDetails(phone);
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

        [HttpGet]
        public IActionResult GetEditLog(int id)
        {
            var logs = _svc.GetEditLog(id);
            return Json(logs);
        }
    }
}
