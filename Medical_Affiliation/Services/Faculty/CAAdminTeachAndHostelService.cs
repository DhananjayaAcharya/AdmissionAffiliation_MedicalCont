using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAAdminTeachAndHostelService : ICAAdminTeachAndHostel
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CAAdminTeachAndHostelService(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<AdminTeachAndHostelDisplayVM> GetAdminTeachAndHostelDetails()
        {
            var facultyId = _userContext.FacultyId;
            var collegeCode = _userContext.CollegeCode;

            // ✅ Call the LIST method (not itself)
            var adminTeachingBlocks = await _context.AffAdminTeachingBlocks
                .Where(x => x.CollegeCode == collegeCode
                         && x.FacultyCode == facultyId.ToString())
                .OrderBy(x => x.Facilities)
                .Select(x => new AffAdminTeachingBlockDisplayVM
                {
                    Facilities = x.Facilities,
                    SizeSqFtAsPerNorms = x.SizeSqFtAsPerNorms,
                    IsAvailable = x.IsAvailable,
                    NoOfRooms = x.NoOfRooms,
                    SizeSqFtAvailablePerRoom = x.SizeSqFtAvailablePerRoom
                })
                .ToListAsync();

            var hostelDetails = await GetHostelDetails();
            var hostelFaciltyDetails = await GetHostelFacilities();

            return new AdminTeachAndHostelDisplayVM
            {
                AdminTeachingBlockDisplayVM = adminTeachingBlocks,
                HostelDetailsVM = hostelDetails,
                AffHostelFacilitiesVM = hostelFaciltyDetails

            };
        }

        public async Task<List<HostelDetailDisplayVM>> GetHostelDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyId = _userContext.FacultyId;

            var items = await _context.AffHostelDetails
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyId.ToString())
                .OrderBy(x => x.HostelType)
                .Select(x => new HostelDetailDisplayVM
                {
                    HostelType = x.HostelType,
                    BuiltUpAreaSqFt = x.BuiltUpAreaSqFt,
                    HasSeparateHostel = x.HasSeparateHostel,
                    SeparateProvisionMaleFemale = x.SeparateProvisionMaleFemale,
                    TotalFemaleStudents = x.TotalFemaleStudents,
                    TotalFemaleRooms = x.TotalFemaleRooms,
                    TotalMaleStudents = x.TotalMaleStudents,
                    TotalMaleRooms = x.TotalMaleRooms,
                    HasPossessionProof = x.PossessionProofPath != null
                })
                .ToListAsync();

            return items;
        }

        public async Task<List<AffHostelFacilityDisplayVM>> GetHostelFacilities()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyId = _userContext.FacultyId;

            var data = await (
                from d in _context.AffHostelFacilityDetails
                join m in _context.MstHostelFacilities
                    on d.FacilityId equals m.HostelFacilityId
                where d.CollegeCode == collegeCode
                      && d.FacultyCode == facultyId.ToString()
                orderby m.HostelFacilityName
                select new AffHostelFacilityDisplayVM
                {
                    FacilityName = m.HostelFacilityName ?? d.FacilityName,
                    IsAvailable = d.IsAvailable
                }
            ).ToListAsync();

            return data;
        }

    }
}
