using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AITeacherAssistant.Models;
using AITeacherAssistant.Services;

namespace AITeacherAssistant.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// Main overlay window that provides transparent, always-on-top functionality
/// </summary>
public partial class MainWindow : Window
{
    private bool _isOverlayActive = true;
    private IntPtr _windowHandle;
    private AnnotationRenderer? _annotationRenderer;
    private DispatcherTimer? _annotationPollingTimer;
    private readonly ApiService _apiService = new();
    private SupabaseService _supabaseService = new();
    private string _sessionCode = "";
    private string _sessionId = "";

    public MainWindow(string sessionCode, string sessionId)
    {
        InitializeComponent();
        _sessionCode = sessionCode;
        _sessionId = sessionId;
        
        // Set up event handlers
        Loaded += MainWindow_Loaded;
        KeyDown += MainWindow_KeyDown;
    }

    /// <summary>
    /// Window loaded event - initialize overlay functionality
    /// </summary>
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Get the window handle for Win32 API calls
        _windowHandle = new WindowInteropHelper(this).Handle;
        
        // Set focus to receive keyboard input
        this.Focus();
        
        UpdateStatusIndicator(true);
        
        // Initialize annotation renderer
        _annotationRenderer = new AnnotationRenderer(AnnotationCanvas);
        
        // Initialize realtime subscription
        await InitializeRealtimeSubscription();
    }

    /// <summary>
    /// Handle keyboard shortcuts
    /// Ctrl+Shift+Q = Toggle overlay visibility
    /// Ctrl+Shift+C = Clear annotations
    /// </summary>
    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        // Check for Ctrl+Shift+Q
        if (e.Key == Key.Q && 
            Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
        {
            ToggleOverlay();
            e.Handled = true;
        }
        
        // Check for Ctrl+Shift+C
        if (e.Key == Key.C && 
            Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
        {
            _annotationRenderer?.ClearAnnotations();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Toggle overlay visibility on/off
    /// </summary>
    private void ToggleOverlay()
    {
        _isOverlayActive = !_isOverlayActive;

        if (_isOverlayActive)
        {
            // Show overlay
            this.Visibility = Visibility.Visible;
            UpdateStatusIndicator(true);
        }
        else
        {
            // Hide overlay
            this.Visibility = Visibility.Collapsed;
            UpdateStatusIndicator(false);
        }
    }

    /// <summary>
    /// Update the status indicator in the UI
    /// </summary>
    private void UpdateStatusIndicator(bool isActive)
    {
        if (isActive)
        {
            StatusDot.Fill = new SolidColorBrush(Colors.LimeGreen);
            StatusText.Text = "Overlay Active";
        }
        else
        {
            StatusDot.Fill = new SolidColorBrush(Colors.Red);
            StatusText.Text = "Overlay Inactive";
        }
    }

    /// <summary>
    /// Start polling for new annotations from n8n
    /// </summary>
    private void StartAnnotationPolling()
    {
        _annotationPollingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3) // Poll every 3 seconds
        };
        _annotationPollingTimer.Tick += async (s, e) => await PollForAnnotations();
        _annotationPollingTimer.Start();
    }
    
    /// <summary>
    /// Poll n8n for new annotations
    /// </summary>
    private async Task PollForAnnotations()
    {
        try
        {
            // TODO: Call ApiService.GetAnnotations(sessionCode)
            // For now, placeholder
            await Task.Delay(1); // Placeholder to avoid async warning
            
            // Example: var response = await _apiService.GetAnnotations(_sessionCode);
            // if (response.Success && response.Annotations?.Count > 0)
            // {
            //     foreach (var annotation in response.Annotations)
            //     {
            //         _annotationRenderer?.RenderAnnotation(annotation);
            //     }
            // }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Annotation polling error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Test annotations with hardcoded JSON
    /// </summary>
    private void TestAnnotations()
    {
        var testJson = @"{
            ""type"": ""annotation"",
            ""elements"": [
                {
                    ""type"": ""text"",
                    ""content"": ""Step 1: Solve for x"",
                    ""x"": 100,
                    ""y"": 100,
                    ""fontSize"": 20,
                    ""color"": ""#667eea"",
                    ""fontWeight"": ""bold""
                },
                {
                    ""type"": ""arrow"",
                    ""from"": {""x"": 100, ""y"": 130},
                    ""to"": {""x"": 300, ""y"": 130},
                    ""color"": ""#f56565"",
                    ""thickness"": 3
                },
                {
                    ""type"": ""circle"",
                    ""center"": {""x"": 200, ""y"": 200},
                    ""radius"": 40,
                    ""strokeColor"": ""#48bb78"",
                    ""fillColor"": ""transparent"",
                    ""thickness"": 2
                },
                {
                    ""type"": ""rectangle"",
                    ""x"": 150,
                    ""y"": 300,
                    ""width"": 100,
                    ""height"": 60,
                    ""strokeColor"": ""#667eea"",
                    ""fillColor"": ""#f0f0f0"",
                    ""thickness"": 2
                },
                {
                    ""type"": ""line"",
                    ""from"": {""x"": 50, ""y"": 400},
                    ""to"": {""x"": 250, ""y"": 400},
                    ""color"": ""#ed8936"",
                    ""thickness"": 2,
                    ""dashed"": true
                },
                {
                    ""type"": ""freehand"",
                    ""points"": [
                        {""x"": 300, ""y"": 100},
                        {""x"": 320, ""y"": 120},
                        {""x"": 340, ""y"": 110},
                        {""x"": 360, ""y"": 130},
                        {""x"": 380, ""y"": 125}
                    ],
                    ""color"": ""#9f7aea"",
                    ""thickness"": 3
                }
            ]
        }";
        
        try
        {
            var annotation = JsonSerializer.Deserialize<AnnotationResponse>(testJson);
            if (annotation != null)
            {
                _annotationRenderer?.RenderAnnotation(annotation);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deserializing test JSON: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Test button click handler
    /// </summary>
    private void TestButton_Click(object sender, RoutedEventArgs e)
    {
        TestAnnotations();
    }

    /// <summary>
    /// Initialize Supabase realtime subscription for this session
    /// </summary>
    private async Task InitializeRealtimeSubscription()
    {
        try
        {
            // Initialize Supabase client
            await _supabaseService.Initialize(
                Configuration.SupabaseUrl,
                Configuration.SupabaseAnonKey
            );
            
            // Subscribe to events
            _supabaseService.AnnotationReceived += OnAnnotationReceived;
            _supabaseService.TextMessageReceived += OnTextMessageReceived;
            _supabaseService.ErrorOccurred += OnSupabaseError;
            
            // Subscribe to session messages
            await _supabaseService.SubscribeToSession(_sessionId);
            
            System.Diagnostics.Debug.WriteLine($"✓ Realtime subscription active for session: {_sessionId}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to connect to realtime service:\n{ex.Message}\n\nAnnotations may not appear automatically.",
                "Realtime Connection Warning",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }
    }

    /// <summary>
    /// Handle annotation received from Supabase
    /// </summary>
    private void OnAnnotationReceived(object? sender, AnnotationResponse annotation)
    {
        // Ensure we're on UI thread
        Dispatcher.Invoke(() =>
        {
            try
            {
                _annotationRenderer?.RenderAnnotation(annotation);
                System.Diagnostics.Debug.WriteLine($"✓ Rendered annotation with {annotation.Elements.Count} elements");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rendering annotation: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// Handle text message received from Supabase
    /// </summary>
    private void OnTextMessageReceived(object? sender, string message)
    {
        Dispatcher.Invoke(() =>
        {
            System.Diagnostics.Debug.WriteLine($"Text message received: {message}");
            // TODO: Add UI element to display text messages
            // For now, just log to debug output
        });
    }

    /// <summary>
    /// Handle Supabase errors
    /// </summary>
    private void OnSupabaseError(object? sender, string error)
    {
        Dispatcher.Invoke(() =>
        {
            System.Diagnostics.Debug.WriteLine($"Supabase error: {error}");
        });
    }

    /// <summary>
    /// Close button click handler
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // Show confirmation dialog
        var result = MessageBox.Show(
            "Are you sure you want to close AI Teacher Assistant?",
            "Confirm Close",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            Application.Current.Shutdown();
        }
    }



    /// <summary>
    /// Override to handle window closing
    /// </summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Unsubscribe from realtime
        _supabaseService.Unsubscribe().Wait();
        
        // Unsubscribe from events
        _supabaseService.AnnotationReceived -= OnAnnotationReceived;
        _supabaseService.TextMessageReceived -= OnTextMessageReceived;
        _supabaseService.ErrorOccurred -= OnSupabaseError;
        
        _annotationPollingTimer?.Stop();
        base.OnClosing(e);
    }
}