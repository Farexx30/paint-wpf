using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
/// Interaction logic for MatrixFilterWindow.xaml
/// </summary>
/// 

public partial class MatrixFilterWindow : Window
{
    private readonly MainWindow _parentWindow;

    public MatrixFilterWindow(MainWindow parentWindow)
    {
        InitializeComponent();
        _parentWindow = parentWindow;
    }

    private void MatrixFilterWindow_Closed(object sender, EventArgs e)
    {
        _parentWindow.matrixFilterMenuItem.IsEnabled = true;
    }

    private void MatrixCoordinateTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var textBox = sender as TextBox;
        var newText = textBox!.Text.Insert(textBox.CaretIndex, e.Text);
        var newChar = Convert.ToChar(e.Text);

        if ((!float.TryParse(newText, out _) && (newText.Length != 1 || newChar != '-'))
               || (!char.IsDigit(newChar) && newChar != ',' && newChar != '-'))
        {
            e.Handled = true;
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var matrix = GetMatrix();
        var shouldNormalize = normalizationCheckBox.IsChecked;
        var shouldApplyGrayscale = grayscaleCheckBox.IsChecked;

        _parentWindow.ApplyMatrixFilterOnCanvas(matrix, shouldNormalize!.Value, shouldApplyGrayscale!.Value);

        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }


    // =========================================================
    // PRIVATE HELP METHODS
    // =========================================================
    private float[,] GetMatrix()
    {
        _ = float.TryParse(matrix00ValueTextBox.Text, out var result00);
        _ = float.TryParse(matrix01ValueTextBox.Text, out var result01);
        _ = float.TryParse(matrix02ValueTextBox.Text, out var result02);
        _ = float.TryParse(matrix10ValueTextBox.Text, out var result10);
        _ = float.TryParse(matrix11ValueTextBox.Text, out var result11);
        _ = float.TryParse(matrix12ValueTextBox.Text, out var result12);
        _ = float.TryParse(matrix20ValueTextBox.Text, out var result20);
        _ = float.TryParse(matrix21ValueTextBox.Text, out var result21);
        _ = float.TryParse(matrix22ValueTextBox.Text, out var result22);

        float[,] matrix =
        {
            { result00, result01, result02 },
            { result10, result11, result12 },
            { result20, result21, result22 },
        };

        return matrix;
    }
}
