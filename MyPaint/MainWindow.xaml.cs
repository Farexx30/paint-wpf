using Emgu.CV.Structure;
using Microsoft.Win32;
using System.ComponentModel;
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

internal enum EditSegmentMode
{
    StartPoint,
    EndPoint,
}

public partial class MainWindow : Window
{
    private DrawStyle _drawStyle = DrawStyle.Freestyle;
    private Point? _currentMousePosition;

    // !! COLOR PICKER WINDOW FIELDS !! //
    private ColorPickerWindow? _colorPickerWindow;
    // !! SEGMENT AND BROKE LINE FIELDS !!! //
    private Line? _currentSegment;
    private Point? _newSegmentStartPoint;
    // !!! SEGMENT FIELDS !!! //
    private Point? _oldSegmentPoint;
    private EditSegmentMode? _editSegmentMode;
    private readonly List<Line> _segments = [];
    private readonly List<System.Windows.Shapes.Ellipse> _segmentPointEffects = [];

    // !!! HELP FIELDS !!! /!
    private bool _isMenuFocused = false;

    public MainWindow()
    {
        InitializeComponent();
    }




    // ==================================================
    // PRIVATE EVENT METHODS
    // ==================================================

    // !!! MAIN WINDOW EVENTS !!! //
    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        _colorPickerWindow?.Close();
        _colorPickerWindow = null;
    }


    // !!! MOUSE EVENTS !!! //
    private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed 
            && _drawStyle == DrawStyle.Freestyle
            && _currentMousePosition is not null)
        {
            if (_isMenuFocused)
            {
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(menu), null); //Removes the focus from "menu".
                return;
            }

            var brushColor = DrawManager.GlobalProperties.BrushColor;
            var line = DrawManager.DrawLine(_currentMousePosition!.Value, e.GetPosition(mainCanvas), brushColor);

            _currentMousePosition = e.GetPosition(mainCanvas);

            mainCanvas.Children.Add(line);
        }
        else if ((_drawStyle == DrawStyle.Segment || _drawStyle == DrawStyle.BrokenLine) 
                && _newSegmentStartPoint is not null)
        {
            _currentMousePosition = e.GetPosition(mainCanvas);

            if (_currentSegment is null)
            {
                AddSegmentAndMakeVisible(out Line segment);
                _currentSegment = segment;
            }
            else
            {
                _currentSegment.X2 = _currentMousePosition!.Value.X;
                _currentSegment.Y2 = _currentMousePosition!.Value.Y;
            }
        }
        else if (_drawStyle == DrawStyle.EditSegment && _currentSegment is not null)
        {
            _currentMousePosition = e.GetPosition(mainCanvas);

            if (_editSegmentMode == EditSegmentMode.StartPoint)
            {
                _currentSegment.X1 = _currentMousePosition!.Value.X;
                _currentSegment.Y1 = _currentMousePosition!.Value.Y;
            }
            else
            {
                _currentSegment.X2 = _currentMousePosition!.Value.X;
                _currentSegment.Y2 = _currentMousePosition!.Value.Y;
            }
        }      
    }

    private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _currentMousePosition = e.GetPosition(mainCanvas);

        switch (_drawStyle)
        {
            case DrawStyle.Point:
                AddPointAndMakeVisible();
                break;
            case DrawStyle.BrokenLine:
                HandleSegmentCreation(); //Becase broken line basically consists of multiple segments.
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

    private void MainCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_currentSegment is not null)
        {
            if (_drawStyle == DrawStyle.BrokenLine || _drawStyle == DrawStyle.Segment)
            {
                mainCanvas.Children.Remove(_currentSegment);
                _currentSegment = null;
                _newSegmentStartPoint = null;
            }
            else if (_drawStyle == DrawStyle.EditSegment)
            {
                if (_editSegmentMode == EditSegmentMode.StartPoint)
                {
                    _currentSegment.X1 = _oldSegmentPoint!.Value.X;
                    _currentSegment.Y1 = _oldSegmentPoint!.Value.Y;                   
                }
                else
                {
                    _currentSegment.X2 = _oldSegmentPoint!.Value.X;
                    _currentSegment.Y2 = _oldSegmentPoint!.Value.Y;
                }
                AddSegmentPointEffect(_oldSegmentPoint!.Value);

                _currentSegment = null;
                _oldSegmentPoint = null;
                _editSegmentMode = null;
            }
        }              
    }

    // !!! TOOLBAR EVENTS !!! //
    private void ToolBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _currentMousePosition = null;
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

    // !!! SAVE AND LOAD FILE EVENTS !!! //
    private void SaveToFileButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        var saveFileDialog = new SaveFileDialog
        {
            Title = "Save an image file",
            Filter = "Image file (*.png)|*.png|Image file (*.jpeg)|*.jpeg"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            var path = new Uri(saveFileDialog.FileName);
            var fileExtension = System.IO.Path.GetExtension(saveFileDialog.FileName);

            if (path is not null)
            {
                var pngExtension = $".{nameof(FileExtension.Png)}";
                var jpegExtension = $".{nameof(FileExtension.Jpeg)}";

                switch (fileExtension)
                {
                    case var extension when extension.Equals(pngExtension, StringComparison.OrdinalIgnoreCase):
                        FileManager.SaveToFile(path, mainCanvas, FileExtension.Png);
                        break;
                    case var extension when extension.Equals(jpegExtension, StringComparison.OrdinalIgnoreCase):
                        FileManager.SaveToFile(path, mainCanvas, FileExtension.Jpeg);
                        break;
                    default:
                        MessageBox.Show("Nieobsługiwany format pliku",
                            "Wystąpił błąd przy zapisie!",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        break;
                }
            }
        }
    }

    private void LoadFromFileButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        var openFileDialog = new OpenFileDialog
        {
            Title = "Load an image file",
            Filter = "Image file (*.png)|*.png|Image file (*.jpeg)|*.jpeg"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            var path = new Uri(openFileDialog.FileName);
            var fileExtension = System.IO.Path.GetExtension(openFileDialog.FileName);

            if (path is not null)
            {
                var pngExtension = $".{nameof(FileExtension.Png)}";
                var jpegExtension = $".{nameof(FileExtension.Jpeg)}";

                Image? image = fileExtension switch
                {
                    var extension when extension.Equals(pngExtension, StringComparison.OrdinalIgnoreCase) 
                        => FileManager.LoadFromFile(path, FileExtension.Png),

                    var extension when extension.Equals(jpegExtension, StringComparison.OrdinalIgnoreCase)
                        => FileManager.LoadFromFile(path, FileExtension.Jpeg),

                    _ => null
                };

                HandleResultImage(image);
            }
        }
    }

   

    // !!! CHANGE DRAW STYLE EVENTS !!! //
    private void DrawFreestyleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Freestyle;

        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _currentSegment = null;
        _editSegmentMode = null;
    }

    private void DrawPointsButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Point;

        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _currentSegment = null;
        _editSegmentMode = null;
    }

    private void DrawBrokenLine_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.BrokenLine;

        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _currentSegment = null;
        _editSegmentMode = null;
    }

    private void DrawSegmentMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Segment;

        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _currentSegment = null;
        _editSegmentMode = null;
    }

    private void EditSegmentMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _currentSegment = null;
        _editSegmentMode = null;

        _drawStyle = DrawStyle.EditSegment;

        AddSegmentPointsEffect();
    }

    private void DrawRectangleMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Rectangle;

        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _currentSegment = null;
        _editSegmentMode = null;
    }

    private void DrawEllipseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Ellipse;

        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _currentSegment = null;
        _editSegmentMode = null;
    }

    private void DrawArrowButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Arrow;

        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _currentSegment = null;
        _editSegmentMode = null;
    }

    private void DrawTreeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Tree;

        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _currentSegment = null;
        _editSegmentMode = null;
    }

    private void DrawPolygonMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Polygon;

        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _currentSegment = null;
        _editSegmentMode = null;
    }

    // !!! NEW WINDOW EVENTS !!! //
    private void ColorPickerRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _colorPickerWindow = new ColorPickerWindow(this);

        _colorPickerWindow.Show();
    }




    // =========================================================
    // PRIVATE HELP METHODS
    // =========================================================

    // !!! RAISE EVENT METHODS !!! //
    private void RaiseMouseRightButtonDownEvent()
    {
        var mouseEventArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
        {
            RoutedEvent = MouseRightButtonDownEvent
        };
        mainCanvas.RaiseEvent(mouseEventArgs);
    }

    // !!! FILE METHODS !!! //
    private void HandleResultImage(Image? image)
    {
        if (image is not null)
        {
            var result = MessageBox.Show("Ta akcja wyczyści Twoje płótno! Czy na pewno chcesz kontynuować?",
                "Potwierdź",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                mainCanvas.Children.Clear();
                mainCanvas.Children.Add(image);
            }
        }
        else
        {
            MessageBox.Show("Nieobsługiwany format pliku",
                "Wystąpił błąd przy odczycie!",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // !!! ADD AND MAKE VISIBLE METHODS !!! //
    private void AddPointAndMakeVisible()
    {
        var width = DrawManager.EllipseProperties.Width;
        var height = DrawManager.EllipseProperties.Height;
        var brushColor = DrawManager.GlobalProperties.BrushColor;
        var ellipse = DrawManager.DrawEllipse(width, height, brushColor, true);

        Canvas.SetLeft(ellipse, _currentMousePosition!.Value.X - ellipse.Width / 2);
        Canvas.SetTop(ellipse, _currentMousePosition!.Value.Y - ellipse.Height / 2);

        mainCanvas.Children.Add(ellipse);
    }

    private void AddSegmentAndMakeVisible(out Line segment)
    {
        var brushColor = DrawManager.GlobalProperties.BrushColor;
        segment = DrawManager.DrawLine(_newSegmentStartPoint!.Value, _currentMousePosition!.Value, brushColor);

        mainCanvas.Children.Add(segment);
    }

    private void AddRectangleAndMakeVisible()
    {
        var width = DrawManager.RectangleProperties.Width;
        var height = DrawManager.RectangleProperties.Height;
        var brushColor = DrawManager.GlobalProperties.BrushColor;
        var rectangle = DrawManager.DrawRectangle(width, height, brushColor);

        Canvas.SetLeft(rectangle, _currentMousePosition!.Value.X - rectangle.Width / 2);
        Canvas.SetTop(rectangle, _currentMousePosition!.Value.Y - rectangle.Height / 2);

        mainCanvas.Children.Add(rectangle);
    }

    private void AddEllipseAndMakeVisible()
    {
        var width = DrawManager.EllipseProperties.Width;
        var height = DrawManager.EllipseProperties.Height;
        var brushColor = DrawManager.GlobalProperties.BrushColor;
        var ellipse = DrawManager.DrawEllipse(width, height, brushColor);

        Canvas.SetLeft(ellipse, _currentMousePosition!.Value.X - ellipse.Width / 2);
        Canvas.SetTop(ellipse, _currentMousePosition!.Value.Y - ellipse.Height / 2);

        mainCanvas.Children.Add(ellipse);
    }

    private void AddArrowAndMakeVisible()
    {
        var size = DrawManager.ArrowProperties.Size;
        var brushColor = DrawManager.GlobalProperties.BrushColor;
        var arrow = DrawManager.DrawArrow(_currentMousePosition!.Value, size, brushColor);

        mainCanvas.Children.Add(arrow);
    }

    private void AddTreeAndMakeVisible()
    {
        var size = DrawManager.TreeProperties.Size;
        var brushColor = DrawManager.GlobalProperties.BrushColor;
        var tree = DrawManager.DrawTree(_currentMousePosition!.Value, size, brushColor);

        mainCanvas.Children.Add(tree);
    }

    private void AddPolygonAndMakeVisible()
    {
        var size = DrawManager.PolygonProperties.Size;
        var brushColor = DrawManager.GlobalProperties.BrushColor;
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
        else
        {
            if (_drawStyle == DrawStyle.Segment)
            {
                _segments.Add(_currentSegment!);

                _newSegmentStartPoint = null;
            }
            else if (_drawStyle == DrawStyle.BrokenLine)
            {
                _newSegmentStartPoint = _currentMousePosition;
            }

            _currentSegment = null;
        }
    }

    private void HandleSegmentEdit()
    {
        if (_currentSegment is null)
        {
            foreach (var segment in _segments)
            {
                if (IsMouseDownOnSegmentPoint(segment.X1, segment.Y1))
                {
                    _currentSegment = segment;
                    _oldSegmentPoint = new Point(segment.X1, segment.Y1);
                    _editSegmentMode = EditSegmentMode.StartPoint;

                    RemoveRelatedSegmentPointEffect();
                    break;
                }
                else if (IsMouseDownOnSegmentPoint(segment.X2, segment.Y2))
                {
                    _currentSegment = segment;
                    _oldSegmentPoint = new Point(segment.X2, segment.Y2);
                    _editSegmentMode = EditSegmentMode.EndPoint;

                    RemoveRelatedSegmentPointEffect();
                    break;
                }
            }
        }
        else
        {
            if (_editSegmentMode == EditSegmentMode.StartPoint)
            {
                var newPointEffect = new Point(_currentSegment.X1, _currentSegment.Y1);
                AddSegmentPointEffect(newPointEffect);
            }
            else
            {
                var newPointEffect = new Point(_currentSegment.X2, _currentSegment.Y2);
                AddSegmentPointEffect(newPointEffect);
            }

            _currentSegment = null;
            _oldSegmentPoint = null;
            _editSegmentMode = null;
        }
    }


    private bool IsMouseDownOnSegmentPoint(double segmentEndpointPositionX, double segmentEndpointPositionY)
    {
        var errorMargin = 4d;
        return Math.Abs(segmentEndpointPositionX - _currentMousePosition!.Value.X) <= errorMargin
               && Math.Abs(segmentEndpointPositionY - _currentMousePosition!.Value.Y) <= errorMargin;
    }

    private void RemoveRelatedSegmentPointEffect()
    {
        foreach (var segmentPoint in _segmentPointEffects)
        {
            var leftCoordinate = Canvas.GetLeft(segmentPoint);
            var topCoordinate = Canvas.GetTop(segmentPoint);
            var width = segmentPoint.Width;
            var height = segmentPoint.Height;

            if (IsMouseDownOnSegmentEditPoint(leftCoordinate, topCoordinate, width, height))
            {
                RemoveSegmentPointEffect(segmentPoint); //Removes the effect from canvas.
                _segmentPointEffects.Remove(segmentPoint);
                break;
            }
        }
    }

    private bool IsMouseDownOnSegmentEditPoint(double leftCoordinate, double topCoordinate, double width, double height)
    {
        return _currentMousePosition!.Value.X >= leftCoordinate && _currentMousePosition!.Value.X <= leftCoordinate + width
               && _currentMousePosition!.Value.Y >= topCoordinate && _currentMousePosition!.Value.Y <= topCoordinate + height;
    }

    private void AddSegmentPointEffect(Point coordinates)
    {
        const double width = 8d;
        const double height = 8d;
        var brushColor = new SolidColorBrush(Color.FromRgb(255, 0, 0));

        var ellipseEffect = DrawManager.DrawEllipse(width, height, brushColor, true);

        Canvas.SetLeft(ellipseEffect, coordinates.X - ellipseEffect.Width / 2);
        Canvas.SetTop(ellipseEffect, coordinates.Y - ellipseEffect.Height / 2);

        mainCanvas.Children.Add(ellipseEffect);
        _segmentPointEffects.Add(ellipseEffect);
    }

    private void RemoveSegmentPointEffect(System.Windows.Shapes.Ellipse segmentPoint)
    {
        mainCanvas.Children.Remove(segmentPoint);
    }

    private void AddSegmentPointsEffect()
    {
        foreach (var segment in _segments)
        {
            var startPoint = new Point(segment.X1, segment.Y1);
            AddSegmentPointEffect(startPoint);

            var endPoint = new Point(segment.X2, segment.Y2);
            AddSegmentPointEffect(endPoint);
        }
    }

    private void RemoveSegmentPointsEffect()
    {
        foreach (var segmentPoint in _segmentPointEffects)
        {
            RemoveSegmentPointEffect(segmentPoint);
        }
        _segmentPointEffects.Clear();
    }



    // =========================================================
    // PUBLIC HELP METHODS
    // =========================================================
    public void UpdateColorPicker()
    {
        colorPickerRectangle.Fill = DrawManager.GlobalProperties.BrushColor;
    }
}