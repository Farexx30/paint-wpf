using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    enum DrawStyle
    {
        Freestyle,
        Point,
        Segment,
        Rectangle,
        Polygon
    }

    public partial class MainWindow : Window
    {
        private DrawStyle _drawStyle = DrawStyle.Freestyle;
        private Point _currentMousePosition = new();
        private Color _currentColor = Color.FromRgb(0, 0, 0); //Black color by default

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
        }

        private void DrawPointsButton_Click(object sender, RoutedEventArgs e)
        {
            _drawStyle = DrawStyle.Point;
        }

        private void DrawSegmentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _drawStyle = DrawStyle.Segment;
        }

        private void EditSegmentMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DrawRectangleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _drawStyle = DrawStyle.Rectangle;
        }

        private void DrawPolygonMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _drawStyle = DrawStyle.Polygon;
        }


        // !!! NEW WINDOW EVENTS !!! //
        private void ColorPickerRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var colorPickerWindow = new ColorPickerWindow();

            colorPickerWindow.Show();
        }




        // =========================================================
        // PRIVATE HELPFUL METHODS
        // =========================================================
        private void AddPointAndMakeVisible()
        {
            const double width = 6d;
            const double height = 6d;
            var brushColor = new SolidColorBrush(_currentColor);
            var ellipse = DrawManager.DrawEllipse(width, height, brushColor);

            Canvas.SetLeft(ellipse, _currentMousePosition.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, _currentMousePosition.Y - ellipse.Height / 2);

            mainCanvas.Children.Add(ellipse);
        }

        private void AddRectangleAndMakeVisible()
        {
            const double width = 60d;
            const double height = 40d;
            var brushColor = new SolidColorBrush(_currentColor);
            var rectangle = DrawManager.DrawRectangle(width, height, brushColor);

            Canvas.SetLeft(rectangle, _currentMousePosition.X - rectangle.Width / 2);
            Canvas.SetTop(rectangle, _currentMousePosition.Y - rectangle.Height / 2);

            mainCanvas.Children.Add(rectangle);
        }

        private void AddPolygonAndMakeVisible()
        {
            const double polygonSize = 20d;
            var brushColor = new SolidColorBrush(_currentColor);
            var polygon = DrawManager.DrawPolygon(_currentMousePosition, polygonSize, brushColor);

            mainCanvas.Children.Add(polygon);
        }
    }
}