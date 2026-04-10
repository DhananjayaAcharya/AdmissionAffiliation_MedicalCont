using System.ComponentModel.DataAnnotations;

public class CA_Med_Lib_CommitteeVM
{

    
    public int CommitteeId { get; set; }

    public string CommitteeName { get; set; } = "";

    [Required(ErrorMessage = "Please select Yes or No")]
    public string IsPresent { get; set; } = "";   // 🔥 SAME AS DB

    public IFormFile? CommitteePdf { get; set; }

    public string? CommitteePdfName { get; set; }

    public List<CA_Med_Lib_CommitteeVM> Committees { get; set; } = new();




}
