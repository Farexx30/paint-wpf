using Emgu.CV.Structure;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyPaint;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
/// 

enum EditSegmentMode
{
    StartPoint,
    EndPoint,
}

enum DrawStyle
{
    Freestyle,
    Point,
    Segment,
    EditSegment,
    Rectangle,
    Polygon
}

public partial class MainWindow : Window
{
    private DrawStyle _drawStyle = DrawStyle.Freestyle;
    private Point _currentMousePosition = new();
    private Color _currentColor = Color.FromRgb(0, 0, 0); //Black color by default

    // !!! SEGMENT FIELDS !!! //
    private Point? _newSegmentStartPoint;
    private Line? _selectedSegmentToEdit;
    private EditSegmentMode? _editSegmentMode;
    private readonly List<Line> _segments = [];
    private readonly List<System.Windows.Shapes.Ellipse> _editSegmentPoints = [];

    // !!! HELP FIELDS !!! /!
    private bool _isMenuFocused = false;

    public MainWindow()
    {
        InitializeComponent();
    }




    // ==================================================
    // PRIVATE EVENT METHODS
    // ==================================================

    // !!! MOUSE EVENTS !!! //
    private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && _drawStyle == DrawStyle.Freestyle)
        {
            if (_isMenuFocused)
            {
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(menu), null); //Removes the focus from "menu".
                return;
            }

            var brushColor = new SolidColorBrush(_currentColor);
            var line = DrawManager.DrawLine(_currentMousePosition, e.GetPosition(this), brushColor);

            _currentMousePosition = e.GetPosition(this);

            mainCanvas.Children.Add(line);
        }
    }

    private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _currentMousePosition = e.GetPosition(this);

        switch(_drawStyle)
        {
            case DrawStyle.Point:
                AddPointAndMakeVisible();
                break;
            case DrawStyle.Segment:
                HandleSegmentCreation();
                break;
            case DrawStyle.EditSegment:
                HandleSegmentEdit();
                break;
            case DrawStyle.Rectangle:
                AddRectangleAndMakeVisible();
                break;
            case DrawStyle.Polygon:
                AddPolygonAndMakeVisible();
                break;
        }
    }

    // !!! MENU EVENTS !!! //
    private void Menu_GotFocus(object sender, RoutedEventArgs e)
    {
        _isMenuFocused = true;
    }

    private void Menu_LostFocus(object sender, RoutedEventArgs e)
    {
        _isMenuFocused = false;
    }

    // !!! CHANGE DRAW STYLE EVENTS !!! //
    private void DrawFreestyleButton_Click(object sender, RoutedEventArgs e)
    {
        _drawStyle = DrawStyle.Freestyle;
        _newSegmentStartPoint = null;

        RemoveSegmentPointsEffect();
        _selectedSegmentToEdit = null;
        _editSegmentMode = null;
    }

    private void DrawPointsButton_Click(object sender, RoutedEventArgs e)
    {
        _drawStyle = DrawStyle.Point;
        _newSegmentStartPoint = null;

        RemoveSegmentPointsEffect();
        _selectedSegmentToEdit = null;
        _editSegmentMode = null;
    }

    private void DrawSegmentMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _drawStyle = DrawStyle.Segment;

        RemoveSegmentPointsEffect();
        _selectedSegmentToEdit = null;
        _editSegmentMode = null;
    }

    private void EditSegmentMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _drawStyle = DrawStyle.EditSegment;
        _newSegmentStartPoint = null;

        AddSegmentPointsEffect();
    }

    private void DrawRectangleMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _drawStyle = DrawStyle.Rectangle;
        _newSegmentStartPoint = null;

        RemoveSegmentPointsEffect();
        _selectedSegmentToEdit = null;
        _editSegmentMode = null;
    }

    private void DrawPolygonMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _drawStyle = DrawStyle.Polygon;
        _newSegmentStartPoint = null;

        RemoveSegmentPointsEffect();
        _selectedSegmentToEdit = null;
        _editSegmentMode = null;
    }

    // !!! NEW WINDOW EVENTS !!! //
    private void ColorPickerRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var colorPickerWindow = new ColorPickerWindow();

        colorPickerWindow.Show();
    }




    // =========================================================
    // PRIVATE HELP METHODS
    // =========================================================

    // !!! ADD AND MAKE VISIBLE METHOD !!! //
    private void AddPointAndMakeVisible()
    {
        var width = DrawManager.EllipseProperties.Width;
        var height = DrawManager.EllipseProperties.Height;
        var brushColor = DrawManager.EllipseProperties.BrushColor;
        var ellipse = DrawManager.DrawEllipse(width, height, brushColor);

        Canvas.SetLeft(ellipse, _currentMousePosition.X - ellipse.Width / 2);
        Canvas.SetTop(ellipse, _currentMousePosition.Y - ellipse.Height / 2);

        mainCanvas.Children.Add(ellipse);
    }

    private void AddSegmentAndMakeVisible()
    {
        var brushColor = DrawManager.LineProperties.BrushColor;
        var segment = DrawManager.DrawLine(_newSegmentStartPoint!.Value, _currentMousePosition, brushColor);

        mainCanvas.Children.Add(segment);
        _segments.Add(segment);

        _newSegmentStartPoint = null;
    }

    private void AddRectangleAndMakeVisible()
    {
        var width = DrawManager.RectangleProperties.Width;
        var height = DrawManager.RectangleProperties.Height;
        var brushColor = DrawManager.RectangleProperties.BrushColor;
        var rectangle = DrawManager.DrawRectangle(width, height, brushColor);

        Canvas.SetLeft(rectangle, _currentMousePosition.X - rectangle.Width / 2);
        Canvas.SetTop(rectangle, _currentMousePosition.Y - rectangle.Height / 2);

        mainCanvas.Children.Add(rectangle);
    }

    private void AddPolygonAndMakeVisible()
    {
        var size = DrawManager.PolygonProperties.Size;
        var brushColor = DrawManager.PolygonProperties.BrushColor;
        var polygon = DrawManager.DrawPolygon(_currentMousePosition, size, brushColor);

        mainCanvas.Children.Add(polygon);
    }

    // !!! SEGMENT SPECIFIC METHODS !!! //
    private void HandleSegmentCreation()
    {
        if (_newSegmentStartPoint is null)
        {
            _newSegmentStartPoint = new Point(_currentMousePosition.X, _currentMousePosition.Y);
        }
        else if (_currentMousePosition != _newSegmentStartPoint.Value)
        {
            AddSegmentAndMakeVisible();
        }
    }

    private void HandleSegmentEdit()
    {
        if (_selectedSegmentToEdit is null)
        {
            foreach (var segment in _segments)
            {
                if (IsMouseDownOnSegmentPoint(segment.X1, segment.Y1))
                {
                    _selectedSegmentToEdit = segment;
                    _editSegmentMode = EditSegmentMode.StartPoint;
                }
                else if (IsMouseDownOnSegmentPoint(segment.X2, segment.Y2))
                {
                    _selectedSegmentToEdit = segment;
                    _editSegmentMode = EditSegmentMode.EndPoint;
                }
            }
        }
        else
        {
            if (_editSegmentMode == EditSegmentMode.StartPoint)
            {

                _selectedSegmentToEdit.X1 = _currentMousePosition.X;
                _selectedSegmentToEdit.Y1 = _currentMousePosition.Y;
            }
            else
            {
                _selectedSegmentToEdit.X2 = _currentMousePosition.X;
                _selectedSegmentToEdit.Y2 = _currentMousePosition.Y;
            }
            RefreshSegmentPointsEffect();

            _selectedSegmentToEdit = null;
            _editSegmentMode = null;
        }       
    }


    private bool IsMouseDownOnSegmentPoint(double segmentEndpointPositionX, double segmentEndpointPositionY)
    {
        var errorMargin = 4d;
        return Math.Abs(segmentEndpointPositionX - _currentMousePosition.X) <= errorMargin
               && Math.Abs(segmentEndpointPositionY - _currentMousePosition.Y) <= errorMargin;
    }

    private void AddSegmentPointsEffect()
    {
        const double width = 8d;
        const double height = 8d;
        var brushColor = new SolidColorBrush(Color.FromRgb(255, 0, 0));

        foreach (var segment in _segments)
        {
            var startEllipse = DrawManager.DrawEllipse(width, height, brushColor);
            var endEllipse = DrawManager.DrawEllipse(width, height, brushColor);
            var startPoint = new Point(segment.X1, segment.Y1);
            var endPoint = new Point(segment.X2, segment.Y2);

            Canvas.SetLeft(startEllipse, startPoint.X - startEllipse.Width / 2);
            Canvas.SetTop(startEllipse, startPoint.Y - startEllipse.Height / 2);
            Canvas.SetLeft(endEllipse, endPoint.X - endEllipse.Height / 2);
            Canvas.SetTop(endEllipse, endPoint.Y - endEllipse.Height / 2);

            mainCanvas.Children.Add(startEllipse);
            mainCanvas.Children.Add(endEllipse);

            _editSegmentPoints.Add(startEllipse);
            _editSegmentPoints.Add(endEllipse);
        }
    }

    private void RemoveSegmentPointsEffect()
    {
        foreach (var segmentPoint in _editSegmentPoints)
        {
            mainCanvas.Children.Remove(segmentPoint);
        }
        _editSegmentPoints.Clear();
    }

    private void RefreshSegmentPointsEffect()
    {
        RemoveSegmentPointsEffect();
        AddSegmentPointsEffect();
    }
}