namespace AutoPulse.Shared.DTO;

public class ConversationListDTO
{
    public int IdConversation { get; set; }
    public int IdAnnonce { get; set; }
    public string LibelleAnnonce { get; set; }
    public string DernierMessage { get; set; }
    public DateTime DateDernierMessage { get; set; }
    public string ParticipantPseudo { get; set; }
    public int IdParticipant { get; set; }
    
    public int NombreNonLu { get; set; }
}