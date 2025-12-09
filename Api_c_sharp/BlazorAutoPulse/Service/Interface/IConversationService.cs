using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface IConversationService: IService<ConversationDetailDTO>
{
    Task<IEnumerable<ConversationDetailDTO>> GetConversationsByCompteID(int compteId);
}