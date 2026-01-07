using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface IAPourConversationService: IService<APourConversationDTO>
{
    Task<bool> ConvExist(int idCompteUn, int idCompteDeux, int idAnnonce);
}