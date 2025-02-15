﻿using System;
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

namespace MyPaint;

/// <summary>
/// Interaction logic for ColorPickerWindow.xaml
/// </summary>
///

internal enum InputColorSpace
{
    Rgb,
    Hsv
}

public partial class ColorPickerWindow : Window, INotifyPropertyChanged
{
    // !!! OnPropertyChanged for data binding !!! //
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // !!! PRIVATE FIELDS !!! //
    private bool _isInitializing = true;
    private readonly MainWindow _parentWindow;
    private Color _currentColor = DrawManager.GlobalProperties.BrushColor.Color;
    private InputColorSpace _currentInputColorSpace;
    private const string _hValueTextBoxName = "hValueTextBox";
    private const string _sValueTextBoxName = "sValueTextBox";
    private const string _vValueTextBoxName = "vValueTextBox";
    private const double _hMaxValue = 360d; //Actually the _hMaxValue is < 360d but i covered it in validation logic.
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

                if (_currentInputColorSpace == InputColorSpace.Rgb && !_isInitializing)
                {
                    CalculateAndUpdateHsvValues();
                }
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

                if (_currentInputColorSpace == InputColorSpace.Rgb && !_isInitializing)
                {
                    CalculateAndUpdateHsvValues();
                }
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

                if (_currentInputColorSpace == InputColorSpace.Rgb && !_isInitializing)
                {
                    CalculateAndUpdateHsvValues();
                }
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

                if (_currentInputColorSpace == InputColorSpace.Hsv)
                {
                    CalculateAndUpdateRgbValues();
                }
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

                if (_currentInputColorSpace == InputColorSpace.Hsv)
                {
                    CalculateAndUpdateRgbValues();
                }
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

                if (_currentInputColorSpace == InputColorSpace.Hsv)
                {
                    CalculateAndUpdateRgbValues();
                }
            }
        }
    }


    // !!! CONSTRUCTORS AND INITIALIZATION !!! //
    public ColorPickerWindow(MainWindow parentWindow) //Pass the reference to MainWindow for simplicity. The ColorPickerWindow is tightly coupled to MainWindow anyway.
    {
        InitializeComponent();
        DataContext = this;
        _parentWindow = parentWindow;

        Initialize();
    }

    private void Initialize()
    {
        R = _currentColor.R.ToString();
        G = _currentColor.G.ToString();
        B = _currentColor.B.ToString();
        CalculateAndUpdateHsvValues();
        _isInitializing = false;
    }

    private void ColorPickerWindow_Closed(object sender, EventArgs e)
    {
        _parentWindow.colorPickerRectangle.IsEnabled = true;
    }

    // !!! VALIDATION EVENTS !!! //
    private void RgbValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var textBox = sender as TextBox;
        var newText = textBox!.Text.Insert(textBox.CaretIndex, e.Text);
        var newChar = Convert.ToChar(e.Text);

        if (!byte.TryParse(newText, out _) || !char.IsDigit(newChar))
        {
            e.Handled = true;
        }
    }

    private void HsvValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var textBox = sender as TextBox;
        var textBoxName = textBox!.Name;
        var newText = textBox!.Text.Insert(textBox.CaretIndex, e.Text);
        var newChar = Convert.ToChar(e.Text);

        if (textBoxName == _hValueTextBoxName)
        {
            if (!double.TryParse(newText, out var result) 
                || result >= _hMaxValue 
                || result < 0
                || (!char.IsDigit(newChar) && newChar != ','))
            {
                e.Handled = true;
            }
        }
        else if (textBoxName == _sValueTextBoxName)
        {
            if (!double.TryParse(newText, out var result) 
                || result > _sMaxValue 
                || result < 0
                || (!char.IsDigit(newChar) && newChar != ','))
            {
                e.Handled = true;
            }
        }
        else if (textBoxName == _vValueTextBoxName)
        {
            if (!double.TryParse(newText, out var result) 
                || result > _vMaxValue 
                || result < 0
                || (!char.IsDigit(newChar) && newChar != ','))
            {
                e.Handled = true;
            }
        }
    }

    // !!! GOT FOCUS EVENTS !!! //
    private void RgbTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        _currentInputColorSpace = InputColorSpace.Rgb;
    }

    private void HsvTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        _currentInputColorSpace = InputColorSpace.Hsv;
    }

    // !!! BUTTON CLICK EVENTS !!! //
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        DrawManager.GlobalProperties.BrushColor = new SolidColorBrush(_currentColor);
        _parentWindow.UpdateColorPicker(); //Updates color picker in Main Window. I used this approach only for simplicity because this window is coupled to MainWindow anyway.

        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }


    // !!! CONVERT METHODS !!! //
    private void CalculateAndUpdateHsvValues() //This method also updates the ColorRectangle Fill in seperate method.
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
            //newH = 0d; well, no need to update, but kept this if statement for readability.
        }
        else if (mMax == rPrim)
        {
            newH = 60d * ((gPrim - bPrim) / delta % 6d);
        }
        else if (mMax == gPrim)
        {
            newH = 60d * ((bPrim - rPrim) / delta + 2d);
        }
        else if (mMax == bPrim)
        {
            newH = 60d * ((rPrim - gPrim) / delta + 4d);
        }

        //Because the value of H should be a value from 0 to 360 and it is possible that newH will be negative value from formulas above:
        newH = newH < 0d
            ? newH + 360d
            : newH;

        //Calculate S:
        double newS = mMax == 0d
            ? 0d
            : delta / mMax * 100d; //* 100 because the displayed is in %.

        //Calculate V:
        double newV = mMax * 100d; //* 100 because the displayed value is in %.

        //Update H,S and V:
        H = Math.Round(newH, 3).ToString();
        S = Math.Round(newS, 3).ToString();
        V = Math.Round(newV, 3).ToString();

        //Update the selected color rectangle:
        UpdateColor();
    }

    private void CalculateAndUpdateRgbValues() //This method also updates the ColorRectangle Fill in seperate method.
    {
        //We are going to use these variables in conversion:
        _ = double.TryParse(H, out var hResult);
        _ = double.TryParse(S, out var sResult);
        sResult /= 100; //Because the displayed value is in %.
        _ = double.TryParse(V, out var vResult);
        vResult /= 100; //Because the displayed value is in %.
        var c = vResult * sResult;
        var x = c * (1 - Math.Abs(hResult / 60d % 2d - 1d));
        var m = vResult - c;

        //Calculate the R', G' and B'
        double rPrim = 0d, gPrim = 0d, bPrim = 0d;
        if (hResult >= 0d && hResult < 60)
        {
            rPrim = c;
            gPrim = x;
            bPrim = 0d; //Basically no need to update, but I left it for readability and consistency.
        }
        else if (hResult >= 60d && hResult < 120d)
        {
            rPrim = x;
            gPrim = c;
            bPrim = 0d;
        }
        else if (hResult >= 120d && hResult < 180d)
        {
            rPrim = 0d;
            gPrim = c;
            bPrim = x;
        }
        else if (hResult >= 180d && hResult < 240d)
        {
            rPrim = 0d;
            gPrim = x;
            bPrim = c;
        }
        else if (hResult >= 240d && hResult < 300d)
        {
            rPrim = x;
            gPrim = 0d;
            bPrim = c;
        }
        else if (hResult >= 300d && hResult < 360d)
        {
            rPrim = c;
            gPrim = 0d;
            bPrim = x;
        }

        //Update R, G and B:
        R = (Math.Round((rPrim + m) * 255d)).ToString();
        G = (Math.Round((gPrim + m) * 255d)).ToString();
        B = (Math.Round((bPrim + m) * 255d)).ToString();

        //Update color:
        UpdateColor();
    }

    private void UpdateColor()
    {

        _ = byte.TryParse(R, out var rResult);
        _ = byte.TryParse(G, out var gResult);
        _ = byte.TryParse(B, out var bResult);

        _currentColor = Color.FromRgb(rResult, gResult, bResult);
        selectedColorRectangle.Fill = new SolidColorBrush(_currentColor);
    }
}
