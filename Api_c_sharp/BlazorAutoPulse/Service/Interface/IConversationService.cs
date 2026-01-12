using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface IConversationService: IService<ConversationListDTO>
{
    Task<IEnumerable<ConversationListDTO>> GetConversationsByCompteID(int compteId, int idAnnonce);
    Task<ConversationListDTO> PostComplet(ConversationCreateDTO conversation, int idCompteEnvoie, int idCompteRecoi);
}