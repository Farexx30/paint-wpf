using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Markup;

namespace MyPaint;

internal enum FileExtension
{
    Png,
    Jpeg,
    Bmp
}

internal static class FileManager
{
    internal static void SaveToFile(Uri path, Canvas canvas, FileExtension fileExtension = FileExtension.Png)
    {
        var transform = canvas.LayoutTransform;
        canvas.LayoutTransform = null;

        var size = new Size(canvas.ActualWidth, canvas.ActualHeight);

        canvas.Measure(size);
        canvas.Arrange(new Rect(size));

        var renderTargetBitMap = canvas.ToRenderTargetBitmap();

        using (var fileStream = new FileStream(path.LocalPath, FileMode.Create))
        {
            var encoder = GetBitmapEncoder(fileExtension);
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitMap));
            encoder.Save(fileStream);
        }

        canvas.LayoutTransform = transform;
    }

    internal static Image? LoadFromFile(Uri path)
    {
        var bitmapImage = new BitmapImage();
        using (var fileStream = new FileStream(path.LocalPath, FileMode.Open, FileAccess.Read))
        {
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = fileStream;
            bitmapImage.EndInit();
        }

        var image = new Image
        {
            Source = bitmapImage
        };

        return image;
    }

    internal static void DeleteFile(Uri path)
    {
        File.Delete(path.LocalPath);
    }

    private static BitmapEncoder GetBitmapEncoder(FileExtension fileExtension)
    {
        BitmapEncoder? bitmapEncoder = null;
        switch (fileExtension)
        {
            case FileExtension.Png:
                bitmapEncoder = new PngBitmapEncoder();
                break;
            case FileExtension.Jpeg:
                bitmapEncoder = new JpegBitmapEncoder();
                break;
            case FileExtension.Bmp:
                bitmapEncoder = new BmpBitmapEncoder();
                break;
        }

        return bitmapEncoder!;
    }
}
