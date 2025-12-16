using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IIAService
    {
        Task<ResultatCNN> RecognizeVehicleAsync(DataCNN data);
        Task<ResultatPrediction> PredictPriceAsync(DataPrediction data);
        Task<ResultatAjustement> AdjustPriceAsync(DataAjustement data);
        Task<bool> HealthCheckAsync();
    }
}