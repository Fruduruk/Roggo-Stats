using Newtonsoft.Json;
using RLStats_Classes.AdvancedModels;
using RLStats_Classes.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses
{
    public class Database
    {
        public string SavingDirectory
        {
            get
            {
                if (!Directory.Exists(savingDirectory))
                    Directory.CreateDirectory(savingDirectory);
                return savingDirectory;
            }
            private set => savingDirectory = value;
        }

        private string savingDirectory;

        public Database()
        {
            SavingDirectory = RLConstants.RLStatsFolder + @"\Data";
            Console.WriteLine(SavingDirectory);
            if (!Directory.Exists(SavingDirectory))
            {
                Directory.CreateDirectory(SavingDirectory);
            }
        }
        public bool IsReplayInDatabase(Replay replay)
        {
            var task = GetReplayPath(replay);
            task.Wait();
            var result = !(task.Result is null);
            return result;
        }
        public async Task SaveReplayListAsync(List<AdvancedReplay> replays)
        {
            foreach (var replay in replays)
            {
                await SaveReplayAsync(replay);
            }
        }
        public async Task SaveReplayAsync(AdvancedReplay replay)
        {
            var path = CreateReplayPath(replay);
            var jsonString = JsonConvert.SerializeObject(replay);
            var compressedString = Compressor.CompressString(jsonString);
            await File.WriteAllBytesAsync(path, compressedString);
        }

        private string CreateReplayPath(AdvancedReplay replay)
        {
            var path = $@"{SavingDirectory}\{replay.Date:yy.MM.dd}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += $@"\{replay.Id}.rply";
            if (File.Exists(path))
                File.Delete(path);
            return path;
        }

        public async Task<AdvancedReplay> LoadReplayAsync(Replay r)
        {
            AdvancedReplay replay = null;
            var replayPath = await GetReplayPath(r);
            await Task.Run(async () =>
            {
                if (replayPath is null)
                    throw new Exception($"Couldn't load Replay: {r.ID}");
                var path = replayPath;
                var compressedString = await File.ReadAllBytesAsync(path);
                replay = JsonConvert.DeserializeObject<AdvancedReplay>(Compressor.DecompressBytes(compressedString));
            });
            if (replay is null)
                throw new Exception("replay was null");
            return replay;
        }
        private async Task<string> GetReplayPath(Replay replay)
        {
            var directories = Directory.EnumerateDirectories(SavingDirectory);
            foreach (var d in directories)
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
