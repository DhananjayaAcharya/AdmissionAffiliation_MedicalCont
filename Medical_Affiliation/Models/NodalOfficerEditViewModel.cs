namespace Medical_Affiliation.Models
{
    public class NodalOfficerEditViewModel
    {
        public int Id { get; set; }
        public string NodalOfficerName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailId { get; set; }

        public List<InitiativeCheckboxViewModel> InitiativeList { get; set; }
    }


}
