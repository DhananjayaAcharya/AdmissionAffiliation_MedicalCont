namespace Medical_Affiliation.Models
{
    public class MedicalUGBedDistributionVm
    {
        public int Id { get; set; }


        public int? GenMedicine { get; set; }
        public int? Paediatrics { get; set; }
        public int? SkinVD { get; set; }
        public int? Psychiatry { get; set; }


        public int? GenSurgery { get; set; }
        public int? Orthopaedics { get; set; }
        public int? Ophthalmology { get; set; }
        public int? ENT { get; set; }


        public int? ObstetricsANC { get; set; }
        public int? Gynaecology { get; set; }
        public int? Postpartum { get; set; }


        public int? MajorOT { get; set; }
        public int? MinorOT { get; set; }


        public int? ICCU { get; set; }
        public int? ICU { get; set; }
        public int? PICU_NICU { get; set; }
        public int? SICU { get; set; }
        public int? TotalICUBeds { get; set; }
        public int? CasualtyBeds { get; set; }
    }
}