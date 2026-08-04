using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAVehiclePreviewService : CAPreviewServiceBase, ICAVehiclePreviewService
    {
        public CAVehiclePreviewService( ApplicationDbContext context,  IUserContext userContext) : base(context, userContext)
        {
        }

        public async Task<VehicleDetailPreviewViewModel> GetVehiclePreviewAsync()
        {
            var vm = new VehicleDetailPreviewViewModel();

            vm.Vehicles = await (
                from vehicle in _context.CaVehicleDetails

                join vehicleFor in _context.CaMstVdVehicleFors
                    on new
                    {
                        vehicle.VehicleForCode,
                        FacultyCode = Convert.ToInt32(vehicle.FacultyCode)
                    }
                    equals new
                    {
                        vehicleFor.VehicleForCode,
                        vehicleFor.FacultyCode
                    }

                where vehicle.CollegeCode == CollegeCode
                   && vehicle.FacultyCode == FacultyCode.ToString()

                orderby vehicle.VehicleRegNo

                select new VehiclePreviewVM
                {
                    VehicleRegNo = vehicle.VehicleRegNo,

                    VehicleFor = vehicleFor.VehicleForName,

                    SeatingCapacity = vehicle.SeatingCapacity,

                    ValidityDate = vehicle.ValidityDate.HasValue
                        ? vehicle.ValidityDate.Value.ToDateTime(TimeOnly.MinValue)
                        : (DateTime?)null,

                    RcBookAvailable = vehicle.RcBookStatus == "Y",

                    InsuranceAvailable = vehicle.InsuranceStatus == "Y",

                    DrivingLicenseAvailable = vehicle.DrivingLicenseStatus == "Y"
                })
                .ToListAsync();

            return vm;
        }
    }
}
