using Medical_Affiliation.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Medical_Affiliation.Services
{
    public class LicTadaService
    {
        private readonly string _conn;

        public LicTadaService(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("DefaultConnection missing.");
        }

        // ══ SAFE HELPERS ══════════════════════════════════════════════════════

        private static string S(SqlDataReader r, string col)
        {
            try { int i = r.GetOrdinal(col); return r.IsDBNull(i) ? "" : r.GetValue(i)?.ToString() ?? ""; }
            catch { return ""; }
        }
        private static int N(SqlDataReader r, string col)
        {
            try { int i = r.GetOrdinal(col); return r.IsDBNull(i) ? 0 : Convert.ToInt32(r.GetValue(i)); }
            catch { return 0; }
        }
        private static decimal M(SqlDataReader r, string col)
        {
            try { int i = r.GetOrdinal(col); return r.IsDBNull(i) ? 0m : Convert.ToDecimal(r.GetValue(i)); }
            catch { return 0m; }
        }
        private static bool B(SqlDataReader r, string col)
        {
            try
            {
                int i = r.GetOrdinal(col); if (r.IsDBNull(i)) return false;
                var v = r.GetValue(i);
                if (v is bool bv) return bv;
                if (v is byte byv) return byv != 0;
                return Convert.ToInt32(v) != 0;
            }
            catch { return false; }
        }
        private static DateTime? DT(SqlDataReader r, string col)
        {
            try { int i = r.GetOrdinal(col); return r.IsDBNull(i) ? (DateTime?)null : Convert.ToDateTime(r.GetValue(i)); }
            catch { return null; }
        }

        // ══ PRIVATE RAW ROW ════════════════════════════════════════════════════

        private class RawMemberRow
        {
            public int Id { get; set; }
            public string CollegeCode { get; set; } = "";
            public string CollegeName { get; set; } = "";
            public string CollegeTown { get; set; } = "";
            public string AcademicYear { get; set; } = "";
            public string TypeOfMembers { get; set; } = "";
            public string MemberName { get; set; } = "";
            public string MobileNo { get; set; } = "";
            public decimal TotalClaimAmount { get; set; }
            public string CaseWorkerRemarks { get; set; } = "";
            public string FO_Level2_Status { get; set; } = "";
            public string Cashier_Status { get; set; } = "";
            public string AccountHolderName { get; set; } = "";
            public string AccountNumber { get; set; } = "";
            public string IFSCCode { get; set; } = "";
            public string BankName { get; set; } = "";
            public string BranchName { get; set; } = "";
            public string PANNumber { get; set; } = "";
            public string AadhaarNumber { get; set; } = "";
            public string Email { get; set; } = "";
            public string Address { get; set; } = "";
            public string ModeOfTravel { get; set; } = "";
            public DateOnly? InspectionDate { get; set; } = null;
            public decimal? Old_TotalClaimAmount { get; set; } = null;

        }

        private List<RawMemberRow> ExecRawMembers(string sql, string? year, string? faculty, string? college)
        {
            var list = new List<RawMemberRow>();
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Year", string.IsNullOrEmpty(year) ? (object)DBNull.Value : year);
            cmd.Parameters.AddWithValue("@Faculty", string.IsNullOrEmpty(faculty) ? (object)DBNull.Value : faculty);
            cmd.Parameters.AddWithValue("@College", string.IsNullOrEmpty(college) ? (object)DBNull.Value : college);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
                list.Add(new RawMemberRow
                {
                    Id = N(dr, "Id"),
                    CollegeCode = S(dr, "CollegeCode"),
                    CollegeName = S(dr, "CollegeName"),
                    CollegeTown = S(dr, "CollegeTown"),
                    AcademicYear = S(dr, "AcademicYear"),
                    TypeOfMembers = S(dr, "TypeOfMembers"),
                    MemberName = S(dr, "memberName"),
                    MobileNo = S(dr, "MobileNo"),
                    TotalClaimAmount = M(dr, "TotalClaimAmount"),
                    CaseWorkerRemarks = S(dr, "CaseWorkerRemarks"),
                    FO_Level2_Status = S(dr, "FO_Level2_Status"),
                    Cashier_Status = S(dr, "Cashier_Status"),
                    AccountHolderName = S(dr, "AccountHolderName"),
                    AccountNumber = S(dr, "AccountNumber"),
                    IFSCCode = S(dr, "IFSCCode"),
                    BankName = S(dr, "BankName"),
                    BranchName = S(dr, "BranchName"),
                    PANNumber = S(dr, "PANNumber"),
                    AadhaarNumber = S(dr, "AadhaarNumber"),
                    Email = S(dr, "Email"),
                    Address = S(dr, "Address"),
                    ModeOfTravel = S(dr, "ModeOfTravel"),
                    InspectionDate = dr["InspectionDate"] == DBNull.Value

    ? (DateOnly?)null
    : DateOnly.FromDateTime(Convert.ToDateTime(dr["InspectionDate"])),
                    Old_TotalClaimAmount = M(dr, "Old_TotalClaimAmount"),

                });
            return list;
        }

        private static List<LicTadaGridRow> GroupToGrid(List<RawMemberRow> raw)
        {
            return raw
                .GroupBy(x => new { x.CollegeCode, x.AcademicYear })
                .Select(g =>
                {
                    var senate = g.FirstOrDefault(x => x.TypeOfMembers == "Senate Members");
                    var ac = g.FirstOrDefault(x => x.TypeOfMembers == "Academic Council");
                    var expert = g.FirstOrDefault(x => x.TypeOfMembers == "Subject Expertise");
                    var first = g.First();

                    return new LicTadaGridRow
                    {
                        MemberId = first.Id,
                        CollegeCode = first.CollegeCode,
                        CollegeName = first.CollegeName,
                        CollegeTown = first.CollegeTown,
                        AcademicYear = first.AcademicYear,
                        TotalClaimAmount = g.Sum(x => x.TotalClaimAmount),
                        SenetMemberAmount = senate?.TotalClaimAmount ?? 0m,
                        AcMemberAmount = ac?.TotalClaimAmount ?? 0m,
                        SubjectExpertiseAmount = expert?.TotalClaimAmount ?? 0m,
                        SenetMember = senate?.MemberName ?? "",
                        SenetMemberPhNo = senate?.MobileNo ?? "",
                        SenetMemberId = senate?.Id ?? 0,
                        AcMember = ac?.MemberName ?? "",
                        AcMemberPhNo = ac?.MobileNo ?? "",
                        AcMemberId = ac?.Id ?? 0,
                        SubjectExpertise = expert?.MemberName ?? "",
                        SubjectExpertisePhNo = expert?.MobileNo ?? "",
                        SubjectExpertiseId = expert?.Id ?? 0,
                        CaseWorkerRemarks = first.CaseWorkerRemarks,
                        FO_Level2_Status = first.FO_Level2_Status,
                        Cashier_Status = first.Cashier_Status,
                        AccountHolderName = first.AccountHolderName,
                        AccountNumber = first.AccountNumber,
                        IFSCCode = first.IFSCCode,
                        BankName = first.BankName,
                        BranchName = first.BranchName,
                        PANNumber = first.PANNumber,
                        AadhaarNumber = first.AadhaarNumber,
                        Email = first.Email,
                        Address = first.Address,
                        ModeOfTravel = first.ModeOfTravel,
                        InspectionDate = first.InspectionDate,
                        SenetMemberOldAmount = senate?.Old_TotalClaimAmount ?? 0m,
                        AcMemberOldAmount = ac?.Old_TotalClaimAmount ?? 0m,
                        SubjectExpertiseOldAmount = expert?.Old_TotalClaimAmount ?? 0m,


                    };
                })
                .OrderBy(x => x.CollegeName)
                .ToList();
        }

        // ══ DROPDOWNS ══════════════════════════════════════════════════════════

        //public List<LicTadaDDItem> GetYears()
        //{
        //    var list = new List<LicTadaDDItem>();
        //    using var con = new SqlConnection(_conn);
        //    using var cmd = new SqlCommand(@"
        //        SELECT AcademicYear
        //        FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
        //        GROUP BY AcademicYear ORDER BY AcademicYear DESC", con);
        //    con.Open();
        //    using var dr = cmd.ExecuteReader();
        //    while (dr.Read())
        //    {
        //        var y = S(dr, "AcademicYear");
        //        list.Add(new LicTadaDDItem { Value = y, Text = y });
        //    }
        //    return list;
        //}

        //public List<LicTadaDDItem> GetFaculties()
        //{
        //    var list = new List<LicTadaDDItem>();
        //    using var con = new SqlConnection(_conn);
        //    using var cmd = new SqlCommand(@"
        //        SELECT f.FacultyId, f.FacultyName
        //        FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] a
        //        INNER JOIN dbo.Faculty f ON f.FacultyId = a.FacultyCode
        //        GROUP BY f.FacultyId, f.FacultyName ORDER BY f.FacultyId", con);
        //    con.Open();
        //    using var dr = cmd.ExecuteReader();
        //    while (dr.Read())
        //        list.Add(new LicTadaDDItem { Value = S(dr, "FacultyId"), Text = S(dr, "FacultyName") });
        //    return list;
        //}

        //public List<LicTadaDDItem> GetColleges()
        //{
        //    var list = new List<LicTadaDDItem>();
        //    using var con = new SqlConnection(_conn);
        //    using var cmd = new SqlCommand(@"
        //        SELECT ac.CollegeCode, ac.CollegeName, ac.CollegeTown
        //        FROM [Admission_Affiliation].[dbo].[LICClaimDetails] id
        //        INNER JOIN dbo.Affiliation_College_Master ac ON ac.CollegeCode = id.Collegecode
        //        INNER JOIN [Admission_Affiliation].[dbo].[LICCollegeApproval] cl ON cl.CollegeCode = id.Collegecode
        //        GROUP BY ac.CollegeCode, ac.CollegeName, ac.CollegeTown
        //        ORDER BY ac.CollegeName", con);
        //    con.Open();
        //    using var dr = cmd.ExecuteReader();
        //    while (dr.Read())
        //        list.Add(new LicTadaDDItem
        //        {
        //            Value = S(dr, "CollegeCode"),
        //            Text = S(dr, "CollegeName") + " — " + S(dr, "CollegeTown")
        //        });
        //    return list;
        //}


        // ══ DROPDOWNS — role-filtered ══════════════════════════════════════════════

        public List<LicTadaDDItem> GetYears(string? role = null)
        {
            var list = new List<LicTadaDDItem>();
            var where = BuildRoleWhereClause(role);
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand($@"
SELECT DISTINCT ca.AcademicYear
FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca
WHERE {where}
ORDER BY ca.AcademicYear DESC", con);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var y = dr["AcademicYear"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(y))
                    list.Add(new LicTadaDDItem { Value = y, Text = y });
            }
            return list;
        }

        public List<LicTadaDDItem> GetFaculties(string? role = null)
        {
            var list = new List<LicTadaDDItem>();
            var where = BuildRoleWhereClause(role);
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand($@"
SELECT DISTINCT f.FacultyId, f.FacultyName
FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca
INNER JOIN dbo.Faculty f ON f.FacultyId = ca.FacultyCode
WHERE {where}
ORDER BY f.FacultyName", con);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
                list.Add(new LicTadaDDItem
                {
                    Value = dr["FacultyId"]?.ToString() ?? "",
                    Text = dr["FacultyName"]?.ToString() ?? ""
                });
            return list;
        }
        public List<LicTadaDDItem> GetColleges(string? role = null)
        {
            var list = new List<LicTadaDDItem>();
            var where = BuildRoleWhereClause(role);
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand($@"
SELECT DISTINCT ac.CollegeCode AS CollegeCode, ac.CollegeName, ac.CollegeTown
FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca
INNER JOIN dbo.Affiliation_College_Master ac
    ON ac.CollegeCode = ca.CollegeCode
WHERE {where}
ORDER BY ac.CollegeName", con);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
                list.Add(new LicTadaDDItem
                {
                    Value = dr["CollegeCode"]?.ToString() ?? "",
                    Text = (dr["CollegeName"]?.ToString() ?? "")
                          + " — "
                          + (dr["CollegeTown"]?.ToString() ?? "")
                });
            return list;
        }

        public List<LicTadaDDItem> GetForwardRoutingUsers()
        {
            var list = new List<LicTadaDDItem>();
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
                SELECT Id,
                       ISNULL(UserName,'') AS UserName,
                       ISNULL(FinanceDesignation,'') AS FinanceDesignation
                FROM [Admission_Affiliation].[dbo].[TblRguhsFacultyUser]
                WHERE IsFinance = 1
                  AND FinanceDesignation IN ('SO','AO','AS')
                ORDER BY FinanceDesignation, UserName", con);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var id = N(dr, "Id");
                var user = S(dr, "UserName");
                var desig = S(dr, "FinanceDesignation");
                if (id <= 0 || string.IsNullOrWhiteSpace(user)) continue;
                list.Add(new LicTadaDDItem
                {
                    Value = id.ToString(),
                    Text = $"{desig} - {user}"
                });
            }
            return list;
        }
        /// <summary>
        /// Returns the WHERE clause fragment that matches exactly
        /// the records visible in each role's grid query.
        /// </summary>
        private static string BuildRoleWhereClause(string? role) => role switch
        {
            "FO_Fresh" =>
                @"ca.DR_ApprovalStatus IS NOT NULL
          AND ca.DR_ApprovalStatus <> ''
          AND (ca.FO_Level1_ApprovedStatus IS NULL OR ca.FO_Level1_ApprovedStatus = '')",

            "FO_Final" =>
                @"ca.CurrentStage = 'FO_FINAL'
          AND (ca.FO_Level2_ApprovedStatus IS NULL OR ca.FO_Level2_ApprovedStatus = '')",

            "CaseWorker" =>
                @"ca.CurrentStage = 'CW'",

            "AO" =>
                @"ca.CurrentStage IN ('SO_ROUTING','SO_VERIFY')",

            "Cashier" =>
                @"ca.FO_Level2_ApprovedStatus = 'Approved'
          AND (ca.Cashier_Update IS NULL OR ca.Cashier_Update = '')",

            "Cashier_Paid" =>
                @"ca.FO_Level2_ApprovedStatus = 'Approved'
          AND ca.Cashier_Update = 'Paid'",

            _ => "1 = 0"
        };

        // ══ GRID QUERIES ═══════════════════════════════════════════════════════

        public List<LicTadaGridRow> GetFOFreshRecords(string? year, string? faculty, string? college)
        {
            const string sql = @"
                SELECT ca.Id,
                    ac.CollegeCode, ac.CollegeName, ac.CollegeTown,
                    ca.AcademicYear, ca.TypeOfMembers, ca.memberName, ca.MobileNo,
                    ca.TotalClaimAmount,
                    '' AS CaseWorkerRemarks, '' AS FO_Level2_Status, '' AS Cashier_Status,
                    '' AS AccountHolderName, '' AS AccountNumber,    '' AS IFSCCode,
                    '' AS BankName,          '' AS BranchName,       '' AS PANNumber,
                    '' AS AadhaarNumber,     '' AS Email,            '' AS Address, '' AS ModeOfTravel,
                       ca.InspectionDate
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca
                INNER JOIN dbo.Affiliation_College_Master ac ON ac.CollegeCode = ca.CollegeCode
                WHERE ca.DR_ApprovalStatus IS NOT NULL
                  AND ca.DR_ApprovalStatus <> ''
                  AND (ca.FO_Level1_ApprovedStatus IS NULL OR ca.FO_Level1_ApprovedStatus = '')
                AND (@Year    IS NULL OR ca.AcademicYear = @Year)
                AND (@Faculty IS NULL OR ca.FacultyCode  = @Faculty)
                AND (@College IS NULL OR ca.CollegeCode  = @College)
                ORDER BY ac.CollegeName";
            return GroupToGrid(ExecRawMembers(sql, year, faculty, college));
        }

        public List<LicTadaGridRow> GetFOFinalApprovalRecords(string? year, string? faculty, string? college)
        {
            const string sql = @"
                SELECT ca.Id,
                    ac.CollegeCode, ac.CollegeName, ac.CollegeTown,
                    ca.AcademicYear, ca.TypeOfMembers, ca.memberName, ca.MobileNo,
                    ca.TotalClaimAmount,
                    ISNULL(ca.F_CaseWorker_Approve_Remarks,'') AS CaseWorkerRemarks,
                    ISNULL(ca.FO_Level2_ApprovedStatus,'')     AS FO_Level2_Status,
                    '' AS Cashier_Status,
                    '' AS AccountHolderName, '' AS AccountNumber, '' AS IFSCCode,
                    '' AS BankName,          '' AS BranchName,    '' AS PANNumber,
                    '' AS AadhaarNumber,     '' AS Email,         '' AS Address, '' AS ModeOfTravel,
                        ca.InspectionDate
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca
                INNER JOIN dbo.Affiliation_College_Master ac ON ac.CollegeCode = ca.CollegeCode
                WHERE ca.CollegeCode IN (
                    SELECT CollegeCode FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
                    WHERE F_AO_SP_Approved_Status IN ('Approved','Rejected')
                      AND (FO_Level2_ApprovedStatus IS NULL OR FO_Level2_ApprovedStatus = '')
                )
                AND (@Year    IS NULL OR ca.AcademicYear = @Year)
                AND (@Faculty IS NULL OR ca.FacultyCode  = @Faculty)
                AND (@College IS NULL OR ca.CollegeCode  = @College)
                ORDER BY ac.CollegeName";
            return GroupToGrid(ExecRawMembers(sql, year, faculty, college));
        }

        public List<LicTadaGridRow> GetCWRecords(string? year, string? faculty, string? college)
            => GetCWRecords(year, faculty, college, 0);

        public List<LicTadaGridRow> GetCWRecords(string? year, string? faculty, string? college, int userId)
        {
            const string sql = @"
                SELECT ca.Id,
                    ac.CollegeCode, ac.CollegeName, ac.CollegeTown,
                    ca.AcademicYear, ca.TypeOfMembers, ca.memberName, ca.MobileNo,
                    ca.TotalClaimAmount,
                    ISNULL(ca.F_CaseWorker_Approve_Remarks,'') AS CaseWorkerRemarks,
                    '' AS FO_Level2_Status, '' AS Cashier_Status,
                    '' AS AccountHolderName, '' AS AccountNumber, '' AS IFSCCode,
                    '' AS BankName,          '' AS BranchName,    '' AS PANNumber,
                    '' AS AadhaarNumber,     '' AS Email,         '' AS Address, '' AS ModeOfTravel,
                   ca.InspectionDate
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca
                INNER JOIN dbo.Affiliation_College_Master ac ON ac.CollegeCode = ca.CollegeCode
                WHERE ca.CurrentStage = 'CW'
                  AND (@UserId = 0 OR ca.CurrentOwnerUserId = @UserId)
                AND (@Year    IS NULL OR ca.AcademicYear = @Year)
                AND (@Faculty IS NULL OR ca.FacultyCode  = @Faculty)
                AND (@College IS NULL OR ca.CollegeCode  = @College)
                ORDER BY ac.CollegeName";
            var list = new List<RawMemberRow>();
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Year", string.IsNullOrEmpty(year) ? (object)DBNull.Value : year);
            cmd.Parameters.AddWithValue("@Faculty", string.IsNullOrEmpty(faculty) ? (object)DBNull.Value : faculty);
            cmd.Parameters.AddWithValue("@College", string.IsNullOrEmpty(college) ? (object)DBNull.Value : college);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
                list.Add(new RawMemberRow
                {
                    Id = N(dr, "Id"),
                    CollegeCode = S(dr, "CollegeCode"),
                    CollegeName = S(dr, "CollegeName"),
                    CollegeTown = S(dr, "CollegeTown"),
                    AcademicYear = S(dr, "AcademicYear"),
                    TypeOfMembers = S(dr, "TypeOfMembers"),
                    MemberName = S(dr, "memberName"),
                    MobileNo = S(dr, "MobileNo"),
                    TotalClaimAmount = M(dr, "TotalClaimAmount"),
                    CaseWorkerRemarks = S(dr, "CaseWorkerRemarks"),
                    FO_Level2_Status = S(dr, "FO_Level2_Status"),
                    Cashier_Status = S(dr, "Cashier_Status"),
                    InspectionDate = dr["InspectionDate"] == DBNull.Value
                        ? (DateOnly?)null
                        : DateOnly.FromDateTime(Convert.ToDateTime(dr["InspectionDate"]))
                });
            return GroupToGrid(list);
        }

        public List<LicTadaGridRow> GetAORecords(string? year, string? faculty, string? college)
            => GetAORecords(year, faculty, college, 0);

        public List<LicTadaGridRow> GetAORecords(string? year, string? faculty, string? college, int userId)
        {
            const string sql = @"
                SELECT ca.Id,
                    ac.CollegeCode, ac.CollegeName, ac.CollegeTown,
                    ca.AcademicYear, ca.TypeOfMembers, ca.memberName, ca.MobileNo,
                    ca.TotalClaimAmount,
                    ISNULL(ca.F_CaseWorker_Approve_Remarks,'') AS CaseWorkerRemarks,
                    '' AS FO_Level2_Status, '' AS Cashier_Status,
                    '' AS AccountHolderName, '' AS AccountNumber, '' AS IFSCCode,
                    '' AS BankName,          '' AS BranchName,    '' AS PANNumber,
                    '' AS AadhaarNumber,     '' AS Email,         '' AS Address, '' AS ModeOfTravel,
                    ca.InspectionDate
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca
                INNER JOIN dbo.Affiliation_College_Master ac ON ac.CollegeCode = ca.CollegeCode
                WHERE ca.CurrentStage IN ('SO_ROUTING','SO_VERIFY')
                  AND (@UserId = 0 OR ca.CurrentOwnerUserId = @UserId)
                AND (@Year    IS NULL OR ca.AcademicYear = @Year)
                AND (@Faculty IS NULL OR ca.FacultyCode  = @Faculty)
                AND (@College IS NULL OR ca.CollegeCode  = @College)
                ORDER BY ac.CollegeName";
            var list = new List<RawMemberRow>();
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Year", string.IsNullOrEmpty(year) ? (object)DBNull.Value : year);
            cmd.Parameters.AddWithValue("@Faculty", string.IsNullOrEmpty(faculty) ? (object)DBNull.Value : faculty);
            cmd.Parameters.AddWithValue("@College", string.IsNullOrEmpty(college) ? (object)DBNull.Value : college);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
                list.Add(new RawMemberRow
                {
                    Id = N(dr, "Id"),
                    CollegeCode = S(dr, "CollegeCode"),
                    CollegeName = S(dr, "CollegeName"),
                    CollegeTown = S(dr, "CollegeTown"),
                    AcademicYear = S(dr, "AcademicYear"),
                    TypeOfMembers = S(dr, "TypeOfMembers"),
                    MemberName = S(dr, "memberName"),
                    MobileNo = S(dr, "MobileNo"),
                    TotalClaimAmount = M(dr, "TotalClaimAmount"),
                    CaseWorkerRemarks = S(dr, "CaseWorkerRemarks"),
                    FO_Level2_Status = S(dr, "FO_Level2_Status"),
                    Cashier_Status = S(dr, "Cashier_Status"),
                    InspectionDate = dr["InspectionDate"] == DBNull.Value
                        ? (DateOnly?)null
                        : DateOnly.FromDateTime(Convert.ToDateTime(dr["InspectionDate"]))
                });
            return GroupToGrid(list);
        }

        public List<LicTadaGridRow> GetCashierRecords(string? year, string? faculty, string? college)
        {
            const string sql = @"SELECT 
                                ca.Id,
                                ac.CollegeCode, 
                                ac.CollegeName, 
                                ac.CollegeTown,
                                ca.AcademicYear, 
                                ca.TypeOfMembers, 
                                ca.memberName, 
                                ca.MobileNo,

                                -- ✅ CURRENT VALUE
                                ca.TotalClaimAmount AS TotalClaimAmount,

                                -- ✅ OLD VALUE (first change if exists)
                                ISNULL(ed.First_Old_TotalClaimAmount, ca.TotalClaimAmount) AS Old_TotalClaimAmount,

                                '' AS CaseWorkerRemarks,
                                ISNULL(ca.FO_Level2_ApprovedStatus,'') AS FO_Level2_Status,
                                ISNULL(ca.Cashier_Update,'') AS Cashier_Status,

                                ISNULL(li.AccountHolderName,'') AS AccountHolderName,
                                ISNULL(li.AccountNumber,'')     AS AccountNumber,
                                ISNULL(li.IFSCCode,'')          AS IFSCCode,
                                ISNULL(li.BankName,'')          AS BankName,
                                ISNULL(li.BranchName,'')        AS BranchName,
                                ISNULL(li.PANNumber,'')         AS PANNumber,
                                ISNULL(li.AadhaarNumber,'')     AS AadhaarNumber,
                                ISNULL(li.Email,'')             AS Email,
                                ISNULL(li.Address,'')           AS Address,
                                ISNULL(li.ModeOfTravel,'')      AS ModeOfTravel,

                                ca.InspectionDate

                            FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca

                            INNER JOIN dbo.Affiliation_College_Master ac 
                                ON ac.CollegeCode = ca.CollegeCode

                            LEFT JOIN [Admission_Affiliation].[dbo].[LIC_Inspection] li
                                ON li.PhoneNumber = ca.MobileNo

                            OUTER APPLY 
                            (
                                SELECT TOP 1 
                                    edlog.Old_TotalClaimAmount AS First_Old_TotalClaimAmount
                                FROM [Admission_Affiliation].[dbo].[LIC_TA_DA_Edit_Log] edlog
                                WHERE edlog.MobileNo = ca.MobileNo
                                  AND edlog.ApprovalId = ca.Id
                                ORDER BY edlog.EditedAt ASC
                            ) ed
                            WHERE ca.FO_Level2_ApprovedStatus = 'Approved'
                              AND (ca.Cashier_Update IS NULL OR ca.Cashier_Update = '')
                                              AND (@Year    IS NULL OR ca.AcademicYear = @Year)
                                              AND (@Faculty IS NULL OR ca.FacultyCode  = @Faculty)
                                              AND (@College IS NULL OR ca.CollegeCode  = @College)
                                            
                            ORDER BY ac.CollegeName;";
            return GroupToGrid(ExecRawMembers(sql, year, faculty, college));
        }

        // ══ MEMBER DETAIL ═══════════════════════════════════════════════════════

        public LicTadaMember GetMember(string mobileNo)
        {
            var m = new LicTadaMember();

            // ── Query 1: All scalar fields — NO binary columns ─────────────────
            const string sql1 = @"
        SELECT TOP 1
            [Id],[FacultyCode],[CollegeCode],[TypeOfMembers],[MobileNo],[memberName],
            [FromPlace],[ToPlace],[Kilometers],
            [ReturnFromPlace],[ReturnToPlace],[ReturnKilometers],
            [TotalClaimAmount],[IsBanglore],[AirFair],[NoOfDays],
            [TravelCost],[DACost],[LCACost],[CollegeCost],[AirRoadCost],
            [Division],[IsLCA],
            [DR_ApprovalStatus],[DR_Remarks],[DRApprovedBy],[DRApprovedDate],
            [FO_Level1_ApprovedStatus],[FO_Level1_Remarks],[FO_Level1_ForwardedDate],
            [F_CaseWorker_Approve_Status],[F_CaseWorker_Approve_Remarks],[F_CaseWorker_Approved_Date],
            [F_AO_SP_Approved_Status],[F_AO_SP_Approved_Remarks],[F_AO_SP_Approved_Date],
            [FO_Level2_ApprovedStatus],[FO_Level2_ApprovedRemarks],[FO_Level2_ApprovedDate],
            [Cashier_Update],[CashierApprovedDate],[InspectionDate],
            CASE WHEN LicApprovalFile IS NOT NULL AND DATALENGTH(LicApprovalFile) > 0
                 THEN 1 ELSE 0 END AS HasLicFile
        FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
        WHERE MobileNo = @Phone";

            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(sql1, con))
            {
                cmd.CommandTimeout = 120;
                cmd.Parameters.AddWithValue("@Phone", mobileNo ?? "");
                con.Open();
                using var dr = cmd.ExecuteReader();
                if (!dr.Read()) return m;

                m.Id = N(dr, "Id");
                m.FacultyCode = S(dr, "FacultyCode");
                m.CollegeCode = S(dr, "CollegeCode");
                m.TypeOfMembers = S(dr, "TypeOfMembers");
                m.MobileNo = S(dr, "MobileNo");
                m.MemberName = S(dr, "memberName");
                m.FromPlace = S(dr, "FromPlace");
                m.ToPlace = S(dr, "ToPlace");
                m.Kilometers = M(dr, "Kilometers");
                m.ReturnFromPlace = S(dr, "ReturnFromPlace");
                m.ReturnToPlace = S(dr, "ReturnToPlace");
                m.ReturnKilometers = M(dr, "ReturnKilometers");
                m.TotalClaimAmount = M(dr, "TotalClaimAmount");
                m.IsBanglore = B(dr, "IsBanglore");
                m.AirFair = M(dr, "AirFair");
                m.NoOfDays = N(dr, "NoOfDays");
                m.TravelCost = M(dr, "TravelCost");
                m.DACost = M(dr, "DACost");
                m.LCACost = M(dr, "LCACost");
                m.CollegeCost = M(dr, "CollegeCost");
                m.AirRoadCost = M(dr, "AirRoadCost");
                m.Division = S(dr, "Division");
                m.IsLCA = B(dr, "IsLCA");
                m.DR_ApprovalStatus = S(dr, "DR_ApprovalStatus");
                m.DR_Remarks = S(dr, "DR_Remarks");
                m.DRApprovedBy = S(dr, "DRApprovedBy");
                m.DRApprovedDate = DT(dr, "DRApprovedDate");
                m.FO_Level1_ApprovedStatus = S(dr, "FO_Level1_ApprovedStatus");
                m.FO_Level1_Remarks = S(dr, "FO_Level1_Remarks");
                m.FO_Level1_ForwardedDate = DT(dr, "FO_Level1_ForwardedDate");
                m.F_CaseWorker_Approve_Status = S(dr, "F_CaseWorker_Approve_Status");
                m.F_CaseWorker_Approve_Remarks = S(dr, "F_CaseWorker_Approve_Remarks");
                m.F_CaseWorker_Approved_Date = DT(dr, "F_CaseWorker_Approved_Date");
                m.F_AO_SP_Approved_Status = S(dr, "F_AO_SP_Approved_Status");
                m.F_AO_SP_Approved_Remarks = S(dr, "F_AO_SP_Approved_Remarks");
                m.F_AO_SP_Approved_Date = DT(dr, "F_AO_SP_Approved_Date");
                m.FO_Level2_ApprovedStatus = S(dr, "FO_Level2_ApprovedStatus");
                m.FO_Level2_ApprovedRemarks = S(dr, "FO_Level2_ApprovedRemarks");
                m.FO_Level2_ApprovedDate = DT(dr, "FO_Level2_ApprovedDate");
                m.Cashier_Update = S(dr, "Cashier_Update");
                m.CashierApprovedDate = DT(dr, "CashierApprovedDate");
                var inspectionDate = DT(dr, "InspectionDate");
                m.InspectionDate = inspectionDate.HasValue
                    ? DateOnly.FromDateTime(inspectionDate.Value)
                    : (DateOnly?)null;

                // Flag: file exists but don't download it yet
                m.HasLicFile = N(dr, "HasLicFile") == 1;
            }

            // ── Query 2: LicApprovalFile binary — separate round trip ──────────
            if (m.HasLicFile)
            {
                const string sqlFile = @"
            SELECT TOP 1 LicApprovalFile
            FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
            WHERE MobileNo = @Phone
              AND LicApprovalFile IS NOT NULL";

                using var conF = new SqlConnection(_conn);
                using var cmdF = new SqlCommand(sqlFile, conF);
                cmdF.CommandTimeout = 180; // large file needs more time
                cmdF.Parameters.AddWithValue("@Phone", mobileNo ?? "");
                conF.Open();
                using var drF = cmdF.ExecuteReader(CommandBehavior.SequentialAccess);
                if (drF.Read() && !drF.IsDBNull(0))
                {
                    var bytes = (byte[])drF.GetValue(0);
                    if (bytes?.Length > 0)
                        m.LicApprovalFileName = Convert.ToBase64String(bytes);
                }
            }

            // ── Query 3: UploadBills binary ─────────────────────────────────────
            if (!string.IsNullOrEmpty(m.MobileNo))
            {
                const string sql2 = @"
            SELECT TOP 1 UploadBills
            FROM [Admission_Affiliation].[dbo].[LICClaimDetails]
            WHERE PhoneNumber = @Phone
              AND UploadBills IS NOT NULL";

                using var con2 = new SqlConnection(_conn);
                using var cmd2 = new SqlCommand(sql2, con2);
                cmd2.CommandTimeout = 180;
                cmd2.Parameters.AddWithValue("@Phone", m.MobileNo);
                con2.Open();
                using var dr2 = cmd2.ExecuteReader(CommandBehavior.SequentialAccess);
                if (dr2.Read() && !dr2.IsDBNull(0))
                {
                    var bytes = (byte[])dr2.GetValue(0);
                    if (bytes?.Length > 0)
                        m.UploadedBills = Convert.ToBase64String(bytes);
                }
            }

            return m;
        }
        // ══ BANK DETAILS ═══════════════════════════════════════════════════════
        // Returns a strongly-typed LicBankDetailsVM so ASP.NET Core JSON
        // serializer always produces consistent camelCase property names.
        // Anonymous types with new { } caused unreliable serialization.
        public class LicBankDetailsVM
        {
            public bool Found { get; set; } = false;
            public string AccountHolderName { get; set; } = "";
            public string AccountNumber { get; set; } = "";
            public string IfscCode { get; set; } = "";
            public string BankName { get; set; } = "";
            public string BranchName { get; set; } = "";
            public string PanNumber { get; set; } = "";
            public string AadhaarNumber { get; set; } = "";
            public string ModeOfTravel { get; set; } = "";
        }


        public LicBankDetailsVM GetBankDetails(string phone)
        {
            var vm = new LicBankDetailsVM();

            if (string.IsNullOrWhiteSpace(phone))
                return vm;

            const string sql = @"
                SELECT TOP 1
                    ISNULL(AccountHolderName, '') AS AccountHolderName,
                    ISNULL(AccountNumber,     '') AS AccountNumber,
                    ISNULL(IFSCCode,          '') AS IFSCCode,
                    ISNULL(BankName,          '') AS BankName,
                    ISNULL(BranchName,        '') AS BranchName,
                    ISNULL(PANNumber,         '') AS PANNumber,
                    ISNULL(AadhaarNumber,     '') AS AadhaarNumber,
                    ISNULL(ModeOfTravel,      '') AS ModeOfTravel
                FROM [Admission_Affiliation].[dbo].[LIC_Inspection]
                WHERE PhoneNumber = @Phone";

            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Phone", phone.Trim());
            con.Open();
            using var dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                vm.AccountHolderName = S(dr, "AccountHolderName");
                vm.AccountNumber = S(dr, "AccountNumber");
                vm.IfscCode = S(dr, "IFSCCode");
                vm.BankName = S(dr, "BankName");
                vm.BranchName = S(dr, "BranchName");
                vm.PanNumber = S(dr, "PANNumber");
                vm.AadhaarNumber = S(dr, "AadhaarNumber");
                vm.ModeOfTravel = S(dr, "ModeOfTravel");
                vm.Found = true;
            }

            return vm;
        }
        // ══ UPDATE METHODS ══════════════════════════════════════════════════════

        public bool ForwardToCW(int id, string remarks)
        {
            return ForwardToCW(id, remarks, 0, 0, "FO");
        }

        public bool ForwardToCW(int id, string remarks, string routedTo)
        {
            int.TryParse(routedTo, out var soUserId);
            return ForwardToCW(id, remarks, soUserId, 0, "FO");
        }

        public bool ForwardToCW(int id, string remarks, int soUserId, int actionByUserId, string designation)
        {
            using var con = new SqlConnection(_conn);
            con.Open();
            using var txn = con.BeginTransaction();
            try
            {
                string collegeCode = "";
                string academicYear = "";

                using (var getCmd = new SqlCommand(@"
                    SELECT TOP 1 CollegeCode, AcademicYear
                    FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
                    WHERE Id = @Id", con, txn))
                {
                    getCmd.Parameters.AddWithValue("@Id", id);
                    using var dr = getCmd.ExecuteReader();
                    if (!dr.Read()) { txn.Rollback(); return false; }
                    collegeCode = dr["CollegeCode"]?.ToString() ?? "";
                    academicYear = dr["AcademicYear"]?.ToString() ?? "";
                }

                if (string.IsNullOrWhiteSpace(collegeCode) || string.IsNullOrWhiteSpace(academicYear))
                {
                    txn.Rollback();
                    return false;
                }

                var routeLabel = soUserId > 0 ? $"UserId:{soUserId}" : "SO/AS/AO";
                var composedRemarks = string.IsNullOrWhiteSpace(remarks)
                    ? $"Routed by FO to {routeLabel}"
                    : $"{remarks} | Routed by FO to {routeLabel}";

                using (var updCmd = new SqlCommand(@"
                    UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
                    SET FO_Level1_ApprovedStatus = 'AssignedToSO',
                        FO_Level1_Remarks        = @Remarks,
                        FO_Level1_ForwardedDate  = GETDATE(),
                        FO_RoutedToUserId        = @SOUserId,
                        CurrentOwnerUserId       = @SOUserId,
                        CurrentStage             = 'SO_ROUTING',
                        UpdatedDate              = GETDATE()
                    WHERE CollegeCode  = @CollegeCode
                      AND AcademicYear = @AcademicYear
                      AND DR_ApprovalStatus IS NOT NULL
                      AND DR_ApprovalStatus <> ''", con, txn))
                {
                    updCmd.Parameters.AddWithValue("@Remarks", composedRemarks);
                    updCmd.Parameters.AddWithValue("@SOUserId", soUserId > 0 ? soUserId : (object)DBNull.Value);
                    updCmd.Parameters.AddWithValue("@CollegeCode", collegeCode);
                    updCmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                    if (updCmd.ExecuteNonQuery() <= 0) { txn.Rollback(); return false; }
                }

                txn.Commit();
                return true;
            }
            catch
            {
                try { txn.Rollback(); } catch { }
                return false;
            }
        }

        public bool SOAssignCW(int id, int cwUserId, string remarks, int actionByUserId, string designation)
        {
            using var con = new SqlConnection(_conn);
            con.Open();
            using var txn = con.BeginTransaction();

            try
            {
                if (cwUserId <= 0) { txn.Rollback(); return false; }

                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.LICCollegeApproval
                    SET SO_AssignedCWUserId      = @CWUserId,
                        FO_Level1_ApprovedStatus = 'AssignedToCW',
                        CurrentOwnerUserId       = @CWUserId,
                        CurrentStage             = 'CW',
                        UpdatedDate              = GETDATE()
                    WHERE CollegeCode = (SELECT TOP 1 CollegeCode FROM dbo.LICCollegeApproval WHERE Id = @Id)
                      AND AcademicYear = (SELECT TOP 1 AcademicYear FROM dbo.LICCollegeApproval WHERE Id = @Id)
                      AND CurrentStage = 'SO_ROUTING'", con, txn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@CWUserId", cwUserId);
                    if (cmd.ExecuteNonQuery() <= 0) { txn.Rollback(); return false; }
                }

                using (var log = new SqlCommand(@"
                    INSERT INTO dbo.LIC_Workflow_Movement_Log
                    (ApprovalId, FromStage, ToStage, FromUserId, ToUserId, ActionType, Remarks, ActionByUserId, ActionByDesignation)
                    VALUES
                    (@ApprovalId, 'SO_ROUTING', 'CW', @FromUserId, @ToUserId, 'ASSIGN', @Remarks, @ActionByUserId, @Desig)", con, txn))
                {
                    log.Parameters.AddWithValue("@ApprovalId", id);
                    log.Parameters.AddWithValue("@FromUserId", actionByUserId > 0 ? actionByUserId : (object)DBNull.Value);
                    log.Parameters.AddWithValue("@ToUserId", cwUserId);
                    log.Parameters.AddWithValue("@Remarks", remarks ?? "");
                    log.Parameters.AddWithValue("@ActionByUserId", actionByUserId > 0 ? actionByUserId : 0);
                    log.Parameters.AddWithValue("@Desig", designation ?? "");
                    log.ExecuteNonQuery();
                }

                txn.Commit();
                return true;
            }
            catch
            {
                try { txn.Rollback(); } catch { }
                return false;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  LicTadaService.cs — SaveKm (complete replacement)
        //
        //  Business rules:
        //    KM ≤ 40  →  TravelCost=0, DACost=0, LCACost=auto(500/300), IsLCA=1
        //    KM > 40  →  TravelCost=(km+rkm)×15, DACost=user value, LCACost=0, IsLCA=0
        //
        //  Both LICCollegeApproval and LICClaimDetails are updated.
        //  All changes are written as JSON to CWEditLog for the audit banner.
        // ═══════════════════════════════════════════════════════════════════

        public bool SaveKm(int id, decimal km, decimal rkm,
       decimal? da = null,
       decimal? lca = null,
       decimal? airRoad = null,
       decimal? airFare = null,
       string editedBy = "",
       string editorDesignation = "",
       string editedAtStage = "")   // "CaseWorker" | "AO" | "FO2"
        {
            decimal totalKm = km + rkm;

            // ── RULE: EITHER one side ≤ 40 → LCA (no TA, no DA)
            //          BOTH  sides > 40     → TA + DA, LCA = 0
            bool shortTrip = (km > 0 && km <= 40m) || (rkm > 0 && rkm <= 40m);

            decimal travelCost = shortTrip ? 0m : totalKm * 15m;

            using var con = new SqlConnection(_conn);
            con.Open();

            // ── Variables to hold current DB values ────────────────────────
            string phone = "";
            decimal originalDA = 0m, originalLCA = 0m;
            decimal originalAirRoad = 0m, originalAirFare = 0m;
            decimal originalKM = 0m, originalRKM = 0m;
            decimal originalTravel = 0m, originalTotal = 0m;
            decimal collegeCost = 0m;
            bool isBangalore = false;
            string academicYear = "", collegeCode = "", memberName = "";
            string typeOfMembers = "", mobileNo = "", fromPlace = "";
            string toPlace = "", returnFrom = "", returnTo = "", division = "";
            int facultyCode = 0;

            // ── Step 1: Read current values BEFORE opening transaction ──────
            // (read-only, no need to be inside transaction)
            using (var cmd = new SqlCommand(@"
        SELECT TOP 1
            MobileNo,
            ISNULL(DACost,           0) AS DACost,
            ISNULL(LCACost,          0) AS LCACost,
            ISNULL(CollegeCost,      0) AS CollegeCost,
            ISNULL(AirRoadCost,      0) AS AirRoadCost,
            ISNULL(AirFair,          0) AS AirFair,
            ISNULL(Kilometers,       0) AS Kilometers,
            ISNULL(ReturnKilometers, 0) AS ReturnKilometers,
            ISNULL(TravelCost,       0) AS TravelCost,
            ISNULL(TotalClaimAmount, 0) AS TotalClaimAmount,
            ISNULL(IsBanglore,       0) AS IsBanglore,
            ISNULL(AcademicYear,    '') AS AcademicYear,
            ISNULL(FacultyCode,      0) AS FacultyCode,
            ISNULL(CollegeCode,     '') AS CollegeCode,
            ISNULL(memberName,      '') AS memberName,
            ISNULL(TypeOfMembers,   '') AS TypeOfMembers,
            ISNULL(FromPlace,       '') AS FromPlace,
            ISNULL(ToPlace,         '') AS ToPlace,
            ISNULL(ReturnFromPlace, '') AS ReturnFromPlace,
            ISNULL(ReturnToPlace,   '') AS ReturnToPlace,
            ISNULL(Division,        '') AS Division
        FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
        WHERE Id = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using var dr = cmd.ExecuteReader();
                if (!dr.Read()) return false;

                // Read by column name — immune to index/ordering issues
                phone = dr["MobileNo"]?.ToString() ?? "";
                originalDA = dr.IsDBNull(dr.GetOrdinal("DACost")) ? 0m : Convert.ToDecimal(dr["DACost"]);
                originalLCA = dr.IsDBNull(dr.GetOrdinal("LCACost")) ? 0m : Convert.ToDecimal(dr["LCACost"]);
                collegeCost = dr.IsDBNull(dr.GetOrdinal("CollegeCost")) ? 0m : Convert.ToDecimal(dr["CollegeCost"]);
                originalAirRoad = dr.IsDBNull(dr.GetOrdinal("AirRoadCost")) ? 0m : Convert.ToDecimal(dr["AirRoadCost"]);
                originalAirFare = dr.IsDBNull(dr.GetOrdinal("AirFair")) ? 0m : Convert.ToDecimal(dr["AirFair"]);
                originalKM = dr.IsDBNull(dr.GetOrdinal("Kilometers")) ? 0m : Convert.ToDecimal(dr["Kilometers"]);
                originalRKM = dr.IsDBNull(dr.GetOrdinal("ReturnKilometers")) ? 0m : Convert.ToDecimal(dr["ReturnKilometers"]);
                originalTravel = dr.IsDBNull(dr.GetOrdinal("TravelCost")) ? 0m : Convert.ToDecimal(dr["TravelCost"]);
                originalTotal = dr.IsDBNull(dr.GetOrdinal("TotalClaimAmount")) ? 0m : Convert.ToDecimal(dr["TotalClaimAmount"]);
                isBangalore = !dr.IsDBNull(dr.GetOrdinal("IsBanglore")) && Convert.ToBoolean(dr["IsBanglore"]);
                academicYear = dr["AcademicYear"]?.ToString() ?? "";
                facultyCode = dr.IsDBNull(dr.GetOrdinal("FacultyCode")) ? 0 : Convert.ToInt32(dr["FacultyCode"]);
                collegeCode = dr["CollegeCode"]?.ToString() ?? "";
                memberName = dr["memberName"]?.ToString() ?? "";
                typeOfMembers = dr["TypeOfMembers"]?.ToString() ?? "";
                mobileNo = dr["MobileNo"]?.ToString() ?? "";
                fromPlace = dr["FromPlace"]?.ToString() ?? "";
                toPlace = dr["ToPlace"]?.ToString() ?? "";
                returnFrom = dr["ReturnFromPlace"]?.ToString() ?? "";
                returnTo = dr["ReturnToPlace"]?.ToString() ?? "";
                division = dr["Division"]?.ToString() ?? "";
            }

            if (string.IsNullOrEmpty(phone)) return false;

            // ── Defaults based on Bangalore flag ───────────────────────────
            decimal lcaDefault = isBangalore ? 500m : 300m;   // LCA: BLR=500, Other=300
            decimal daDefault = isBangalore ? 1500m : 900m;   // DA:  BLR=1500, Other=900

            decimal effectiveDA, effectiveLCA;
            bool effectiveIsLCA;

            if (shortTrip)
            {
                // Either side ≤ 40 → No TA, No DA, LCA applies
                effectiveDA = 0m;
                effectiveLCA = lca.HasValue ? lca.Value
                               : (originalLCA > 0 ? originalLCA : lcaDefault);
                effectiveIsLCA = true;
            }
            else
            {
                // Both sides > 40 → TA + DA apply, LCA = 0
                effectiveDA = da.HasValue ? da.Value
                               : (originalDA > 0 ? originalDA : daDefault);
                effectiveLCA = 0m;
                effectiveIsLCA = false;
            }

            decimal effectiveAirRoad = airRoad.HasValue ? airRoad.Value : originalAirRoad;
            decimal effectiveAirFare = airFare.HasValue ? airFare.Value : originalAirFare;

            decimal totalClaim = travelCost + effectiveDA + effectiveLCA
                               + collegeCost + effectiveAirRoad + effectiveAirFare;

            // ── Detect which fields actually changed ────────────────────────
            bool kmChanged = Math.Abs(km - originalKM) > 0.01m;
            bool rkmChanged = Math.Abs(rkm - originalRKM) > 0.01m;
            bool travelChanged = Math.Abs(travelCost - originalTravel) > 0.01m;
            bool daChanged = Math.Abs(effectiveDA - originalDA) > 0.01m;
            bool lcaChanged = Math.Abs(effectiveLCA - originalLCA) > 0.01m;
            bool airRoadChanged = Math.Abs(effectiveAirRoad - originalAirRoad) > 0.01m;
            bool airFareChanged = Math.Abs(effectiveAirFare - originalAirFare) > 0.01m;
            bool totalChanged = Math.Abs(totalClaim - originalTotal) > 0.01m;

            bool anyChanged = kmChanged || rkmChanged || travelChanged || daChanged
                           || lcaChanged || airRoadChanged || airFareChanged || totalChanged;

            // ── Build ChangedFields summary string ─────────────────────────
            var changedList = new List<string>();
            if (kmChanged) changedList.Add("KM Onward");
            if (rkmChanged) changedList.Add("KM Return");
            if (travelChanged) changedList.Add("Travel Cost");
            if (daChanged) changedList.Add("DA");
            if (lcaChanged) changedList.Add("LCA");
            if (airRoadChanged) changedList.Add("Air/Road");
            if (airFareChanged) changedList.Add("Air Fare");
            if (totalChanged) changedList.Add("Total");
            string changedFields = string.Join(", ", changedList);

            // ── IsLCA before this edit ──────────────────────────────────────
            bool oldIsLCA = originalLCA > 0m;

            // ══════════════════════════════════════════════════════════════════
            // Step 2: Begin transaction — ALL writes must succeed or ALL rollback
            // ══════════════════════════════════════════════════════════════════
            using var txn = con.BeginTransaction();
            try
            {
                // ── UPDATE LICCollegeApproval ─────────────────────────────────
                using (var cmd = new SqlCommand(@"
            UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
            SET Kilometers       = @KM,
                ReturnKilometers = @RKM,
                TravelCost       = @TravelCost,
                DACost           = @DACost,
                LCACost          = @LCA,
                AirRoadCost      = @AirRoad,
                AirFair          = @AirFare,
                TotalClaimAmount = @TotalClaim,
                IsLCA            = @IsLCA,
                UpdatedDate      = GETDATE()
            WHERE Id = @Id", con, txn))
                {
                    cmd.Parameters.AddWithValue("@KM", km);
                    cmd.Parameters.AddWithValue("@RKM", rkm);
                    cmd.Parameters.AddWithValue("@TravelCost", travelCost);
                    cmd.Parameters.AddWithValue("@DACost", effectiveDA);
                    cmd.Parameters.AddWithValue("@LCA", effectiveLCA);
                    cmd.Parameters.AddWithValue("@AirRoad", effectiveAirRoad);
                    cmd.Parameters.AddWithValue("@AirFare", effectiveAirFare);
                    cmd.Parameters.AddWithValue("@TotalClaim", totalClaim);
                    cmd.Parameters.AddWithValue("@IsLCA", effectiveIsLCA ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }

                // ── UPDATE LICClaimDetails ────────────────────────────────────
                // Column is AirFair (NOT AirFareCost)
                using (var cmd = new SqlCommand(@"
            UPDATE [Admission_Affiliation].[dbo].[LICClaimDetails]
            SET Kilometers       = @KM,
                ReturnKilometers = @RKM,
                TravelCost       = @TravelCost,
                DACost           = @DACost,
                LCACost          = @LCA,
                AirRoadCost      = @AirRoad,
                AirFare          = @AirFare,
                TotalCost = @TotalClaim,
                IsLCA            = @IsLCA
            WHERE PhoneNumber = @Phone", con, txn))
                {
                    cmd.Parameters.AddWithValue("@KM", km);
                    cmd.Parameters.AddWithValue("@RKM", rkm);
                    cmd.Parameters.AddWithValue("@TravelCost", travelCost);
                    cmd.Parameters.AddWithValue("@DACost", effectiveDA);
                    cmd.Parameters.AddWithValue("@LCA", effectiveLCA);
                    cmd.Parameters.AddWithValue("@AirRoad", effectiveAirRoad);
                    cmd.Parameters.AddWithValue("@AirFare", effectiveAirFare);
                    cmd.Parameters.AddWithValue("@TotalClaim", totalClaim);
                    cmd.Parameters.AddWithValue("@IsLCA", effectiveIsLCA ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.ExecuteNonQuery();
                }

                // ── INSERT into LIC_TA_DA_Edit_Log (only if something changed) ─
                if (anyChanged)
                {
                    using var logCmd = new SqlCommand(@"
                INSERT INTO [Admission_Affiliation].[dbo].[LIC_TA_DA_Edit_Log]
                (
                    ApprovalId,
                    AcademicYear,   FacultyCode,    CollegeCode,
                    MemberName,     TypeOfMembers,  MobileNo,
                    IsBanglore,     Division,

                    Old_Kilometers,         New_Kilometers,
                    Old_ReturnKilometers,   New_ReturnKilometers,
                    Old_TravelCost,         New_TravelCost,
                    Old_DACost,             New_DACost,
                    Old_LCACost,            New_LCACost,
                    Old_IsLCA,              New_IsLCA,
                    Old_AirRoadCost,        New_AirRoadCost,
                    Old_AirFair,            New_AirFair,
                    Old_TotalClaimAmount,   New_TotalClaimAmount,

                    ChangedFields,
                    EditedBy,   EditorDesignation,  EditedAtStage,  EditedAt
                )
                VALUES
                (
                    @ApprovalId,
                    @AcademicYear,  @FacultyCode,   @CollegeCode,
                    @MemberName,    @TypeOfMembers, @MobileNo,
                    @IsBangalore,   @Division,

                    @Old_KM,                @New_KM,
                    @Old_RKM,               @New_RKM,
                    @Old_Travel,            @New_Travel,
                    @Old_DA,                @New_DA,
                    @Old_LCA,               @New_LCA,
                    @Old_IsLCA,             @New_IsLCA,
                    @Old_AirRoad,           @New_AirRoad,
                    @Old_AirFare,           @New_AirFare,
                    @Old_Total,             @New_Total,

                    @ChangedFields,
                    @EditedBy,  @EditorDesignation, @EditedAtStage, GETDATE()
                )", con, txn);

                    // Record info
                    logCmd.Parameters.AddWithValue("@ApprovalId", id);
                    logCmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                    logCmd.Parameters.AddWithValue("@FacultyCode", facultyCode);
                    logCmd.Parameters.AddWithValue("@CollegeCode", collegeCode);
                    logCmd.Parameters.AddWithValue("@MemberName", memberName);
                    logCmd.Parameters.AddWithValue("@TypeOfMembers", typeOfMembers);
                    logCmd.Parameters.AddWithValue("@MobileNo", mobileNo);
                    logCmd.Parameters.AddWithValue("@IsBangalore", isBangalore ? 1 : 0);
                    logCmd.Parameters.AddWithValue("@Division", division);

                    // KM Onward — NULL if not changed
                    logCmd.Parameters.AddWithValue("@Old_KM",
                        kmChanged ? (object)originalKM : DBNull.Value);
                    logCmd.Parameters.AddWithValue("@New_KM",
                        kmChanged ? (object)km : DBNull.Value);

                    // KM Return
                    logCmd.Parameters.AddWithValue("@Old_RKM",
                        rkmChanged ? (object)originalRKM : DBNull.Value);
                    logCmd.Parameters.AddWithValue("@New_RKM",
                        rkmChanged ? (object)rkm : DBNull.Value);

                    // Travel Cost (auto-computed from KM)
                    logCmd.Parameters.AddWithValue("@Old_Travel",
                        travelChanged ? (object)originalTravel : DBNull.Value);
                    logCmd.Parameters.AddWithValue("@New_Travel",
                        travelChanged ? (object)travelCost : DBNull.Value);

                    // DA
                    logCmd.Parameters.AddWithValue("@Old_DA",
                        daChanged ? (object)originalDA : DBNull.Value);
                    logCmd.Parameters.AddWithValue("@New_DA",
                        daChanged ? (object)effectiveDA : DBNull.Value);

                    // LCA
                    logCmd.Parameters.AddWithValue("@Old_LCA",
                        lcaChanged ? (object)originalLCA : DBNull.Value);
                    logCmd.Parameters.AddWithValue("@New_LCA",
                        lcaChanged ? (object)effectiveLCA : DBNull.Value);

                    // IsLCA flag (always stored)
                    logCmd.Parameters.AddWithValue("@Old_IsLCA", oldIsLCA ? 1 : 0);
                    logCmd.Parameters.AddWithValue("@New_IsLCA", effectiveIsLCA ? 1 : 0);

                    // Air/Road
                    logCmd.Parameters.AddWithValue("@Old_AirRoad",
                        airRoadChanged ? (object)originalAirRoad : DBNull.Value);
                    logCmd.Parameters.AddWithValue("@New_AirRoad",
                        airRoadChanged ? (object)effectiveAirRoad : DBNull.Value);

                    // Air Fare
                    logCmd.Parameters.AddWithValue("@Old_AirFare",
                        airFareChanged ? (object)originalAirFare : DBNull.Value);
                    logCmd.Parameters.AddWithValue("@New_AirFare",
                        airFareChanged ? (object)effectiveAirFare : DBNull.Value);

                    // Total Claim Amount
                    logCmd.Parameters.AddWithValue("@Old_Total",
                        totalChanged ? (object)originalTotal : DBNull.Value);
                    logCmd.Parameters.AddWithValue("@New_Total",
                        totalChanged ? (object)totalClaim : DBNull.Value);

                    // Audit fields
                    logCmd.Parameters.AddWithValue("@ChangedFields",
                        changedFields);
                    logCmd.Parameters.AddWithValue("@EditedBy",
                        string.IsNullOrEmpty(editedBy) ? DBNull.Value : (object)editedBy);
                    logCmd.Parameters.AddWithValue("@EditorDesignation",
                        string.IsNullOrEmpty(editorDesignation) ? DBNull.Value : (object)editorDesignation);
                    logCmd.Parameters.AddWithValue("@EditedAtStage",
                        string.IsNullOrEmpty(editedAtStage) ? DBNull.Value : (object)editedAtStage);

                    logCmd.ExecuteNonQuery();
                }

                // ══════════════════════════════════════════════════════════════
                // All 3 operations succeeded → COMMIT
                // ══════════════════════════════════════════════════════════════
                txn.Commit();
                return true;
            }
            catch (Exception ex)
            {
                // ══════════════════════════════════════════════════════════════
                // Any failure → ROLLBACK everything — DB stays in original state
                // ══════════════════════════════════════════════════════════════
                try { txn.Rollback(); } catch { /* rollback itself failed — connection lost */ }

                // Log the exception (use your existing logger)
                // _logger.LogError(ex, "SaveKm failed for ApprovalId={Id}", id);

                return false;
            }
        }

        //public bool SaveKm(int id, decimal km, decimal rkm)
        //{
        //    decimal totalKm = km + rkm;
        //    bool shortTrip = totalKm > 0 && totalKm <= 40m;
        //    decimal travelCost = shortTrip ? 0m : totalKm * 15m;

        //    using var con = new SqlConnection(_conn);
        //    con.Open();

        //    string phone = "";
        //    decimal originalDA = 0m;
        //    decimal lcaCost = 0m;
        //    decimal collegeCost = 0m;
        //    decimal airRoadCost = 0m;
        //    decimal airFair = 0m;

        //    using (var cmd = new SqlCommand(@"
        //        SELECT TOP 1 MobileNo,
        //            ISNULL(DACost,     0),
        //            ISNULL(LCACost,    0),
        //            ISNULL(CollegeCost,0),
        //            ISNULL(AirRoadCost,0),
        //            ISNULL(AirFair,    0)
        //        FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
        //        WHERE Id = @Id", con))
        //    {
        //        cmd.Parameters.AddWithValue("@Id", id);
        //        using var dr = cmd.ExecuteReader();
        //        if (!dr.Read()) return false;
        //        phone = dr.IsDBNull(0) ? "" : dr.GetValue(0)?.ToString() ?? "";
        //        originalDA = dr.IsDBNull(1) ? 0m : Convert.ToDecimal(dr.GetValue(1));
        //        lcaCost = dr.IsDBNull(2) ? 0m : Convert.ToDecimal(dr.GetValue(2));
        //        collegeCost = dr.IsDBNull(3) ? 0m : Convert.ToDecimal(dr.GetValue(3));
        //        airRoadCost = dr.IsDBNull(4) ? 0m : Convert.ToDecimal(dr.GetValue(4));
        //        airFair = dr.IsDBNull(5) ? 0m : Convert.ToDecimal(dr.GetValue(5));
        //    }

        //    if (string.IsNullOrEmpty(phone)) return false;

        //    decimal effectiveDA = shortTrip ? 0m : originalDA;
        //    decimal totalClaim = travelCost + effectiveDA + lcaCost + collegeCost + airRoadCost + airFair;

        //    using (var cmd = new SqlCommand(@"
        //        UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
        //        SET Kilometers       = @KM,
        //            ReturnKilometers = @RKM,
        //            TravelCost       = @TravelCost,
        //            DACost           = @DACost,
        //            TotalClaimAmount = @TotalClaim,
        //            UpdatedDate      = GETDATE()
        //        WHERE Id = @Id", con))
        //    {
        //        cmd.Parameters.AddWithValue("@KM", km);
        //        cmd.Parameters.AddWithValue("@RKM", rkm);
        //        cmd.Parameters.AddWithValue("@TravelCost", travelCost);
        //        cmd.Parameters.AddWithValue("@DACost", effectiveDA);
        //        cmd.Parameters.AddWithValue("@TotalClaim", totalClaim);
        //        cmd.Parameters.AddWithValue("@Id", id);
        //        cmd.ExecuteNonQuery();
        //    }

        //    using (var cmd = new SqlCommand(@"
        //        UPDATE [Admission_Affiliation].[dbo].[LICClaimDetails]
        //        SET Kilometers       = @KM,
        //            ReturnKilometers = @RKM,
        //            TravelCost       = @TravelCost,
        //            DACost           = @DACost,
        //            TotalCost        = @TotalClaim
        //        WHERE PhoneNumber = @Phone", con))
        //    {
        //        cmd.Parameters.AddWithValue("@KM", km);
        //        cmd.Parameters.AddWithValue("@RKM", rkm);
        //        cmd.Parameters.AddWithValue("@TravelCost", travelCost);
        //        cmd.Parameters.AddWithValue("@DACost", effectiveDA);
        //        cmd.Parameters.AddWithValue("@TotalClaim", totalClaim);
        //        cmd.Parameters.AddWithValue("@Phone", phone);
        //        cmd.ExecuteNonQuery();
        //    }

        //    return true;
        //}

        public bool CWVerify(int id, string remarks)
        {
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
                UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
                SET F_CaseWorker_Approve_Status  = 'Verified',
                    F_CaseWorker_Approve_Remarks = @Remarks,
                    F_CaseWorker_Approved_Date   = GETDATE(),
                    FO_Level1_ApprovedStatus     = 'CWReviewed',
                    CurrentStage                 = 'SO_VERIFY',
                    CurrentOwnerUserId           = FO_RoutedToUserId,
                    UpdatedDate                  = GETDATE()
                WHERE CollegeCode = (SELECT TOP 1 CollegeCode FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] WHERE Id = @Id)
                  AND AcademicYear = (SELECT TOP 1 AcademicYear FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] WHERE Id = @Id)", con);
            cmd.Parameters.AddWithValue("@Remarks", remarks ?? "");
            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool CWReject(int id, string remarks)
        {
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
                UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
                SET F_CaseWorker_Approve_Status  = 'Rejected',
                    F_CaseWorker_Approve_Remarks = @Remarks,
                    F_CaseWorker_Approved_Date   = GETDATE(),
                    FO_Level1_ApprovedStatus     = 'CWReviewed',
                    CurrentStage                 = 'SO_VERIFY',
                    CurrentOwnerUserId           = FO_RoutedToUserId,
                    UpdatedDate                  = GETDATE()
                WHERE CollegeCode = (SELECT TOP 1 CollegeCode FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] WHERE Id = @Id)
                  AND AcademicYear = (SELECT TOP 1 AcademicYear FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] WHERE Id = @Id)", con);
            cmd.Parameters.AddWithValue("@Remarks", remarks ?? "");
            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool AOApprove(int id, string remarks)
        {
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
                UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
                SET F_AO_SP_Approved_Status  = 'Approved',
                    F_AO_SP_Approved_Remarks = @Remarks,
                    F_AO_SP_Approved_Date    = GETDATE(),
                    FO_Level1_ApprovedStatus = 'SOReviewed',
                    CurrentStage             = 'FO_FINAL',
                    CurrentOwnerUserId       = NULL,
                    UpdatedDate              = GETDATE()
                WHERE CollegeCode = (SELECT TOP 1 CollegeCode FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] WHERE Id = @Id)
                  AND AcademicYear = (SELECT TOP 1 AcademicYear FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] WHERE Id = @Id)", con);
            cmd.Parameters.AddWithValue("@Remarks", remarks ?? "");
            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool AOReject(int id, string remarks)
        {
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
                UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
                SET F_AO_SP_Approved_Status  = 'Rejected',
                    F_AO_SP_Approved_Remarks = @Remarks,
                    F_AO_SP_Approved_Date    = GETDATE(),
                    FO_Level1_ApprovedStatus = 'SOReviewed',
                    CurrentStage             = 'CW',
                    CurrentOwnerUserId       = SO_AssignedCWUserId,
                    UpdatedDate              = GETDATE()
                WHERE CollegeCode = (SELECT TOP 1 CollegeCode FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] WHERE Id = @Id)
                  AND AcademicYear = (SELECT TOP 1 AcademicYear FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] WHERE Id = @Id)", con);
            cmd.Parameters.AddWithValue("@Remarks", remarks ?? "");
            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool FO2Approve(int id, string remarks)
        {
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
                UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
                SET FO_Level2_ApprovedStatus  = 'Approved',
                    FO_Level2_ApprovedRemarks = @Remarks,
                    FO_Level2_ApprovedDate    = GETDATE(),
                    FO_Level1_ApprovedStatus  = 'FOFinalApproved',
                    CurrentStage              = 'CASHIER',
                    CurrentOwnerUserId        = NULL,
                    UpdatedDate               = GETDATE()
                WHERE CollegeCode = (SELECT TOP 1 CollegeCode FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] WHERE Id = @Id)
                  AND AcademicYear = (SELECT TOP 1 AcademicYear FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] WHERE Id = @Id)", con);
            cmd.Parameters.AddWithValue("@Remarks", remarks ?? "");
            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool FO2Reject(int id, string remarks)
        {
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
                UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
                SET FO_Level2_ApprovedStatus  = 'Rejected',
                    FO_Level2_ApprovedRemarks = @Remarks,
                    FO_Level2_ApprovedDate    = GETDATE(),
                    FO_Level1_ApprovedStatus  = 'ReworkFromFOFinal',
                    F_CaseWorker_Approve_Status = NULL,
                    F_AO_SP_Approved_Status     = NULL,
                    CurrentStage                = 'SO_ROUTING',
                    CurrentOwnerUserId          = FO_RoutedToUserId,
                    UpdatedDate               = GETDATE()
                WHERE CollegeCode = (SELECT TOP 1 CollegeCode FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] WHERE Id = @Id)
                  AND AcademicYear = (SELECT TOP 1 AcademicYear FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] WHERE Id = @Id)", con);
            cmd.Parameters.AddWithValue("@Remarks", remarks ?? "");
            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool CashierPay(int id, string remarks)
        {
            using var con = new SqlConnection(_conn);
            con.Open();

            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 Id
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
                WHERE Id = @Id
                  AND FO_Level2_ApprovedStatus = 'Approved'
                  AND (Cashier_Update IS NULL OR Cashier_Update = '')", con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using var dr = cmd.ExecuteReader();
                if (!dr.Read()) return false;
            }

            using (var cmd = new SqlCommand(@"
                UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
                SET Cashier_Update      = 'Paid',
                    CashierApprovedDate = GETDATE(),
                    UpdatedDate         = GETDATE()
                WHERE Id = @Id
                  AND FO_Level2_ApprovedStatus = 'Approved'
                  AND (Cashier_Update IS NULL OR Cashier_Update = '')", con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public List<LicTadaGridRow> GetCashierPaidRecords(string? year, string? faculty, string? college)
        {
            const string sql = @" SELECT 
                                    ca.Id,
                                    ac.CollegeCode, 
                                    ac.CollegeName, 
                                    ac.CollegeTown,
                                    ca.AcademicYear, 
                                    ca.TypeOfMembers, 
                                    ca.memberName, 
                                    ca.MobileNo,

                                    ca.TotalClaimAmount AS TotalClaimAmount,

                                    ISNULL(ed.First_Old_TotalClaimAmount, ca.TotalClaimAmount) AS Old_TotalClaimAmount,

                                    '' AS CaseWorkerRemarks,
                                    ISNULL(ca.FO_Level2_ApprovedStatus, '') AS FO_Level2_Status,
                                    ISNULL(ca.Cashier_Update, '')            AS Cashier_Status,

                                    ISNULL(li.AccountHolderName, '') AS AccountHolderName,
                                    ISNULL(li.AccountNumber,     '') AS AccountNumber,
                                    ISNULL(li.IFSCCode,          '') AS IFSCCode,
                                    ISNULL(li.BankName,          '') AS BankName,
                                    ISNULL(li.BranchName,        '') AS BranchName,
                                    ISNULL(li.PANNumber,         '') AS PANNumber,
                                    ISNULL(li.AadhaarNumber,     '') AS AadhaarNumber,
                                    ISNULL(li.Email,             '') AS Email,
                                    ISNULL(li.Address,           '') AS Address,
                                    ISNULL(li.ModeOfTravel,      '') AS ModeOfTravel,

                                    ca.InspectionDate

                                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca

                                INNER JOIN dbo.Affiliation_College_Master ac 
                                    ON ac.CollegeCode = ca.CollegeCode

                                LEFT JOIN [Admission_Affiliation].[dbo].[LIC_Inspection] li
                                    ON li.PhoneNumber = ca.MobileNo

                                OUTER APPLY 
                                (
                                    SELECT TOP 1 
                                        edlog.Old_TotalClaimAmount AS First_Old_TotalClaimAmount
                                    FROM [Admission_Affiliation].[dbo].[LIC_TA_DA_Edit_Log] edlog
                                    WHERE edlog.MobileNo    = ca.MobileNo
                                      AND edlog.ApprovalId  = ca.Id
                                    ORDER BY edlog.EditedAt ASC
                                ) ed

                                WHERE ca.FO_Level2_ApprovedStatus = 'Approved'
                                  AND ca.Cashier_Update           = 'Paid'
                                  AND (@Year    IS NULL OR ca.AcademicYear = @Year)
                                  AND (@Faculty IS NULL OR ca.FacultyCode  = @Faculty)
                                  AND (@College IS NULL OR ca.CollegeCode  = @College)

                                ORDER BY ca.CashierApprovedDate DESC";

            return GroupToGrid(ExecRawMembers(sql, year, faculty, college));
        }
        public bool CashierPayAll(string collegeCode, string academicYear, string remarks)
        {
            using var con = new SqlConnection(_conn);
            con.Open();

            using var cmd = new SqlCommand(@"
        UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
        SET Cashier_Update      = 'Paid',
            CashierApprovedDate = GETDATE(),
            UpdatedDate         = GETDATE()
        WHERE CollegeCode  = @CollegeCode
          AND AcademicYear = @AcademicYear
          AND FO_Level2_ApprovedStatus = 'Approved'
          AND (Cashier_Update IS NULL OR Cashier_Update = '')", con);

            cmd.Parameters.AddWithValue("@CollegeCode", collegeCode ?? "");
            cmd.Parameters.AddWithValue("@AcademicYear", academicYear ?? "");
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<LicTadaEditLogEntry> GetEditLog(int approvalId)
        {
            var list = new List<LicTadaEditLogEntry>();
            using var con = new SqlConnection(_conn);
            con.Open();
            using var cmd = new SqlCommand(@"
        SELECT
            LogId,
            FORMAT(EditedAt, 'dd MMM yyyy hh:mm tt') AS EditedAt,
            ISNULL(EditedBy,          '—') AS EditedBy,
            ISNULL(EditorDesignation, '—') AS EditorDesignation,
            ISNULL(EditedAtStage,     '—') AS EditedAtStage,
            ISNULL(ChangedFields,     '—') AS ChangedFields,
            Old_Kilometers,         New_Kilometers,
            Old_ReturnKilometers,   New_ReturnKilometers,
            Old_TravelCost,         New_TravelCost,
            Old_DACost,             New_DACost,
            Old_LCACost,            New_LCACost,
            Old_IsLCA,              New_IsLCA,
            Old_AirRoadCost,        New_AirRoadCost,
            Old_AirFair,            New_AirFair,
            Old_TotalClaimAmount,   New_TotalClaimAmount
        FROM [Admission_Affiliation].[dbo].[LIC_TA_DA_Edit_Log]
        WHERE ApprovalId = @ApprovalId
        ORDER BY EditedAt DESC", con);
            cmd.Parameters.AddWithValue("@ApprovalId", approvalId);
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                decimal? N(string col) => dr.IsDBNull(dr.GetOrdinal(col)) ? (decimal?)null : Convert.ToDecimal(dr[col]);
                bool? B(string col) => dr.IsDBNull(dr.GetOrdinal(col)) ? (bool?)null : Convert.ToBoolean(dr[col]);
                list.Add(new LicTadaEditLogEntry
                {
                    LogId = Convert.ToInt32(dr["LogId"]),
                    EditedAt = dr["EditedAt"]?.ToString() ?? "",
                    EditedBy = dr["EditedBy"]?.ToString() ?? "",
                    EditorDesignation = dr["EditorDesignation"]?.ToString() ?? "",
                    EditedAtStage = dr["EditedAtStage"]?.ToString() ?? "",
                    ChangedFields = dr["ChangedFields"]?.ToString() ?? "",
                    Old_Kilometers = N("Old_Kilometers"),
                    New_Kilometers = N("New_Kilometers"),
                    Old_ReturnKilometers = N("Old_ReturnKilometers"),
                    New_ReturnKilometers = N("New_ReturnKilometers"),
                    Old_TravelCost = N("Old_TravelCost"),
                    New_TravelCost = N("New_TravelCost"),
                    Old_DACost = N("Old_DACost"),
                    New_DACost = N("New_DACost"),
                    Old_LCACost = N("Old_LCACost"),
                    New_LCACost = N("New_LCACost"),
                    Old_IsLCA = B("Old_IsLCA"),
                    New_IsLCA = B("New_IsLCA"),
                    Old_AirRoadCost = N("Old_AirRoadCost"),
                    New_AirRoadCost = N("New_AirRoadCost"),
                    Old_AirFair = N("Old_AirFair"),
                    New_AirFair = N("New_AirFair"),
                    Old_TotalClaimAmount = N("Old_TotalClaimAmount"),
                    New_TotalClaimAmount = N("New_TotalClaimAmount"),
                });
            }
            return list;
        }


        public (int Paid, int Failed) CashierPayBulk(List<int> ids, string remarks)
        {
            int paid = 0, failed = 0;
            foreach (var id in ids)
            {
                try
                {
                    bool ok = CashierPay(id, remarks);
                    if (ok) paid++; else failed++;
                }
                catch { failed++; }
            }
            return (paid, failed);
        }

        public class WorkflowUserItem
        {
            public int Id { get; set; }
            public string UserName { get; set; } = "";
            public string Designation { get; set; } = "";
        }

        public List<WorkflowUserItem> GetSOUsers()
        {
            var list = new List<WorkflowUserItem>();
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
        SELECT Id, ISNULL(UserName,'') AS UserName, ISNULL(FinanceDesignation,'') AS FinanceDesignation
        FROM dbo.TblRguhsFacultyUser
        WHERE IsActive = 1
          AND ISNULL(IsFinance,0) = 1
          AND FinanceDesignation IN ('SO','AS','AO')
        ORDER BY UserName", con);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new WorkflowUserItem
                {
                    Id = N(dr, "Id"),
                    UserName = S(dr, "UserName"),
                    Designation = S(dr, "FinanceDesignation")
                });
            }
            return list;
        }

        public List<WorkflowUserItem> GetCWUsers()
        {
            var list = new List<WorkflowUserItem>();
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
        SELECT Id, ISNULL(UserName,'') AS UserName, ISNULL(FinanceDesignation,'') AS FinanceDesignation
        FROM dbo.TblRguhsFacultyUser
        WHERE IsActive = 1
          AND ISNULL(IsFinance,0) = 1
          AND FinanceDesignation = 'CW'
        ORDER BY UserName", con);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new WorkflowUserItem
                {
                    Id = N(dr, "Id"),
                    UserName = S(dr, "UserName"),
                    Designation = S(dr, "FinanceDesignation")
                });
            }
            return list;
        }
    }
}
