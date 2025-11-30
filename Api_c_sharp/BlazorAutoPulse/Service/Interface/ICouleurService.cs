using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface;

public interface ICouleurService: IService<Couleur>
{
    Task<List<Couleur>> GetCouleursByVoitureId(int voitureId);
}