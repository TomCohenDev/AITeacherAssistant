using System.Text.Json.Serialization;

namespace AITeacherAssistant.Models;

public class AnnotationResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "annotation";
    
    [JsonPropertyName("elements")]
    public List<AnnotationElement> Elements { get; set; } = new();
}

public class AnnotationElement
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = ""; // text, arrow, circle, rectangle, line, freehand
    
    // Text properties
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    [JsonPropertyName("fontSize")]
    public int? FontSize { get; set; }
    
    [JsonPropertyName("fontWeight")]
    public string? FontWeight { get; set; }
    
    [JsonPropertyName("maxWidth")]
    public double? MaxWidth { get; set; }
    
    // Position properties
    [JsonPropertyName("x")]
    public double? X { get; set; }
    
    [JsonPropertyName("y")]
    public double? Y { get; set; }
    
    // Shape properties
    [JsonPropertyName("from")]
    public Point? From { get; set; }
    
    [JsonPropertyName("to")]
    public Point? To { get; set; }
    
    [JsonPropertyName("center")]
    public Point? Center { get; set; }
    
    [JsonPropertyName("radius")]
    public double? Radius { get; set; }
    
    [JsonPropertyName("width")]
    public double? Width { get; set; }
    
    [JsonPropertyName("height")]
    public double? Height { get; set; }
    
    // Freehand properties
    [JsonPropertyName("points")]
    public List<Point>? Points { get; set; }
    
    // Style properties
    [JsonPropertyName("color")]
    public string? Color { get; set; }
    
    [JsonPropertyName("strokeColor")]
    public string? StrokeColor { get; set; }
    
    [JsonPropertyName("fillColor")]
    public string? FillColor { get; set; }
    
    [JsonPropertyName("thickness")]
    public double? Thickness { get; set; }
    
    [JsonPropertyName("dashed")]
    public bool? Dashed { get; set; }
}

public class Point
{
    [JsonPropertyName("x")]
    public double X { get; set; }
    
    [JsonPropertyName("y")]
    public double Y { get; set; }
}

