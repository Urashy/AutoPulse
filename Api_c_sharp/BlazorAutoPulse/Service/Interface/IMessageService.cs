using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface IMessageService: IService<MessageDTO>
{
    Task<IEnumerable<MessageDTO>> GetMessagesParConversation(int idConv);
}