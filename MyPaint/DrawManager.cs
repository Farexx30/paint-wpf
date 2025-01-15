using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Reflection;

namespace MyPaint;

internal static class DrawManager
{
    //Static properties:
    internal static class LineProperties
    {
        internal static SolidColorBrush BrushColor { get; set; } = new SolidColorBrush(Color.FromRgb(0, 0, 0));
    }

    internal static class RectangleProperties
    {
        internal static double Width { get; set; } = 60d;
        internal static double Height { get; set; } = 40d;
        internal static SolidColorBrush BrushColor { get; set; } = new SolidColorBrush(Color.FromRgb(0, 0, 0));
    }

    internal static class EllipseProperties
    {
        internal static double Width { get; set; } = 60d;
        internal static double Height { get; set; } = 60d;
        internal static SolidColorBrush BrushColor { get; set; } = new SolidColorBrush(Color.FromRgb(0, 0, 0));
    }

    internal static class PolygonProperties
    {
        internal static double Size { get; set; } = 60d;
        internal static SolidColorBrush BrushColor { get; set; } = new SolidColorBrush(Color.FromRgb(0, 0, 0));
    }

    internal static class ArrowProperties
    {
        internal static double Size { get; set; } = 60d;
        internal static SolidColorBrush BrushColor { get; set; } = new SolidColorBrush(Color.FromRgb(0, 0, 0));
    }


    //Methods:
    internal static Line DrawLine(Point startPoint, Point endPoint, SolidColorBrush brushColor)
    {
        var line = new Line
        {
            X1 = startPoint.X,
            Y1 = startPoint.Y,
            X2 = endPoint.X,
            Y2 = endPoint.Y,
            Stroke = brushColor
        };

        return line;
    }

    internal static Rectangle DrawRectangle(double width, double height, SolidColorBrush brushColor)
    {
        var rectangle = new Rectangle
        {
            Width = width,
            Height = height,
            Stroke = brushColor
        };

        return rectangle;
    }

    internal static Ellipse DrawEllipse(double width, double height, SolidColorBrush brushColor, bool shouldFill = false)
    {
        var ellipse = new Ellipse
        {
            Width = width,
            Height = height,      
            Stroke = brushColor
        };

        if (shouldFill)
        {
            ellipse.Fill = brushColor;
        }

        return ellipse;
    }

    internal static Polygon DrawRegularPolygon(Point mousePosition, double radius, uint vertices, SolidColorBrush brushColor)
    {
        var mousePositionX = mousePosition.X;
        var mousePositionY = mousePosition.Y;

        var angle = 2 * Math.PI / vertices;
        var angleOffset = Math.PI / 2;

        var polygonPoints = new PointCollection();
        for (int i = 0; i < vertices; ++i)
        {
            var currentAngle = i * angle + angleOffset;
            var xCoordinate = mousePositionX + radius * Math.Cos(currentAngle);
            var yCoordinate = mousePositionY - radius * Math.Sin(currentAngle);

            polygonPoints.Add(new Point(xCoordinate, yCoordinate));
        }

        var polygon = new Polygon
        {
            Points = polygonPoints,
            Stroke = brushColor
        };

        return polygon;
    }

    internal static Polygon DrawArrow(Point mousePosition, double size, SolidColorBrush brushColor)
    {
        var mousePositionX = mousePosition.X;
        var mousePositionY = mousePosition.Y;

        var point1 = new Point(mousePositionX - size * 2, mousePositionY - size);
        var point2 = new Point(mousePositionX + size, mousePositionY - size);
        var point3 = new Point(mousePositionX + size, mousePositionY - size * 2);
        var point4 = new Point(mousePositionX + size * 4, mousePositionY);
        var point5 = new Point(mousePositionX + size, mousePositionY + size * 2);
        var point6 = new Point(mousePositionX + size, mousePositionY + size);
        var point7 = new Point(mousePositionX - size * 2, mousePositionY + size);

        var points = new PointCollection
        {
            point1,
            point2, 
            point3, 
            point4, 
            point5, 
            point6, 
            point7
        };

        var arrow = new Polygon
        {
            Stroke = brushColor,
            Points = points
        };
        
        return arrow;
    }
}
