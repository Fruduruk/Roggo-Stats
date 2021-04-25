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
                if (!_savingDirectory.Exists)
                    _savingDirectory.Create();
                return _savingDirectory;
            }
            private set => _savingDirectory = value;
        }

        private DirectoryInfo _savingDirectory = new DirectoryInfo(RLConstants.RLStatsFolder + @"\Data");

        public FileInfo IndexFile
        {
            get
            {
                if (!_indexFile.Exists)
                    _indexFile.Create().Dispose();
                return _indexFile;
            }
            private set => _indexFile = value;
        }

        private FileInfo _indexFile;

        public int CacheSize { get; set; }= 4096;

        public int CacheHits { get; set; } = 0;

        public int CacheMisses { get; set; } = 0;

        public ObservableCollection<AdvancedReplay> ReplayCache { get; set; } = new();

        public Database()
        {
            IndexFile = new FileInfo(SavingDirectory + @"\index.dat");
            IdCollection = LoadIdListFromFile();
            IdCollection.CollectionChanged += IdList_CollectionChanged;
            ReplayCache.CollectionChanged += ReplayCache_CollectionChanged;
        }

        private void ReplayCache_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            while (ReplayCache.Count > CacheSize)
            {
                ReplayCache.RemoveAt(0);
            }
        }

        private void IdList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => SaveIdsInIndexFile(IdCollection);

        public bool IsReplayInDatabase(Replay replay)
        {
            return IdCollection.Contains(Guid.Parse(replay.Id));
        }
        public void SaveReplayList(List<AdvancedReplay> replays)
        {
            foreach (var replay in replays)
            {
                SaveReplayAsync(replay);
            }
        }
        public async void SaveReplayAsync(AdvancedReplay replay)
        {
            await Task.Run(() =>
            {
                lock (IdCollection)
                {
                    if (IdCollection.Contains(Guid.Parse(replay.Id)))
                        return;
                    var path = CreateReplayPath(replay);
                    var replayBatch = GetReplayBatch(path).GetAwaiter().GetResult();
                    if (replayBatch.Any(savedReplay => savedReplay.Id.Equals(replay.Id)))
                    {
                        IdCollection.Add(Guid.Parse(replay.Id));
                        return;
                    }
                    replayBatch.Add(replay);
                    var bytes = Compressor.ConvertObject(replayBatch);
                    File.WriteAllBytesAsync(path, bytes).Wait();
                    IdCollection.Add(Guid.Parse(replay.Id));
                }
            });
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
            lock (ReplayCache)
                foreach (var aReplay in ReplayCache)
                {
                    if (aReplay.Id.Equals(r.Id))
                    {
                        CacheHits++;
                        return aReplay;
                    }
                }
            CacheMisses++;
            var replayPath = await GetReplayPath(r);
            if (replayPath is null)
            {
                lock (IdCollection)
                    IdCollection.Remove(new Guid(r.Id));
                return advancedReplay;
            }
            var replayBatch = await GetReplayBatch(replayPath);
            foreach (var replay in replayBatch)
            {
                if (replay.Id.Equals(r.Id))
                    advancedReplay = replay;
            }
            if (advancedReplay is null)
                lock (IdCollection)
                    IdCollection.Remove(new Guid(r.Id));
            else
                foreach (var replay in replayBatch)
                {
                    lock (ReplayCache)
                    {
                        var alreadyIn = ReplayCache.Any(cacheEntry => replay.Id.Equals(cacheEntry.Id));
                        if (!alreadyIn)
                            ReplayCache.Add(replay);
                    }
                }
            return advancedReplay;
        }
        private async Task<string> GetReplayPath(Replay replay)
        {
            var directories = Directory.EnumerateDirectories(SavingDirectory.FullName);
            foreach (var d in directories)
            {
                if (new DirectoryInfo(d).Name.Equals(replay.Id.Substring(0, 1)))
                {
                    var filenames = Directory.EnumerateFiles(d);
                    return await Task.Run(() =>
                    {
                        foreach (var s in filenames)
                            if (new FileInfo(s).Name.StartsWith(replay.Id.Substring(1, 1)))
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
