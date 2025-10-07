using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AITeacherAssistant.Services;

/// <summary>
/// Manages interactive annotation tools for the overlay
/// </summary>
public class AnnotationToolManager
{
    private readonly Canvas _canvas;
    private AnnotationTool _currentTool = AnnotationTool.Select;
    private string _currentColor = "#667eea";
    private double _currentSize = 2;
    
    // Drawing state
    private bool _isDrawing = false;
    private System.Windows.Point _startPoint;
    private UIElement? _currentElement;
    private List<System.Windows.Point> _freehandPoints = new();
    
    // Selection state
    private UIElement? _selectedElement;
    private System.Windows.Point _selectionOffset;
    
    public event EventHandler<AnnotationTool>? ToolChanged;
    public event EventHandler<UIElement?>? SelectionChanged;
    
    public AnnotationTool CurrentTool
    {
        get => _currentTool;
        set
        {
            _currentTool = value;
            ToolChanged?.Invoke(this, value);
        }
    }
    
    public string CurrentColor
    {
        get => _currentColor;
        set => _currentColor = value;
    }
    
    public double CurrentSize
    {
        get => _currentSize;
        set => _currentSize = value;
    }
    
    public UIElement? SelectedElement
    {
        get => _selectedElement;
        private set
        {
            if (_selectedElement != value)
            {
                // Remove previous selection highlight
                if (_selectedElement is Shape shape)
                {
                    shape.StrokeDashArray = null;
                }
                
                _selectedElement = value;
                
                // Add selection highlight
                if (_selectedElement is Shape newShape)
                {
                    newShape.StrokeDashArray = new DoubleCollection { 4, 2 };
                }
                
                SelectionChanged?.Invoke(this, _selectedElement);
            }
        }
    }
    
    public AnnotationToolManager(Canvas canvas)
    {
        _canvas = canvas;
    }
    
    /// <summary>
    /// Handle mouse down on canvas
    /// </summary>
    public void OnMouseDown(System.Windows.Point position, System.Windows.Input.MouseButtonEventArgs e)
    {
        _isDrawing = true;
        _startPoint = position;
        
        switch (_currentTool)
        {
            case AnnotationTool.Select:
                HandleSelectMouseDown(position, e);
                break;
                
            case AnnotationTool.Pen:
                StartFreehandDraw(position);
                break;
                
            case AnnotationTool.Arrow:
            case AnnotationTool.Circle:
            case AnnotationTool.Rectangle:
                StartShapeDraw(position);
                break;
                
            case AnnotationTool.Text:
                AddTextAnnotation(position);
                break;
                
            case AnnotationTool.Eraser:
                EraseAtPoint(position);
                break;
        }
    }
    
    /// <summary>
    /// Handle mouse move on canvas
    /// </summary>
    public void OnMouseMove(System.Windows.Point position, System.Windows.Input.MouseEventArgs e)
    {
        if (!_isDrawing) return;
        
        switch (_currentTool)
        {
            case AnnotationTool.Select:
                HandleSelectMouseMove(position);
                break;
                
            case AnnotationTool.Pen:
                ContinueFreehandDraw(position);
                break;
                
            case AnnotationTool.Arrow:
            case AnnotationTool.Circle:
            case AnnotationTool.Rectangle:
                UpdateShapeDraw(position);
                break;
                
            case AnnotationTool.Eraser:
                EraseAtPoint(position);
                break;
        }
    }
    
    /// <summary>
    /// Handle mouse up on canvas
    /// </summary>
    public void OnMouseUp(System.Windows.Point position, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (!_isDrawing) return;
        
        _isDrawing = false;
        
        switch (_currentTool)
        {
            case AnnotationTool.Pen:
                FinalizeFreehandDraw();
                break;
                
            case AnnotationTool.Arrow:
            case AnnotationTool.Circle:
            case AnnotationTool.Rectangle:
                FinalizeShapeDraw();
                break;
        }
        
        _currentElement = null;
        _freehandPoints.Clear();
    }
    
    private void HandleSelectMouseDown(System.Windows.Point position, System.Windows.Input.MouseButtonEventArgs e)
    {
        // Find element at position
        var hitElement = FindElementAtPoint(position);
        
        if (hitElement != null && hitElement != _canvas)
        {
            SelectedElement = hitElement;
            
            var left = Canvas.GetLeft(hitElement);
            var top = Canvas.GetTop(hitElement);
            
            // Handle NaN values
            if (double.IsNaN(left)) left = 0;
            if (double.IsNaN(top)) top = 0;
            
            _selectionOffset = new System.Windows.Point(
                position.X - left,
                position.Y - top
            );
        }
        else
        {
            SelectedElement = null;
        }
    }
    
    private void HandleSelectMouseMove(System.Windows.Point position)
    {
        if (SelectedElement != null)
        {
            // Move selected element
            Canvas.SetLeft(SelectedElement, position.X - _selectionOffset.X);
            Canvas.SetTop(SelectedElement, position.Y - _selectionOffset.Y);
        }
    }
    
    private void StartFreehandDraw(System.Windows.Point position)
    {
        _freehandPoints.Clear();
        _freehandPoints.Add(position);
    }
    
    private void ContinueFreehandDraw(System.Windows.Point position)
    {
        _freehandPoints.Add(position);
        
        if (_freehandPoints.Count > 1)
        {
            var line = new Line
            {
                X1 = _freehandPoints[^2].X,
                Y1 = _freehandPoints[^2].Y,
                X2 = position.X,
                Y2 = position.Y,
                Stroke = ParseColor(_currentColor),
                StrokeThickness = _currentSize,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            
            _canvas.Children.Add(line);
        }
    }
    
    private void FinalizeFreehandDraw()
    {
        // Freehand is already drawn line by line
        _freehandPoints.Clear();
    }
    
    private void StartShapeDraw(System.Windows.Point position)
    {
        // Create preview shape
        _currentElement = _currentTool switch
        {
            AnnotationTool.Arrow => CreateArrowPreview(position, position),
            AnnotationTool.Circle => CreateCirclePreview(position, 0),
            AnnotationTool.Rectangle => CreateRectanglePreview(position, 0, 0),
            _ => null
        };
        
        if (_currentElement != null)
        {
            _canvas.Children.Add(_currentElement);
        }
    }
    
    private void UpdateShapeDraw(System.Windows.Point position)
    {
        if (_currentElement == null) return;
        
        switch (_currentTool)
        {
            case AnnotationTool.Arrow:
                UpdateArrow(position);
                break;
            case AnnotationTool.Circle:
                UpdateCircle(position);
                break;
            case AnnotationTool.Rectangle:
                UpdateRectangle(position);
                break;
        }
    }
    
    private void FinalizeShapeDraw()
    {
        // Shape is already added to canvas
        _currentElement = null;
    }
    
    private void AddTextAnnotation(System.Windows.Point position)
    {
        var textBox = new System.Windows.Controls.TextBox
        {
            Text = "Type here...",
            FontSize = 16 + (_currentSize * 2),
            Foreground = ParseColor(_currentColor),
            Background = System.Windows.Media.Brushes.Transparent,
            BorderThickness = new Thickness(0),
            MinWidth = 100
        };
        
        Canvas.SetLeft(textBox, position.X);
        Canvas.SetTop(textBox, position.Y);
        
        _canvas.Children.Add(textBox);
        textBox.Focus();
        textBox.SelectAll();
        
        // Remove on focus lost if empty
        textBox.LostFocus += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text == "Type here...")
            {
                _canvas.Children.Remove(textBox);
            }
        };
        
        _isDrawing = false; // Don't continue "drawing" text
    }
    
    private void EraseAtPoint(System.Windows.Point position)
    {
        var element = FindElementAtPoint(position);
        if (element != null && element != _canvas)
        {
            _canvas.Children.Remove(element);
        }
    }
    
    public void DeleteSelected()
    {
        if (SelectedElement != null)
        {
            _canvas.Children.Remove(SelectedElement);
            SelectedElement = null;
        }
    }
    
    public void ClearAll()
    {
        _canvas.Children.Clear();
        SelectedElement = null;
    }
    
    private UIElement? FindElementAtPoint(System.Windows.Point position)
    {
        HitTestResult result = VisualTreeHelper.HitTest(_canvas, position);
        
        // Walk up the visual tree to find a canvas child
        var visual = result?.VisualHit as DependencyObject;
        while (visual != null && visual != _canvas)
        {
            if (visual is UIElement element && _canvas.Children.Contains(element))
            {
                return element;
            }
            visual = VisualTreeHelper.GetParent(visual);
        }
        
        return null;
    }
    
    private Line CreateArrowPreview(System.Windows.Point from, System.Windows.Point to)
    {
        var line = new Line
        {
            X1 = from.X,
            Y1 = from.Y,
            X2 = to.X,
            Y2 = to.Y,
            Stroke = ParseColor(_currentColor),
            StrokeThickness = _currentSize,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Triangle
        };
        
        return line;
    }
    
    private void UpdateArrow(System.Windows.Point to)
    {
        if (_currentElement is Line line)
        {
            line.X2 = to.X;
            line.Y2 = to.Y;
        }
    }
    
    private Ellipse CreateCirclePreview(System.Windows.Point center, double radius)
    {
        var ellipse = new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Stroke = ParseColor(_currentColor),
            StrokeThickness = _currentSize,
            Fill = System.Windows.Media.Brushes.Transparent
        };
        
        Canvas.SetLeft(ellipse, center.X - radius);
        Canvas.SetTop(ellipse, center.Y - radius);
        
        return ellipse;
    }
    
    private void UpdateCircle(System.Windows.Point currentPoint)
    {
        if (_currentElement is Ellipse ellipse)
        {
            var radius = Math.Sqrt(
                Math.Pow(currentPoint.X - _startPoint.X, 2) +
                Math.Pow(currentPoint.Y - _startPoint.Y, 2)
            );
            
            ellipse.Width = radius * 2;
            ellipse.Height = radius * 2;
            Canvas.SetLeft(ellipse, _startPoint.X - radius);
            Canvas.SetTop(ellipse, _startPoint.Y - radius);
        }
    }
    
    private System.Windows.Shapes.Rectangle CreateRectanglePreview(System.Windows.Point topLeft, double width, double height)
    {
        var rectangle = new System.Windows.Shapes.Rectangle
        {
            Width = width,
            Height = height,
            Stroke = ParseColor(_currentColor),
            StrokeThickness = _currentSize,
            Fill = System.Windows.Media.Brushes.Transparent
        };
        
        Canvas.SetLeft(rectangle, topLeft.X);
        Canvas.SetTop(rectangle, topLeft.Y);
        
        return rectangle;
    }
    
    private void UpdateRectangle(System.Windows.Point currentPoint)
    {
        if (_currentElement is System.Windows.Shapes.Rectangle rectangle)
        {
            var width = Math.Abs(currentPoint.X - _startPoint.X);
            var height = Math.Abs(currentPoint.Y - _startPoint.Y);
            
            rectangle.Width = width;
            rectangle.Height = height;
            
            Canvas.SetLeft(rectangle, Math.Min(_startPoint.X, currentPoint.X));
            Canvas.SetTop(rectangle, Math.Min(_startPoint.Y, currentPoint.Y));
        }
    }
    
    private System.Windows.Media.Brush ParseColor(string hexColor)
    {
        try
        {
            return (System.Windows.Media.Brush)new BrushConverter().ConvertFrom(hexColor)!;
        }
        catch
        {
            return System.Windows.Media.Brushes.Black;
        }
    }
}

/// <summary>
/// Available annotation tools
/// </summary>
public enum AnnotationTool
{
    Select,
    Pen,
    Text,
    Arrow,
    Circle,
    Rectangle,
    Eraser
}

