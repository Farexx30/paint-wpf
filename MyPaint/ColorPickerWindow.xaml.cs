using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyPaint
{
    /// <summary>
    /// Interaction logic for ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window, INotifyPropertyChanged
    {
        // !!! OnPropertyChanged for data binding !!! //
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // !!! PRIVATE FIELDS !!! //
        private const string _hValueTextBoxName = "hValueTextBox";
        private const string _sValueTextBoxName = "sValueTextBox";
        private const string _vValueTextBoxName = "vValueTextBox";
        private const double _hMaxValue = 360d;
        private const double _sMaxValue = 100d;
        private const double _vMaxValue = 100d;

        // !!! BINDINGS !!! //
        private string _r = string.Empty;
        public string R
        {
            get => _r;
            set
            {
                if (_r != value)
                {
                    _r = value;
                    OnPropertyChanged();

                    CalculateAndUpdateHsvValues();
                }
            }
        }

        private string _g = string.Empty;
        public string G
        {
            get => _g;
            set
            {
                if (_g != value)
                {
                    _g = value;
                    OnPropertyChanged();

                    CalculateAndUpdateHsvValues();
                }
            }
        }

        private string _b = string.Empty;
        public string B
        {
            get => _b;
            set
            {
                if (_b != value)
                {
                    _b = value;
                    OnPropertyChanged();

                    CalculateAndUpdateHsvValues();
                }
            }
        }

        private string _h = string.Empty;
        public string H
        {
            get => _h;
            set
            {
                if (_h != value)
                {
                    _h = value;
                    OnPropertyChanged();

                    CalculateAndUpdateRgbValues();
                }
            }
        }

        private string _s = string.Empty;
        public string S
        {
            get => _s;
            set
            {
                if (_s != value)
                {
                    _s = value;
                    OnPropertyChanged();

                    CalculateAndUpdateRgbValues();
                }
            }
        }

        private string _v = string.Empty;
        public string V
        {
            get => _v;
            set
            {
                if (_v != value)
                {
                    _v = value;
                    OnPropertyChanged();

                    CalculateAndUpdateRgbValues();
                }
            }
        }


        // !!! CONSTRUCTORS !!! //
        public ColorPickerWindow()
        {
            InitializeComponent();
            DataContext = this;
        }


        // !!! VALIDATION EVENTS !!! //
        private void RgbValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var newText = textBox!.Text.Insert(textBox.CaretIndex, e.Text);

            if (!byte.TryParse(newText, out _))
            {
                e.Handled = true;
            }
        }

        private void HsvValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var textBoxName = textBox!.Name;
            var newText = textBox!.Text.Insert(textBox.CaretIndex, e.Text);

            if (textBoxName == _hValueTextBoxName)
            {
                if (!double.TryParse(newText, out var result) || result > _hMaxValue)
                {
                    e.Handled = true;
                }
            }
            else if (textBoxName == _sValueTextBoxName)
            {
                if (!double.TryParse(newText, out var result) || result > _sMaxValue)
                {
                    e.Handled = true;
                }
            }
            else if (textBoxName == _vValueTextBoxName)
            {
                if (!double.TryParse(newText, out var result) || result > _vMaxValue)
                {
                    e.Handled = true;
                }
            }
        }


        // !!! CONVERT METHODS !!! //
        private void CalculateAndUpdateHsvValues()
        {
            //We are going to use these variables in conversion:
            _ = byte.TryParse(R, out var rResult); //If parsing will fail, the rResult will be equal to 0 (default value).
            double rPrim = rResult / 255d;

            _ = byte.TryParse(G, out var gResult); //If parsing will fail, the gResult will be equal to 0 (default value).
            double gPrim = gResult / 255d;

            _ = byte.TryParse(B, out var bResult); //If parsing will fail, the bResult will be equal to 0 (default value).
            double bPrim = bResult / 255d;

            double mMax = Math.Max(rPrim, Math.Max(gPrim, bPrim));
            double mMin = Math.Min(rPrim, Math.Min(gPrim, bPrim));
            double delta = mMax - mMin;

            //Calculate H:
            double newH = 0d;
            if (delta == 0d)
            {
                //newH = 0d; well, no need to update, but kept the if statement for readability.
            }
            else if (mMax == rPrim)
            {
                newH = 60 * ((gPrim - bPrim) / delta % 6d);
            }
            else if (mMax == gPrim)
            {
                newH = 60 * ((bPrim - rPrim) / delta + 2d);
            }
            else if (mMax == bPrim)
            {
                newH = 60 * ((rPrim - gPrim) / delta + 4d);
            }

            //Calculate S:
            double newS = mMax == 0d
                ? 0d
                : delta / mMax * 100d;

            //Calculate V:
            double newV = mMax * 100d;

            //Update H,S and V:
            H = Math.Round(newH, 3).ToString();
            S = Math.Round(newS, 3).ToString();
            V = Math.Round(newV, 3).ToString();

            //Update the selected color rectangle:
            UpdateColor();
        }

        private void CalculateAndUpdateRgbValues()
        {
            //TODO: HSV -> RGB
        }


        private void UpdateColor()
        {
            //We are going to use these variables in conversion:
            _ = byte.TryParse(R, out var rResult);
            _ = byte.TryParse(G, out var gResult);
            _ = byte.TryParse(B, out var bResult);

            var newColor = new SolidColorBrush(Color.FromRgb(rResult, gResult, bResult));
            selectedColorRectangle.Fill = newColor;
            DrawManager.GlobalProperties.BrushColor = newColor;
        }
    }
}
