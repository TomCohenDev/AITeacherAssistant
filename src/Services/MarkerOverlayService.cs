using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AITeacherAssistant.Services;

/// <summary>
/// Service for displaying numbered reference markers to help calibrate AI annotation positioning
/// </summary>
public class MarkerOverlayService
{
    private readonly Canvas _canvas;
    private bool _markersVisible = false;
    
    // 25 marker positions in 5x5 grid
    private readonly List<(int number, int x, int y)> _markers = new()
    {
        // Row 1 (y=100)
        (1, 100, 100), (2, 580, 100), (3, 1060, 100), (4, 1540, 100), (5, 1820, 100),
        
        // Row 2 (y=320)
        (6, 100, 320), (7, 580, 320), (8, 1060, 320), (9, 1540, 320), (10, 1820, 320),
        
        // Row 3 (y=540 - center)
        (11, 100, 540), (12, 580, 540), (13, 1060, 540), (14, 1540, 540), (15, 1820, 540),
        
        // Row 4 (y=760)
        (16, 100, 760), (17, 580, 760), (18, 1060, 760), (19, 1540, 760), (20, 1820, 760),
        
        // Row 5 (y=980)
        (21, 100, 980), (22, 580, 980), (23, 1060, 980), (24, 1540, 980), (25, 1820, 980)
    };
    
    public MarkerOverlayService(Canvas canvas)
    {
        _canvas = canvas;
    }
    
    /// <summary>
    /// Toggle marker visibility
    /// </summary>
    public void ToggleMarkers()
    {
        if (_markersVisible)
        {
            HideMarkers();
        }
        else
        {
            ShowMarkers();
        }
    }
    
    /// <summary>
    /// Show the marker overlay
    /// </summary>
    public void ShowMarkers()
    {
        if (_markersVisible) return;
        
        DrawMarkers();
        _canvas.Visibility = Visibility.Visible;
        _markersVisible = true;
        
        System.Diagnostics.Debug.WriteLine("✓ Marker overlay shown with 25 reference points");
    }
    
    /// <summary>
    /// Hide the marker overlay
    /// </summary>
    public void HideMarkers()
    {
        if (!_markersVisible) return;
        
        _canvas.Visibility = Visibility.Collapsed;
        _markersVisible = false;
        
        System.Diagnostics.Debug.WriteLine("✓ Marker overlay hidden");
    }
    
    /// <summary>
    /// Draw all 25 reference markers
    /// </summary>
    private void DrawMarkers()
    {
        ClearMarkers();
        
        foreach (var (number, x, y) in _markers)
        {
            var marker = CreateMarker(number, x, y);
            _canvas.Children.Add(marker);
        }
        
        System.Diagnostics.Debug.WriteLine($"✓ Drew {_markers.Count} reference markers");
    }
    
    /// <summary>
    /// Clear all marker elements
    /// </summary>
    public void ClearMarkers()
    {
        _canvas.Children.Clear();
    }
    
    /// <summary>
    /// Create a single marker with number and coordinates
    /// </summary>
    private UIElement CreateMarker(int number, int x, int y)
    {
        var container = new Canvas();
        
        // White background circle for number readability
        var backgroundCircle = new Ellipse
        {
            Width = 30,
            Height = 30,
            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 255, 255, 255)), // More transparent white
            Stroke = new SolidColorBrush(System.Windows.Media.Colors.Black),
            StrokeThickness = 1
        };
        Canvas.SetLeft(backgroundCircle, x - 15);
        Canvas.SetTop(backgroundCircle, y - 15);
        container.Children.Add(backgroundCircle);
        
        // Main marker circle (cyan)
        var markerCircle = new Ellipse
        {
            Width = 40,
            Height = 40,
            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(120, 0, 255, 255)), // More transparent cyan
            Stroke = new SolidColorBrush(System.Windows.Media.Colors.Black),
            StrokeThickness = 3
        };
        Canvas.SetLeft(markerCircle, x - 20);
        Canvas.SetTop(markerCircle, y - 20);
        container.Children.Add(markerCircle);
        
        // Marker number text
        var numberText = new TextBlock
        {
            Text = number.ToString(),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(System.Windows.Media.Colors.Black),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center
        };
        Canvas.SetLeft(numberText, x - 8);
        Canvas.SetTop(numberText, y - 10);
        container.Children.Add(numberText);
        
        // Coordinates text below marker
        var coordText = new TextBlock
        {
            Text = $"({x}, {y})",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(System.Windows.Media.Colors.White)
        };
        
        // Add thicker stroke effect for better visibility
        coordText.Effect = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = System.Windows.Media.Colors.Black,
            BlurRadius = 4,
            ShadowDepth = 0,
            Opacity = 1
        };
        
        Canvas.SetLeft(coordText, x - 25);
        Canvas.SetTop(coordText, y + 30);
        container.Children.Add(coordText);
        
        return container;
    }
    
    /// <summary>
    /// Get current marker visibility state
    /// </summary>
    public bool MarkersVisible => _markersVisible;
    
    /// <summary>
    /// Get marker count
    /// </summary>
    public int MarkerCount => _markers.Count;
    
    /// <summary>
    /// Get marker position by number (1-25)
    /// </summary>
    public (int x, int y)? GetMarkerPosition(int markerNumber)
    {
        var marker = _markers.Find(m => m.number == markerNumber);
        return marker.number == 0 ? null : (marker.x, marker.y);
    }
}
