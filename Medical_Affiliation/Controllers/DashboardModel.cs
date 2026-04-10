using Medical_Affiliation.DATA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Medical_Affiliation.ViewModels;

namespace Medical_Affiliation.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string? SearchTerm { get; set; }

        [BindProperty]
        public string? SelectedTown { get; set; }

        public DashboardPageViewModel VM { get; set; } = new();

        // ── GET ───────────────────────────────────────────────
        public async Task OnGetAsync()
        {
            VM.TotalColleges = await _context.AffiliationCollegeMasters.CountAsync();

            VM.Towns = await _context.AffiliationCollegeMasters
                .Where(c => c.CollegeTown != null)
                .Select(c => c.CollegeTown!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            VM.Colleges = await _context.AffiliationCollegeMasters
                .OrderBy(c => c.CollegeName)
                .Select(c => new DashboardCollegeViewModel
                {
                    CollegeCode = c.CollegeCode,
                    CollegeName = c.CollegeName,
                    CollegeTown = c.CollegeTown,
                    FacultyCode = c.FacultyCode
                })
                .Take(1000)
                .ToListAsync();
        }

        // ── POST ──────────────────────────────────────────────
        public async Task OnPostAsync()
        {
            VM.SearchTerm = SearchTerm;
            VM.SelectedTown = SelectedTown;
            VM.TotalColleges = await _context.AffiliationCollegeMasters.CountAsync();

            VM.Towns = await _context.AffiliationCollegeMasters
                .Where(c => c.CollegeTown != null)
                .Select(c => c.CollegeTown!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            var query = _context.AffiliationCollegeMasters.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
                query = query.Where(c =>
                    c.CollegeName.Contains(SearchTerm) ||
                    c.CollegeCode.Contains(SearchTerm));

            if (!string.IsNullOrWhiteSpace(SelectedTown))
                query = query.Where(c => c.CollegeTown == SelectedTown);

            VM.Colleges = await query
                .OrderBy(c => c.CollegeName)
                .Select(c => new DashboardCollegeViewModel
                {
                    CollegeCode = c.CollegeCode,
                    CollegeName = c.CollegeName,
                    CollegeTown = c.CollegeTown,
                    FacultyCode = c.FacultyCode
                })
                .Take(1000)
                .ToListAsync();
        }


        public class DashboardCollegeViewModel
        {
            public string CollegeCode { get; set; } = string.Empty;
            public string CollegeName { get; set; } = string.Empty;
            public string? CollegeTown { get; set; }
            public string? FacultyCode { get; set; }
        }

        public class DashboardPageViewModel
        {
            public List<DashboardCollegeViewModel> Colleges { get; set; } = new();
            public int TotalColleges { get; set; }
            public string? SearchTerm { get; set; }
            public string? SelectedTown { get; set; }
            public List<string> Towns { get; set; } = new();
        }
    }
}
