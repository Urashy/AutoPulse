using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

public interface IOffreService : IService<OffreDTO>
{
    Task<IEnumerable<OffreDTO>> GetOffresByMessageAsync(int idMessage);
    Task<bool> AccepterOffreAsync(int idOffre);
    Task<bool> RefuserOffreAsync(int idOffre);
    Task CreateAsync(OffreCreateDTO offreDto);
}