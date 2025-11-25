using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface;

public interface ICompteService: IService<Compte>
{
    Task<Compte> GetMe();
}