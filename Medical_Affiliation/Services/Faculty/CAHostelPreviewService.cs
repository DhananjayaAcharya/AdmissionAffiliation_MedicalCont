using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAHostelPreviewService : ICAHostelPreviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CAHostelPreviewService(
            ApplicationDbContext context,
            IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<AffHostelPreviewViewModel> GetHostelPreviewAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId.ToString();

            var hostel = await _context.AffHostelDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

            if (hostel == null)
                return new AffHostelPreviewViewModel();

            return new AffHostelPreviewViewModel
            {
                HostelType = hostel.HostelType,
                BuiltUpAreaSqFt = hostel.BuiltUpAreaSqFt,
                HasSeparateHostel = hostel.HasSeparateHostel,
                SeparateProvisionMaleFemale = hostel.SeparateProvisionMaleFemale,

                TotalFemaleStudents = hostel.TotalFemaleStudents,
                TotalFemaleRooms = hostel.TotalFemaleRooms,
                TotalMaleStudents = hostel.TotalMaleStudents,
                TotalMaleRooms = hostel.TotalMaleRooms,

                PossessionProofPath = hostel.PossessionProofPath,

                CommonRoomMen = hostel.CommonRoomMen,
                CommonRoomWomen = hostel.CommonRoomWomen,

                AnyOtherFacility = hostel.AnyOtherFacility,
                HostelFacilityDetails = hostel.HostelFacilityDetails,

                HostelMenCount = hostel.HostelMenCount,
                HostelWomenCount = hostel.HostelWomenCount,

                OwnOrRented = hostel.OwnOrRented?.Trim(),
                SpacePerStudent = hostel.SpacePerStudent,

                SleepingFurniture = hostel.SleepingFurniture,
                SanitaryBathing = hostel.SanitaryBathing,
                DiningHall = hostel.DiningHall,
                HostelCommonRoom = hostel.HostelCommonRoom,
                VisitorsRoom = hostel.VisitorsRoom,
                KitchenPantry = hostel.KitchenPantry,
                WardenOffice = hostel.WardenOffice,
                ReceptionCounter = hostel.ReceptionCounter,
                GamesRecreation = hostel.GamesRecreation,
                MedicalFacilities = hostel.MedicalFacilities,

                MenHostelAreaSqFt = hostel.MenHostelAreaSqFt,
                WomenHostelAreaSqFt = hostel.WomenHostelAreaSqFt
            };
        }
    }
}