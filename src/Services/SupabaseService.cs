using Supabase;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using AITeacherAssistant.Models;

namespace AITeacherAssistant.Services;

/// <summary>
/// Service for managing Supabase realtime subscriptions
/// Listens for new messages in the messages table and raises events
/// </summary>
public class SupabaseService
{
    private Client? _supabase;
    private System.Threading.Timer? _pollingTimer;
    private System.Threading.Timer? _screenshotPollingTimer;
    private string _sessionId = "";
    private bool _isSubscribed = false;
    private DateTime _lastMessageTime = DateTime.MinValue;
    private DateTime _lastScreenshotCheck = DateTime.MinValue;
    
    /// <summary>
    /// Fired when an annotation message is received from AI
    /// </summary>
    public event EventHandler<AnnotationResponse>? AnnotationReceived;
    
    /// <summary>
    /// Fired when a text message is received from AI
    /// </summary>
    public event EventHandler<string>? TextMessageReceived;
    
    /// <summary>
    /// Fired when any error occurs during realtime operations
    /// </summary>
    public event EventHandler<string>? ErrorOccurred;
    
    /// <summary>
    /// Fired when a screenshot request is received
    /// </summary>
    public event EventHandler<ScreenshotRequest>? ScreenshotRequestReceived;
    
    /// <summary>
    /// Initialize Supabase client with credentials
    /// </summary>
    /// <param name="supabaseUrl">Supabase project URL</param>
    /// <param name="supabaseKey">Supabase anon key</param>
    public async Task Initialize(string supabaseUrl, string supabaseKey)
    {
        try
        {
            // Validate credentials
            if (string.IsNullOrEmpty(supabaseUrl) || supabaseUrl.Contains("YOUR_PROJECT"))
            {
                throw new ArgumentException("Invalid Supabase URL - please configure your Supabase project URL");
            }
            
            if (string.IsNullOrEmpty(supabaseKey) || supabaseKey.Contains("YOUR_ANON_KEY"))
            {
                throw new ArgumentException("Invalid Supabase key - please configure your Supabase anon key");
            }
            
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };
            
            _supabase = new Client(supabaseUrl, supabaseKey, options);
            await _supabase.InitializeAsync();
            
            System.Diagnostics.Debug.WriteLine("✓ Supabase client initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Failed to initialize Supabase: {ex.Message}");
            ErrorOccurred?.Invoke(this, $"Failed to initialize Supabase: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Subscribe to messages for a specific session
    /// </summary>
    /// <param name="sessionId">UUID of the session to monitor</param>
    public async Task SubscribeToSession(string sessionId)
    {
        if (_supabase == null)
        {
            ErrorOccurred?.Invoke(this, "Supabase client not initialized");
            return;
        }
        
        if (_isSubscribed)
        {
            await Unsubscribe();
        }
        
        _sessionId = sessionId;
        
        try
        {
            // Start polling for new messages every 2 seconds
            _pollingTimer = new System.Threading.Timer(async _ => await PollForMessages(), null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
            _isSubscribed = true;
            System.Diagnostics.Debug.WriteLine($"✓ Message polling active for session: {sessionId}");
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to subscribe to session: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Poll for new messages from Supabase
    /// </summary>
    private async Task PollForMessages()
    {
        if (_supabase == null || string.IsNullOrEmpty(_sessionId))
            return;

        try
        {
            // Query messages table for new messages since last check
            var response = await _supabase
                .From<SupabaseMessage>()
                .Where(x => x.SessionId == _sessionId && x.Role == "assistant")
                .Order(x => x.CreatedAt, Postgrest.Constants.Ordering.Descending)
                .Limit(10)
                .Get();

            if (response?.Models != null)
            {
                foreach (var message in response.Models)
                {
                    // Check if this is a new message
                    if (DateTime.TryParse(message.CreatedAt, out var messageTime) && 
                        messageTime > _lastMessageTime)
                    {
                        _lastMessageTime = messageTime;
                        HandleNewMessage(message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error polling messages: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle new message received from Supabase
    /// </summary>
    private void HandleNewMessage(SupabaseMessage message)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Processing message: {message.Id}");

            // Parse metadata JSON
            if (string.IsNullOrEmpty(message.MetadataJson))
            {
                System.Diagnostics.Debug.WriteLine("No metadata in message");
                return;
            }
            
            // Deserialize metadata
            var metadata = JsonSerializer.Deserialize<MessageMetadata>(message.MetadataJson);
            
            if (metadata == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to parse metadata");
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"Message type: {metadata.Type}");
            
            // Handle based on message type
            switch (metadata.Type?.ToLower())
            {
                case "annotation":
                    // Pure annotation response
                    if (metadata.Annotation != null)
                    {
                        AnnotationReceived?.Invoke(this, metadata.Annotation);
                    }
                    break;
                    
                case "mixed":
                    // Both text and annotation
                    if (metadata.Annotation != null)
                    {
                        AnnotationReceived?.Invoke(this, metadata.Annotation);
                    }
                    if (!string.IsNullOrEmpty(message.Content))
                    {
                        TextMessageReceived?.Invoke(this, message.Content);
                    }
                    break;
                    
                case "text_response":
                    // Pure text response
                    if (!string.IsNullOrEmpty(message.Content))
                    {
                        TextMessageReceived?.Invoke(this, message.Content);
                    }
                    break;
                    
                default:
                    System.Diagnostics.Debug.WriteLine($"Unknown message type: {metadata.Type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error handling message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Error handling message: {ex}");
        }
    }
    
    /// <summary>
    /// Unsubscribe from current session and clean up
    /// </summary>
    public async Task Unsubscribe()
    {
        if (_pollingTimer != null)
        {
            try
            {
                _pollingTimer.Dispose();
                _pollingTimer = null;
                System.Diagnostics.Debug.WriteLine("✓ Stopped message polling");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error unsubscribing: {ex.Message}");
            }
        }
        
        if (_screenshotPollingTimer != null)
        {
            try
            {
                _screenshotPollingTimer.Dispose();
                _screenshotPollingTimer = null;
                System.Diagnostics.Debug.WriteLine("✓ Stopped screenshot polling");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error unsubscribing from screenshots: {ex.Message}");
            }
        }
        
        _isSubscribed = false;
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Check if currently subscribed to a session
    /// </summary>
    public bool IsSubscribed => _isSubscribed;
    
    /// <summary>
    /// Get the current session ID being monitored
    /// </summary>
    public string CurrentSessionId => _sessionId;
    
    /// <summary>
    /// Subscribe to screenshot requests for this session
    /// </summary>
    public async Task SubscribeToScreenshotRequests(string sessionId)
    {
        if (_supabase == null) return;
        
        try
        {
            // Start polling for screenshot requests every 2 seconds
            _screenshotPollingTimer = new System.Threading.Timer(
                async _ => await PollForScreenshotRequests(sessionId), 
                null, 
                TimeSpan.Zero, 
                TimeSpan.FromSeconds(2));
                
            System.Diagnostics.Debug.WriteLine($"✓ Screenshot request polling active");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting screenshot polling: {ex.Message}");
        }
        
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Poll for pending screenshot requests
    /// </summary>
    private async Task PollForScreenshotRequests(string sessionId)
    {
        if (_supabase == null) return;
        
        try
        {
            var response = await _supabase
                .From<ScreenshotRequestModel>()
                .Where(x => x.SessionId == sessionId && x.Status == "pending")
                .Order(x => x.RequestedAt, Postgrest.Constants.Ordering.Descending)
                .Limit(1)
                .Get();
            
            if (response?.Models != null && response.Models.Count > 0)
            {
                var request = response.Models[0];
                
                // Check if this is a new request
                if (DateTime.TryParse(request.RequestedAt, out var requestTime) &&
                    requestTime > _lastScreenshotCheck)
                {
                    _lastScreenshotCheck = requestTime;
                    
                    // Fire event
                    ScreenshotRequestReceived?.Invoke(this, new ScreenshotRequest
                    {
                        RequestId = request.Id,
                        SessionId = request.SessionId
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error polling screenshot requests: {ex.Message}");
        }
    }
}

/// <summary>
/// Screenshot request data
/// </summary>
public class ScreenshotRequest
{
    public string RequestId { get; set; } = "";
    public string SessionId { get; set; } = "";
}
