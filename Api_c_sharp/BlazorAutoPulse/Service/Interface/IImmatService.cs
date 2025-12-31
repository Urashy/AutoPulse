using AutoPulse.Shared.DTO.Immat;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IImmatService
    {
        Task<ImmatResponseDTO> SearchByPlateNumberAsync(string plateNumber);
    }
}