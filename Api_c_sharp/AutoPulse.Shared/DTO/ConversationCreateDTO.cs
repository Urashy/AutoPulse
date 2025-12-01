namespace Api_c_sharp.DTO;

public class ConversationCreateDTO
{
    public int IdConversation { get; set; }
    public int IdAnnonce { get; set; }
    public DateTime DateDernierMessage { get; set; }
}