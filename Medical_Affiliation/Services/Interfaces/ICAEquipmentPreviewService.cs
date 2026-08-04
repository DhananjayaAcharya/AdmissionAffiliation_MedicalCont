using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface ICAEquipmentPreviewService
    {
        Task<EquipmentPreviewViewModel> GetEquipmentPreviewAsync();
    }
}
