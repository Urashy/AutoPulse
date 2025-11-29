using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface;

public interface ICompteService: IService<Compte>
{
    Task<Compte> GetByNameAsync(string name);
    Task<Compte> GetMe();
    Task<bool> VerifUser(ChangementMdp changementMdp);
    Task<bool> ChangementMdp(ChangementMdp changementMdp);
}