using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RocketLeagueStats.Components
{
    /// <summary>
    /// Gives you a customizable chart
    /// </summary>
    public partial class Chart : UserControl
    {
        private double YLineLength => Height -60;
        private double XLineLength => Width - 90;
        private const double HeaderHeight = 30;

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }


        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Chart), new PropertyMetadata("Chart"));

        public Dictionary<string, double> ChartBarValues { get; set; } = new Dictionary<string, double>();

        public Chart(double height = 250 *1.5, double width = 350 *2)
        {
            Height = height;
            Width = width;
            InitializeComponent();
        }

        public void ReDraw()
        {
            cvChart.Children.Clear();
            DrawLines();
            DrawNamesAndValues();
            DrawColumns();
            DrawYUnit();
        }

        private void DrawColumns()
        {
            int i = 0;
            foreach(var pair in ChartBarValues)
            {
                i++;
                DrawColumn(i,pair.Value);
            }
        }

        private void DrawColumn(int i, double value)
        {
            var x = GetX(i);
            var canvasStart = ConvertToCanvas(new Point(x, 0));
            var canvasEnd = ConvertToCanvas(new Point(x, ConvertValueToPixel(value)));
            Line line = new Line();
            line.Stroke = GetBrush(i);
            line.StrokeThickness = 20;
            line.X1 = canvasStart.X;
            line.Y1 = canvasStart.Y;
            line.X2 = canvasEnd.X;
            line.Y2 = canvasEnd.Y;
            cvChart.Children.Add(line);
        }

        private double GetX(int i)
        {
            return XLineLength / (ChartBarValues.Count + 1)*i;
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

        private void DrawNamesAndValues()
        {
            int i = 0;
            foreach (var pair in ChartBarValues)
            {
                i++;
                PlaceName(i, pair.Key,pair.Value);
            }
        }

        private void PlaceName(int i, string name,double value)
        {
            var x = GetX(i);
            var label = GetCanvasLabel(name+$" ({value:0.##})", ConvertToCanvas(new Point(x-30,-5)));
            cvChart.Children.Add(label);
        }

        private void DrawYUnit()
        {
            cvChart.Children.Add(GetCanvasLabel(GetHighestValue().ToString("0.##"), new Point(0,0)));
        }

        private Label GetCanvasLabel(string content,Point point)
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
            foreach(var pair in ChartBarValues)
            {
                if (pair.Value > highest)
                    highest = pair.Value;
            }
            return highest;
        }

        private double ConvertValueToPixel(double value)
        {
            var highest = GetHighestValue();
            if (highest == 0)
                highest = 1;
            var factor = (YLineLength - 20) / highest;
            return value * factor;
        }

        private void DrawLines()
        {
            cvChart.Children.Add(GetCanvasLine(new Point(0, 0), new Point(0, YLineLength)));
            cvChart.Children.Add(GetCanvasLine(new Point(0, 0), new Point(XLineLength, 0)));
        }

        private Line GetCanvasLine(Point start,Point end)
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
            p.Y = (Height- HeaderHeight - pointOnGraph.Y)-30;
            return p;
        }
    }
}
