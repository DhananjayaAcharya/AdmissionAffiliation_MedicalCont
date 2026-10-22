using Medical_Affiliation.DATA;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Interfaces
{
    public static class CourseLevelHelper
    {
        public static readonly string[] Order = { "UG", "PG", "SS" };

        public static string Normalize(string? level)
        {
            var l = level?.Trim().ToUpperInvariant() ?? string.Empty;

            if (l.Contains("UG") || l.Contains("UNDERGRADUATE")) return "UG";
            if (l.Contains("SS") || l.Contains("SUPERSPECIAL")) return "SS";
            if (l.Contains("PG") || l.Contains("POSTGRADUATE")) return "PG";

            return l;
        }

        // Course levels (UG / PG / SS) a college has in Mst_MedicalCollegeCourseIntake
        public static async Task<List<string>> GetMedicalIntakeLevelsAsync(
            ApplicationDbContext ctx, string? collegeCode)
        {
            var raw = await ctx.MstMedicalCollegeCourseIntakes
                .AsNoTracking()
                .Where(i => i.CollCode == collegeCode && i.UgPg != null)
                .Select(i => i.UgPg!)
                .Distinct()
                .ToListAsync();

            return raw
                .Select(Normalize)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Distinct()
                .OrderBy(l =>
                {
                    var idx = Array.IndexOf(Order, l);
                    return idx >= 0 ? idx : int.MaxValue;
                })
                .ToList();
        }
    }
}
