using RLStats_Classes.Models;
using RLStats_Classes.Models.Advanced;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses.CacheHandlers
{
    public class Database : WithIndexFile<Guid>
    {
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

        public int CacheSize { get; set; } = 10000;

        public int CacheHits { get; set; } = 0;

        public int CacheMisses { get; set; } = 0;

        public ObservableCollection<AdvancedReplay> ReplayCache { get; set; } = new();

        public Database() : base(compressed: true)
        {
            base.InitializeLate(Path.Combine(SavingDirectory.ToString(), @"index.dat"));
            ReplayCache.CollectionChanged += ReplayCache_CollectionChanged;
        }
        private void ReplayCache_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            while (ReplayCache.Count > CacheSize)
            {
                ReplayCache.RemoveAt(0);
            }
        }

        public bool IsReplayInDatabase(Replay replay)
        {
            return IndexCollection.Contains(Guid.Parse(replay.Id));
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
                lock (IndexCollection)
                {
                    if (IndexCollection.Contains(Guid.Parse(replay.Id)))
                        return;
                    var path = CreateReplayPath(replay);
                    var replayBatch = GetReplayBatch(path).GetAwaiter().GetResult();
                    if (replayBatch.Any(savedReplay => savedReplay.Id.Equals(replay.Id)))
                    {
                        IndexCollection.Add(Guid.Parse(replay.Id));
                        return;
                    }
                    replayBatch.Add(replay);
                    var bytes = Compressor.ConvertObject(replayBatch);
                    File.WriteAllBytesAsync(path, bytes).Wait();
                    IndexCollection.Add(Guid.Parse(replay.Id));
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
            var path = Path.Combine(SavingDirectory.ToString(), replay.Id.Substring(0, 1));
            Directory.CreateDirectory(path);
            path = Path.Combine(path, replay.Id.Substring(1, 1));
            return path;
        }

        public async Task<AdvancedReplay> LoadReplayAsync(string id, CancellationToken cancellationToken)
        {
            AdvancedReplay advancedReplay = null;
            if (cancellationToken.IsCancellationRequested)
                return null;
            lock (ReplayCache)
                foreach (var cacheEntry in ReplayCache)
                {
                    if (cacheEntry.Id.Equals(id))
                    {
                        CacheHits++;
                        _ = ReplayCache.Remove(cacheEntry);
                        return cacheEntry;
                    }
                }
            if (cancellationToken.IsCancellationRequested)
                return null;

            CacheMisses++;
            var replayPath = await GetReplayPath(id);
            if (replayPath is null)
            {
                lock (IndexCollection)
                    _ = IndexCollection.Remove(new Guid(id));
                return advancedReplay;
            }
            if (cancellationToken.IsCancellationRequested)
                return null;
            var replayBatch = await GetReplayBatch(replayPath);
            if (cancellationToken.IsCancellationRequested)
                return null;
            foreach (var replay in replayBatch)
            {
                if (replay.Id.Equals(id))
                    advancedReplay = replay;
            }
            if (advancedReplay is null)
                lock (IndexCollection)
                    IndexCollection.Remove(new Guid(id));
            else
                lock (ReplayCache)
                {
                    foreach (var replay in replayBatch)
                    {
                        if (!replay.Id.Equals(id))
                        {
                            ReplayCache.Add(replay);
                        }
                    }
                }
            return advancedReplay;
        }
        private async Task<string> GetReplayPath(string id)
        {
            var directories = Directory.EnumerateDirectories(SavingDirectory.FullName);
            foreach (var d in directories)
            {
                if (new DirectoryInfo(d).Name.Equals(id.Substring(0, 1)))
                {
                    var filenames = Directory.EnumerateFiles(d);
                    return await Task.Run(() =>
                    {
                        foreach (var s in filenames)
                            if (new FileInfo(s).Name.StartsWith(id.Substring(1, 1)))
                                return s;
                        return null;
                    });
                }
            }
            return null;
        }
    }
}
