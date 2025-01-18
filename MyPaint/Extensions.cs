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
}
