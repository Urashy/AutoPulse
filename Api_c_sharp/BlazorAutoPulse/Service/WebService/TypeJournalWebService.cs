using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.WebService;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    public class TypeJournalWebService : BaseWebService<TypeJournalDTO>, IService<TypeJournalDTO>
    {
        public TypeJournalWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "TypeJournal";
    }
}