namespace Api_c_sharp.DTO;

public class ConversationDetailDTO
{
    public int IdConversation { get; set; }
    public int IdAnnonce { get; set; }
    public string LibelleAnnonce { get; set; }
    public List<MessageDTO> Messages { get; set; } = new List<MessageDTO>();
    public List<CompteListDTO> Participants { get; set; } = new List<CompteListDTO>();
}