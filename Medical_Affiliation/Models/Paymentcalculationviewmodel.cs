using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models
{
    /// <summary>
    /// Top-level ViewModel for the College Affiliation Payment Calculation page.
    /// Holds the filter inputs, dropdown lists, and the calculated results
    /// returned by usp_CalculateCollegeAffiliationPayment.
    /// </summary>
    public class PaymentCalculationViewModel
    {
        // ---- Filter inputs (posted from the form) ----
        public string CollegeCode { get; set; }
        public int FacultyCode { get; set; }
        public int AffiliationTypeId { get; set; }
        public string TypeOfAffiliation { get; set; }
        public string CourseLevel { get; set; }
        public string CourseLevelGroup { get; set; }
        public string AcademicYear { get; set; } = "2025-26";

        // ---- Dropdown sources ----
        public List<AffiliationTypeOption> AffiliationTypeList { get; set; } = new List<AffiliationTypeOption>();

        // ---- Results (populated only after Calculate is clicked) ----
        public bool HasResult { get; set; }
        public string ErrorMessage { get; set; }

        public List<MatchedCourseVM> MatchedCourses { get; set; } = new List<MatchedCourseVM>();
        public List<FeeLineVM> FeeLines { get; set; } = new List<FeeLineVM>();
        public PaymentSummaryVM Summary { get; set; }
    }

    public class AffiliationTypeOption
    {
        public int AffiliationTypeId { get; set; }
        public string AffiliationCategory { get; set; }
        public string FormNo { get; set; }
    }

    /// <summary>Maps to Result Set 1 of usp_CalculateCollegeAffiliationPayment.</summary>
    public class MatchedCourseVM
    {
        public int SLNO { get; set; }
        public int Facultycode { get; set; }
        public string coll_code { get; set; }
        public string collegename { get; set; }
        public string course { get; set; }
        public string ug_pg { get; set; }
        public int? Intake_26_27 { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string RawCourseLevel { get; set; }
        public string MatchNote { get; set; }
    }

    /// <summary>Maps to Result Set 2 of usp_CalculateCollegeAffiliationPayment.</summary>
    public class FeeLineVM
    {
        public string FeeHeadName { get; set; }
        public decimal UnitAmount { get; set; }
        public bool IsPerCourse { get; set; }
        public bool IsPerSeat { get; set; }
        public int? SeatRangeFrom { get; set; }
        public int? SeatRangeTo { get; set; }
        public int Multiplier { get; set; }
        public decimal LineAmount { get; set; }
    }

    /// <summary>Maps to Result Set 3 of usp_CalculateCollegeAffiliationPayment.</summary>
    public class PaymentSummaryVM
    {
        public string CollegeCode { get; set; }
        public int FacultyCode { get; set; }
        public int AffiliationTypeId { get; set; }
        public string CourseLevelGroup { get; set; }
        public int MatchedCourseCount { get; set; }
        public int TotalIntakeSeats { get; set; }
        public decimal GrandTotal { get; set; }
    }
}