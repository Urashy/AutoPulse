using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface IMessageService: IService<MessageDTO>
{
    Task<IEnumerable<MessageDTO>> GetMessagesByConversationAndMarkAsRead(int conversationId, int userId);
}