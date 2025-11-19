using System.Net.Http.Json;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service;

public class CompteWebService : BaseWebService<Compte>
{
    protected override string ApiEndpoint => "Compte";
}