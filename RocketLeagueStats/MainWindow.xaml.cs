using Microsoft.Win32;
using Newtonsoft.Json;
using RocketLeagueStats.Components;
using RocketLeagueStats.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RocketLeagueStats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Replay> replays = new List<Replay>();
        private int index;

        List<Replay> Replays
        {
            get => replays;
            set
            {
                replays = value;
                lvReplays.Items.Clear();
                foreach (var replay in replays)
                    lvReplays.Items.Add(replay);
            }
        }
        NavigatorWindow Navigator { get; set; } = new NavigatorWindow();
        AdvancedInfoWindow DetailWindow { get; set; } = new AdvancedInfoWindow();
        ServiceWindow Service { get; set; } = new ServiceWindow();
        private int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
                if (Replays.Count != 0)
                    ShowReplay(Replays[index]);
            }
        }

        public MainWindow(AuthTokenInfo tokenInfo)
        {
            DataContext = this;
            InitializeComponent();
            Connection.Instance = new Connection(tokenInfo);
            Closing += MainWindow_Closing;
            Navigator.GetReplaysClicked += Navigator_GetReplaysClicked;
        }

        private void Navigator_GetReplaysClicked(object sender, ApiDataPack e)
        {
            if (e.Success)
            {
                Index = 0;
                Replays = e.Replays;
                ShowReplay(Replays[0]);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Navigator.DontClose = false;
            DetailWindow.DontClose = false;
            Service.DontClose = false;
            Navigator.Close();
            DetailWindow.Close();
            Service.Close();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (lvReplays.Items.Count != 0)
            {
                if (lvReplays.Items.Count < lvReplays.SelectedIndex + 2)
                    lvReplays.SelectedIndex = 0;
                else
                    lvReplays.SelectedIndex++;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (lvReplays.Items.Count != 0)
            {
                if (lvReplays.SelectedIndex - 1 < 0)
                    lvReplays.SelectedIndex = lvReplays.Items.Count - 1;
                else
                    lvReplays.SelectedIndex--;
            }
        }

        private void ShowReplay(Replay replay)
        {
            tbReplayIndex.Text = (Index + 1).ToString() + "/" + Replays.Count.ToString();
            tbDate.Text = "Date: " + replay.Date;
            tbId.Text = "ID: " + replay.ID;
            tbLink.Text = "Link: " + replay.Link;
            tbPlaylist.Text = "Playlist: " + replay.Playlist;
            tbSeason.Text = "Season: " + replay.Season.ToString();
            tbReplayTitle.Text = "Title: " + replay.Title;
            tbUploader.Text = "Uploader: " + replay.Uploader;
            tbWinner.Text = "Winner: " + ((replay.Blue.Goals > replay.Orange.Goals) ? "Blue" : "Orange");
            lblBlueGoals.Text = replay.Blue.Goals.ToString();
            lblOrangeGoals.Text = replay.Orange.Goals.ToString();
            InitializePlayerBoxes(replay);
        }

        private void InitializePlayerBoxes(Replay replay)
        {
            CreatePlayerBoxes(blueSideGrid, replay.Blue);
            CreatePlayerBoxes(orangeSideGrid, replay.Orange);
        }

        private void CreatePlayerBoxes(Grid grid, Team team)
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            for (int i = 0; i < team.Players.Count; i++)
            {
                Player player = team.Players[i];
                grid.RowDefinitions.Add(new RowDefinition());

                var view = new PlayerView();
                view.tbPlayerName.Text = player.Name;
                view.tbScore.Text = player.Score.ToString();
                if (player.MVP)
                    view.tbPlayerName.Foreground = Brushes.Lime;
                grid.Children.Add(view);
                Grid.SetRow(view, i);
            }
        }

        private void BtnNavigator_Click(object sender, RoutedEventArgs e) => Navigator.Show();

        private void BtnSaveReplaysToFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Title = "Save replays";
            dialog.DefaultExt = "rpls";
            dialog.FileName = "replays";
            dialog.Filter = "RPLS files (*.rpls)|*.rpls|All files(*.*)|*.*";
            dialog.FileOk += SaveDialog_FileOk;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.ShowDialog();
        }

        private void BtnLoadReplaysFromFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Load replays";
            dialog.FileOk += LoadDialog_FileOk; ;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.Filter = "RPLS files (*.rpls)|*.rpls|All files(*.*)|*.*";
            dialog.ShowDialog();
        }

        private void LoadDialog_FileOk(object sender, CancelEventArgs e)
        {
            var dialog = (OpenFileDialog)sender;
            var compressedBytes = File.ReadAllBytes(dialog.FileName);
            var jsonString = Compressor.DecompressBytes(compressedBytes);
            Replays = JsonConvert.DeserializeObject<List<Replay>>(jsonString);
        }

        private void SaveDialog_FileOk(object sender, CancelEventArgs e)
        {
            var dialog = (SaveFileDialog)sender;
            var compressedBytes = Compressor.CompressString(JsonConvert.SerializeObject(Replays));
            File.WriteAllBytes(dialog.FileName, compressedBytes);
        }

        private void LvReplays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvReplays.SelectedIndex != -1)
                Index = lvReplays.SelectedIndex;
        }
        private void BtnAdvancedInfo_Click(object sender, RoutedEventArgs e)
        {
            DetailWindow.Show();
            DetailWindow.LoadReplaysAsync(Replays);
        }
        private void BtnService_Click(object sender, RoutedEventArgs e) => Service.Show();
    }
}
