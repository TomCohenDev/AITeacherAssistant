using System.Text.Json.Serialization;
using Postgrest.Attributes;
using Postgrest.Models;

namespace AITeacherAssistant.Models;

/// <summary>
/// Screenshot request from Supabase screenshot_requests table
/// </summary>
[Table("screenshot_requests")]
public class ScreenshotRequestModel : BaseModel
{
    [PrimaryKey("id")]
    [Column("id")]
    public string Id { get; set; } = "";
    
    [Column("session_id")]
    public string SessionId { get; set; } = "";
    
    [Column("status")]
    public string Status { get; set; } = "";
    
    [Column("image_url")]
    public string? ImageUrl { get; set; }
    
    [Column("width")]
    public int? Width { get; set; }
    
    [Column("height")]
    public int? Height { get; set; }
    
    [Column("requested_at")]
    public string RequestedAt { get; set; } = "";
    
    [Column("completed_at")]
    public string? CompletedAt { get; set; }
    
    [Column("error")]
    public string? Error { get; set; }
}

