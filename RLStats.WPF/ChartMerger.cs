using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace RLStatsWPF
{
    public class ChartMerger
    {
        private readonly List<ChartCreator> _charts;

        public ChartMerger(IEnumerable<ChartCreator> charts)
        {
            _charts = new List<ChartCreator>(charts);
        }

        public string CreatePngImageAsStream(string filePath, int maxColumns = 4)
        {
            var columns = (_charts.Count > maxColumns) ? maxColumns : _charts.Count;
            var canvas = CreateGreatCanvas(columns);
            var converter = new CanvasToImageConverter();
            var wBitmap = converter.SaveAsWriteableBitmap(canvas);
            if (wBitmap is null) throw new Exception("Writable Bitmap was null.");
            var result = wBitmap.SaveAsPngFile(filePath);
            if (result is null) throw new Exception("Stream was null.");
            return result;
        }

        public Canvas CreateGreatCanvas(int columnCount)
        {
            var rowCount = GetRowCount(columnCount);
            var greatCanvas = new Canvas();
            for (var i = 0; i < _charts.Count; i++)
            {
                var chart = _charts[i];
                var chartCanvas = chart.CreateCanvas(true);
                if (i.Equals(0))
                {
                    greatCanvas.Width = chart.Width * columnCount;
                    greatCanvas.Height = chart.Height * rowCount;
                }
                greatCanvas.Children.Add(chartCanvas);
                Canvas.SetLeft(chartCanvas, (i % columnCount) * chart.Width);
                Canvas.SetTop(chartCanvas, GetCurrentRow(i, columnCount) * chart.Height);
            }

            return greatCanvas;
        }

        private static int GetCurrentRow(int i, int columnCount)
        {
            var dI = Convert.ToDouble(i);
            var dColumnCount = Convert.ToDouble(columnCount);
            var dResult = dI / dColumnCount;
            var result = Convert.ToInt32(Math.Round(dResult, MidpointRounding.ToNegativeInfinity));
            return result;
        }

        private int GetRowCount(int columns)
        {
            return Convert.ToInt32(Math.Round(Convert.ToDouble(_charts.Count) / Convert.ToDouble(columns), MidpointRounding.ToPositiveInfinity));
        }
    }
}
