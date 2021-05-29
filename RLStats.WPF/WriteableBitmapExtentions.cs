using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace RLStats_WPF
{
    public static class WriteableBitmapExtentions
    {
        // Save the WriteableBitmap into a PNG file.
        public static void Save(this WriteableBitmap wbitmap, string filename)
        {
            // Save the bitmap into a file.
            using var stream = new FileStream(filename, FileMode.Create);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(wbitmap));
            encoder.Save(stream);
        }

        public static string SaveAsPngFile(this WriteableBitmap wbitmap, string filePah)
        {
            var result = $"{filePah}.png";
            using var stream = new FileStream(result, FileMode.Create);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(wbitmap));
            encoder.Save(stream);

            return result;
        }
    }
}