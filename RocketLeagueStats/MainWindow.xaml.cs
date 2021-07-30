using Microsoft.Win32;

using Newtonsoft.Json;

using RLStats_Classes.MainClasses;
using RLStats_Classes.Models;

using RocketLeagueStats.Components;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        private List<Replay> _replays = new();
        private int _index;

        private List<Replay> Replays
        {
            get => _replays;
            set
            {
                _replays = value;
                lvReplays.Items.Clear();
                foreach (var replay in _replays)
                    lvReplays.Items.Add(replay);
            }
        }

        private List<Replay> ReplaysToCompare { get; set; } = null;
        private NavigatorWindow Navigator { get; set; }
        private AdvancedInfoWindow DetailWindow { get; set; }
        private ServiceWindow Service { get; set; }
        private int Index
        {
            get => _index;
            set
            {
                _index = value;
                if (Replays.Count != 0)
                    ShowReplay(Replays[_index]);
            }
        }

        public MainWindow(AuthTokenInfo tokenInfo, string filePath)
        {
            DataContext = this;
            InitializeComponent();
            Service = new ServiceWindow(tokenInfo);
            Navigator = new NavigatorWindow(tokenInfo);
            DetailWindow = new AdvancedInfoWindow(new AdvancedReplayProvider(tokenInfo));
            Closing += MainWindow_Closing;
            Navigator.GetReplaysClicked += Navigator_GetReplaysClicked;
            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    LoadFile(filePath);
                }
                catch
                {
                    MessageBox.Show("Couldn't load file");
                }
            }
        }

        private void Navigator_GetReplaysClicked(object sender, (List<Replay> replays, List<Replay> replaysToCompare) pack)
        {
            Index = 0;
            Replays = pack.replays;
            ReplaysToCompare = pack.replaysToCompare;
            ShowReplay(Replays[0]);
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
            tbId.Text = "ID: " + replay.Id;
            tbRLId.Text = $"RocketLeagueID: {replay.RocketLeagueId}";
            tbSeasonType.Text = $"Seasontype: {replay.SeasonType}";
            tbVisibility.Text = $"Visibility: {replay.Visibility}";
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

        private static void CreatePlayerBoxes(Grid grid, Team team)
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
            var dialog = new SaveFileDialog
            {
                Title = "Save replays",
                DefaultExt = "rpls",
                FileName = "replays",
                Filter = "RPLS files (*.rpls)|*.rpls|All files(*.*)|*.*"
            };
            dialog.FileOk += SaveDialog_FileOk;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.ShowDialog();
        }

        private void BtnLoadReplaysFromFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Title = "Load replays" };
            dialog.FileOk += LoadDialog_FileOk; ;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.Filter = "RPLS files (*.rpls)|*.rpls|All files(*.*)|*.*";
            dialog.ShowDialog();
        }

        private void LoadDialog_FileOk(object sender, CancelEventArgs e)
        {
            var dialog = (OpenFileDialog)sender;
            LoadFile(dialog.FileName);
        }

        private void LoadFile(string fileName)
        {
            var compressedBytes = File.ReadAllBytes(fileName);
            var jsonString = Compressor.DecompressBytesToString(compressedBytes);
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
            DetailWindow.LoadReplaysAsync(Replays, ReplaysToCompare);
        }
        private void BtnService_Click(object sender, RoutedEventArgs e) => Service.Show();
    }
}
