using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AITeacherAssistant.Services;

namespace AITeacherAssistant.Views;

/// <summary>
/// Startup window for connecting users via QR code or session code
/// </summary>
public partial class StartupWindow : Window
{
    private readonly SessionService _sessionService;
    private readonly QRCodeService _qrCodeService;
    private readonly ApiService _apiService;
    private readonly string _sessionCode;
    private string _sessionId = "";
    
    // Polling fields
    private DispatcherTimer? _pollingTimer;
    private DateTime _pollingStartTime;
    private const int POLLING_INTERVAL_MS = 2500; // 2.5 seconds
    private const int TIMEOUT_SECONDS = 300; // 5 minutes
    
    public StartupWindow(string sessionCode)
    {
        InitializeComponent();
        
        _sessionCode = sessionCode;
        
        // Initialize services
        _sessionService = new SessionService();
        _qrCodeService = new QRCodeService();
        _apiService = new ApiService();
        
        // Subscribe to session events
        _sessionService.UserConnected += OnUserConnected;
        _sessionService.UserDisconnected += OnUserDisconnected;
        
        // Initialize the window
        Loaded += StartupWindow_Loaded;
    }
    
    /// <summary>
    /// Window loaded event - display session code and QR code
    /// </summary>
    private void StartupWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Use the passed session code
        _sessionService.CurrentSessionCode = _sessionCode;
        
        // Update UI with session code (formatted with spaces)
        SessionCodeText.Text = string.Join(" ", _sessionCode.ToCharArray());
        
        // Generate and display QR code
        try
        {
            var qrCodeImage = _qrCodeService.GenerateSessionQRCode(_sessionCode, 300);
            QRCodeImage.Source = qrCodeImage;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error generating QR code: {ex.Message}", "Error", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        // Start polling for connection
        StartPolling();
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
            
            // Hide back to menu button
            BackToMenuButton.Visibility = Visibility.Collapsed;
            
            // Start the connection animation
            StartConnectionAnimation();
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
            
            // Show back to menu button
            BackToMenuButton.Visibility = Visibility.Visible;
        });
    }
    
    /// <summary>
    /// Start the connection animation and auto-route to overlay
    /// </summary>
    private void StartConnectionAnimation()
    {
        var animation = (Storyboard)FindResource("ConnectionAnimation");
        if (animation != null)
        {
            // Start the animation
            animation.Begin();
            
            // Auto-route to overlay after animation completes and showing for 3 seconds
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4.0) // 1.0s animation + 3s display time
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                NavigateToOverlay();
            };
            timer.Start();
        }
        else
        {
            // Fallback if animation resource not found
            NavigateToOverlay();
        }
    }
    
    /// <summary>
    /// Navigate to the overlay (MainWindow)
    /// </summary>
    private void NavigateToOverlay()
    {
        try
        {
            // Create and show the main overlay window with session info
            var mainWindow = new MainWindow(_sessionCode, _sessionId);
            mainWindow.Show();
            
            // Close this startup window
            this.Close();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error starting overlay: {ex.Message}", "Error", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    
    /// <summary>
    /// Back to menu button click handler
    /// </summary>
    private void BackToMenuButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Stop polling if active
            StopPolling();
            
            // Create and show the home page
            var homePage = new HomePage();
            homePage.Show();
            
            // Close this startup window
            this.Close();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error returning to menu: {ex.Message}", "Error", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    
    /// <summary>
    /// Start polling for session connection status
    /// </summary>
    private void StartPolling()
    {
        _pollingStartTime = DateTime.UtcNow;
        
        _pollingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(POLLING_INTERVAL_MS)
        };
        _pollingTimer.Tick += PollingTimer_Tick;
        _pollingTimer.Start();
        
        // Do first check immediately
        _ = CheckConnectionStatus();
    }
    
    /// <summary>
    /// Stop polling timer
    /// </summary>
    private void StopPolling()
    {
        
        if (_pollingTimer != null)
        {
            _pollingTimer.Stop();
            _pollingTimer.Tick -= PollingTimer_Tick;
            _pollingTimer = null;
        }
    }
    
    /// <summary>
    /// Polling timer tick event
    /// </summary>
    private async void PollingTimer_Tick(object? sender, EventArgs e)
    {
        await CheckConnectionStatus();
    }
    
    /// <summary>
    /// Check if user has connected via webapp
    /// </summary>
    private async Task CheckConnectionStatus()
    {
        try
        {
            // Check timeout
            var elapsedTime = DateTime.UtcNow - _pollingStartTime;
            if (elapsedTime.TotalSeconds > TIMEOUT_SECONDS)
            {
                StopPolling();
                HandleTimeout();
                return;
            }
            
            // Call API to check status
            var response = await _apiService.GetSessionStatus(_sessionCode);
            
            if (response.Success)
            {
                // Store session ID for later use
                if (!string.IsNullOrEmpty(response.SessionId))
                {
                    _sessionId = response.SessionId;
                }
                
                // Check if user connected
                if (response.Webapp?.Connected == true)
                {
                    // User connected!
                    StopPolling();
                    OnUserConnected(this, EventArgs.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but continue polling
            System.Diagnostics.Debug.WriteLine($"Polling error: {ex.Message}");
            // Don't stop polling on transient errors
        }
    }
    
    /// <summary>
    /// Handle connection timeout (5 minutes elapsed)
    /// </summary>
    private void HandleTimeout()
    {
        Dispatcher.Invoke(() =>
        {
            // Update UI to show timeout
            StatusText.Text = "⏱️ Connection Timeout";
            StatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Colors.Orange);
            StatusDot.Fill = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Colors.Orange);
            
            // Show back to menu button
            BackToMenuButton.Visibility = Visibility.Visible;
            
            // Show message and return to menu after OK is clicked
            System.Windows.MessageBox.Show(
                "No user connected within 5 minutes.",
                "Connection Timeout",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            
            // Return to menu after user clicks OK
            BackToMenuButton_Click(this, new RoutedEventArgs());
        });
    }
    
    /// <summary>
    /// Handle window closing
    /// </summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Stop polling if active
        StopPolling();
        
        // Unsubscribe from events
        _sessionService.UserConnected -= OnUserConnected;
        _sessionService.UserDisconnected -= OnUserDisconnected;
        
        // Dispose API service
        _apiService?.Dispose();
        
        base.OnClosing(e);
    }
}
