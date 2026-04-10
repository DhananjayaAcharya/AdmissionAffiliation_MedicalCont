namespace Medical_Affiliation.Services.Interfaces
{
    public interface IUserContext
    {
        string CollegeCode { get; }

        string CourseLevel {  get; }
        int FacultyId { get; }
        string SeatSlabId { get; }
        int TypeOfAffiliation {  get; }
    }
}
