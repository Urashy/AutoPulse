using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IAutoCompleteService
    {
        Task<List<NominatimResult>> SearchAddressAsync(string query);
    }
}
