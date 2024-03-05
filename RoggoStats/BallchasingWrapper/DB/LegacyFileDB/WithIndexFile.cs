using System.Collections.ObjectModel;

namespace BallchasingWrapper.DB.LegacyFileDB
{
    public class WithIndexFile<T>
    {
        private bool _isInitialized = false;
        private bool IndexFileCompression { get; set; }
        protected ObservableCollection<T> IndexCollection { get; private set; } = new();
        private FileInfo IndexFile
        {
            get
            {
                if (!_indexFile.Exists)
                    _indexFile.Create().Dispose();
                return _indexFile;
            }
            set => _indexFile = value;
        }

        private FileInfo _indexFile;
        public WithIndexFile(bool compressed, string filePath = null)
        {
            IndexFileCompression = compressed;
            if (filePath is not null)
                InitializeLate(filePath);
        }

        protected void InitializeLate(string filePath)
        {
            if (!_isInitialized)
            {
                IndexFile = new FileInfo(filePath);
                IndexCollection = LoadIndexFile();
                IndexCollection.CollectionChanged += IdList_CollectionChanged;
                _isInitialized = true;
            }
        }

        private void IdList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => SaveInIndexFile(IndexCollection);

        protected void SaveInIndexFile(IEnumerable<T> ids)
        {
            if (IndexFileCompression)
            {
                var bytes = Compressor.ConvertObject(ids.ToList());
                File.WriteAllBytes(IndexFile.FullName, bytes);
            }
            else
            {
                var text = JsonConvert.SerializeObject(ids.ToList());
                File.WriteAllText(IndexFile.FullName, text);
            }
        }

        private ObservableCollection<T> LoadIndexFile()
        {
            if (IndexFileCompression)
            {
                var bytes = File.ReadAllBytes(IndexFile.FullName);
                try
                {
                    var idList = Compressor.ConvertObject<List<T>>(bytes);
                    return new ObservableCollection<T>(idList);
                }
                catch
                {
                    return new ObservableCollection<T>();
                }
            }
            else
            {
                var text = File.ReadAllText(IndexFile.FullName);
                try
                {
                    var idList = JsonConvert.DeserializeObject<List<T>>(text);
                    return new ObservableCollection<T>(idList);
                }
                catch
                {
                    return new ObservableCollection<T>();
                }
            }
        }
    }
}