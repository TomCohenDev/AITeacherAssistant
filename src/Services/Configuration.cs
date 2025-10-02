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
    public const string SupabaseUrl = "https://YOUR_PROJECT.supabase.co";
    public const string SupabaseAnonKey = "YOUR_ANON_KEY_HERE";
    
    // Polling Configuration
    public const int SessionPollingIntervalMs = 2500; // 2.5 seconds
    public const int SessionTimeoutSeconds = 300; // 5 minutes
    
    // Application Settings
    public const string AppName = "AI Teacher Assistant";
    public const string AppVersion = "0.2.0-alpha";
}
