namespace Medical_Affiliation.Models
{
    public class LandBuildingDetailsVM
    {
        // LAND DETAILS
        public bool AgreeTerms { get; set; }

        public int RequiredBuildingArea { get; set; }

        public decimal LandAcres { get; set; }
        public long BuildingArea { get; set; }
        public string BuildingType { get; set; }
        public string OwnerName { get; set; }

        // COURT CASE & COURSES
        public string CourtCase { get; set; }
        public string CoursesInBuilding { get; set; }

        // CLASSROOMS & LABS
        public int Classrooms { get; set; }
        public int Labs { get; set; }

        // IDENTIFIERS
        public string Survey { get; set; }
        public string RR { get; set; }

        // FACILITIES
        public string WaterSupply { get; set; }
        public string Auditorium { get; set; }
        public string OfficeFacilities { get; set; }
        public string Seating { get; set; }
        public string Electricity { get; set; }

        // CERTIFICATE NUMBERS
        public string BlueprintCertNo { get; set; }
        public string ApprovalCertNo { get; set; }
        public string TaxCertNo { get; set; }
        public string RTCCertNo { get; set; }
        public string OccupancyCertNo { get; set; }
        public string SaleDeedCertNo { get; set; }


        // 📄 PDF FILES (Uploads)
        public IFormFile BlueprintDoc { get; set; }
        public IFormFile ApprovalCert { get; set; }
        public IFormFile TaxReceipt { get; set; }
        public IFormFile RTC { get; set; }
        public IFormFile OccupancyCert { get; set; }
        public IFormFile SaleDeed { get; set; }


        // Existing PDF files from DB (for View PDF)
        public byte[] BlueprintPdf { get; set; }
        public byte[] ApprovalCertPdf { get; set; }
        public byte[] TaxReceiptPdf { get; set; }
        public byte[] RTCPdf { get; set; }
        public byte[] OccupancyCertPdf { get; set; }
        public byte[] SaleDeedPdf { get; set; }
    }

}
