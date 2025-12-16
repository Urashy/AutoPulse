using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface IVoitureService: IService<VoitureDetailDTO>
{
    Task UpdateVoitureAsync(int id, VoitureUpdateDTO entity);
}