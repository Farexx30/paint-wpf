using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
using Ellipse = System.Windows.Shapes.Ellipse;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;

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

public partial class MainWindow : Window, INotifyPropertyChanged
{
    // !!! OnPropertyChanged for data binding !!! //
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string _numberOfPoints = "6";
    public string NumberOfPoints
    {
        get => _numberOfPoints;
        set
        {
            if (_numberOfPoints != value)
            {
                _numberOfPoints = value;
                OnPropertyChanged();

                if (!string.IsNullOrEmpty(_numberOfPoints))
                {
                    DrawManager.PolygonProperties.NumberOfPoints = Convert.ToUInt32(_numberOfPoints);
                }
            }
        }
    }


    private DrawStyle _drawStyle = DrawStyle.Freestyle;
    private Point? _currentMousePosition;

    // !! WINDOW FIELDS !! //
    private ColorPickerWindow? _colorPickerWindow;
    private MatrixFilterWindow? _matrixFilterWindow;
    // !! SEGMENT AND BROKE LINE FIELDS !!! //
    private Line? _currentSegment;
    private Point? _newSegmentStartPoint;
    // !! SEGMENT FIELDS !! //
    private Point? _oldSegmentPoint;
    private EditSegmentMode? _editSegmentMode;
    private readonly List<Line> _segments = [];
    private readonly List<Ellipse> _segmentPointEffects = [];
    // !! FOR TEMP FILE !! //
    private const string _tempFileName = "temp.bmp";

    // !!! HELP FIELDS !!! /!
    private bool _isMenuFocused = false;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }




    // ==================================================
    // PRIVATE EVENT METHODS
    // ==================================================

    // !!! MAIN WINDOW EVENTS !!! //
    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        _colorPickerWindow?.Close();
        _matrixFilterWindow?.Close();
        _colorPickerWindow = null;
        _matrixFilterWindow = null;
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
        else if (e.LeftButton == MouseButtonState.Pressed && _drawStyle == DrawStyle.Erase)
        {
            var element = e.Source as FrameworkElement;

            if (element is not null && element is not Canvas)
            {
                if (mainCanvas.Children.Contains(element))
                {
                    if (_segments.Contains(element))
                    {
                        _segments.Remove((Line)element);
                    }
                }

                mainCanvas.Children.Remove(element);
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
    private void SaveToFileMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        var saveFileDialog = new SaveFileDialog
        {
            Title = "Save an image file",
            Filter = "Image file (*.png)|*.png|Image file (*.jpeg)|*.jpeg|Image file (*.bmp)|*.bmp"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            var pathUri = new Uri(saveFileDialog.FileName);
            var fileExtension = Path.GetExtension(saveFileDialog.FileName);

            if (pathUri is not null)
            {
                var pngExtension = $".{nameof(FileExtension.Png)}";
                var jpegExtension = $".{nameof(FileExtension.Jpeg)}";
                var bmpExtension = $".{nameof(FileExtension.Bmp)}";

                switch (fileExtension)
                {
                    case var extension when extension.Equals(pngExtension, StringComparison.OrdinalIgnoreCase):
                        FileManager.SaveToFile(pathUri, mainCanvas, FileExtension.Png);
                        break;
                    case var extension when extension.Equals(jpegExtension, StringComparison.OrdinalIgnoreCase):
                        FileManager.SaveToFile(pathUri, mainCanvas, FileExtension.Jpeg);
                        break;
                    case var extension when extension.Equals(bmpExtension, StringComparison.OrdinalIgnoreCase):
                        FileManager.SaveToFile(pathUri, mainCanvas, FileExtension.Bmp);
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

    private void LoadFromFileMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        var openFileDialog = new OpenFileDialog
        {
            Title = "Load an image file",
            Filter = "Image file (*.png)|*.png|Image file (*.jpeg)|*.jpeg|Image file (*.bmp)|*.bmp"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            var pathUri = new Uri(openFileDialog.FileName);
            var fileExtension = Path.GetExtension(openFileDialog.FileName);

            if (pathUri is not null)
            {
                var pngExtension = $".{nameof(FileExtension.Png)}";
                var jpegExtension = $".{nameof(FileExtension.Jpeg)}";
                var bmpExtension = $".{nameof(FileExtension.Bmp)}";

                Image? image = null;
                if (fileExtension.Equals(pngExtension, StringComparison.OrdinalIgnoreCase)
                    || fileExtension.Equals(jpegExtension, StringComparison.OrdinalIgnoreCase)
                    || fileExtension.Equals(bmpExtension, StringComparison.OrdinalIgnoreCase))
                {
                    image = FileManager.LoadFromFile(pathUri);
                }

                HandleResultImage(image); //adds to canvas
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

        HideNumberOfPointsInPolygonUIElements();
        ResetSegmentProperties();
    }

    private void DrawPointsButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Point;

        HideNumberOfPointsInPolygonUIElements();
        ResetSegmentProperties();
    }

    private void DrawBrokenLine_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.BrokenLine;

        HideNumberOfPointsInPolygonUIElements();
        ResetSegmentProperties();
    }

    private void DrawSegmentMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Segment;

        HideNumberOfPointsInPolygonUIElements();
        ResetSegmentProperties();
    }

    private void EditSegmentMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        ResetSegmentProperties();

        _drawStyle = DrawStyle.EditSegment;

        HideNumberOfPointsInPolygonUIElements();
        AddSegmentPointsEffect();
    }

    private void DrawRectangleMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Rectangle;

        HideNumberOfPointsInPolygonUIElements();
        ResetSegmentProperties();
    }

    private void DrawEllipseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Ellipse;

        HideNumberOfPointsInPolygonUIElements();
        ResetSegmentProperties();
    }

    private void DrawArrowButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Arrow;

        HideNumberOfPointsInPolygonUIElements();
        ResetSegmentProperties();
    }

    private void DrawTreeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Tree;

        HideNumberOfPointsInPolygonUIElements();
        ResetSegmentProperties();
    }

    private void DrawPolygonMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }
        
        _drawStyle = DrawStyle.Polygon;

        MakeVisibleNumberOfPointsInPolygonUIElements();

        ResetSegmentProperties();
    }

    private void NumberOfPointsInPolygonTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var newChar = Convert.ToChar(e.Text);
        var textBox = sender as TextBox;
        var newText = textBox!.Text.Insert(textBox.CaretIndex, e.Text);

        if (!char.IsDigit(newChar) || Convert.ToUInt32(newText) < 1u)
        {
            e.Handled = true;
        }
    }

    private void MakeVisibleNumberOfPointsInPolygonUIElements()
    {
        NumberOfPointsInPolygonLabel.Visibility = Visibility.Visible;
        NumberOfPointsInPolygonTextBox.Visibility = Visibility.Visible;
    }

    private void HideNumberOfPointsInPolygonUIElements()
    {
        NumberOfPointsInPolygonLabel.Visibility = Visibility.Hidden;
        NumberOfPointsInPolygonTextBox.Visibility = Visibility.Hidden;
    }


    private void EraseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _drawStyle = DrawStyle.Erase;

        HideNumberOfPointsInPolygonUIElements();
        ResetSegmentProperties();
    }

    // !!! APPLY FILTER EVENTS !!! //
    private void ApplySobelFilterMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        var tempFileFullPath = Path.Combine(Directory.GetCurrentDirectory(), _tempFileName);
        var tempFileUri = new Uri(tempFileFullPath);
        FileManager.SaveToFile(tempFileUri, mainCanvas, FileExtension.Bmp);

        ApplySobelFilterOnTempFile();

        var processedImage = FileManager.LoadFromFile(tempFileUri);
        ClearCanvas();
        mainCanvas.Children.Add(processedImage);

        FileManager.DeleteFile(tempFileUri);
    }


    // !!! HELP METHOD !!! //
    private void ResetSegmentProperties()
    {
        _newSegmentStartPoint = null;
        RemoveSegmentPointsEffect();
        _currentSegment = null;
        _editSegmentMode = null;
    }


    // !!! NEW WINDOW EVENTS !!! //
    private void ColorPickerRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }

        _colorPickerWindow = new ColorPickerWindow(this);
        _colorPickerWindow.Show();

        colorPickerRectangle.IsEnabled = false;
    }

    private void ApplyMatrixFilterMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSegment is not null)
        {
            RaiseMouseRightButtonDownEvent();
        }
        _matrixFilterWindow = new MatrixFilterWindow(this);
        _matrixFilterWindow.Show();

        matrixFilterMenuItem.IsEnabled = false;
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
                ClearCanvas();
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
        var width = DrawManager.EllipseProperties.Width / 6;
        var height = DrawManager.EllipseProperties.Height / 6;
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
        var numberOfPoints = DrawManager.PolygonProperties.NumberOfPoints;
        var polygon = DrawManager.DrawRegularPolygon(_currentMousePosition!.Value, size, numberOfPoints, brushColor);

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

    private void RemoveSegmentPointEffect(Ellipse segmentPoint)
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

    // !!! FILTER METHODS !!! //
    private static void ApplySobelFilterOnTempFile()
    {
        var image = new Image<Rgb, byte>(_tempFileName);
        var grayImage = image.Convert<Gray, byte>();
        var graySobelImage = grayImage.Sobel(0, 1, 3);
        graySobelImage.Save(_tempFileName);
    }

    private static void ApplyMatrixFilter(float[,] matrix, bool shouldNormalize, bool shouldApplyGrayscale)
    {
        var image = new Image<Rgb, byte>(_tempFileName);
        matrix = shouldNormalize ? matrix.Normalize() : matrix;
        var kernel = new ConvolutionKernelF(matrix);

        if (shouldApplyGrayscale)
        {
            var grayImage = image.Convert<Gray, byte>();
            var dst = new Mat(image.Size, DepthType.Cv8U, 1);
            var anchor = new System.Drawing.Point(-1, -1);
            CvInvoke.Filter2D(grayImage,
                dst,
                kernel,
                anchor);

            dst.Save(_tempFileName);
        }
        else
        {
            var dst = new Mat(image.Size, DepthType.Cv8U, 3);
            var anchor = new System.Drawing.Point(-1, -1);
            CvInvoke.Filter2D(image,
                dst,
                kernel,
                anchor);

            dst.Save(_tempFileName);
        }
    }

    // !!! CANVAS METHODS !!! //
    private void ClearCanvas()
    {
        mainCanvas.Children.Clear();

        _currentSegment = null;
        _newSegmentStartPoint = null;
        _oldSegmentPoint = null;
        _editSegmentMode = null;
        _segments.Clear();
        _segmentPointEffects.Clear();
    }


    // =========================================================
    // PUBLIC METHODS
    // =========================================================
    public void UpdateColorPicker()
    {
        colorPickerRectangle.Fill = DrawManager.GlobalProperties.BrushColor;
    }

    public void ApplyMatrixFilterOnCanvas(float[,] matrix, bool shouldNormalize, bool shouldApplyGrayscale)
    {
        var tempFileFullPath = Path.Combine(Directory.GetCurrentDirectory(), _tempFileName);
        var tempFileUri = new Uri(tempFileFullPath);
        FileManager.SaveToFile(tempFileUri, mainCanvas, FileExtension.Bmp);

        ApplyMatrixFilter(matrix, shouldNormalize, shouldApplyGrayscale);

        var processedImage = FileManager.LoadFromFile(tempFileUri);
        ClearCanvas();
        mainCanvas.Children.Add(processedImage);

        FileManager.DeleteFile(tempFileUri);
    }
}