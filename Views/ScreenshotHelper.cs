using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;

namespace StockTrack.Avalonia.Views;

public static class ScreenshotHelper
{
    public static void CaptureVisual(Visual visual, string outputPath, int width, int height)
    {
        var pixelSize = new PixelSize(width, height);
        using var rtb = new RenderTargetBitmap(pixelSize, new Vector(96, 96));
        rtb.Render(visual);
        using var stream = File.Create(outputPath);
        rtb.Save(stream);
    }
}
