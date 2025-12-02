using System.Net.Http.Headers;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BlazorAutoPulse.Model;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BlazorAutoPulse.Service;

public abstract class BaseWebService<T> : IService<T> where T : class
{
    protected readonly HttpClient _httpClient;
    protected abstract string ApiEndpoint { get; }
    
    protected BaseWebService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    protected string BuildUrl(string relativeUrl)
    {
        return $"{ApiEndpoint}/{relativeUrl}";
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("GetAll"));
        var response = await SendWithCredentialsAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<T>>();
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetById/{id.ToString()}"));
        var response = await SendWithCredentialsAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        var json = JsonSerializer.Serialize(entity);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("Post"));
        request.Content = content;

        var response = await SendWithCredentialsAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public virtual async Task UpdateAsync(int id, T entity)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"Put/{id}"))
        {
            Content = JsonContent.Create(entity)
        };
        
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, BuildUrl($"Delete/{id}"));
        var response = await SendWithCredentialsAsync(request);

        response.EnsureSuccessStatusCode();
    }
    
    public async Task<ServiceResult<T>> PostWithErrorHandlingAsync(T entity, string action = "Post")
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl(action))
            {
                Content = JsonContent.Create(entity)
            };

            var response = await SendWithCredentialsAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>();
                return ServiceResult<T>.SuccessResult(result);
            }

            // Gestion des erreurs de validation (400)
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                try
                {
                    var validationError = JsonSerializer.Deserialize<ValidationErrorResponse>(
                        errorContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (validationError?.Errors != null && validationError.Errors.Any())
                    {
                        var errorMessages = validationError.Errors
                            .SelectMany(e => e.Value)
                            .ToList();
                        
                        return ServiceResult<T>.ErrorResult(
                            string.Join("\n", errorMessages),
                            validationError.Errors
                        );
                    }
                }
                catch (JsonException)
                {
                    return ServiceResult<T>.ErrorResult("Erreur de validation : " + errorContent);
                }
            }

            return ServiceResult<T>.ErrorResult($"Erreur {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception PostWithErrorHandlingAsync : {ex.Message}");
            return ServiceResult<T>.ErrorResult("Une erreur s'est produite");
        }
    }
    
    protected async Task<HttpResponseMessage> SendWithCredentialsAsync(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return await _httpClient.SendAsync(request);
    }
}