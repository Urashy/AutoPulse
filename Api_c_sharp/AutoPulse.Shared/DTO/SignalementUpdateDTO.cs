namespace AutoPulse.Shared.DTO;

public class SignalementUpdateDTO
{
    public int IdSignalement { get; set; }
    public string DescriptionSignalement { get; set; }
    public int IdCompteSignalant { get; set; }

    public int? IdAnnonceSignale { get; set; }

    public int? IdCompteSignale { get; set; }

    public int IdTypeSignalement { get; set; }
    public int IdEtatSignalement { get; set; }
}