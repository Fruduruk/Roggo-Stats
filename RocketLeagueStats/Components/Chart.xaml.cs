using RLStats_WPF;
using System.Windows;
using System.Windows.Controls;

namespace RocketLeagueStats.Components
{
    /// <summary>
    /// Gives you a customizable chart
    /// </summary>
    public partial class Chart : UserControl
    {
        private readonly ChartCreator _creator;

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Chart), new PropertyMetadata("Chart"));

        public Chart(ChartCreator creator, double height = (375+100), double width = 700)
        {
            _creator = creator;
            _creator.Height = height;
            _creator.Width = width;
            Title = creator.Title;
            Height = height;
            Width = width;
            InitializeComponent();
        }

        public void ReDraw()
        {
            BackgroundPanel.Children.Clear();
            var canvas = _creator.CreateCanvas(true);
            BackgroundPanel.Children.Add(canvas);
        }
    }
}
