using ServerWatchAgent.Models;

namespace ServerWatchAgent.Services;

public sealed class ServiceHealthService
{
    private readonly HttpClient _httpClient = new();

    public async Task<ServiceHealth> GetStatusAsync(
        string name,
        string url)
    {
        try
        {
            using var response = await _httpClient.GetAsync(url);

            return new ServiceHealth
            {
                Name = name,
                State = response.IsSuccessStatusCode
                    ? ServiceState.Up
                    : ServiceState.Degraded,
                Detail = $"HTTP {(int)response.StatusCode}"
            };
        }
        catch (HttpRequestException exception)
        {
            return new ServiceHealth
            {
                Name = name,
                State = ServiceState.Down,
                Detail = exception.Message
            };
        }
        catch (TaskCanceledException)
        {
            return new ServiceHealth
            {
                Name = name,
                State = ServiceState.Down,
                Detail = "Request timed out"
            };
        }
    }
}
