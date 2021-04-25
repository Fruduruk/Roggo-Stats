using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RLStats_WPF
{
    public class ChartCreator
    {
        private const double HeaderHeight = 30;
        private readonly Dictionary<string, double> _barValues;
        public string Title { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }

        private double YLineLength => Height - 60;
        private double XLineLength => Width - 90;
        public ChartCreator(string title, Dictionary<string, double> barValues)
        {
            _barValues = barValues;
            Title = title;
        }

        public Canvas CreateCanvas()
        {
            var canvas = new Canvas();
            canvas.Height = Height;
            canvas.Width = Width;
            DrawLines(canvas);
            DrawNamesAndValues(canvas);
            DrawColumns(canvas);
            DrawYUnit(canvas);
            return canvas;
        }

        public string CreatePngImageAsStream(string fileName)
        {
            var canvas = CreateCanvas();
            if (canvas is null) throw new Exception("Canvas was null.");
            var converter = new CanvasToImageConverter();
            var wBitmap = converter.SaveAsWriteableBitmap(canvas);
            if (wBitmap is null) throw new Exception("Writable Bitmap was null.");
            var result = wBitmap.SaveAsPngFile(fileName);
            if (result is null) throw new Exception("Stream was null.");
            return result;
        }

        private void DrawYUnit(Canvas canvas)
        {
            canvas.Children.Add(GetCanvasLabel(GetHighestValue().ToString("0.##"), new Point(0, 0)));
        }

        private void DrawColumns(Canvas canvas)
        {
            int i = 0;
            foreach (var pair in _barValues)
            {
                i++;
                DrawColumn(canvas, i, pair.Value);
            }
        }

        private void DrawColumn(Canvas canvas, int i, double pairValue)
        {
            var x = GetX(i);
            var canvasStart = ConvertToCanvas(new Point(x, 0));
            var canvasEnd = ConvertToCanvas(new Point(x, ConvertValueToPixel(pairValue)));
            Line line = new Line();
            line.Stroke = GetBrush(i);
            line.StrokeThickness = 20;
            line.X1 = canvasStart.X;
            line.Y1 = canvasStart.Y;
            line.X2 = canvasEnd.X;
            line.Y2 = canvasEnd.Y;
            canvas.Children.Add(line);
        }

        private Label GetCanvasLabel(string content, Point point)
        {
            Label label = new Label();
            label.FontSize = 15;
            label.Foreground = Brushes.AliceBlue;
            label.Content = content;
            Canvas.SetLeft(label, point.X);
            Canvas.SetTop(label, point.Y);
            return label;
        }

        private double GetHighestValue()
        {
            double highest = 0;
            foreach (var pair in _barValues)
            {
                if (pair.Value > highest)
                    highest = pair.Value;
            }
            return highest;
        }

        private void DrawLines(Canvas canvas)
        {
            canvas.Children.Add(GetCanvasLine(new Point(0, 0), new Point(0, YLineLength)));
            canvas.Children.Add(GetCanvasLine(new Point(0, 0), new Point(XLineLength, 0)));
        }

        private double GetX(int i)
        {
            return XLineLength / (_barValues.Count + 1) * i;
        }

        private Brush GetBrush(int i)
        {
            return i switch
            {
                1 => Brushes.LightGreen,
                2 => Brushes.LightCoral,
                3 => Brushes.LightBlue,
                4 => Brushes.LightCyan,
                5 => Brushes.LightPink,
                6 => Brushes.LightSalmon,
                7 => Brushes.LightYellow,
                8 => Brushes.LightSteelBlue,
                _ => Brushes.AliceBlue,
            };
        }

        private double ConvertValueToPixel(double value)
        {
            var highest = GetHighestValue();
            if (highest == 0)
                highest = 1;
            var factor = (YLineLength - 20) / highest;
            return value * factor;
        }

        private void DrawNamesAndValues(Canvas canvas)
        {
            int i = 0;
            foreach (var pair in _barValues)
            {
                i++;
                PlaceName(canvas, i, pair.Key, pair.Value);
            }
        }

        private void PlaceName(Canvas canvas, int i, string pairKey, double pairValue)
        {
            var x = GetX(i);
            var label = GetCanvasLabel(pairKey + $" ({pairValue:0.##})", ConvertToCanvas(new Point(x - 30, -5)));
            canvas.Children.Add(label);
        }

        private Line GetCanvasLine(Point start, Point end)
        {
            Line line = new Line();
            line.Stroke = Brushes.AliceBlue;
            line.StrokeThickness = 4;
            var canvasStart = ConvertToCanvas(start);
            var canvasEnd = ConvertToCanvas(end);
            line.X1 = canvasStart.X;
            line.Y1 = canvasStart.Y;
            line.X2 = canvasEnd.X;
            line.Y2 = canvasEnd.Y;
            return line;
        }

        private Point ConvertToCanvas(Point pointOnGraph)
        {
            Point p = new Point();
            p.X = pointOnGraph.X + 60;
            p.Y = (Height - HeaderHeight - pointOnGraph.Y) - 30;
            return p;
        }
    }
}
