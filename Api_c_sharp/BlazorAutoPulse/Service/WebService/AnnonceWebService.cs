using System.Net.Http.Json;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service;

public class AnnonceWebService : BaseWebService<Annonce>
{
    protected override string ApiEndpoint => "Annonce";
}