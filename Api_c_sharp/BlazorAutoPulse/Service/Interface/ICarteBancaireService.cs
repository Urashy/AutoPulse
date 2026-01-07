using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface
{
    public interface ICarteBancaireService : IService<CarteBancaireDTO>
    {
        Task<IEnumerable<CarteBancaireDTO>> GetCarteBancaireByCompte(int id);
        Task UpdateAdresseAsync(int id, CarteBancaireUpdateDTO entity);
    }
}
