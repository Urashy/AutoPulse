namespace AutoPulse.Shared.DTO;

public class ConversationCreateDTO
{
    public int IdAnnonce { get; set; }
    public string? message { get; set; }
    public DateTime DateDernierMessage { get; set; }
}