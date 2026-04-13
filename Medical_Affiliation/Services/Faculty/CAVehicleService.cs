using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Medical_Affiliation.Services.Faculty
{
    public class CAVehicleService : ICAVehicleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CAVehicleService(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }
        public async Task<VehicleDetailListDisplayViewModel> GetVehicleDetailsAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var vehicles = await _context.CaVehicleDetails
                .Where(v => v.CollegeCode == collegeCode && v.FacultyCode == facultyCode.ToString())
                .Select(v => new CaVehicleDetailDisplayViewModel
                {
                    Id = v.Id,
                    CollegeCode = v.CollegeCode,
                    FacultyCode = v.FacultyCode,
                    RegistrationNo = v.RegistrationNo,
                    VehicleRegNo = v.VehicleRegNo,
                    VehicleForCode = v.VehicleForCode,
                    SeatingCapacity = v.SeatingCapacity,
                    ValidityDate = v.ValidityDate,
                    RcBookStatus = v.RcBookStatus,
                    InsuranceStatus = v.InsuranceStatus,
                    DrivingLicenseStatus = v.DrivingLicenseStatus
                })
                .ToListAsync();

            return new VehicleDetailListDisplayViewModel
            {
                CollegeCode = collegeCode,
                Items = vehicles
            };
        }

    }
}
