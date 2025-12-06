using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface;

public interface ICompteService: IService<Compte>
{
    Task<Compte> GetByNameAsync(string name);
    Task<CompteDetailDTO> GetMe();
    Task<bool> VerifUser(ChangementMdp changementMdp);
    Task<ServiceResult<bool>> ChangementMdp(ChangementMdp changementMdp);
    Task<bool> Anonymisation(int idCompte);
    Task<bool> PutTypeCompte(int idCompte, CompteModifTypeCompteDTO compte);
    new Task<IEnumerable<CompteGetDTO>> GetAllAsync();

}