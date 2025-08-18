using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PrimalEngineEditor.Content
{
    static class AssetRegistry
    {
        private static readonly Dictionary<string, AssetInfo> _assetDictionary = new Dictionary<string, AssetInfo>();
        private static readonly ObservableCollection<AssetInfo> _assets = new ObservableCollection<AssetInfo>();
        private static readonly FileSystemWatcher _contentWatcher = new FileSystemWatcher()
        {
            IncludeSubdirectories = true,
            Filter = "",
            
            NotifyFilter = NotifyFilters.CreationTime |
                           NotifyFilters.DirectoryName|
                           NotifyFilters.FileName|
                           NotifyFilters.LastWrite
        };
        public static ReadOnlyObservableCollection<AssetInfo> Assets { get; } = new ReadOnlyObservableCollection<AssetInfo>(_assets);

        private static void RegisterAllAssets(string path)
        {
            Debug.Assert(Directory.Exists(path));
            foreach (var entry in Directory.GetFileSystemEntries(path))
            {
                if(ContentHelper.IsDirectory(entry))
                {
                    RegisterAllAssets(entry);
                }
                else
                {
                    RegisterAsset(entry);

                }
            }
        }

        private static void RegisterAsset(string file)
        {
            Debug.Assert(File.Exists(file));
            try
            {
                var fileInfo = new FileInfo(file);
                if(!_assetDictionary.ContainsKey(file) || _assetDictionary[file].ImportDate.IsOlder(fileInfo.LastWriteTime))
                {
                    var info = Asset.GetAssetInfo(file);
                    Debug.Assert(info != null);
                    _assetDictionary[file] = info;
                }
                Debug.Assert(_assetDictionary.ContainsKey(file));
                _assets.Add(_assetDictionary[file]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private static void OnContentModified(object sender, FileSystemEventArgs e)
        {
            
        }
        public static void Clear()
        {
            _contentWatcher.EnableRaisingEvents = false;
            _assetDictionary.Clear();
            _assets.Clear();
        }
        public static void Reset(string contentFolder)
        {
            Clear();
            Debug.Assert(Directory.Exists(contentFolder));
            RegisterAllAssets(contentFolder);
            _contentWatcher.Path = contentFolder;
            _contentWatcher.EnableRaisingEvents = true;
        }
        static AssetRegistry()
        {
            _contentWatcher.Changed += OnContentModified;
            _contentWatcher.Created += OnContentModified;
            _contentWatcher.Deleted += OnContentModified;
            _contentWatcher.Renamed += OnContentModified;
        }
    }
}
