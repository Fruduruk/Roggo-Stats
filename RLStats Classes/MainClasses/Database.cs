using RLStats_Classes.AdvancedModels;
using RLStats_Classes.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses
{
    public class Database
    {
        private ObservableCollection<Guid> IdCollection { get; } = new ObservableCollection<Guid>();

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
                if (!indexFile.Exists)
                    indexFile.Create().Dispose();
                return indexFile;
            }
            private set => indexFile = value;
        }

        private FileInfo indexFile;
        public Database()
        {
            IndexFile = new FileInfo(SavingDirectory + @"\index.dat");
            IdCollection = LoadIdListFromFile();
            IdCollection.CollectionChanged += IdList_CollectionChanged;
        }

        private void IdList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => SaveIdsInIndexFile(IdCollection);

        public bool IsReplayInDatabase(Replay replay)
        {
            return IdCollection.Contains(Guid.Parse(replay.ID));
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
            var replayBatch = await GetReplayBatch(path);
            replayBatch.Add(replay);
            var compressedString = Compressor.ConvertObject(replayBatch);
            await File.WriteAllBytesAsync(path, compressedString);
            IdCollection.Add(Guid.Parse(replay.Id));
        }

        private async Task<List<AdvancedReplay>> GetReplayBatch(string path)
        {
            if (!File.Exists(path))
                return new List<AdvancedReplay>();
            var bytes = await File.ReadAllBytesAsync(path);
            List<AdvancedReplay> replayBatch;
            try
            {
                replayBatch = Compressor.ConvertObject<List<AdvancedReplay>>(bytes);
            }
            catch
            {
                replayBatch = new List<AdvancedReplay>();
            }
            return replayBatch;

        }

        private string CreateReplayPath(AdvancedReplay replay)
        {
            var path = $@"{SavingDirectory}\{replay.Id.Substring(0, 1)}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += $@"\{replay.Id.Substring(1, 1)}.rply";
            return path;
        }

        public async Task<AdvancedReplay> LoadReplayAsync(Replay r)
        {
            AdvancedReplay advancedReplay = null;
            var replayPath = await GetReplayPath(r);
            if (replayPath is null)
                throw new Exception($"Couldn't load Replay: {r.ID}");
            var replayBatch = await GetReplayBatch(replayPath);
            foreach (var replay in replayBatch)
            {
                if (replay.Id.Equals(r.ID))
                    advancedReplay = replay;
            }
            if (advancedReplay is null)
                throw new Exception("replay was null");
            return advancedReplay;
        }
        private async Task<string> GetReplayPath(Replay replay)
        {
            var directories = Directory.EnumerateDirectories(SavingDirectory.FullName);
            foreach (var d in directories)
            {
                if (new DirectoryInfo(d).Name.Equals(replay.ID.Substring(0, 1)))
                {
                    var filenames = Directory.EnumerateFiles(d);
                    return await Task.Run(() =>
                    {
                        foreach (var s in filenames)
                            if (new FileInfo(s).Name.StartsWith(replay.ID.Substring(1, 1)))
                                return s;
                        return null;
                    });
                }
            }
            return null;
        }

        private void SaveIdsInIndexFile(IEnumerable<Guid> ids)
        {
            var bytes = Compressor.ConvertObject(ids.ToList());
            File.WriteAllBytes(IndexFile.FullName, bytes);
        }

        private ObservableCollection<Guid> LoadIdListFromFile()
        {
            var bytes = File.ReadAllBytes(IndexFile.FullName);
            try
            {
                var idList = Compressor.ConvertObject<List<Guid>>(bytes);
                return new ObservableCollection<Guid>(idList);
            }
            catch
            {
                return new ObservableCollection<Guid>();
            }

        }

    }
}
