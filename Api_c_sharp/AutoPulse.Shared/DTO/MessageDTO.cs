namespace AutoPulse.Shared.DTO;

public class MessageDTO
{
    public int IdMessage { get; set; }
    public string ContenuMessage { get; set; }
    public DateTime DateEnvoiMessage { get; set; }
    public int IdExpediteur { get; set; }
    public string PseudoExpediteur { get; set; }
}