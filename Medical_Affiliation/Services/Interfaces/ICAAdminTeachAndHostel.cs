using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface ICAAdminTeachAndHostel
    {
        Task<AdminTeachAndHostelDisplayVM> GetAdminTeachAndHostelDetails();
        //Task<AccountAndFew>
    }

}
