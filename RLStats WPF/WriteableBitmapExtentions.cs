using System.IO;
using System.IO.Enumeration;
using System.Text.Json;
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

        public static MemoryStream GetPngImageAsStream(this WriteableBitmap wbitmap)
        {
            var stream = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(wbitmap));
            encoder.Save(stream);
            return stream;
        }
    }
}