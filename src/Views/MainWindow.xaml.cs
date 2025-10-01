using System.Runtime.InteropServices;
using System.Text;
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

namespace AITeacherAssistant.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// Main overlay window that provides transparent, always-on-top functionality
/// </summary>
public partial class MainWindow : Window
{

    private bool _isOverlayActive = true;
    private IntPtr _windowHandle;

    public MainWindow()
    {
        InitializeComponent();
        
        // Set up event handlers
        Loaded += MainWindow_Loaded;
        KeyDown += MainWindow_KeyDown;
    }

    /// <summary>
    /// Window loaded event - initialize overlay functionality
    /// </summary>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Get the window handle for Win32 API calls
        _windowHandle = new WindowInteropHelper(this).Handle;
        
        // Set focus to receive keyboard input
        this.Focus();
        
        UpdateStatusIndicator(true);
    }

    /// <summary>
    /// Handle keyboard shortcuts
    /// Ctrl+Shift+Q = Toggle overlay visibility
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
        base.OnClosing(e);
        // Clean up resources if needed
    }
}