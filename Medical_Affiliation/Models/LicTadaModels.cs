using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models
{
    // ── Dropdown item ─────────────────────────────────────────────────────────
    public class LicTadaDDItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    // ── Dashboard VM (filter dropdowns) ──────────────────────────────────────
    public class LicTadaDashboardVM
    {
        public List<LicTadaDDItem> AcademicYears { get; set; } = new();
        public List<LicTadaDDItem> Faculties { get; set; } = new();
        public List<LicTadaDDItem> Colleges { get; set; } = new();
    }

    // ── Grid row (used by ALL dashboards) ─────────────────────────────────────
    public class LicTadaGridRow
    {
        public int MemberId { get; set; }
        public int SenetMemberId { get; set; }
        public int AcMemberId { get; set; }
        public int SubjectExpertiseId { get; set; }

        public string CollegeCode { get; set; } = "";
        public string CollegeName { get; set; } = "";
        public string CollegeTown { get; set; } = "";
        public string AcademicYear { get; set; } = "";

        public string SenetMember { get; set; } = "";
        public string SenetMemberPhNo { get; set; } = "";
        public string AcMember { get; set; } = "";
        public string AcMemberPhNo { get; set; } = "";
        public string SubjectExpertise { get; set; } = "";
        public string SubjectExpertisePhNo { get; set; } = "";

        public decimal TotalClaimAmount { get; set; }
        public string CaseWorkerRemarks { get; set; } = "";
        public string FO_Level2_Status { get; set; } = "";
        public string Cashier_Status { get; set; } = "";

        // ── Bank / Account details (Cashier view only) ──────────
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

        public decimal SenetMemberAmount { get; set; }
        public decimal AcMemberAmount { get; set; }
        public decimal SubjectExpertiseAmount { get; set; }
    }
    // ── Full member detail (modal) ────────────────────────────────────────────
    public class LicTadaMember
    {
        public int Id { get; set; }
        public string FacultyCode { get; set; } = string.Empty;
        public string CollegeCode { get; set; } = string.Empty;
        public string TypeOfMembers { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;

        public string MobileNo { get; set; } = string.Empty;
        public string FromPlace { get; set; } = string.Empty;
        public string ToPlace { get; set; } = string.Empty;
        public decimal Kilometers { get; set; }
        public string ReturnFromPlace { get; set; } = string.Empty;
        public string ReturnToPlace { get; set; } = string.Empty;
        public decimal ReturnKilometers { get; set; }
        public decimal TotalClaimAmount { get; set; }
        public bool IsBanglore { get; set; }
        public decimal AirFair { get; set; }
        public int NoOfDays { get; set; }
        public decimal TravelCost { get; set; }
        public decimal DACost { get; set; }
        public decimal LCACost { get; set; }
        public decimal CollegeCost { get; set; }
        public decimal AirRoadCost { get; set; }
        public string Division { get; set; } = string.Empty;
        public bool IsLCA { get; set; }
        public string DR_ApprovalStatus { get; set; } = string.Empty;
        public string DR_Remarks { get; set; } = string.Empty;
        public string DRApprovedBy { get; set; } = string.Empty;
        public DateTime? DRApprovedDate { get; set; }
        public string FO_Level1_ApprovedStatus { get; set; } = string.Empty;
        public string FO_Level1_Remarks { get; set; } = string.Empty;
        public DateTime? FO_Level1_ForwardedDate { get; set; }
        public string F_CaseWorker_Approve_Status { get; set; } = string.Empty;
        public string F_CaseWorker_Approve_Remarks { get; set; } = string.Empty;
        public DateTime? F_CaseWorker_Approved_Date { get; set; }
        public string F_AO_SP_Approved_Status { get; set; } = string.Empty;
        public string F_AO_SP_Approved_Remarks { get; set; } = string.Empty;
        public DateTime? F_AO_SP_Approved_Date { get; set; }
        public string FO_Level2_ApprovedStatus { get; set; } = string.Empty;
        public string FO_Level2_ApprovedRemarks { get; set; } = string.Empty;
        public DateTime? FO_Level2_ApprovedDate { get; set; }
        public string Cashier_Update { get; set; } = string.Empty;
        public DateTime? CashierApprovedDate { get; set; }
        public string LicApprovalFileName { get; set; } = string.Empty;

        //public string LicApprovalFileName { get; set; } = string.Empty;

        public string UploadedBills { get; set; } = string.Empty;
    }

    // ── Request models (form-encoded) ─────────────────────────────────────────
    public class LicTadaForwardReq
    {
        public int Id { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    public class LicTadaVerifyReq
    {
        public int Id { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    public class LicTadaRejectReq
    {
        public int Id { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    public class LicTadaApproveReq
    {
        public int Id { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    public class LicTadaCashierReq
    {
        public int Id { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}