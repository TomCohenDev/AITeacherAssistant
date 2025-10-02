using System.Text.Json.Serialization;

namespace AITeacherAssistant.Models;

/// <summary>
/// Message metadata structure from Supabase messages table
/// </summary>
public class MessageMetadata
{
    /// <summary>
    /// Type of message: "annotation", "text_response", "mixed", "text", "text_with_image"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
    
    /// <summary>
    /// Annotation data if type includes annotation
    /// </summary>
    [JsonPropertyName("annotation")]
    public AnnotationResponse? Annotation { get; set; }
    
    /// <summary>
    /// Image URL if user uploaded an image
    /// </summary>
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Timestamp when message was processed
    /// </summary>
    [JsonPropertyName("processed_at")]
    public string? ProcessedAt { get; set; }
}

/// <summary>
/// Complete message structure from Supabase
/// </summary>
public class SupabaseMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = "";
    
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
    
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = "";
    
    [JsonPropertyName("metadata")]
    public MessageMetadata? Metadata { get; set; }
}
