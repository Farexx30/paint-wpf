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
        private const uint _hMaxValue = 360u;
        private const uint _sMaxValue = 100u;
        private const uint _vMaxValue = 100u;

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

                    UpdateHsvValues();
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

                    UpdateHsvValues();
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

                    UpdateHsvValues();
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

                    UpdateRgbValues();
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

                    UpdateRgbValues();
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

                    UpdateRgbValues();
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
                if (!uint.TryParse(newText, out var result) || result > _hMaxValue)
                {
                    e.Handled = true;
                }
            }
            else if (textBoxName == _sValueTextBoxName)
            {
                if (!uint.TryParse(newText, out var result) || result > _sMaxValue)
                {
                    e.Handled = true;
                }
            }
            else if (textBoxName == _vValueTextBoxName)
            {
                if (!uint.TryParse(newText, out var result) || result > _vMaxValue)
                {
                    e.Handled = true;
                }
            }
        }


        // !!! CONVERT METHODS !!! //
        private void UpdateHsvValues()
        {
            //TODO: RGB -> HSV
        }

        private void UpdateRgbValues()
        {
            //TODO: HSV -> RGB
        }
    }
}
