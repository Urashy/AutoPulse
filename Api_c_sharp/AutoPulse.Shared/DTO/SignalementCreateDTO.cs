namespace Api_c_sharp.DTO;

public class SignalementCreateDTO
{
    public string DescriptionSignalement { get; set; }
    public int IdCompteSignale { get; set; }
    public int IdTypeSignalement { get; set; }
}