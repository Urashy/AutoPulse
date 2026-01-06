using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface APourConversationService: IService<APourConversationDTO>
{
    Task<bool> ConvExist(int idCompteUn, int idCompteDeux, int idAnnonce);
}