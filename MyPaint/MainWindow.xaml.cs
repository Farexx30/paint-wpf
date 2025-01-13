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
        Line,
        Rectangle,
        Polygon
    }

    public partial class MainWindow : Window
    {
        private DrawStyle _drawStyle = DrawStyle.Freestyle;

        private Point _currentPoint = new();
        private Color _currentColor = Color.FromRgb(0, 0, 0); //Black color by default

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ButtonState == MouseButtonState.Pressed)
            //{
            //   e.GetPosition(this);
            //}
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _drawStyle == DrawStyle.Freestyle)
            {
                var brushColor = new SolidColorBrush(_currentColor);
                var line = new Line
                {
                    X1 = _currentPoint.X,
                    Y1 = _currentPoint.Y,
                    X2 = e.GetPosition(this).X,
                    Y2 = e.GetPosition(this).Y,
                    Stroke = brushColor
                };

                _currentPoint = e.GetPosition(this);

                MainCanvas.Children.Add(line);
            }
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _currentPoint = e.GetPosition(this);

            if (_drawStyle == DrawStyle.Point)
            {
                const double width = 6;
                const double height = 6;
                var brushColor = new SolidColorBrush(_currentColor);

                var ellipse = new Ellipse
                {
                    Width = width,
                    Height = height,
                    Fill = brushColor
                };

                Canvas.SetLeft(ellipse, e.GetPosition(this).X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, e.GetPosition(this).Y - ellipse.Height / 2);

                MainCanvas.Children.Add(ellipse);
            }
            else if (_drawStyle == DrawStyle.Rectangle)
            {
                const double width = 60;
                const double height = 40;
                var brushColor = new SolidColorBrush(_currentColor);
                var rectangle = new Rectangle
                {
                    Width = width,
                    Height = height,
                    Stroke = brushColor
                };

                Canvas.SetLeft(rectangle, e.GetPosition(this).X - rectangle.Width / 2);
                Canvas.SetTop(rectangle, e.GetPosition(this).Y - rectangle.Height / 2);

                MainCanvas.Children.Add(rectangle);
            }
            else if (_drawStyle == DrawStyle.Polygon)
            {
                var mouseX = e.GetPosition(this).X;
                var mouseY = e.GetPosition(this).Y;
                double polygonSize = 20.0;

                var point1 = new Point(mouseX - polygonSize, mouseY + polygonSize * 2);
                var point2 = new Point(mouseX + polygonSize, mouseY + polygonSize * 2);
                var point3 = new Point(mouseX + polygonSize * 2, mouseY + 0);
                var point4 = new Point(mouseX + polygonSize, mouseY - polygonSize * 2);
                var point5 = new Point(mouseX - polygonSize, mouseY - polygonSize * 2);
                var point6 = new Point(mouseX - polygonSize * 2, mouseY + 0);

                var polyglonPoints = new PointCollection
                {
                    point1,
                    point2,
                    point3,
                    point4,
                    point5,
                    point6
                };

                var brushColor = new SolidColorBrush(_currentColor);

                var polygon = new Polygon
                {
                    Points = polyglonPoints,
                    Stroke = brushColor
                };

                MainCanvas.Children.Add(polygon);
            }
        }

        private void buttonDraw_Click(object sender, RoutedEventArgs e)
        {
            _drawStyle = DrawStyle.Freestyle;
        }

        private void buttonPoints_Click(object sender, RoutedEventArgs e)
        {
            _drawStyle = DrawStyle.Point;
        }

        private void buttonLines_Click(object sender, RoutedEventArgs e)
        {
            _drawStyle = DrawStyle.Line;
        }

        private void drawSegment_Click(object sender, RoutedEventArgs e)
        {

        }

        private void editSegment_Click(object sender, RoutedEventArgs e)
        {

        }

        private void drawRect_Click(object sender, RoutedEventArgs e)
        {
            _drawStyle = DrawStyle.Rectangle;
        }

        private void editPolygon_Click(object sender, RoutedEventArgs e)
        {
            _drawStyle = DrawStyle.Polygon;
        }

        private void colorPicker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var colorPickerWindow = new ColorPickerWindow();

            colorPickerWindow.Show();
        }
    }
}