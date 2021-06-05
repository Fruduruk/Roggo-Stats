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
        private enum Position
        {
            Left,
            Middle,
            Right
        };

        private const double HeaderHeight = 30;
        private readonly Dictionary<string, double> _barValues;
        private readonly Dictionary<string, double> _barValuesToCompare;
        private readonly bool _compareToOtherStats = false;
        public string Title { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }

        private double YLineLength => Height - (60 + 100);
        private double XLineLength => Width - 90;
        public ChartCreator(string title, Dictionary<string, double> barValues, double height, double width)
        {
            Height = height;
            Width = width;
            _barValues = barValues;
            Title = title;
        }
        public ChartCreator(string title, Dictionary<string, double> barValues, Dictionary<string, double> barValuesToCompare, double height, double width)
        {
            Height = height;
            Width = width;
            _barValues = barValues;
            _barValuesToCompare = barValuesToCompare;
            _compareToOtherStats = true;
            Title = title;
        }

        public Canvas CreateCanvas(bool addTitle = false)
        {
            var canvas = new Canvas { Height = Height, Width = Width };
            DrawLines(canvas);

            if (addTitle)
                DrawTitle(canvas);

            DrawNamesAndValues(canvas);
            DrawColumns(canvas);

            DrawYUnit(canvas);
            return canvas;
        }

        public string CreatePngImageAsStream(string filePath, bool addTitle = false)
        {
            var canvas = CreateCanvas(addTitle);
            if (canvas is null) throw new Exception("Canvas was null.");
            var converter = new CanvasToImageConverter();
            var wBitmap = converter.SaveAsWriteableBitmap(canvas);
            if (wBitmap is null) throw new Exception("Writable Bitmap was null.");
            var result = wBitmap.SaveAsPngFile(filePath);
            if (result is null) throw new Exception("Stream was null.");
            return result;
        }

        private void DrawYUnit(Canvas canvas)
        {
            canvas.Children.Add(GetCanvasLabel(GetHighestValue().ToString("0.##"), new Point(0, 70)));
        }

        private void DrawTitle(Canvas canvas)
        {
            var label = new Label
            {
                FontSize = 40,
                Foreground = Brushes.AliceBlue,
                Opacity = 0.5,
                Content = Title
            };
            Canvas.SetLeft(label, Width * 0.05);
            Canvas.SetTop(label, Height * 0.02);
            canvas.Children.Add(label);
        }

        private void DrawColumns(Canvas canvas)
        {
            int i = 0;
            foreach (var pair in _barValues)
            {
                i++;
                if (_compareToOtherStats)
                {
                    DrawColumn(canvas, i, GetBrush(i), pair.Value, Position.Left);
                    DrawColumn(canvas, i, GetBrush(i + 5), _barValuesToCompare[pair.Key], Position.Right);
                }
                else
                    DrawColumn(canvas, i, GetBrush(i), pair.Value);
            }
        }

        private void DrawColumn(Canvas canvas, int i, Brush brush, double pairValue, Position position = Position.Middle)
        {
            double offset = 0;
            switch (position)
            {
                case Position.Left:
                    offset = -10;
                    break;
                case Position.Middle:
                    break;
                case Position.Right:
                    offset = +10;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }
            var x = GetX(i, offset);
            var canvasStart = ConvertToCanvas(new Point(x, 0));
            var canvasEnd = ConvertToCanvas(new Point(x, ConvertValueToPixel(pairValue)));
            var line = new Line
            {
                Stroke = brush,
                StrokeThickness = 20,
                X1 = canvasStart.X,
                Y1 = canvasStart.Y,
                X2 = canvasEnd.X,
                Y2 = canvasEnd.Y
            };
            canvas.Children.Add(line);
        }

        private Label GetCanvasLabel(string content, Point point)
        {
            var label = new Label
            {
                FontSize = 18,
                Foreground = Brushes.AliceBlue,
                Content = content
            };
            Canvas.SetLeft(label, point.X);
            Canvas.SetTop(label, point.Y);
            return label;
        }

        private double GetHighestValue()
        {
            double highest = 0;
            foreach (var pair in _barValues)
                if (pair.Value > highest)
                    highest = pair.Value;

            if (_compareToOtherStats)
                foreach (var pair in _barValuesToCompare)
                    if (pair.Value > highest)
                        highest = pair.Value;
            return highest;
        }

        private void DrawLines(Canvas canvas)
        {
            canvas.Children.Add(GetCanvasLine(new Point(0, 0), new Point(0, YLineLength)));
            canvas.Children.Add(GetCanvasLine(new Point(0, 0), new Point(XLineLength, 0)));
        }

        private double GetX(int i, double offset = 0)
        {
            var x = XLineLength / (_barValues.Count + 1) * i;
            return x + offset;
        }

        private Brush GetBrush(int i)
        {
            int k = i % 8;
            return k switch
            {
                1 => Brushes.LightGreen,
                2 => Brushes.LightCoral,
                3 => Brushes.LightBlue,
                4 => Brushes.LightPink,
                5 => Brushes.LightSalmon,
                6 => Brushes.LightSteelBlue,
                7 => Brushes.LightCyan,
                8 => Brushes.LightYellow,
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
                var valueString = pair.Value.ToString("0.##");
                if (valueString.Length < 7)
                {
                    if (_compareToOtherStats)
                    {
                        var difference = _barValuesToCompare[pair.Key] - pair.Value;
                        var differenceString = difference.ToString("0.##");
                        differenceString = difference >= 0 ? $"+{differenceString}" : $"{differenceString}";
                        valueString += $" {differenceString}";
                    }
                }
                else
                {
                    valueString = "too big";
                }
                PlaceName(canvas, i, pair.Key, valueString,_barValues.Count > 1);
            }
        }

        private void PlaceName(Canvas canvas, int i, string name, string value, bool moveDown)
        {
            var x = GetX(i);
            var y =  0;
            if (moveDown && i % 2 != 0)
                y = -25;
            var label = GetCanvasLabel(name + $" ({value})", ConvertToCanvas(new Point(x - 70, y)));
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
            var p = new Point();
            p.X = pointOnGraph.X + 60;
            p.Y = (Height - HeaderHeight - pointOnGraph.Y) - 60;
            return p;
        }
    }
}
