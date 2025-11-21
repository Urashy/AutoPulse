namespace Api_c_sharp.DTO;

public class ConversationListDTO
{
    public int IdConversation { get; set; }
    public int IdAnnonce { get; set; }
    public string LibelleAnnonce { get; set; }
    public string DernierMessage { get; set; }
    public DateTime DateDernierMessage { get; set; }
    public List<string> ParticipantsPseudos { get; set; }
}