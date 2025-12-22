using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    public class OffreWebService : BaseWebService<OffreDTO>, IOffreService
    {
        public OffreWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Offre";

        public async Task<IEnumerable<OffreDTO>> GetOffresByMessageAsync(int idMessage)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    BuildUrl($"GetByMessage/{idMessage}")
                );
                var response = await SendWithCredentialsAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<IEnumerable<OffreDTO>>()
                       ?? Enumerable.Empty<OffreDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetOffresByMessageAsync: {ex.Message}");
                return Enumerable.Empty<OffreDTO>();
            }
        }
        public async Task<bool> AccepterOffreAsync(int idOffre)
        {
            try
            {
                Console.WriteLine($"🔄 Acceptation offre {idOffre}...");

                // ✅ Récupérer l'offre actuelle (maintenant avec IdAnnonce inclus)
                var offre = await GetByIdAsync(idOffre);
                if (offre == null)
                {
                    Console.WriteLine($"❌ Offre {idOffre} introuvable");
                    return false;
                }

                // ✅ Créer le DTO de mise à jour avec EstAccepte = true
                var updateDto = new OffreUpdateDTO
                {
                    IdOffre = offre.IdOffre,
                    IdMessage = offre.IdMessage,
                    Valeur = offre.Valeur,
                    IdAnnonce = offre.IdAnnonce,
                    DateOffre = offre.DateOffre,
                    EstAccepte = true // ✅ ACCEPTER
                };

                var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    BuildUrl($"Put/{idOffre}")
                )
                {
                    Content = JsonContent.Create(updateDto)
                };

                var response = await SendWithCredentialsAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Offre {idOffre} acceptée avec succès");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Erreur acceptation ({response.StatusCode}): {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception AccepterOffreAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RefuserOffreAsync(int idOffre)
        {
            try
            {
                Console.WriteLine($"🔄 Refus offre {idOffre}...");

                // ✅ Récupérer l'offre actuelle
                var offre = await GetByIdAsync(idOffre);
                if (offre == null)
                {
                    Console.WriteLine($"❌ Offre {idOffre} introuvable");
                    return false;
                }

                // ✅ Créer le DTO de mise à jour avec EstAccepte = false
                var updateDto = new OffreUpdateDTO
                {
                    IdOffre = offre.IdOffre,
                    IdMessage = offre.IdMessage,
                    Valeur = offre.Valeur,
                    DateOffre = offre.DateOffre,
                    IdAnnonce = offre.IdAnnonce,
                    EstAccepte = false // ✅ REFUSER
                };

                var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    BuildUrl($"Put/{idOffre}")
                )
                {
                    Content = JsonContent.Create(updateDto)
                };

                var response = await SendWithCredentialsAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Offre {idOffre} refusée avec succès");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Erreur refus ({response.StatusCode}): {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception RefuserOffreAsync: {ex.Message}");
                return false;
            }
        }
        public async Task CreateAsync(OffreCreateDTO offreDto)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    BuildUrl($"Post")
                )
                {
                    Content = JsonContent.Create(offreDto)
                };
                var response = await SendWithCredentialsAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur CreateAsync: {ex.Message}");
            }
        }
    }
}