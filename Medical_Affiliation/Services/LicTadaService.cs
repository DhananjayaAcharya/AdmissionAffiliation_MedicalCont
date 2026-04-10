using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Medical_Affiliation.Models;
using Microsoft.Extensions.Configuration;

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
                    };
                })
                .OrderBy(x => x.CollegeName)
                .ToList();
        }

        // ══ DROPDOWNS ══════════════════════════════════════════════════════════

        public List<LicTadaDDItem> GetYears()
        {
            var list = new List<LicTadaDDItem>();
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
                SELECT AcademicYear
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
                GROUP BY AcademicYear ORDER BY AcademicYear DESC", con);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var y = S(dr, "AcademicYear");
                list.Add(new LicTadaDDItem { Value = y, Text = y });
            }
            return list;
        }

        public List<LicTadaDDItem> GetFaculties()
        {
            var list = new List<LicTadaDDItem>();
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
                SELECT f.FacultyId, f.FacultyName
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] a
                INNER JOIN dbo.Faculty f ON f.FacultyId = a.FacultyCode
                GROUP BY f.FacultyId, f.FacultyName ORDER BY f.FacultyId", con);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
                list.Add(new LicTadaDDItem { Value = S(dr, "FacultyId"), Text = S(dr, "FacultyName") });
            return list;
        }

        public List<LicTadaDDItem> GetColleges()
        {
            var list = new List<LicTadaDDItem>();
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
                SELECT ac.CollegeCode, ac.CollegeName, ac.CollegeTown
                FROM [Admission_Affiliation].[dbo].[LICClaimDetails] id
                INNER JOIN dbo.Affiliation_College_Master ac ON ac.CollegeCode = id.Collegecode
                INNER JOIN [Admission_Affiliation].[dbo].[LICCollegeApproval] cl ON cl.CollegeCode = id.Collegecode
                GROUP BY ac.CollegeCode, ac.CollegeName, ac.CollegeTown
                ORDER BY ac.CollegeName", con);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
                list.Add(new LicTadaDDItem
                {
                    Value = S(dr, "CollegeCode"),
                    Text = S(dr, "CollegeName") + " — " + S(dr, "CollegeTown")
                });
            return list;
        }

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
                    '' AS AadhaarNumber,     '' AS Email,            '' AS Address, '' AS ModeOfTravel
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca
                INNER JOIN dbo.Affiliation_College_Master ac ON ac.CollegeCode = ca.CollegeCode
                WHERE ca.CollegeCode IN (
                    SELECT CollegeCode FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
                    WHERE DR_ApprovalStatus IS NOT NULL AND DR_ApprovalStatus <> ''
                      AND (FO_Level1_ApprovedStatus IS NULL OR FO_Level1_ApprovedStatus = '')
                )
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
                    '' AS AadhaarNumber,     '' AS Email,         '' AS Address, '' AS ModeOfTravel
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca
                INNER JOIN dbo.Affiliation_College_Master ac ON ac.CollegeCode = ca.CollegeCode
                WHERE ca.CollegeCode IN (
                    SELECT CollegeCode FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
                    WHERE F_AO_SP_Approved_Status = 'Approved'
                      AND (FO_Level2_ApprovedStatus IS NULL OR FO_Level2_ApprovedStatus = '')
                )
                AND (@Year    IS NULL OR ca.AcademicYear = @Year)
                AND (@Faculty IS NULL OR ca.FacultyCode  = @Faculty)
                AND (@College IS NULL OR ca.CollegeCode  = @College)
                ORDER BY ac.CollegeName";
            return GroupToGrid(ExecRawMembers(sql, year, faculty, college));
        }

        public List<LicTadaGridRow> GetCWRecords(string? year, string? faculty, string? college)
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
                    '' AS AadhaarNumber,     '' AS Email,         '' AS Address, '' AS ModeOfTravel
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca
                INNER JOIN dbo.Affiliation_College_Master ac ON ac.CollegeCode = ca.CollegeCode
                WHERE ca.CollegeCode IN (
                    SELECT CollegeCode FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
                    WHERE FO_Level1_ApprovedStatus = 'Forwarded'
                      AND (F_CaseWorker_Approve_Status IS NULL OR F_CaseWorker_Approve_Status = '')
                )
                AND (@Year    IS NULL OR ca.AcademicYear = @Year)
                AND (@Faculty IS NULL OR ca.FacultyCode  = @Faculty)
                AND (@College IS NULL OR ca.CollegeCode  = @College)
                ORDER BY ac.CollegeName";
            return GroupToGrid(ExecRawMembers(sql, year, faculty, college));
        }

        public List<LicTadaGridRow> GetAORecords(string? year, string? faculty, string? college)
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
                    '' AS AadhaarNumber,     '' AS Email,         '' AS Address, '' AS ModeOfTravel
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca
                INNER JOIN dbo.Affiliation_College_Master ac ON ac.CollegeCode = ca.CollegeCode
                WHERE ca.CollegeCode IN (
                    SELECT CollegeCode FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
                    WHERE F_CaseWorker_Approve_Status = 'Verified'
                      AND (F_AO_SP_Approved_Status IS NULL OR F_AO_SP_Approved_Status = '')
                )
                AND (@Year    IS NULL OR ca.AcademicYear = @Year)
                AND (@Faculty IS NULL OR ca.FacultyCode  = @Faculty)
                AND (@College IS NULL OR ca.CollegeCode  = @College)
                ORDER BY ac.CollegeName";
            return GroupToGrid(ExecRawMembers(sql, year, faculty, college));
        }

        public List<LicTadaGridRow> GetCashierRecords(string? year, string? faculty, string? college)
        {
            const string sql = @"
                SELECT ca.Id,
                    ac.CollegeCode, ac.CollegeName, ac.CollegeTown,
                    ca.AcademicYear, ca.TypeOfMembers, ca.memberName, ca.MobileNo,
                    ca.TotalClaimAmount,
                    ''                                         AS CaseWorkerRemarks,
                    ISNULL(ca.FO_Level2_ApprovedStatus,'')     AS FO_Level2_Status,
                    ISNULL(ca.Cashier_Update,'')               AS Cashier_Status,
                    ISNULL(li.AccountHolderName,'') AS AccountHolderName,
                    ISNULL(li.AccountNumber,'')     AS AccountNumber,
                    ISNULL(li.IFSCCode,'')          AS IFSCCode,
                    ISNULL(li.BankName,'')          AS BankName,
                    ISNULL(li.BranchName,'')        AS BranchName,
                    ISNULL(li.PANNumber,'')         AS PANNumber,
                    ISNULL(li.AadhaarNumber,'')     AS AadhaarNumber,
                    ISNULL(li.Email,'')             AS Email,
                    ISNULL(li.Address,'')           AS Address,
                    ISNULL(li.ModeOfTravel,'')      AS ModeOfTravel
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca
                INNER JOIN dbo.Affiliation_College_Master ac ON ac.CollegeCode = ca.CollegeCode
                LEFT  JOIN [Admission_Affiliation].[dbo].[LIC_Inspection] li
                       ON li.PhoneNumber = ca.MobileNo
                WHERE ca.FO_Level2_ApprovedStatus = 'Approved'
                  AND (ca.Cashier_Update IS NULL OR ca.Cashier_Update = '')
                  AND (@Year    IS NULL OR ca.AcademicYear = @Year)
                  AND (@Faculty IS NULL OR ca.FacultyCode  = @Faculty)
                  AND (@College IS NULL OR ca.CollegeCode  = @College)
                ORDER BY ac.CollegeName";
            return GroupToGrid(ExecRawMembers(sql, year, faculty, college));
        }

        // ══ MEMBER DETAIL ═══════════════════════════════════════════════════════

        public LicTadaMember GetMember(string mobileNo)
        {
            var m = new LicTadaMember();
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
                    [Cashier_Update],[CashierApprovedDate],[LicApprovalFile]
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
                WHERE MobileNo = @Phone";

            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(sql1, con))
            {
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
                m.LicApprovalFileName = S(dr, "LicApprovalFile");


                var licOrdinal = dr.GetOrdinal("LicApprovalFile");
                if (!dr.IsDBNull(licOrdinal))
                {
                    var licBytes = (byte[])dr.GetValue(licOrdinal);
                    if (licBytes != null && licBytes.Length > 0)
                        m.LicApprovalFileName = Convert.ToBase64String(licBytes);
                }
            }



            if (!string.IsNullOrEmpty(m.MobileNo))
            {
                const string sql2 = @"
                    SELECT TOP 1 [UploadBills]
                    FROM [Admission_Affiliation].[dbo].[LICClaimDetails]
                    WHERE PhoneNumber = @Phone";
                using var con2 = new SqlConnection(_conn);
                using var cmd2 = new SqlCommand(sql2, con2);
                cmd2.Parameters.AddWithValue("@Phone", m.MobileNo);
                con2.Open();
                using var dr2 = cmd2.ExecuteReader();
                if (dr2.Read())
                {
                    var ordinal = dr2.GetOrdinal("UploadBills");
                    if (!dr2.IsDBNull(ordinal))
                    {
                        var bytes = (byte[])dr2.GetValue(ordinal);
                        if (bytes != null && bytes.Length > 0)
                            m.UploadedBills = Convert.ToBase64String(bytes);
                    }
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
            using var con = new SqlConnection(_conn);
            con.Open();

            string collegeCode = "";
            string academicYear = "";

            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 CollegeCode, AcademicYear
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
                WHERE Id = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using var dr = cmd.ExecuteReader();
                if (!dr.Read()) return false;
                collegeCode = dr.IsDBNull(0) ? "" : dr.GetValue(0)?.ToString() ?? "";
                academicYear = dr.IsDBNull(1) ? "" : dr.GetValue(1)?.ToString() ?? "";
            }

            if (string.IsNullOrEmpty(collegeCode)) return false;

            using (var cmd = new SqlCommand(@"
                UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
                SET FO_Level1_ApprovedStatus = 'Forwarded',
                    FO_Level1_Remarks        = @Remarks,
                    FO_Level1_ForwardedDate  = GETDATE(),
                    UpdatedDate              = GETDATE()
                WHERE CollegeCode  = @CollegeCode
                  AND AcademicYear = @AcademicYear
                  AND (FO_Level1_ApprovedStatus IS NULL OR FO_Level1_ApprovedStatus = '')", con))
            {
                cmd.Parameters.AddWithValue("@Remarks", remarks ?? "");
                cmd.Parameters.AddWithValue("@CollegeCode", collegeCode);
                cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool SaveKm(int id, decimal km, decimal rkm)
        {
            decimal totalKm = km + rkm;
            bool shortTrip = totalKm > 0 && totalKm <= 40m;
            decimal travelCost = shortTrip ? 0m : totalKm * 15m;

            using var con = new SqlConnection(_conn);
            con.Open();

            string phone = "";
            decimal originalDA = 0m;
            decimal lcaCost = 0m;
            decimal collegeCost = 0m;
            decimal airRoadCost = 0m;
            decimal airFair = 0m;

            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 MobileNo,
                    ISNULL(DACost,     0),
                    ISNULL(LCACost,    0),
                    ISNULL(CollegeCost,0),
                    ISNULL(AirRoadCost,0),
                    ISNULL(AirFair,    0)
                FROM [Admission_Affiliation].[dbo].[LICCollegeApproval]
                WHERE Id = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using var dr = cmd.ExecuteReader();
                if (!dr.Read()) return false;
                phone = dr.IsDBNull(0) ? "" : dr.GetValue(0)?.ToString() ?? "";
                originalDA = dr.IsDBNull(1) ? 0m : Convert.ToDecimal(dr.GetValue(1));
                lcaCost = dr.IsDBNull(2) ? 0m : Convert.ToDecimal(dr.GetValue(2));
                collegeCost = dr.IsDBNull(3) ? 0m : Convert.ToDecimal(dr.GetValue(3));
                airRoadCost = dr.IsDBNull(4) ? 0m : Convert.ToDecimal(dr.GetValue(4));
                airFair = dr.IsDBNull(5) ? 0m : Convert.ToDecimal(dr.GetValue(5));
            }

            if (string.IsNullOrEmpty(phone)) return false;

            decimal effectiveDA = shortTrip ? 0m : originalDA;
            decimal totalClaim = travelCost + effectiveDA + lcaCost + collegeCost + airRoadCost + airFair;

            using (var cmd = new SqlCommand(@"
                UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
                SET Kilometers       = @KM,
                    ReturnKilometers = @RKM,
                    TravelCost       = @TravelCost,
                    DACost           = @DACost,
                    TotalClaimAmount = @TotalClaim,
                    UpdatedDate      = GETDATE()
                WHERE Id = @Id", con))
            {
                cmd.Parameters.AddWithValue("@KM", km);
                cmd.Parameters.AddWithValue("@RKM", rkm);
                cmd.Parameters.AddWithValue("@TravelCost", travelCost);
                cmd.Parameters.AddWithValue("@DACost", effectiveDA);
                cmd.Parameters.AddWithValue("@TotalClaim", totalClaim);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SqlCommand(@"
                UPDATE [Admission_Affiliation].[dbo].[LICClaimDetails]
                SET Kilometers       = @KM,
                    ReturnKilometers = @RKM,
                    TravelCost       = @TravelCost,
                    DACost           = @DACost,
                    TotalCost        = @TotalClaim
                WHERE PhoneNumber = @Phone", con))
            {
                cmd.Parameters.AddWithValue("@KM", km);
                cmd.Parameters.AddWithValue("@RKM", rkm);
                cmd.Parameters.AddWithValue("@TravelCost", travelCost);
                cmd.Parameters.AddWithValue("@DACost", effectiveDA);
                cmd.Parameters.AddWithValue("@TotalClaim", totalClaim);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        public bool CWVerify(int id, string remarks)
        {
            using var con = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
                UPDATE [Admission_Affiliation].[dbo].[LICCollegeApproval]
                SET F_CaseWorker_Approve_Status  = 'Verified',
                    F_CaseWorker_Approve_Remarks = @Remarks,
                    F_CaseWorker_Approved_Date   = GETDATE(),
                    UpdatedDate                  = GETDATE()
                WHERE Id = @Id", con);
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
                    UpdatedDate                  = GETDATE()
                WHERE Id = @Id", con);
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
                    UpdatedDate              = GETDATE()
                WHERE Id = @Id", con);
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
                    UpdatedDate              = GETDATE()
                WHERE Id = @Id", con);
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
                    UpdatedDate               = GETDATE()
                WHERE Id = @Id", con);
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
                    UpdatedDate               = GETDATE()
                WHERE Id = @Id", con);
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
            const string sql = @"
        SELECT ca.Id,
            ac.CollegeCode, ac.CollegeName, ac.CollegeTown,
            ca.AcademicYear, ca.TypeOfMembers, ca.memberName, ca.MobileNo,
            ca.TotalClaimAmount,
            ''                                         AS CaseWorkerRemarks,
            ISNULL(ca.FO_Level2_ApprovedStatus,'')     AS FO_Level2_Status,
            ISNULL(ca.Cashier_Update,'')               AS Cashier_Status,
            ISNULL(li.AccountHolderName,'') AS AccountHolderName,
            ISNULL(li.AccountNumber,'')     AS AccountNumber,
            ISNULL(li.IFSCCode,'')          AS IFSCCode,
            ISNULL(li.BankName,'')          AS BankName,
            ISNULL(li.BranchName,'')        AS BranchName,
            ISNULL(li.PANNumber,'')         AS PANNumber,
            ISNULL(li.AadhaarNumber,'')     AS AadhaarNumber,
            ISNULL(li.Email,'')             AS Email,
            ISNULL(li.Address,'')           AS Address,
            ISNULL(li.ModeOfTravel,'')      AS ModeOfTravel
        FROM [Admission_Affiliation].[dbo].[LICCollegeApproval] ca
        INNER JOIN dbo.Affiliation_College_Master ac ON ac.CollegeCode = ca.CollegeCode
        LEFT  JOIN [Admission_Affiliation].[dbo].[LIC_Inspection] li
               ON li.PhoneNumber = ca.MobileNo
        WHERE ca.FO_Level2_ApprovedStatus = 'Approved'
          AND ca.Cashier_Update = 'Paid'
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
    }
}