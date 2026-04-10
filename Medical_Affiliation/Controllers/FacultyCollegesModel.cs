using Medical_Affiliation.DATA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Medical_Affiliation.DATA;
using Medical_Affiliation.ViewModels;

namespace Medical_Affiliation.Pages
{
    public class FacultyCollegesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FacultyCollegesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Bound from form POST ──────────────────────────────
        [BindProperty]
        public string? SelectedFaculty { get; set; }

        [BindProperty]
        public string? SearchTerm { get; set; }

        // ── Page data ─────────────────────────────────────────
        public FacultyCollegePageViewModel VM { get; set; } = new();

        // ── GET ───────────────────────────────────────────────
        public async Task<IActionResult> OnGetAsync(string? faculty, string? search)
        {
            SelectedFaculty = faculty;
            SearchTerm = search;
            await LoadDataAsync();
            return Page();
        }

        // ── POST (filter form submit) ─────────────────────────
        public async Task<IActionResult> OnPostAsync()
        {
            await LoadDataAsync();
            return Page();
        }

        // ── Shared loader ─────────────────────────────────────
        private async Task LoadDataAsync()
        {
            VM.SelectedFaculty = SelectedFaculty;
            VM.SearchTerm = SearchTerm;
            VM.Faculties = await GetFacultiesAsync();
            VM.Colleges = await GetCollegesAsync();
        }

        // ── Fetch Faculties via DbContext ─────────────────────
        private async Task<List<FacultyViewModel>> GetFacultiesAsync()
        {
            return await _context.Faculties
                .OrderBy(f => f.FacultyName)
                .Select(f => new FacultyViewModel
                {
                    FacultyId = f.FacultyId,
                    FacultyName = f.FacultyName,
                    //EMS_FacultyId = f.EMS_FacultyId,
                    //Faculty_Abbre = f.Faculty_Abbre,
                    //Status = f.Status
                })
                .Take(1000)
                .ToListAsync();
        }

        // ── Fetch Colleges via DbContext (filtered + searched) ─
        private async Task<List<CollegeViewModel>> GetCollegesAsync()
        {
            var query = _context.AffiliationCollegeMasters.AsQueryable();

            // Filter by selected faculty
            if (!string.IsNullOrWhiteSpace(SelectedFaculty))
                query = query.Where(c => c.FacultyCode == SelectedFaculty);

            // Filter by search term
            if (!string.IsNullOrWhiteSpace(SearchTerm))
                query = query.Where(c =>
                    c.CollegeName.Contains(SearchTerm) ||
                    c.CollegeCode.Contains(SearchTerm) ||
                    (c.CollegeTown != null && c.CollegeTown.Contains(SearchTerm)));

            return await query
                .OrderBy(c => c.CollegeName)
                .Select(c => new CollegeViewModel
                {
                    CollegeCode = c.CollegeCode,
                    CollegeName = c.CollegeName,
                    CollegeTown = c.CollegeTown,
                    FacultyCode = c.FacultyCode,
                    PrincipalNameDeclared = c.PrincipalNameDeclared,
                    ShowNodalOfficerDetails = c.ShowNodalOfficerDetails,
                    ShowIntakeDetails = c.ShowIntakeDetails,
                    ShowRepositoryDetails = c.ShowRepositoryDetails,
                    PrincipalMobileNumber = c.PrincipalMobileNumber,
                    //DistrictId = c.DistrictId,
                    //TalukId = c.TalukId
                })
                .Take(1000)
                .ToListAsync();
        }




        public class FacultyViewModel
        {
            public int FacultyId { get; set; }
            public string FacultyName { get; set; } = string.Empty;
            public string? EMS_FacultyId { get; set; }
            public string? Faculty_Abbre { get; set; }
            public bool Status { get; set; }
        }

        public class CollegeViewModel
        {
            public string CollegeCode { get; set; } = string.Empty;
            public string CollegeName { get; set; } = string.Empty;
            public string? CollegeTown { get; set; }
            public string? FacultyCode { get; set; }
            public bool? IsDeclared { get; set; }
            public bool? ChangedPassword { get; set; }
            public string? PrincipalNameDeclared { get; set; }
            public bool? ShowNodalOfficerDetails { get; set; }
            public bool? ShowIntakeDetails { get; set; }
            public bool? ShowRepositoryDetails { get; set; }
            public string? PrincipalMobileNumber { get; set; }
            public int? DistrictId { get; set; }
            public int? TalukId { get; set; }
        }

        public class FacultyCollegePageViewModel
        {
            public List<FacultyViewModel> Faculties { get; set; } = new();
            public List<CollegeViewModel> Colleges { get; set; } = new();
            public string? SelectedFaculty { get; set; }
            public string? SearchTerm { get; set; }
        }
    }
}
