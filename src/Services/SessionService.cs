using System;
using System.Linq;

namespace AITeacherAssistant.Services;

/// <summary>
/// Service for managing session codes and connection state
/// </summary>
public class SessionService
{
    private static readonly Random _random = new Random();
    private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    
    /// <summary>
    /// Current session code (5-letter uppercase string)
    /// </summary>
    public string CurrentSessionCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether a user is currently connected to this session
    /// </summary>
    public bool IsUserConnected { get; private set; } = false;
    
    /// <summary>
    /// Event fired when user connects to the session
    /// </summary>
    public event EventHandler? UserConnected;
    
    /// <summary>
    /// Event fired when user disconnects from the session
    /// </summary>
    public event EventHandler? UserDisconnected;
    
    /// <summary>
    /// Generate a new 5-letter session code
    /// </summary>
    /// <returns>Random 5-letter uppercase string (e.g., "ABCDE")</returns>
    public string GenerateSessionCode()
    {
        var code = new string(Enumerable.Repeat(AllowedCharacters, 5)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
        
        return code;
    }
    
    /// <summary>
    /// Create a new session with a generated code
    /// </summary>
    /// <returns>The new session code</returns>
    public string CreateNewSession()
    {
        CurrentSessionCode = GenerateSessionCode();
        IsUserConnected = false;
        
        // TODO: Send session code to n8n backend
        // await SendSessionToBackend(CurrentSessionCode);
        
        return CurrentSessionCode;
    }
    
    /// <summary>
    /// Mark user as connected to the current session
    /// </summary>
    public void MarkUserConnected()
    {
        if (!IsUserConnected)
        {
            IsUserConnected = true;
            UserConnected?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Mark user as disconnected from the current session
    /// </summary>
    public void MarkUserDisconnected()
    {
        if (IsUserConnected)
        {
            IsUserConnected = false;
            UserDisconnected?.Invoke(this, EventArgs.Empty);
        }
    }
    
    
    /// <summary>
    /// Get the JSON payload for QR code generation
    /// </summary>
    /// <returns>JSON string containing session type and code</returns>
    public string GetQRCodePayload()
    {
        return $"{{\"type\":\"session\",\"code\":\"{CurrentSessionCode}\"}}";
    }
}
