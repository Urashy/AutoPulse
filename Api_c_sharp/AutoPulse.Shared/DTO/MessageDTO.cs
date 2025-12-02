namespace AutoPulse.Shared.DTO;

public class MessageDTO
{
    public int IdConversation { get; set; }
    public int IdMessage { get; set; }
    public string ContenuMessage { get; set; }
    public DateTime DateEnvoiMessage { get; set; }
    public int IdCompte { get; set; }
    public string PseudoCompte { get; set; }
}