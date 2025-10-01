using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AITeacherAssistant/1.0");
        
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
            // Match the working API format: {"code":"FLOWA"}
            var payload = new
            {
                code = code
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var fullUrl = $"{BaseUrl}/sessions/create";
            System.Diagnostics.Debug.WriteLine($"Sending payload: {json}");
            System.Diagnostics.Debug.WriteLine($"API URL: {fullUrl}");
            System.Diagnostics.Debug.WriteLine($"HttpClient configured: {_httpClient != null}");

            var response = await _httpClient!.PostAsync(fullUrl, content);
            
            // Read the response content for debugging
            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"API Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"API Response Content: {responseContent}");
            
            if (response.IsSuccessStatusCode)
            {
                // Check if response contains success indicator
                var success = responseContent.Contains("\"success\":true");
                System.Diagnostics.Debug.WriteLine($"Success check result: {success}");
                return success;
            }
            
            System.Diagnostics.Debug.WriteLine($"API call failed with status: {response.StatusCode}");
            return false;
        }
        catch (HttpRequestException ex)
        {
            // Network error - log for debugging
            System.Diagnostics.Debug.WriteLine($"HttpRequestException: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            // Timeout error - log for debugging
            System.Diagnostics.Debug.WriteLine($"TaskCanceledException: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            // Other errors - log for debugging
            System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get current session status
    /// </summary>
    public async Task<SessionStatusResponse> GetSessionStatus(string code)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/sessions/status?code={code}");
            
            if (!response.IsSuccessStatusCode)
            {
                return new SessionStatusResponse
                {
                    Success = false,
                    Error = $"HTTP {response.StatusCode}"
                };
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SessionStatusResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return result ?? new SessionStatusResponse { Success = false };
        }
        catch (Exception ex)
        {
            return new SessionStatusResponse
            {
                Success = false,
                Error = ex.Message
            };
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
/// Response model for session status API calls
/// </summary>
public class SessionStatusResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("sessionId")]
    public string? SessionId { get; set; }
    
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("webapp")]
    public WebappInfo? Webapp { get; set; }
}

/// <summary>
/// Information about webapp connection
/// </summary>
public class WebappInfo
{
    [JsonPropertyName("connected")]
    public bool Connected { get; set; }
    
    [JsonPropertyName("deviceType")]
    public string? DeviceType { get; set; }
    
    [JsonPropertyName("lastPollAt")]
    public string? LastPollAt { get; set; }
}
