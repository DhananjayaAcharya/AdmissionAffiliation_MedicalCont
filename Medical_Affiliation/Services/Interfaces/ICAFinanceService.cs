using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface ICAFinanceService
    {
        Task<FinanceViewModel> GetFinanceDetails();
        //Task<AccountAndFew>
    }

}
