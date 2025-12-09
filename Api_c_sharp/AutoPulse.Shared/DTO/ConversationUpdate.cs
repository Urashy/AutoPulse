namespace AutoPulse.Shared.DTO;

public class ConversationUpdateDTO
{
    public int IdConversation { get; set; }
    public int IdAnnonce { get; set; }
    public DateTime DateDernierMessage { get; set; }
}