using Api_c_sharp.Models.Entity;
using AutoPulse.Shared.DTO;

namespace Api_c_sharp.Models.Repository.Interfaces;

public interface IConversationEnrichmentService
{
    Task<IEnumerable<ConversationListDTO>> EnrichConversationsAsync(
        IEnumerable<Conversation> conversations, 
        int currentUserId);
}