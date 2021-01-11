using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RocketLeagueStats.AdvancedModels;

namespace RocketLeagueStats
{
    public class Database
    {
        public string SavingDirectory { get; private set; }
        //public string SavingFile { get; private set; }
        //private List<AdvancedReplay> Cache { get; set; }

        private const string AdvancedReplayListName = "SavedReplays";

        public Database()
        {
            //Cache = new List<AdvancedReplay>();
            SavingDirectory = Environment.GetEnvironmentVariable("temp") + @"\rl_replay_stats";
            Console.WriteLine(SavingDirectory);
            if (!Directory.Exists(SavingDirectory))
            {
                Directory.CreateDirectory(SavingDirectory);
            }
        }
       
        public async void SaveReplayListAsync(List<AdvancedReplay> replays)
        {
            foreach(var replay in replays)
            {
                await SaveReplayAsync(replay);
            }
        }
        public async Task SaveReplayAsync(AdvancedReplay replay)
        {
            string path = $@"{SavingDirectory}\{replay.Date.ToString("yy.MM.dd")}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += $@"\{replay.Id}.rply";
            var jsonString = JsonConvert.SerializeObject(replay);
            var compressedString = Compressor.CompressString(jsonString);
            if (!File.Exists(path))
            {
                await File.WriteAllBytesAsync(path, compressedString);
            }
        }
        public async Task<AdvancedReplay> LoadReplayAsync(Replay r)
        {
            AdvancedReplay replay = null;
            var replayPath = await GetReplayPath(r);
            await Task.Run(() =>
            {
                if (replayPath is null)
                    throw new Exception($"Couldn't load Replay: {r.ID}");
                else
                {
                    var path = replayPath;
                    var compressedString = File.ReadAllBytes(path);
                    replay = JsonConvert.DeserializeObject<AdvancedReplay>(Compressor.DecompressBytes(compressedString));
                }
            });
            if (replay is null)
                throw new Exception("replay was null");
            return replay;
        }
        public async Task<string> GetReplayPath(Replay replay)
        {
            var directories = Directory.EnumerateDirectories(SavingDirectory);
            foreach(var d in directories)
            {
                if (d.Contains(replay.Date.ToString("yy.MM.dd")))
                {
                    var filenames = Directory.EnumerateFiles(d);
                    return await Task<string>.Run(() =>
                    {
                        foreach (var s in filenames)
                            if (s.Contains(replay.ID))
                                return s;
                        return null;
                    });
                }
            }
            return null;
        }
    }
}
