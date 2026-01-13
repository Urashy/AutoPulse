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

    protected BaseWebService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("ApiClient");
    }

    protected string BuildUrl(string relativeUrl = "")
    {
        return string.IsNullOrEmpty(relativeUrl)
            ? ApiEndpoint
            : $"{ApiEndpoint}/{relativeUrl}";
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            // GET api/Marque
            var request = new HttpRequestMessage(HttpMethod.Get, ApiEndpoint);
            var response = await SendWithCredentialsAsync(request);

            var rawContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"🔍 GET {ApiEndpoint} → {response.StatusCode}");
            Console.WriteLine($"🔍 Content: {rawContent.Substring(0, Math.Min(200, rawContent.Length))}...");

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<IEnumerable<T>>(rawContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? Enumerable.Empty<T>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ HTTP Error GetAllAsync: {ex.Message}");
            throw;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"❌ JSON Error GetAllAsync: {ex.Message}");
            throw;
        }
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        try
        {
            // GET api/Marque/5
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(id.ToString()));
            var response = await SendWithCredentialsAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GetByIdAsync({id}): {ex.Message}");
            throw;
        }
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        try
        {
            // POST api/Marque
            var request = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint)
            {
                Content = JsonContent.Create(entity)
            };

            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CreateAsync: {ex.Message}");
            throw;
        }
    }

    public virtual async Task UpdateAsync(int id, T entity)
    {
        try
        {
            // PUT api/Marque/5
            var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl(id.ToString()))
            {
                Content = JsonContent.Create(entity)
            };

            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ UpdateAsync({id}): {ex.Message}");
            throw;
        }
    }

    public virtual async Task<string?> DeleteAsync(int id)
    {
        try
        {
            // DELETE api/Marque/5
            var request = new HttpRequestMessage(HttpMethod.Delete, BuildUrl(id.ToString()));
            var response = await SendWithCredentialsAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ DeleteAsync({id}): {ex.Message}");
            return ex.Message;
        }
    }

    protected async Task<HttpResponseMessage> SendWithCredentialsAsync(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return await _httpClient.SendAsync(request);
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

}