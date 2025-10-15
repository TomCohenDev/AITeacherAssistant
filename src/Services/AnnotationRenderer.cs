using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AITeacherAssistant.Models;

namespace AITeacherAssistant.Services;

public class AnnotationRenderer
{
    private readonly Canvas _canvas;
    
    public AnnotationRenderer(Canvas canvas)
    {
        _canvas = canvas;
    }
    
    /// <summary>
    /// Render an annotation response on the canvas
    /// </summary>
    public void RenderAnnotation(AnnotationResponse annotation)
    {
        foreach (var element in annotation.Elements)
        {
            try
            {
                var uiElement = CreateUIElement(element);
                if (uiElement != null)
                {
                    _canvas.Children.Add(uiElement);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rendering element: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Clear all annotations from canvas
    /// </summary>
    public void ClearAnnotations()
    {
        _canvas.Children.Clear();
    }
    
    /// <summary>
    /// Create WPF UI element from annotation element
    /// </summary>
    private UIElement? CreateUIElement(AnnotationElement element)
    {
        return element.Type.ToLower() switch
        {
            "text" => CreateTextBlock(element),
            "arrow" => CreateArrow(element),
            "circle" => CreateCircle(element),
            "rectangle" => CreateRectangle(element),
            "line" => CreateLine(element),
            "freehand" => CreateFreehand(element),
            _ => null
        };
    }
    
    private TextBlock CreateTextBlock(AnnotationElement element)
    {
        var textBlock = new TextBlock
        {
            Text = element.Content ?? "",
            FontSize = element.FontSize ?? 16,
            Foreground = ParseColor(element.Color ?? "#000000"),
            TextWrapping = TextWrapping.Wrap
        };
        
        // Set maxWidth from JSON, default to 1800 if not specified
        var maxWidth = element.MaxWidth ?? 1800;
        textBlock.MaxWidth = maxWidth;
        
        if (element.FontWeight == "bold")
        {
            textBlock.FontWeight = FontWeights.Bold;
        }
        
        Canvas.SetLeft(textBlock, element.X ?? 0);
        Canvas.SetTop(textBlock, element.Y ?? 0);
        
        return textBlock;
    }
    
    private UIElement CreateArrow(AnnotationElement element)
    {
        if (element.From == null || element.To == null)
            return new Canvas();
        
        var canvas = new Canvas();
        
        // Arrow line
        var line = new Line
        {
            X1 = element.From.X,
            Y1 = element.From.Y,
            X2 = element.To.X,
            Y2 = element.To.Y,
            Stroke = ParseColor(element.Color ?? "#000000"),
            StrokeThickness = element.Thickness ?? 2
        };
        canvas.Children.Add(line);
        
        // Arrow head
        var angle = Math.Atan2(element.To.Y - element.From.Y, element.To.X - element.From.X);
        var arrowHeadLength = 10;
        var arrowHeadAngle = Math.PI / 6;
        
        var arrowPoint1 = new Line
        {
            X1 = element.To.X,
            Y1 = element.To.Y,
            X2 = element.To.X - arrowHeadLength * Math.Cos(angle - arrowHeadAngle),
            Y2 = element.To.Y - arrowHeadLength * Math.Sin(angle - arrowHeadAngle),
            Stroke = ParseColor(element.Color ?? "#000000"),
            StrokeThickness = element.Thickness ?? 2
        };
        
        var arrowPoint2 = new Line
        {
            X1 = element.To.X,
            Y1 = element.To.Y,
            X2 = element.To.X - arrowHeadLength * Math.Cos(angle + arrowHeadAngle),
            Y2 = element.To.Y - arrowHeadLength * Math.Sin(angle + arrowHeadAngle),
            Stroke = ParseColor(element.Color ?? "#000000"),
            StrokeThickness = element.Thickness ?? 2
        };
        
        canvas.Children.Add(arrowPoint1);
        canvas.Children.Add(arrowPoint2);
        
        return canvas;
    }
    
    private Ellipse CreateCircle(AnnotationElement element)
    {
        if (element.Center == null || element.Radius == null)
            return new Ellipse();
        
        var ellipse = new Ellipse
        {
            Width = element.Radius.Value * 2,
            Height = element.Radius.Value * 2,
            Stroke = ParseColor(element.StrokeColor ?? element.Color ?? "#000000"),
            StrokeThickness = element.Thickness ?? 2
        };
        
        if (!string.IsNullOrEmpty(element.FillColor) && element.FillColor != "transparent")
        {
            ellipse.Fill = ParseColor(element.FillColor);
        }
        else
        {
            ellipse.Fill = System.Windows.Media.Brushes.Transparent;
        }
        
        Canvas.SetLeft(ellipse, element.Center.X - element.Radius.Value);
        Canvas.SetTop(ellipse, element.Center.Y - element.Radius.Value);
        
        return ellipse;
    }
    
    private System.Windows.Shapes.Rectangle CreateRectangle(AnnotationElement element)
    {
        var rect = new System.Windows.Shapes.Rectangle
        {
            Width = element.Width ?? 100,
            Height = element.Height ?? 50,
            Stroke = ParseColor(element.StrokeColor ?? element.Color ?? "#000000"),
            StrokeThickness = element.Thickness ?? 2
        };
        
        if (!string.IsNullOrEmpty(element.FillColor) && element.FillColor != "transparent")
        {
            rect.Fill = ParseColor(element.FillColor);
        }
        else
        {
            rect.Fill = System.Windows.Media.Brushes.Transparent;
        }
        
        Canvas.SetLeft(rect, element.X ?? 0);
        Canvas.SetTop(rect, element.Y ?? 0);
        
        return rect;
    }
    
    private Line CreateLine(AnnotationElement element)
    {
        if (element.From == null || element.To == null)
            return new Line();
        
        var line = new Line
        {
            X1 = element.From.X,
            Y1 = element.From.Y,
            X2 = element.To.X,
            Y2 = element.To.Y,
            Stroke = ParseColor(element.Color ?? "#000000"),
            StrokeThickness = element.Thickness ?? 1
        };
        
        if (element.Dashed == true)
        {
            line.StrokeDashArray = new DoubleCollection { 4, 2 };
        }
        
        return line;
    }
    
    private System.Windows.Shapes.Path CreateFreehand(AnnotationElement element)
    {
        if (element.Points == null || element.Points.Count < 2)
            return new System.Windows.Shapes.Path();
        
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(new System.Windows.Point(element.Points[0].X, element.Points[0].Y), false, false);
            
            for (int i = 1; i < element.Points.Count; i++)
            {
                context.LineTo(new System.Windows.Point(element.Points[i].X, element.Points[i].Y), true, false);
            }
        }
        
        return new System.Windows.Shapes.Path
        {
            Stroke = ParseColor(element.Color ?? "#000000"),
            StrokeThickness = element.Thickness ?? 2,
            Data = geometry
        };
    }
    
    /// <summary>
    /// Parse hex color string to WPF Brush
    /// </summary>
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

