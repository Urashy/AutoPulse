namespace AutoPulse.Shared.DTO;

public class SignalementAnnonceDTO
{
    public int IdSignalement { get; set; }
    public string DescriptionSignalement { get; set; }
    public DateTime DateCreationSignalement { get; set; }
    public string PseudoSignalant { get; set; }
    public string LibelleAnnonceSignale { get; set; }
    public int IdAnnonceSignale { get; set; }
    public string LibelleTypeSignalement { get; set; }
}