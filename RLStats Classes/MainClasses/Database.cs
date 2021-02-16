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
        public DirectoryInfo SavingDirectory
        {
            get
            {
                if (!savingDirectory.Exists)
                   savingDirectory.Create();
                return savingDirectory;
            }
            private set => savingDirectory = value;
        }

        private DirectoryInfo savingDirectory = new DirectoryInfo(RLConstants.RLStatsFolder + @"\Data");

        public FileInfo IndexFile
        {
            get
            {
                if(!indexFile.Exists)
                    indexFile.Create().Close();
                return indexFile;
            }
            private set => indexFile = value;
        }

        private FileInfo indexFile;
        public Database()
        {
            IndexFile = new FileInfo(SavingDirectory+ @"\index.dat");
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
            var compressedString = Compressor.ConvertObject(replay);
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
                var compressedBytes = await File.ReadAllBytesAsync(path);
                replay = Compressor.ConvertObject<AdvancedReplay>(compressedBytes);
            });
            if (replay is null)
                throw new Exception("replay was null");
            return replay;
        }
        private async Task<string> GetReplayPath(Replay replay)
        {
            var directories = Directory.EnumerateDirectories(SavingDirectory.FullName);
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
