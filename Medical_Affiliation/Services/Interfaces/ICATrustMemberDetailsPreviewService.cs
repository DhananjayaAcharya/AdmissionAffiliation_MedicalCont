using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface ICATrustMemberDetailsPreviewService
    {
        Task<CATrustMemberDetailsDisplayVM?> GetTrustMemberDetailsAsync();
    }
}