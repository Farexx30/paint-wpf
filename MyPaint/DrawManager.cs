using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

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
        internal static double Width { get; set; } = 6d;
        internal static double Height { get; set; } = 6d;
        internal static SolidColorBrush BrushColor { get; set; } = new SolidColorBrush(Color.FromRgb(0, 0, 0));
    }

    internal static class PolygonProperties
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

    internal static Ellipse DrawEllipse(double width, double height, SolidColorBrush brushColor)
    {
        var ellipse = new Ellipse
        {
            Width = width,
            Height = height,
            Fill = brushColor
        };

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
}
