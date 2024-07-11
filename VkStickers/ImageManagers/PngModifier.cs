using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VkStickers.StickerManagers
{
    internal static class PngModifier
    {
        public static BitmapSource ReplaceTransparency(this BitmapSource bitmap, System.Windows.Media.Color color)
        {
            var rect = new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            var visual = new DrawingVisual();
            var context = visual.RenderOpen();
            context.DrawRectangle(new SolidColorBrush(color), null, rect);
            context.DrawImage(bitmap, rect);
            context.Close();

            var render = new RenderTargetBitmap(bitmap.PixelWidth, bitmap.PixelHeight,
                96, 96, PixelFormats.Pbgra32);
            render.Render(visual);
            return render;
        }
    }
}
