using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace AITeacherAssistant.Services;

/// <summary>
/// Service for capturing screenshots and uploading to Supabase Storage
/// </summary>
public class ScreenshotService
{
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;
    
    public ScreenshotService(string supabaseUrl, string supabaseKey)
    {
        _supabaseUrl = supabaseUrl;
        _supabaseKey = supabaseKey;
    }
    
    /// <summary>
    /// Capture current screen including overlay annotations
    /// </summary>
    public async Task<byte[]> CaptureScreen()
    {
        return await Task.Run(() =>
        {
            try
            {
                // Get primary screen bounds
                var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
                if (primaryScreen == null)
                    throw new InvalidOperationException("Primary screen not found");
                    
                var bounds = primaryScreen.Bounds;
                
                using var bitmap = new Bitmap(bounds.Width, bounds.Height);
                using var graphics = Graphics.FromImage(bitmap);
                
                // Capture screen
                graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
                
                // Compress to JPEG for faster upload (70% quality)
                using var memoryStream = new MemoryStream();
                var encoder = ImageCodecInfo.GetImageEncoders()
                    .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(
                    System.Drawing.Imaging.Encoder.Quality, 70L);
                
                bitmap.Save(memoryStream, encoder, encoderParams);
                
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error capturing screen: {ex.Message}");
                throw;
            }
        });
    }
    
    /// <summary>
    /// Upload screenshot to Supabase Storage
    /// </summary>
    public async Task<string> UploadScreenshot(string sessionId, byte[] imageData)
    {
        try
        {
            var fileName = $"{sessionId}_{DateTime.UtcNow:yyyyMMddHHmmss}.jpg";
            var filePath = $"screenshots/{fileName}";
            
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");
            
            var content = new ByteArrayContent(imageData);
            content.Headers.Add("Content-Type", "image/jpeg");
            
            var uploadUrl = $"{_supabaseUrl}/storage/v1/object/session-images/{filePath}";
            System.Diagnostics.Debug.WriteLine($"Uploading to: {uploadUrl}");
            
            var response = await httpClient.PostAsync(uploadUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Upload failed ({response.StatusCode}): {errorContent}");
            }
            
            // Return public URL
            var publicUrl = $"{_supabaseUrl}/storage/v1/object/public/session-images/{filePath}";
            System.Diagnostics.Debug.WriteLine($"Screenshot uploaded: {publicUrl}");
            
            return publicUrl;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error uploading screenshot: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Handle screenshot request - capture, upload, and update database
    /// </summary>
    public async Task HandleScreenshotRequest(string requestId, string sessionId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"📸 Handling screenshot request: {requestId}");
            
            // Capture screen
            var imageData = await CaptureScreen();
            System.Diagnostics.Debug.WriteLine($"✓ Screen captured: {imageData.Length} bytes");
            
            // Get screen dimensions
            var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
            if (primaryScreen == null)
                throw new InvalidOperationException("Primary screen not found");
                
            var bounds = primaryScreen.Bounds;
            
            // Upload to Supabase Storage
            var imageUrl = await UploadScreenshot(sessionId, imageData);
            System.Diagnostics.Debug.WriteLine($"✓ Screenshot uploaded: {imageUrl}");
            
            // Update screenshot_requests with image URL
            await UpdateScreenshotRequest(requestId, imageUrl, bounds.Width, bounds.Height);
            System.Diagnostics.Debug.WriteLine($"✓ Database updated for request: {requestId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Screenshot request failed: {ex.Message}");
            // Update with error
            await UpdateScreenshotRequestError(requestId, ex.Message);
        }
    }
    
    /// <summary>
    /// Update screenshot_requests table with successful capture
    /// </summary>
    private async Task UpdateScreenshotRequest(string requestId, string imageUrl, int width, int height)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");
            httpClient.DefaultRequestHeaders.Add("Prefer", "return=minimal");
            
            var payload = new
            {
                status = "ready",
                image_url = imageUrl,
                width = width,
                height = height,
                completed_at = DateTime.UtcNow.ToString("o")
            };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var updateUrl = $"{_supabaseUrl}/rest/v1/screenshot_requests?id=eq.{requestId}";
            var response = await httpClient.PatchAsync(updateUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Database update failed ({response.StatusCode}): {errorContent}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating database: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Update screenshot_requests table with error
    /// </summary>
    private async Task UpdateScreenshotRequestError(string requestId, string error)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");
            httpClient.DefaultRequestHeaders.Add("Prefer", "return=minimal");
            
            var payload = new
            {
                status = "failed",
                error = error,
                completed_at = DateTime.UtcNow.ToString("o")
            };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var updateUrl = $"{_supabaseUrl}/rest/v1/screenshot_requests?id=eq.{requestId}";
            await httpClient.PatchAsync(updateUrl, content);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating error status: {ex.Message}");
        }
    }
}

