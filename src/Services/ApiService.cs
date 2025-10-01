using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AITeacherAssistant.Services;

/// <summary>
/// Service for handling all n8n API communications
/// </summary>
public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://n8n.yarden-zamir.com/webhook/ita/api";

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        
        // Set timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Create a new session in the n8n backend
    /// </summary>
    /// <param name="code">5-letter session code</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> CreateSession(string code)
    {
        try
        {
            var payload = new
            {
                code = code,
                type = "session_create",
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/sessions/create", content);
            
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            // Network error
            return false;
        }
        catch (TaskCanceledException)
        {
            // Timeout error
            return false;
        }
        catch (Exception)
        {
            // Other errors
            return false;
        }
    }

    /// <summary>
    /// Get the status of a session
    /// </summary>
    /// <param name="code">5-letter session code</param>
    /// <returns>Session status information</returns>
    public async Task<SessionStatus?> GetSessionStatus(string code)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/sessions/{code}/status");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SessionStatus>(json);
            }
            
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Connect a user to an existing session
    /// TODO: Implement when n8n endpoint is ready
    /// </summary>
    /// <param name="code">5-letter session code</param>
    /// <param name="userInfo">User connection information</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> ConnectToSession(string code, object userInfo)
    {
        try
        {
            // TODO: Implement when n8n endpoint is ready
            // var payload = new { code = code, user = userInfo };
            // var json = JsonSerializer.Serialize(payload);
            // var content = new StringContent(json, Encoding.UTF8, "application/json");
            // var response = await _httpClient.PostAsync($"/sessions/{code}/connect", content);
            // return response.IsSuccessStatusCode;
            
            await Task.Delay(100); // Placeholder
            return false; // Not implemented yet
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Dispose of the HTTP client
    /// </summary>
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// Represents the status of a session
/// </summary>
public class SessionStatus
{
    public string Code { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public string? UserId { get; set; }
}
