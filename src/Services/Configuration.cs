namespace AITeacherAssistant.Services;

/// <summary>
/// Application configuration and constants
/// </summary>
public static class Configuration
{
    // n8n API Configuration
    public const string N8nBaseUrl = "https://n8n.yarden-zamir.com/webhook/ita/api";
    
    // Supabase Configuration
    // TODO: Replace with your actual Supabase project URL and anon key
    public const string SupabaseUrl = "https://ptgveftlpeyaqnbvcziv.supabase.co";
    public const string SupabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB0Z3ZlZnRscGV5YXFuYnZjeml2Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTkzNTAzMzUsImV4cCI6MjA3NDkyNjMzNX0.X4ozAZSA4rTZC2p6EsFhTvXT9iVGIrV7QcnJPHCdsrk";
    
    // Polling Configuration
    public const int SessionPollingIntervalMs = 2500; // 2.5 seconds
    public const int SessionTimeoutSeconds = 300; // 5 minutes
    
    // Application Settings
    public const string AppName = "AI Teacher Assistant";
    public const string AppVersion = "0.2.0-alpha";
}
