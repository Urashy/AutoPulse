using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IPlainteService : IService<PlainteCreateDTO>
    {
        Task<IEnumerable<PlainteDTO>> GetAllPlaintesAsync();
        Task UpdatePlainteAsync(int id, PlainteUpdateDTO entity);

    }
}