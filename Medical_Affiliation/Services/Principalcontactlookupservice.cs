using Medical_Affiliation.DATA;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services
{
    public interface IPrincipalContactLookupService
    {
        Task<(string? PrincipalMobileNumber, string? NameOfInstitution)> GetPrincipalContactAsync(string collegeCode);
    }

    public class PrincipalContactLookupService : IPrincipalContactLookupService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PrincipalContactLookupService> _logger;

        public PrincipalContactLookupService(
            ApplicationDbContext context,
            ILogger<PrincipalContactLookupService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(string? PrincipalMobileNumber, string? NameOfInstitution)> GetPrincipalContactAsync(string collegeCode)
        {
            try
            {
                Console.WriteLine($"[PrincipalLookup] Starting lookup for college: {collegeCode}");
                var institution = await _context.AffInstitutionsDetails
                    .AsNoTracking()
                    .Where(x => x.CollegeCode != null && x.CollegeCode.Trim() == collegeCode.Trim())
                    .Select(x => new
                    {
                        x.PrincipalMobileNumber,
                        x.DeanMobileNumber,
                        x.MobileNumber,
                        x.NameOfInstitution
                    })
                    .FirstOrDefaultAsync();

                if (institution != null)
                {
                    var mobile = new[]
                    {
                        institution.PrincipalMobileNumber,
                        institution.DeanMobileNumber,
                        institution.MobileNumber
                    }.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
                    var name = institution.NameOfInstitution;

                    Console.WriteLine($"[PrincipalLookup] Query results for {collegeCode}:");
                    Console.WriteLine($"[PrincipalLookup]   PrincipalMobileNumber: '{institution.PrincipalMobileNumber}'");
                    Console.WriteLine($"[PrincipalLookup]   DeanMobileNumber: '{institution.DeanMobileNumber}'");
                    Console.WriteLine($"[PrincipalLookup]   MobileNumber: '{institution.MobileNumber}'");
                    Console.WriteLine($"[PrincipalLookup]   COALESCE Result: '{mobile}'");
                    Console.WriteLine($"[PrincipalLookup]   NameOfInstitution: '{name}'");
                    
                    if (string.IsNullOrWhiteSpace(mobile))
                    {
                        Console.WriteLine($"[PrincipalLookup] ⚠️  WARNING: No mobile number found in any column for college: {collegeCode}");
                        _logger?.LogWarning($"Principal mobile number is empty for college: {collegeCode}");
                    }
                    else
                    {
                        Console.WriteLine($"[PrincipalLookup] ✅ FOUND: Mobile number for {collegeCode}: {mobile}");
                        _logger?.LogInformation($"Found mobile number for {collegeCode}: {mobile}");
                    }

                    return (mobile, name);
                }

                Console.WriteLine($"[PrincipalLookup] ❌ ERROR: No institution record found for college code: {collegeCode}");
                _logger?.LogWarning($"No institution found for college code: {collegeCode}");
                return (null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PrincipalLookup] ❌ EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"[PrincipalLookup] Stack trace: {ex.StackTrace}");
                _logger?.LogError($"Error fetching principal contact for {collegeCode}: {ex.Message}");
                return (null, null);
            }
        }
    }
}