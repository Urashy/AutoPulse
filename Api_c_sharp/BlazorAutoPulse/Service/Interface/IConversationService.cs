using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface IConversationService: IService<ConversationListDTO>
{
    Task<IEnumerable<ConversationListDTO>> GetConversationsByCompteID(int compteId);
}