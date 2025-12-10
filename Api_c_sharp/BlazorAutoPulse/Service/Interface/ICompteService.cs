using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface;

public interface ICompteService: IService<CompteDetailDTO>
{
    Task<ServiceResult<CompteCreateDTO>>PostWithErrorHandlingAsync(CompteCreateDTO compte);
    Task<Compte> GetByNameAsync(string name);
    Task<CompteDetailDTO> GetMe();
    Task<bool> VerifUser(ChangementMdp changementMdp);
    Task<ServiceResult<bool>> ChangementMdp(ChangementMdp changementMdp);
    Task<bool> Anonymisation(int idCompte);
    Task<bool> PutTypeCompte(int idCompte, CompteModifTypeCompteDTO compte);
    new Task<IEnumerable<CompteGetDTO>> GetAllAsync();
    Task<IEnumerable<CompteGetDTO>> GetByTypeCompteAsync(int idTypeCompte);
    Task<CompteProfilPublicDTO> GetComptePublicById(int id);
    Task <bool> ToggleSuspention(int idCompte);
}