using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    public class MoyenPaiementWebService : BaseWebService<MoyenPaiementDTO>, IMoyenPaiementService
    {
        public MoyenPaiementWebService(IHttpClientFactory factory) : base(factory)
        {

        }
        protected override string ApiEndpoint => "moyenpaiement";
    }

}
