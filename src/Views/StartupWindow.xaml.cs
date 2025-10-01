using System;
using System.Windows;
using AITeacherAssistant.Services;

namespace AITeacherAssistant.Views;

/// <summary>
/// Startup window for connecting users via QR code or session code
/// </summary>
public partial class StartupWindow : Window
{
    private readonly SessionService _sessionService;
    private readonly QRCodeService _qrCodeService;
    
    public StartupWindow()
    {
        InitializeComponent();
        
        // Initialize services
        _sessionService = new SessionService();
        _qrCodeService = new QRCodeService();
        
        // Subscribe to session events
        _sessionService.UserConnected += OnUserConnected;
        _sessionService.UserDisconnected += OnUserDisconnected;
        
        // Initialize the window
        Loaded += StartupWindow_Loaded;
    }
    
    /// <summary>
    /// Window loaded event - generate session and QR code
    /// </summary>
    private void StartupWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Generate new session
        var sessionCode = _sessionService.CreateNewSession();
        
        // Update UI with session code (formatted with spaces)
        SessionCodeText.Text = string.Join(" ", sessionCode.ToCharArray());
        
        // Generate and display QR code
        try
        {
            var qrCodeImage = _qrCodeService.GenerateSessionQRCode(sessionCode, 300);
            QRCodeImage.Source = qrCodeImage;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating QR code: {ex.Message}", "Error", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        // TODO: Start polling n8n backend for connection status
        // StartConnectionPolling();
    }
    
    /// <summary>
    /// Handle user connection event
    /// </summary>
    private void OnUserConnected(object? sender, EventArgs e)
    {
        // Update UI on UI thread
        Dispatcher.Invoke(() =>
        {
            StatusDot.Fill = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Colors.LimeGreen);
            StatusText.Text = "✓ User Connected";
            StatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Colors.Green);
            
            // Show begin session button
            BeginSessionButton.Visibility = Visibility.Visible;
            TestConnectionButton.Visibility = Visibility.Collapsed;
        });
    }
    
    /// <summary>
    /// Handle user disconnection event
    /// </summary>
    private void OnUserDisconnected(object? sender, EventArgs e)
    {
        // Update UI on UI thread
        Dispatcher.Invoke(() =>
        {
            StatusDot.Fill = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Colors.Orange);
            StatusText.Text = "Waiting for connection...";
            StatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Colors.Gray);
            
            // Hide begin session button
            BeginSessionButton.Visibility = Visibility.Collapsed;
            TestConnectionButton.Visibility = Visibility.Visible;
        });
    }
    
    /// <summary>
    /// Test connection button click handler
    /// </summary>
    private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
    {
        // Simulate user connection for testing
        _sessionService.SimulateUserConnection();
    }
    
    /// <summary>
    /// Begin session button click handler
    /// </summary>
    private void BeginSessionButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Create and show the main overlay window
            var mainWindow = new MainWindow();
            mainWindow.Show();
            
            // Close this startup window
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error starting session: {ex.Message}", "Error", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Start polling the n8n backend for connection status
    /// TODO: Implement actual API polling
    /// </summary>
    private void StartConnectionPolling()
    {
        // TODO: Replace with actual n8n API polling
        // This would check if a user has connected using the session code
        // and call _sessionService.MarkUserConnected() when they do
        
        // Example implementation:
        // var timer = new DispatcherTimer();
        // timer.Interval = TimeSpan.FromSeconds(2);
        // timer.Tick += async (s, e) => {
        //     var isConnected = await CheckBackendConnectionStatus();
        //     if (isConnected && !_sessionService.IsUserConnected) {
        //         _sessionService.MarkUserConnected();
        //     }
        // };
        // timer.Start();
    }
    
    /// <summary>
    /// Check if user has connected via backend API
    /// TODO: Implement actual API call to n8n
    /// </summary>
    /// <returns>True if user is connected, false otherwise</returns>
    private async Task<bool> CheckBackendConnectionStatus()
    {
        // TODO: Implement actual API call to n8n backend
        // This would check if the current session code has been used
        // to establish a connection from the phone webapp
        
        await Task.Delay(100); // Placeholder for async operation
        return false; // Placeholder return value
    }
    
    /// <summary>
    /// Handle window closing
    /// </summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Unsubscribe from events
        _sessionService.UserConnected -= OnUserConnected;
        _sessionService.UserDisconnected -= OnUserDisconnected;
        
        base.OnClosing(e);
    }
}
