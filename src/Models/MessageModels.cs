using System.Text.Json.Serialization;
using Postgrest.Attributes;
using Postgrest.Models;

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
[Table("messages")]
public class SupabaseMessage : BaseModel
{
    [PrimaryKey("id")]
    [Column("id")]
    public string Id { get; set; } = "";
    
    [Column("session_id")]
    public string SessionId { get; set; } = "";
    
    [Column("role")]
    public string Role { get; set; } = "";
    
    [Column("content")]
    public string Content { get; set; } = "";
    
    [Column("created_at")]
    public string CreatedAt { get; set; } = "";
    
    [Column("metadata")]
    public string MetadataJson { get; set; } = "";
    
    /// <summary>
    /// Parsed metadata object
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public MessageMetadata? Metadata { get; set; }
}
