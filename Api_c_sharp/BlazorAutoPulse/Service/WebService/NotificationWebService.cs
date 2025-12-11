using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class NotificationWebService : BaseWebService<NotificationDTO>, INotificationService
    {
        public NotificationWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Notification";

        public async Task<IEnumerable<NotificationDTO>> GetNotificationsByCompteAsync(int idCompte)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetNotificationByCompteID/{idCompte}"));
            var response = await SendWithCredentialsAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<NotificationDTO>();
            }
            
            return await response.Content.ReadFromJsonAsync<IEnumerable<NotificationDTO>>() 
                   ?? Enumerable.Empty<NotificationDTO>();
        }

        public async Task<IEnumerable<NotificationDTO>> GetUnreadNotificationsByCompteAsync(int idCompte)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetUnreadNotificationByCompte/{idCompte}"));
            var response = await SendWithCredentialsAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<NotificationDTO>();
            }
            
            return await response.Content.ReadFromJsonAsync<IEnumerable<NotificationDTO>>() 
                   ?? Enumerable.Empty<NotificationDTO>();
        }

        public async Task<int> GetUnreadCountAsync(int idCompte)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetUnreadCountByCompte/{idCompte}"));
                var response = await SendWithCredentialsAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    return 0;
                }
                
                return await response.Content.ReadFromJsonAsync<int>();
            }
            catch
            {
                return 0;
            }
        }

        public async Task MarkAsReadAsync(int idNotification)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"MarkAsRead/{idNotification}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task MarkAllAsReadAsync(int idCompte)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"MarkAllAsRead/{idCompte}"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }