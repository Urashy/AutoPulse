using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using AutoMapper;
using AutoPulse.Shared.DTO;

namespace Api_c_sharp.Models.Repository.Managers;

public class ConversationEnrichmentService : IConversationEnrichmentService
{
    private readonly MessageManager _messageManager;
    private readonly IMapper _mapper;

    public ConversationEnrichmentService(
        MessageManager messageManager,
        IMapper mapper)
    {
        _messageManager = messageManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ConversationListDTO>> EnrichConversationsAsync(
        IEnumerable<Conversation> conversations,
        int currentUserId)
    {
        var result = _mapper.Map<IEnumerable<ConversationListDTO>>(conversations).ToList();

        foreach (var conversationDTO in result)
        {
            var conversation = conversations.FirstOrDefault(c => c.IdConversation == conversationDTO.IdConversation);

            if (conversation != null)
            {
                // Enrichir avec les informations du participant
                EnrichWithParticipantInfo(conversationDTO, conversation, currentUserId);

                // Enrichir avec le nombre de messages non lus
                conversationDTO.NombreNonLu = await _messageManager.GetUnreadMessageCount(
                    conversationDTO.IdConversation,
                    currentUserId);
            }
        }

        return result;
    }

    private void EnrichWithParticipantInfo(
        ConversationListDTO dto,
        Conversation conversation,
        int currentUserId)
    {
        var autreParticipant = conversation.ApourConversations
            .FirstOrDefault(apc => apc.IdCompte != currentUserId);

        dto.ParticipantPseudo = autreParticipant?.APourConversationCompteNav?.Pseudo ?? "Utilisateur inconnu";
        dto.IdParticipant = autreParticipant?.APourConversationCompteNav?.IdCompte ?? 0;
    }
}