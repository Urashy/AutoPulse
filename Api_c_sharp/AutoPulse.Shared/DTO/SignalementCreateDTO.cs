namespace AutoPulse.Shared.DTO;

public class SignalementCreateDTO
{
    public int IdSignalement { get; set; }
    public string DescriptionSignalement { get; set; }
    public int IdCompteSignale { get; set; }
    public int IdCompteSignalant { get; set; }
    public int IdTypeSignalement { get; set; }
}