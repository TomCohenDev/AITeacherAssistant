using Supabase;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AITeacherAssistant.Models;

namespace AITeacherAssistant.Services;

/// <summary>
/// Service for managing Supabase realtime subscriptions
/// Listens for new messages in the messages table and raises events
/// </summary>
public class SupabaseService
{
    private Client? _supabase;
    private string _sessionId = "";
    private bool _isSubscribed = false;
    
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
    /// Initialize Supabase client with credentials
    /// </summary>
    /// <param name="supabaseUrl">Supabase project URL</param>
    /// <param name="supabaseKey">Supabase anon key</param>
    public async Task Initialize(string supabaseUrl, string supabaseKey)
    {
        try
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };
            
            _supabase = new Client(supabaseUrl, supabaseKey, options);
            await _supabase.InitializeAsync();
            
            System.Diagnostics.Debug.WriteLine("Supabase client initialized successfully");
        }
        catch (Exception ex)
        {
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
            // TODO: Implement realtime subscription when Supabase API is stable
            // For now, this is a placeholder that will be implemented when
            // the n8n integration is ready and we have a working Supabase setup
            
            _isSubscribed = true;
            System.Diagnostics.Debug.WriteLine($"Realtime subscription placeholder active for session: {sessionId}");
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to subscribe to session: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Handle new message received from Supabase realtime
    /// </summary>
    private void HandleNewMessage(Dictionary<string, object> record)
    {
        try
        {
            // Parse message fields from the record dictionary
            var role = record.GetValueOrDefault("role")?.ToString();
            var content = record.GetValueOrDefault("content")?.ToString() ?? "";
            var sessionIdValue = record.GetValueOrDefault("session_id")?.ToString();
            
            // Filter: Only process messages for our session from the assistant
            if (sessionIdValue != _sessionId || role != "assistant")
            {
                System.Diagnostics.Debug.WriteLine($"Ignoring message: session={sessionIdValue}, role={role}");
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"Processing assistant message for session {_sessionId}");
            
            // Parse metadata JSON
            var metadataObj = record.GetValueOrDefault("metadata");
            if (metadataObj == null)
            {
                System.Diagnostics.Debug.WriteLine("No metadata in message");
                return;
            }
            
            // Convert metadata object to JSON string, then deserialize to our model
            var metadataJson = JsonSerializer.Serialize(metadataObj);
            var metadata = JsonSerializer.Deserialize<MessageMetadata>(metadataJson);
            
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
                    if (!string.IsNullOrEmpty(content))
                    {
                        TextMessageReceived?.Invoke(this, content);
                    }
                    break;
                    
                case "text_response":
                    // Pure text response
                    if (!string.IsNullOrEmpty(content))
                    {
                        TextMessageReceived?.Invoke(this, content);
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
        if (_isSubscribed)
        {
            try
            {
                // TODO: Implement real unsubscribe when realtime is working
                _isSubscribed = false;
                System.Diagnostics.Debug.WriteLine("Unsubscribed from session messages");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error unsubscribing: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Check if currently subscribed to a session
    /// </summary>
    public bool IsSubscribed => _isSubscribed;
    
    /// <summary>
    /// Get the current session ID being monitored
    /// </summary>
    public string CurrentSessionId => _sessionId;
}
