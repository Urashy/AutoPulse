namespace Api_c_sharp.DTO;

public class SignalementDTO
{
    public int IdSignalement { get; set; }
    public string DescriptionSignalement { get; set; }
    public DateTime DateCreationSignalement { get; set; }
    public string PseudoSignalant { get; set; }
    public string PseudoSignale { get; set; }
    public string LibelleTypeSignalement { get; set; }
}