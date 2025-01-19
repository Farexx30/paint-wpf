using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace MyPaint;

public static class Extensions
{
    //Extenston method for rendering a RenderTargetBitmap from Canvas:
    public static RenderTargetBitmap ToRenderTargetBitmap(this Canvas canvas)
    {
        var renderTargetBitMap = new RenderTargetBitmap(
           (int)canvas.ActualWidth,
           (int)canvas.ActualHeight,
           96d,
           96d,
           PixelFormats.Pbgra32);

        renderTargetBitMap.Render(canvas);

        return renderTargetBitMap;
    }

    //Extenston method for matrix normalization (min-max normalization):
    public static float[,] Normalize(this float[,] matrix)
    {
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;
        foreach (var value in matrix)
        {
            if (value < minValue) minValue = value;
            if (value > maxValue) maxValue = value;
        }

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        float maxMinDifference = maxValue - minValue;

        if (maxMinDifference == 0)
        {
            return new float[rows, cols];
        }

        var normalizedMatrix = new float[rows, cols];
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < cols; ++j)
            {
                normalizedMatrix[i, j] = (matrix[i, j] - minValue) / maxMinDifference;
            }
        }

        return normalizedMatrix;
    }
}
