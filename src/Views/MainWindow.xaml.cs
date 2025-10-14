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
    private AnnotationToolManager? _toolManager;
    private GridOverlayService? _gridOverlayService;
    private bool _toolbarVisible = true;
    private DispatcherTimer? _annotationPollingTimer;
    private readonly ApiService _apiService = new();
    private SupabaseService _supabaseService = new();
    private ScreenshotService? _screenshotService;
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
        
        // Initialize tool manager
        _toolManager = new AnnotationToolManager(AnnotationCanvas);
        _toolManager.ToolChanged += OnToolChanged;
        _toolManager.SelectionChanged += OnSelectionChanged;
        
        // Initialize grid overlay service
        _gridOverlayService = new GridOverlayService(GridOverlayCanvas);
        
        // Initialize realtime subscription
        await InitializeRealtimeSubscription();
    }

    /// <summary>
    /// Handle keyboard shortcuts
    /// Ctrl+Shift+Q = Toggle overlay visibility
    /// Ctrl+Shift+C = Clear annotations
    /// Ctrl+G = Toggle grid overlay
    /// </summary>
    private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
        
        // Check for Ctrl+G
        if (e.Key == Key.G && 
            Keyboard.Modifiers == ModifierKeys.Control)
        {
            ToggleGridOverlay();
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
    /// Clear Screen button click handler
    /// </summary>
    private void ClearScreenButton_Click(object sender, RoutedEventArgs e)
    {
        _annotationRenderer?.ClearAnnotations();
        _toolManager?.ClearAll();
        System.Diagnostics.Debug.WriteLine("✓ Screen cleared by user");
    }

    /// <summary>
    /// Show Grid button click handler
    /// </summary>
    private void ShowGridButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleGridOverlay();
    }

    /// <summary>
    /// Toggle grid overlay visibility
    /// </summary>
    private void ToggleGridOverlay()
    {
        if (_gridOverlayService != null)
        {
            _gridOverlayService.ToggleGrid();
            
            // Update button text based on grid state
            ShowGridButton.Content = _gridOverlayService.IsGridVisible 
                ? "Hide Grid (Ctrl+G)" 
                : "Show Grid (Ctrl+G)";
        }
    }

    /// <summary>
    /// Handle mouse down on annotation canvas
    /// </summary>
    private void AnnotationCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition(AnnotationCanvas);
        _toolManager?.OnMouseDown(position, e);
    }

    /// <summary>
    /// Handle mouse move on annotation canvas
    /// </summary>
    private void AnnotationCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        var position = e.GetPosition(AnnotationCanvas);
        _toolManager?.OnMouseMove(position, e);
    }

    /// <summary>
    /// Handle mouse up on annotation canvas
    /// </summary>
    private void AnnotationCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition(AnnotationCanvas);
        _toolManager?.OnMouseUp(position, e);
    }

    /// <summary>
    /// Tool button click handlers
    /// </summary>
    private void SelectTool_Click(object sender, RoutedEventArgs e)
    {
        SetActiveTool(AnnotationTool.Select, SelectToolButton);
    }

    private void PenTool_Click(object sender, RoutedEventArgs e)
    {
        SetActiveTool(AnnotationTool.Pen, PenToolButton);
    }

    private void TextTool_Click(object sender, RoutedEventArgs e)
    {
        SetActiveTool(AnnotationTool.Text, TextToolButton);
    }

    private void ArrowTool_Click(object sender, RoutedEventArgs e)
    {
        SetActiveTool(AnnotationTool.Arrow, ArrowToolButton);
    }

    private void CircleTool_Click(object sender, RoutedEventArgs e)
    {
        SetActiveTool(AnnotationTool.Circle, CircleToolButton);
    }

    private void RectangleTool_Click(object sender, RoutedEventArgs e)
    {
        SetActiveTool(AnnotationTool.Rectangle, RectangleToolButton);
    }

    private void EraserTool_Click(object sender, RoutedEventArgs e)
    {
        SetActiveTool(AnnotationTool.Eraser, EraserToolButton);
    }

    private void SetActiveTool(AnnotationTool tool, System.Windows.Controls.Primitives.ToggleButton button)
    {
        // Uncheck all other tool buttons
        SelectToolButton.IsChecked = false;
        PenToolButton.IsChecked = false;
        TextToolButton.IsChecked = false;
        ArrowToolButton.IsChecked = false;
        CircleToolButton.IsChecked = false;
        RectangleToolButton.IsChecked = false;
        EraserToolButton.IsChecked = false;
        
        // Check the selected button
        button.IsChecked = true;
        
        // Set the tool
        if (_toolManager != null)
        {
            _toolManager.CurrentTool = tool;
        }
    }

    /// <summary>
    /// Initialize Supabase realtime subscription for this session
    /// </summary>
    private async Task InitializeRealtimeSubscription()
    {
        try
        {
            // Check if Supabase credentials are configured
            if (Configuration.SupabaseUrl.Contains("YOUR_PROJECT") || 
                Configuration.SupabaseAnonKey.Contains("YOUR_ANON_KEY"))
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Supabase credentials not configured - skipping realtime subscription");
                return;
            }
            
            // Initialize Supabase client
            await _supabaseService.Initialize(
                Configuration.SupabaseUrl,
                Configuration.SupabaseAnonKey
            );
            
            // Initialize screenshot service
            _screenshotService = new ScreenshotService(
                Configuration.SupabaseUrl,
                Configuration.SupabaseAnonKey
            );
            
            // Subscribe to events
            _supabaseService.AnnotationReceived += OnAnnotationReceived;
            _supabaseService.TextMessageReceived += OnTextMessageReceived;
            _supabaseService.ErrorOccurred += OnSupabaseError;
            _supabaseService.ScreenshotRequestReceived += OnScreenshotRequestReceived;
            
            // Subscribe to session messages
            await _supabaseService.SubscribeToSession(_sessionId);
            
            // Subscribe to screenshot requests
            await _supabaseService.SubscribeToScreenshotRequests(_sessionId);
            
            System.Diagnostics.Debug.WriteLine($"✓ Realtime subscription active for session: {_sessionId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Realtime connection failed: {ex.Message}");
            // Don't show error dialog for now - just log and continue
            // The app will work fine without realtime (test annotations still work)
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
    /// Handle screenshot request from remote
    /// </summary>
    private async void OnScreenshotRequestReceived(object? sender, ScreenshotRequest request)
    {
        System.Diagnostics.Debug.WriteLine($"📸 Screenshot request received: {request.RequestId}");
        
        // Handle on background thread to avoid blocking UI
        await Task.Run(async () =>
        {
            if (_screenshotService != null)
            {
                await _screenshotService.HandleScreenshotRequest(request.RequestId, request.SessionId);
            }
        });
    }

    /// <summary>
    /// Color button click handler
    /// </summary>
    private void ColorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is string color)
        {
            if (_toolManager != null)
            {
                _toolManager.CurrentColor = color;
            }
        }
    }

    /// <summary>
    /// Size slider value changed handler
    /// </summary>
    private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_toolManager != null)
        {
            _toolManager.CurrentSize = e.NewValue;
        }
    }

    /// <summary>
    /// Delete button click handler
    /// </summary>
    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        _toolManager?.DeleteSelected();
    }

    /// <summary>
    /// Clear all annotations button click handler
    /// </summary>
    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
        var result = System.Windows.MessageBox.Show(
            "Clear all annotations?",
            "Confirm",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            _annotationRenderer?.ClearAnnotations();
            _toolManager?.ClearAll();
        }
    }

    /// <summary>
    /// Toggle toolbar visibility
    /// </summary>
    private void ToggleToolbar_Click(object sender, RoutedEventArgs e)
    {
        _toolbarVisible = !_toolbarVisible;
        
        if (_toolbarVisible)
        {
            AnnotationToolbar.Visibility = Visibility.Visible;
            ShowToolbarButton.Visibility = Visibility.Collapsed;
        }
        else
        {
            AnnotationToolbar.Visibility = Visibility.Collapsed;
            ShowToolbarButton.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Tool changed event handler
    /// </summary>
    private void OnToolChanged(object? sender, AnnotationTool tool)
    {
        System.Diagnostics.Debug.WriteLine($"Tool changed to: {tool}");
    }

    /// <summary>
    /// Selection changed event handler
    /// </summary>
    private void OnSelectionChanged(object? sender, UIElement? element)
    {
        DeleteButton.IsEnabled = element != null;
    }

    /// <summary>
    /// Close Session button click handler - returns to main menu
    /// </summary>
    private void CloseSessionButton_Click(object sender, RoutedEventArgs e)
    {
        // Show confirmation dialog
        var result = System.Windows.MessageBox.Show(
            "Close this session and return to main menu?",
            "Confirm Close Session",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // Clean up current session
            CleanupSession();
            
            // Show HomePage (main menu)
            var homePage = new HomePage();
            homePage.Show();
            
            // Close this window
            this.Close();
        }
    }

    /// <summary>
    /// Close App button click handler - exits application
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // Show confirmation dialog
        var result = System.Windows.MessageBox.Show(
            "Are you sure you want to close AI Teacher Assistant?",
            "Confirm Close",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// Clean up session resources
    /// </summary>
    private void CleanupSession()
    {
        try
        {
            // Unsubscribe from realtime
            _supabaseService.Unsubscribe().Wait();
            
            // Unsubscribe from events
            if (_supabaseService != null)
            {
                _supabaseService.AnnotationReceived -= OnAnnotationReceived;
                _supabaseService.TextMessageReceived -= OnTextMessageReceived;
                _supabaseService.ErrorOccurred -= OnSupabaseError;
                _supabaseService.ScreenshotRequestReceived -= OnScreenshotRequestReceived;
            }
            
            // Unsubscribe from tool manager events
            if (_toolManager != null)
            {
                _toolManager.ToolChanged -= OnToolChanged;
                _toolManager.SelectionChanged -= OnSelectionChanged;
            }
            
            // Stop polling timer
            _annotationPollingTimer?.Stop();
            
            System.Diagnostics.Debug.WriteLine("✓ Session cleaned up successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error during session cleanup: {ex.Message}");
        }
    }



    /// <summary>
    /// Override to handle window closing
    /// </summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        CleanupSession();
        base.OnClosing(e);
    }
}