using System;
using System.IO;
using System.Windows.Media.Imaging;
using QRCoder;

namespace AITeacherAssistant.Services;

/// <summary>
/// Service for generating QR codes using QRCoder library
/// </summary>
public class QRCodeService
{
    /// <summary>
    /// Generate a QR code as a BitmapImage for WPF display
    /// </summary>
    /// <param name="data">The data to encode in the QR code</param>
    /// <param name="size">The size of the QR code in pixels (default: 300)</param>
    /// <returns>BitmapImage suitable for WPF Image control</returns>
    public BitmapImage GenerateQRCode(string data, int size = 300)
    {
        if (string.IsNullOrEmpty(data))
            throw new ArgumentException("Data cannot be null or empty", nameof(data));
        
        if (size <= 0)
            throw new ArgumentException("Size must be greater than 0", nameof(size));
        
        // Create QR code generator
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCode(qrCodeData);
        
        // Generate QR code as bitmap
        using var qrCodeBitmap = qrCode.GetGraphic(size, System.Drawing.Color.Black, System.Drawing.Color.White, true);
        
        // Convert to WPF BitmapImage
        return ConvertToBitmapImage(qrCodeBitmap);
    }
    
    /// <summary>
    /// Generate a QR code for a session code
    /// </summary>
    /// <param name="sessionCode">The 5-letter session code</param>
    /// <param name="size">The size of the QR code in pixels (default: 300)</param>
    /// <returns>BitmapImage containing the QR code</returns>
    public BitmapImage GenerateSessionQRCode(string sessionCode, int size = 300)
    {
        var payload = $"{{\"type\":\"session\",\"code\":\"{sessionCode}\"}}";
        return GenerateQRCode(payload, size);
    }
    
    /// <summary>
    /// Convert System.Drawing.Bitmap to WPF BitmapImage
    /// </summary>
    /// <param name="bitmap">The bitmap to convert</param>
    /// <returns>WPF BitmapImage</returns>
    private static BitmapImage ConvertToBitmapImage(System.Drawing.Bitmap bitmap)
    {
        using var memory = new MemoryStream();
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
        memory.Position = 0;
        
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        
        return bitmapImage;
    }
}
