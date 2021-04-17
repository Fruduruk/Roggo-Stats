using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace RocketLeagueStats.Components
{
    /// <summary>
    /// Interaction logic for LoadingGrid.xaml
    /// </summary>
    public partial class LoadingGrid : UserControl
    {
        public double PackCount { get; private set; }
        public double ChunkCount { get; private set; }
        private double ChunkHeight { get; set; } = 0;
        private double ChunkWidth { get; set; } = 0;
        private WrapPanel CurrentChunk { get; set; }

        public LoadingGrid()
        {
            InitializeComponent();
        }

        public void AddPack(Brush backgroundColor)
        {
            Canvas c = new Canvas();
            c.Height = CurrentChunk.Height / 7;
            c.Width = CurrentChunk.Width / 7;
            c.Background = backgroundColor;
            CurrentChunk.Children.Add(c);
        }

        public void Clear()
        {
            loadingPanel.Children.Clear();
        }

        public void AddChunk(Brush backgroundColor)
        {
            CurrentChunk = new WrapPanel{Width = ChunkWidth, Height = ChunkHeight};
            CurrentChunk.Background = backgroundColor;
            loadingPanel.Children.Add(CurrentChunk);
        }

        public void InitializeGrid(int maxPackCount, int maxChunkCount)
        {
            PackCount = maxPackCount;
            ChunkCount = maxChunkCount;
            var rest = PackCount % 50;
            var rowAndColumnChunkCount = GetRowAndColumnChunkCount(ChunkCount);
            ChunkWidth = loadingPanel.ActualWidth / rowAndColumnChunkCount;
            ChunkHeight = loadingPanel.ActualHeight / rowAndColumnChunkCount;
        }

        private double GetRowAndColumnChunkCount(double chunkCount)
        {
            var rowAndColumnChunkCount = Math.Sqrt(chunkCount);
            int i = 1;
            while (rowAndColumnChunkCount % 1 != 0)
            {
                rowAndColumnChunkCount = Math.Sqrt(chunkCount + i);
                i++;
            }

            return rowAndColumnChunkCount;
        }
    }
}
