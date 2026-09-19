using System.Data;
using GeoPhotoModule.Models;
using Microsoft.Data.SqlClient;
namespace GeoPhotoModule.Services
{
    public interface IGeoPhotoRepository
    {
        Task<List<GeoPhotoCategoryVm>> GetPageAsync(string collegeCode, string facultyCode);
        Task<string?> GetSlotPathAsync(string collegeCode, string facultyCode, int categoryId, byte slotNo);
        Task<(long PhotoId, string ImagePath)> SaveAsync(GeoPhotoSaveDto dto);
        Task<GeoPhotoFileInfo?> GetPhotoAsync(long photoId);
        Task<string?> DeleteAsync(long photoId, string collegeCode, string facultyCode);
    }

    public class GeoPhotoRepository : IGeoPhotoRepository
    {
        private readonly string _connectionString;

        public GeoPhotoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ------------------------------------------------------------------
        // Page data : all categories + all slots (filled or empty)
        // ------------------------------------------------------------------
        public async Task<List<GeoPhotoCategoryVm>> GetPageAsync(string collegeCode, string facultyCode)
        {
            var result = new List<GeoPhotoCategoryVm>();
            var lookup = new Dictionary<int, GeoPhotoCategoryVm>();

            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("dbo.usp_GeoPhoto_GetByCollege", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@CollegeCode", SqlDbType.VarChar, 20) { Value = collegeCode });
            cmd.Parameters.Add(new SqlParameter("@FacultyCode", SqlDbType.VarChar, 20) { Value = facultyCode });

            await con.OpenAsync();
            await using var rd = await cmd.ExecuteReaderAsync();

            int oCatId = rd.GetOrdinal("CategoryId");
            int oCatCode = rd.GetOrdinal("CategoryCode");
            int oCatName = rd.GetOrdinal("CategoryName");
            int oReq = rd.GetOrdinal("RequiredImages");
            int oOrder = rd.GetOrdinal("DisplayOrder");
            int oSlot = rd.GetOrdinal("ImageSlotNo");
            int oPhotoId = rd.GetOrdinal("PhotoId");
            int oPath = rd.GetOrdinal("ImagePath");
            int oLat = rd.GetOrdinal("Latitude");
            int oLon = rd.GetOrdinal("Longitude");
            int oAcc = rd.GetOrdinal("AccuracyMeters");
            int oCap = rd.GetOrdinal("CapturedOn");
            int oUp = rd.GetOrdinal("UploadedOn");

            while (await rd.ReadAsync())
            {
                int catId = rd.GetInt32(oCatId);

                if (!lookup.TryGetValue(catId, out var cat))
                {
                    cat = new GeoPhotoCategoryVm
                    {
                        CategoryId = catId,
                        CategoryCode = rd.GetString(oCatCode),
                        CategoryName = rd.GetString(oCatName),
                        RequiredImages = rd.GetByte(oReq),
                        DisplayOrder = rd.GetByte(oOrder)
                    };
                    lookup[catId] = cat;
                    result.Add(cat);
                }

                cat.Slots.Add(new GeoPhotoSlotVm
                {
                    CategoryId = catId,
                    SlotNo = rd.GetByte(oSlot),
                    PhotoId = rd.IsDBNull(oPhotoId) ? null : rd.GetInt64(oPhotoId),
                    ImagePath = rd.IsDBNull(oPath) ? null : rd.GetString(oPath),
                    Latitude = rd.IsDBNull(oLat) ? null : rd.GetDecimal(oLat),
                    Longitude = rd.IsDBNull(oLon) ? null : rd.GetDecimal(oLon),
                    AccuracyMeters = rd.IsDBNull(oAcc) ? null : rd.GetDecimal(oAcc),
                    CapturedOn = rd.IsDBNull(oCap) ? null : rd.GetDateTime(oCap),
                    UploadedOn = rd.IsDBNull(oUp) ? null : rd.GetDateTime(oUp)
                });
            }

            return result;
        }

        // ------------------------------------------------------------------
        // Existing file path for a slot (used to remove the old file on replace)
        // ------------------------------------------------------------------
        public async Task<string?> GetSlotPathAsync(string collegeCode, string facultyCode, int categoryId, byte slotNo)
        {
            const string sql = @"SELECT ImagePath
                                 FROM   dbo.GeoPhotoUpload
                                 WHERE  CollegeCode = @c AND FacultyCode = @f
                                   AND  CategoryId  = @cat AND ImageSlotNo = @slot;";

            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@c", SqlDbType.VarChar, 20) { Value = collegeCode });
            cmd.Parameters.Add(new SqlParameter("@f", SqlDbType.VarChar, 20) { Value = facultyCode });
            cmd.Parameters.Add(new SqlParameter("@cat", SqlDbType.Int) { Value = categoryId });
            cmd.Parameters.Add(new SqlParameter("@slot", SqlDbType.TinyInt) { Value = slotNo });

            await con.OpenAsync();
            var o = await cmd.ExecuteScalarAsync();
            return o == null || o == DBNull.Value ? null : (string)o;
        }

        // ------------------------------------------------------------------
        // Save (insert or replace) one image via usp_GeoPhoto_Save
        // ------------------------------------------------------------------
        public async Task<(long PhotoId, string ImagePath)> SaveAsync(GeoPhotoSaveDto d)
        {
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("dbo.usp_GeoPhoto_Save", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@CollegeCode", SqlDbType.VarChar, 20) { Value = d.CollegeCode });
            cmd.Parameters.Add(new SqlParameter("@FacultyCode", SqlDbType.VarChar, 20) { Value = d.FacultyCode });
            cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = d.CategoryId });
            cmd.Parameters.Add(new SqlParameter("@ImageSlotNo", SqlDbType.TinyInt) { Value = d.ImageSlotNo });
            cmd.Parameters.Add(new SqlParameter("@ImagePath", SqlDbType.VarChar, 500) { Value = d.ImagePath });
            cmd.Parameters.Add(Str("@OriginalFileName", d.OriginalFileName, 255));
            cmd.Parameters.Add(Str("@ContentType", d.ContentType, 100));
            cmd.Parameters.Add(new SqlParameter("@FileSizeKB", SqlDbType.Int) { Value = (object?)d.FileSizeKB ?? DBNull.Value });
            cmd.Parameters.Add(Dec("@Latitude", d.Latitude, 9, 6));
            cmd.Parameters.Add(Dec("@Longitude", d.Longitude, 9, 6));
            cmd.Parameters.Add(Dec("@AccuracyMeters", d.AccuracyMeters, 8, 2));
            cmd.Parameters.Add(new SqlParameter("@CapturedOn", SqlDbType.DateTime) { Value = (object?)d.CapturedOn ?? DBNull.Value });
            cmd.Parameters.Add(Str("@DeviceInfo", d.DeviceInfo, 200));
            cmd.Parameters.Add(Str("@UploadedBy", d.UploadedBy, 100));
            cmd.Parameters.Add(Str("@UploadedIP", d.UploadedIP, 45));

            await con.OpenAsync();
            await using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
                return (rd.GetInt64(0), rd.GetString(1));

            throw new InvalidOperationException("usp_GeoPhoto_Save returned no row.");
        }

        // ------------------------------------------------------------------
        // Lookup one photo (for secure streaming)
        // ------------------------------------------------------------------
        public async Task<GeoPhotoFileInfo?> GetPhotoAsync(long photoId)
        {
            const string sql = @"SELECT PhotoId, CollegeCode, FacultyCode, ImagePath, ContentType
                                 FROM   dbo.GeoPhotoUpload
                                 WHERE  PhotoId = @id AND IsActive = 1;";

            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt) { Value = photoId });

            await con.OpenAsync();
            await using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return null;

            return new GeoPhotoFileInfo
            {
                PhotoId = rd.GetInt64(0),
                CollegeCode = rd.GetString(1),
                FacultyCode = rd.GetString(2),
                ImagePath = rd.GetString(3),
                ContentType = rd.IsDBNull(4) ? null : rd.GetString(4)
            };
        }

        // ------------------------------------------------------------------
        // Soft delete. Returns the file path if a row was deleted, else null.
        // Only the owning college + faculty can delete.
        // ------------------------------------------------------------------
        public async Task<string?> DeleteAsync(long photoId, string collegeCode, string facultyCode)
        {
            const string sql = @"UPDATE dbo.GeoPhotoUpload
                                 SET    IsActive = 0, ModifiedOn = GETDATE()
                                 OUTPUT inserted.ImagePath
                                 WHERE  PhotoId = @id AND CollegeCode = @c AND FacultyCode = @f AND IsActive = 1;";

            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.BigInt) { Value = photoId });
            cmd.Parameters.Add(new SqlParameter("@c", SqlDbType.VarChar, 20) { Value = collegeCode });
            cmd.Parameters.Add(new SqlParameter("@f", SqlDbType.VarChar, 20) { Value = facultyCode });

            await con.OpenAsync();
            var o = await cmd.ExecuteScalarAsync();
            return o == null || o == DBNull.Value ? null : (string)o;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------
        private static SqlParameter Str(string name, string? value, int size) =>
            new SqlParameter(name, SqlDbType.VarChar, size) { Value = (object?)value ?? DBNull.Value };

        private static SqlParameter Dec(string name, decimal? value, byte precision, byte scale) =>
            new SqlParameter(name, SqlDbType.Decimal)
            {
                Precision = precision,
                Scale = scale,
                Value = (object?)value ?? DBNull.Value
            };
    }
}