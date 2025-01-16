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
        //OnPropertyChanged for data binding:
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //Bindings:
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
                }
            }
        }

        public ColorPickerWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void RValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            
        }
    }
}
