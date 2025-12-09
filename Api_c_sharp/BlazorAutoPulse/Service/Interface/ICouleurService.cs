using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface;

public interface ICouleurService: IService<CouleurDTO>
{
    Task<List<CouleurDTO>> GetCouleursByVoitureId(int voitureId);
}