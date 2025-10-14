using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AITeacherAssistant.Services;

/// <summary>
/// Service for displaying a calibration grid overlay to help debug annotation positioning
/// </summary>
public class GridOverlayService
{
    private readonly Canvas _gridCanvas;
    private bool _isGridVisible = false;
    
    // Screen dimensions
    private const int SCREEN_WIDTH = 1920;
    private const int SCREEN_HEIGHT = 1080;
    
    // Grid settings
    private const int GRID_SPACING = 100;
    private const int MAJOR_GRID_SPACING = 500;
    
    public GridOverlayService(Canvas gridCanvas)
    {
        _gridCanvas = gridCanvas;
    }
    
    /// <summary>
    /// Toggle grid visibility
    /// </summary>
    public void ToggleGrid()
    {
        if (_isGridVisible)
        {
            HideGrid();
        }
        else
        {
            ShowGrid();
        }
    }
    
    /// <summary>
    /// Show the grid overlay
    /// </summary>
    public void ShowGrid()
    {
        if (_isGridVisible) return;
        
        DrawGrid();
        _gridCanvas.Visibility = Visibility.Visible;
        _isGridVisible = true;
        
        System.Diagnostics.Debug.WriteLine("✓ Grid overlay shown");
    }
    
    /// <summary>
    /// Hide the grid overlay
    /// </summary>
    public void HideGrid()
    {
        if (!_isGridVisible) return;
        
        _gridCanvas.Visibility = Visibility.Collapsed;
        _isGridVisible = false;
        
        System.Diagnostics.Debug.WriteLine("✓ Grid overlay hidden");
    }
    
    /// <summary>
    /// Draw all grid elements
    /// </summary>
    private void DrawGrid()
    {
        ClearGrid();
        
        // Layer 1: Zone boundaries (bottom layer)
        DrawZoneBoundaries();
        
        // Layer 2: Grid lines
        DrawGridLines();
        
        // Layer 3: Coordinate labels
        DrawCoordinateLabels();
        
        // Layer 4: Waypoint labels (top layer)
        DrawWaypointLabels();
        
        System.Diagnostics.Debug.WriteLine($"✓ Grid drawn with {_gridCanvas.Children.Count} elements");
    }
    
    /// <summary>
    /// Clear all grid elements
    /// </summary>
    public void ClearGrid()
    {
        _gridCanvas.Children.Clear();
    }
    
    /// <summary>
    /// Draw vertical and horizontal grid lines
    /// </summary>
    private void DrawGridLines()
    {
        // Vertical lines
        for (int x = 0; x <= SCREEN_WIDTH; x += GRID_SPACING)
        {
            bool isMajor = (x % MAJOR_GRID_SPACING == 0);
            var line = new Line
            {
                X1 = x,
                Y1 = 0,
                X2 = x,
                Y2 = SCREEN_HEIGHT,
                Stroke = isMajor ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 255)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 215, 0)),
                StrokeThickness = isMajor ? 3 : 1,
                Opacity = 0.7
            };
            _gridCanvas.Children.Add(line);
        }
        
        // Horizontal lines
        for (int y = 0; y <= SCREEN_HEIGHT; y += GRID_SPACING)
        {
            bool isMajor = (y % MAJOR_GRID_SPACING == 0);
            var line = new Line
            {
                X1 = 0,
                Y1 = y,
                X2 = SCREEN_WIDTH,
                Y2 = y,
                Stroke = isMajor ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 255)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 215, 0)),
                StrokeThickness = isMajor ? 3 : 1,
                Opacity = 0.7
            };
            _gridCanvas.Children.Add(line);
        }
    }
    
    /// <summary>
    /// Draw coordinate labels at grid intersections
    /// </summary>
    private void DrawCoordinateLabels()
    {
        for (int x = 0; x <= SCREEN_WIDTH; x += GRID_SPACING)
        {
            for (int y = 0; y <= SCREEN_HEIGHT; y += GRID_SPACING)
            {
                var textBlock = CreateStrokedText(
                    $"{x},{y}",
                    x + 5,
                    y + 5,
                    10,
                    Colors.White,
                    Colors.Black,
                    FontWeights.Bold
                );
                _gridCanvas.Children.Add(textBlock);
            }
        }
    }
    
    /// <summary>
    /// Draw waypoint labels at key positions
    /// </summary>
    private void DrawWaypointLabels()
    {
        var waypoints = new[]
        {
            // Top row
            ("A1", 0, 0),
            ("A10", 1000, 0),
            ("A20", 1920, 0),
            
            // Left column
            ("B1", 0, 100),
            ("C1", 0, 200),
            ("D1", 0, 300),
            ("E1", 0, 400),
            ("F1", 0, 500),
            ("G1", 0, 600),
            ("H1", 0, 700),
            ("I1", 0, 800),
            ("J1", 0, 900),
            ("K1", 0, 1000),
            
            // Key corners
            ("A1", 0, 0),      // Top-left
            ("K20", 1920, 1080), // Bottom-right
            
            // Center point
            ("F10", 960, 540)
        };
        
        foreach (var (code, x, y) in waypoints)
        {
            // Background box
            var background = new System.Windows.Shapes.Rectangle
            {
                Width = 60,
                Height = 30,
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0)),
                Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0)),
                StrokeThickness = 2,
                RadiusX = 5,
                RadiusY = 5
            };
            Canvas.SetLeft(background, x - 30);
            Canvas.SetTop(background, y - 15);
            _gridCanvas.Children.Add(background);
            
            // Waypoint text
            var textBlock = CreateStrokedText(
                code,
                x,
                y,
                14,
                System.Windows.Media.Color.FromRgb(0, 255, 0), // Lime green
                Colors.Black,
                FontWeights.Bold
            );
            
            // Center the text in the box
            Canvas.SetLeft(textBlock, x - 20);
            Canvas.SetTop(textBlock, y - 10);
            _gridCanvas.Children.Add(textBlock);
        }
    }
    
    /// <summary>
    /// Draw zone boundaries for the 9-zone system
    /// </summary>
    private void DrawZoneBoundaries()
    {
        var zones = new[]
        {
            // Format: (name, x, y, width, height, color)
            // Top row (y: 0-360)
            ("TopLeft", 0, 0, 640, 360, Colors.Red),
            ("TopCenter", 640, 0, 640, 360, Colors.Red),
            ("TopRight", 1280, 0, 640, 360, Colors.Red),
            
            // Middle row (y: 360-720)
            ("MiddleLeft", 0, 360, 640, 360, Colors.Blue),
            ("Center", 640, 360, 640, 360, Colors.Blue),
            ("MiddleRight", 1280, 360, 640, 360, Colors.Blue),
            
            // Bottom row (y: 720-1080)
            ("BottomLeft", 0, 720, 640, 360, Colors.Green),
            ("BottomCenter", 640, 720, 640, 360, Colors.Green),
            ("BottomRight", 1280, 720, 640, 360, Colors.Green)
        };
        
        foreach (var (name, x, y, width, height, color) in zones)
        {
            // Zone boundary rectangle
            var rect = new System.Windows.Shapes.Rectangle
            {
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 10, 5 },
                Fill = System.Windows.Media.Brushes.Transparent,
                Opacity = 0.8
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            _gridCanvas.Children.Add(rect);
            
            // Zone label in center
            var centerX = x + width / 2;
            var centerY = y + height / 2;
            
            // Label background
            var labelBg = new System.Windows.Shapes.Rectangle
            {
                Width = 120,
                Height = 40,
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 0, 0, 0)),
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 2,
                RadiusX = 8,
                RadiusY = 8
            };
            Canvas.SetLeft(labelBg, centerX - 60);
            Canvas.SetTop(labelBg, centerY - 20);
            _gridCanvas.Children.Add(labelBg);
            
            // Zone name text
            var label = CreateStrokedText(
                name,
                centerX,
                centerY,
                16,
                Colors.White,
                color,
                FontWeights.Bold
            );
            Canvas.SetLeft(label, centerX - 50);
            Canvas.SetTop(label, centerY - 10);
            _gridCanvas.Children.Add(label);
        }
    }
    
    /// <summary>
    /// Create text with stroke outline for visibility
    /// </summary>
    private TextBlock CreateStrokedText(
        string text,
        double x,
        double y,
        double fontSize,
        System.Windows.Media.Color fillColor,
        System.Windows.Media.Color strokeColor,
        FontWeight fontWeight)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            FontSize = fontSize,
            FontWeight = fontWeight,
            Foreground = new SolidColorBrush(fillColor)
        };
        
        // Add stroke effect using a drop shadow workaround
        textBlock.Effect = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = strokeColor,
            BlurRadius = 2,
            ShadowDepth = 0,
            Opacity = 1
        };
        
        Canvas.SetLeft(textBlock, x);
        Canvas.SetTop(textBlock, y);
        
        return textBlock;
    }
    
    /// <summary>
    /// Get current grid visibility state
    /// </summary>
    public bool IsGridVisible => _isGridVisible;
}

