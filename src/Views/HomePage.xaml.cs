using System;
using System.Diagnostics;
using System.Windows;
using AITeacherAssistant.Services;

namespace AITeacherAssistant.Views;

/// <summary>
/// Home/Welcome page for the AI Teacher Assistant application
/// </summary>
public partial class HomePage : Window
{
    private readonly SessionService _sessionService;
    private readonly ApiService _apiService;

    public HomePage()
    {
        InitializeComponent();
        
        // Initialize services
        _sessionService = new SessionService();
        _apiService = new ApiService();
    }

    /// <summary>
    /// Handle Start New Session button click
    /// </summary>
    private async void StartSessionButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Show loading state
            StartSessionButton.IsEnabled = false;
            StartSessionButton.Content = "Creating session...";

            // Generate session code
            var sessionCode = _sessionService.CreateNewSession();

            // TODO: Call n8n API to create session
            // var success = await _apiService.CreateSession(sessionCode);
            
            // For now, simulate API call success
            await Task.Delay(1000); // Simulate network delay
            var success = true;

            if (success)
            {
                // Open StartupWindow with the session code
                var startupWindow = new StartupWindow(sessionCode);
                startupWindow.Show();
                
                // Close this window
                this.Close();
            }
            else
            {
                // Show error message
                MessageBox.Show(
                    "Failed to create session. Please check your internet connection and try again.",
                    "Session Creation Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                // Reset button state
                StartSessionButton.IsEnabled = true;
                StartSessionButton.Content = "Start New Session";
            }
        }
        catch (Exception ex)
        {
            // Handle unexpected errors
            MessageBox.Show(
                $"An unexpected error occurred: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            // Reset button state
            StartSessionButton.IsEnabled = true;
            StartSessionButton.Content = "Start New Session";
        }
    }

    /// <summary>
    /// Open User Guide documentation
    /// </summary>
    private void OpenUserGuide_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl("https://github.com/your-repo/docs/user-guide");
    }

    /// <summary>
    /// Open Tutorial Videos
    /// </summary>
    private void OpenTutorials_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl("https://youtube.com/your-channel");
    }

    /// <summary>
    /// Open FAQ documentation
    /// </summary>
    private void OpenFAQ_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl("https://github.com/your-repo/docs/faq");
    }

    /// <summary>
    /// Open Issue Reporter
    /// </summary>
    private void ReportIssue_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl("https://github.com/your-repo/issues");
    }

    /// <summary>
    /// Open Discord Community
    /// </summary>
    private void OpenDiscord_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl("https://discord.gg/your-invite");
    }

    /// <summary>
    /// Open Contact Support
    /// </summary>
    private void ContactSupport_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl("mailto:support@yourapp.com");
    }

    /// <summary>
    /// Helper method to open URLs in default browser
    /// </summary>
    /// <param name="url">URL to open</param>
    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to open link: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// Handle API errors
    /// </summary>
    /// <param name="error">Error message</param>
    private void HandleApiError(string error)
    {
        MessageBox.Show(
            $"API Error: {error}",
            "Connection Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
