using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface
{
    public interface ICarteBancaireService
    {
        Task<IEnumerable<CarteBancaireDTO>> GetCarteBancaireByCompte(int id);
    }
}
