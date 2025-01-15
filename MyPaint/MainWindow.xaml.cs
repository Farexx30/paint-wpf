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
    Rectangle,
    Ellipse,
    Arrow,
    Tree,
    Polygon,
    BrokenLine,
    Segment,
    EditSegment,
}

public partial class MainWindow : Window
{
    private DrawStyle _drawStyle = DrawStyle.Freestyle;
    private Point? _currentMousePosition;
    private Color _currentColor = Color.FromRgb(0, 0, 0); //Black color by default

    // !! SEGMENT AND BROKE LINE FIELDS !!! //
    private Point? _newSegmentStartPoint;
    // !!! SEGMENT FIELDS !!! //
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
            var line = DrawManager.DrawLine(_currentMousePosition!.Value, e.GetPosition(this), brushColor);

            _currentMousePosition = e.GetPosition(this);

            mainCanvas.Children.Add(line);
        }
    }

    private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _currentMousePosition = e.GetPosition(this);

        switch (_drawStyle)
        {
            case DrawStyle.Point:
                AddPointAndMakeVisible();
                break;
            case DrawStyle.BrokenLine:
                HandleSegmentCreation(); //Becase basically broken line consists of multiple segments.
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
            case DrawStyle.Ellipse:
                AddEllipseAndMakeVisible();
                break;
            case DrawStyle.Arrow:
                AddArrowAndMakeVisible();
                break;
            case DrawStyle.Tree:
                AddTreeAndMakeVisible();
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

    private void DrawBrokenLine_Click(object sender, RoutedEventArgs e)
    {
        _drawStyle = DrawStyle.BrokenLine;

        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _selectedSegmentToEdit = null;
        _editSegmentMode = null;
    }

    private void DrawSegmentMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _drawStyle = DrawStyle.Segment;

        _newSegmentStartPoint = null;
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

    private void DrawEllipseButton_Click(object sender, RoutedEventArgs e)
    {
        _drawStyle = DrawStyle.Ellipse;

        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _selectedSegmentToEdit = null;
        _editSegmentMode = null;
    }

    private void DrawArrowButton_Click(object sender, RoutedEventArgs e)
    {
        _drawStyle = DrawStyle.Arrow;

        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _selectedSegmentToEdit = null;
        _editSegmentMode = null;
    }

    private void DrawTreeButton_Click(object sender, RoutedEventArgs e)
    {
        _drawStyle = DrawStyle.Tree;

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
        var ellipse = DrawManager.DrawEllipse(width, height, brushColor, true);

        Canvas.SetLeft(ellipse, _currentMousePosition!.Value.X - ellipse.Width / 2);
        Canvas.SetTop(ellipse, _currentMousePosition!.Value.Y - ellipse.Height / 2);

        mainCanvas.Children.Add(ellipse);
    }

    private void AddSegmentAndMakeVisible(out Line segment)
    {
        var brushColor = DrawManager.LineProperties.BrushColor;
        segment = DrawManager.DrawLine(_newSegmentStartPoint!.Value, _currentMousePosition!.Value, brushColor);

        mainCanvas.Children.Add(segment);
    }

    private void AddRectangleAndMakeVisible()
    {
        var width = DrawManager.RectangleProperties.Width;
        var height = DrawManager.RectangleProperties.Height;
        var brushColor = DrawManager.RectangleProperties.BrushColor;
        var rectangle = DrawManager.DrawRectangle(width, height, brushColor);

        Canvas.SetLeft(rectangle, _currentMousePosition!.Value.X - rectangle.Width / 2);
        Canvas.SetTop(rectangle, _currentMousePosition!.Value.Y - rectangle.Height / 2);

        mainCanvas.Children.Add(rectangle);
    }

    private void AddEllipseAndMakeVisible()
    {
        var width = DrawManager.EllipseProperties.Width;
        var height = DrawManager.EllipseProperties.Height;
        var brushColor = DrawManager.EllipseProperties.BrushColor;
        var ellipse = DrawManager.DrawEllipse(width, height, brushColor);

        Canvas.SetLeft(ellipse, _currentMousePosition!.Value.X - ellipse.Width / 2);
        Canvas.SetTop(ellipse, _currentMousePosition!.Value.Y - ellipse.Height / 2);

        mainCanvas.Children.Add(ellipse);
    }

    private void AddArrowAndMakeVisible()
    {
        var size = DrawManager.ArrowProperties.Size;
        var brushColor = DrawManager.ArrowProperties.BrushColor;
        var arrow = DrawManager.DrawArrow(_currentMousePosition!.Value, size, brushColor);

        mainCanvas.Children.Add(arrow);
    }

    private void AddTreeAndMakeVisible()
    {
        var size = DrawManager.TreeProperties.Size;
        var brushColor = DrawManager.TreeProperties.BrushColor;
        var tree = DrawManager.DrawTree(_currentMousePosition!.Value, size, brushColor);

        mainCanvas.Children.Add(tree);
    }

    private void AddPolygonAndMakeVisible()
    {
        var size = DrawManager.PolygonProperties.Size;
        var brushColor = DrawManager.PolygonProperties.BrushColor;
        var polygon = DrawManager.DrawRegularPolygon(_currentMousePosition!.Value, size, 8u, brushColor);

        mainCanvas.Children.Add(polygon);
    }

    // !!! SEGMENT SPECIFIC METHODS !!! //
    private void HandleSegmentCreation()
    {
        if (_newSegmentStartPoint is null)
        {
            _newSegmentStartPoint = new Point(_currentMousePosition!.Value.X, _currentMousePosition!.Value.Y);
        }
        else if (_currentMousePosition != _newSegmentStartPoint.Value)
        {
            AddSegmentAndMakeVisible(out Line segment);

            if (_drawStyle == DrawStyle.Segment)
            {
                _segments.Add(segment);
                _newSegmentStartPoint = null;
            }
            else if (_drawStyle == DrawStyle.BrokenLine)
            {
                _newSegmentStartPoint = _currentMousePosition;
            }
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
                _selectedSegmentToEdit.X1 = _currentMousePosition!.Value.X;
                _selectedSegmentToEdit.Y1 = _currentMousePosition!.Value.Y;
            }
            else
            {
                _selectedSegmentToEdit.X2 = _currentMousePosition!.Value.X;
                _selectedSegmentToEdit.Y2 = _currentMousePosition!.Value.Y;
            }
            RefreshSegmentPointsEffect();

            _selectedSegmentToEdit = null;
            _editSegmentMode = null;
        }
    }


    private bool IsMouseDownOnSegmentPoint(double segmentEndpointPositionX, double segmentEndpointPositionY)
    {
        var errorMargin = 4d;
        return Math.Abs(segmentEndpointPositionX - _currentMousePosition!.Value.X) <= errorMargin
               && Math.Abs(segmentEndpointPositionY - _currentMousePosition!.Value.Y) <= errorMargin;
    }

    private void AddSegmentPointsEffect()
    {
        const double width = 8d;
        const double height = 8d;
        var brushColor = new SolidColorBrush(Color.FromRgb(255, 0, 0));

        foreach (var segment in _segments)
        {
            var startEllipse = DrawManager.DrawEllipse(width, height, brushColor, true);
            var endEllipse = DrawManager.DrawEllipse(width, height, brushColor, true);
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