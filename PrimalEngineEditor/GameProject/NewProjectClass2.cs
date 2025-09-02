using PrimalEngineEditor.Components;
using PrimalEngineEditor.DllWrappers;
using PrimalEngineEditor.GameDev;
using PrimalEngineEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Formats.Asn1.AsnWriter;


namespace PrimalEngineEditor.GameProject
{ 
    [DataContract(Name = "Game")]
    class NewProjectClass2 : ViewModelBase
    {

        public static string Extension => ".primal";
        [DataMember]
        public string Name { get; private set; } = "New Project";
        [DataMember]

        public string Path { get; private set; }

        public string FullPath => $@"{Path}{Name}{Extension}";
        public string Solution => $@"{Path}{Name}.sln";

        public string ContentPath => $@"{Path}Content\";
        public string TempFolder => $@"{Path}.Primal\Temp\";


        private int _buildConfig;
        [DataMember]
        public int BuildConfig
        {
            get => _buildConfig;
            set
            {
                if (_buildConfig != value)
                {
                    _buildConfig = value;
                    OnPropertyChanged(nameof(BuildConfig));
                }
            }
        }

        public BuildConfiguraiton StandAloneBuildConfig => BuildConfig == 0 ? BuildConfiguraiton.Debug : BuildConfiguraiton.Release;
        public BuildConfiguraiton DLLBuildConfig => BuildConfig == 0 ? BuildConfiguraiton.DebugEditor : BuildConfiguraiton.ReleaseEditor;

        private string[] _availableScripts;
        public string[] AvailableScripts
        {
            get => _availableScripts;
            private set
            {
                if (_availableScripts != value)
                {
                    _availableScripts = value;
                    OnPropertyChanged(nameof(AvailableScripts));
                }
            }
        }

        [DataMember(Name =nameof(Scenes))]
        private readonly ObservableCollection<Scene> _scenes = new ObservableCollection<Scene>();

        public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }

        public static NewProjectClass2 Current => Application.Current.MainWindow?.DataContext as NewProjectClass2;
        public static UndoRedo UndoRedo { get; } = new UndoRedo();

        private Scene _activeScene;

        public Scene ActiveScene
        {
            get => _activeScene;
            set
            {
                if (_activeScene != value)
                {
                    _activeScene = value;
                    OnPropertyChanged(nameof(ActiveScene));
                }
            }
        }

        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand AddNewSceneCommand { get; private set; }
        public ICommand RemoveSceneCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand DebugStartCommand { get; private set; }
        public ICommand DebugStartWithoutDebuggingCommand { get; private set; }
        public ICommand DebugStopCommand { get; private set; }
        public ICommand BuildCommand { get; private set; }

        private void SetCommands()
        {
            AddNewSceneCommand = new RelayCommand<object>(x =>
            {
                AddNewSceneInternal($"New Scene{_scenes.Count}");
                var newScene = _scenes.Last();
                var IndexScene = _scenes.Count - 1;

                UndoRedo.Add(new UndoRedoActions(
                    () => RemoveSceneInternal(newScene),
                   () => _scenes.Insert(IndexScene, newScene),
                   $"Add {newScene.Name}"
                   ));
            });



            RemoveSceneCommand = new RelayCommand<Scene>(x =>
            {
                var IndexScene = _scenes.IndexOf(x);
                RemoveSceneInternal(x);
                UndoRedo.Add(new UndoRedoActions(
                   () => _scenes.Insert(IndexScene, x),
                   () => RemoveSceneInternal(x),
                   $"Remove {x.Name}"));

            }, x => !x.IsActive
            );

            UndoCommand = new RelayCommand<object>(x => UndoRedo.Undo(), x => UndoRedo.UndoList.Any());
            RedoCommand = new RelayCommand<object>(x => UndoRedo.Redo(), x => UndoRedo.RedoList.Any());
            SaveCommand = new RelayCommand<object>(x => Save(this));
            DebugStartCommand = new RelayCommand<object>(async x => await RunGame(true), x => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);
            DebugStartWithoutDebuggingCommand = new RelayCommand<object>(async x => await RunGame(false), x => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);
            DebugStopCommand = new RelayCommand<object>(async x => await StopGame(), x => VisualStudio.IsDebugging());
            BuildCommand = new RelayCommand<bool>(async x => await BuildGameCodeDLL(x), x => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);

            OnPropertyChanged(nameof(AddNewSceneCommand));
            OnPropertyChanged(nameof(RemoveSceneCommand));
            OnPropertyChanged(nameof(UndoCommand));
            OnPropertyChanged(nameof(RedoCommand));
            OnPropertyChanged(nameof(SaveCommand));
            OnPropertyChanged(nameof(DebugStartCommand));
            OnPropertyChanged(nameof(DebugStartWithoutDebuggingCommand));
            OnPropertyChanged(nameof(DebugStopCommand));
            OnPropertyChanged(nameof(BuildCommand));
        }


        private void AddNewSceneInternal(string sceneName)
        {
            Debug.Assert(!string.IsNullOrEmpty(sceneName.Trim()));
            _scenes.Add(new Scene(this, sceneName));
        }


        private void RemoveSceneInternal(Scene scene)
        {
            Debug.Assert(_scenes.Contains(scene));
            _scenes.Remove(scene);


        }
        private int _sceneCounter = 1;

        public string GetNextSceneName()
        {
            return $"Scene {_sceneCounter++}";
        }

        public static NewProjectClass2 Load(string file)
        {
            Debug.Assert(File.Exists(file));
            return Serializer.FromFile<NewProjectClass2>(file);
        }
        public void Unload()
        {
            UnloadGameCodeDLL();
            VisualStudio.CloseVS();
            UndoRedo.Reset();
            Logger.Clear();
            DeleteTempFolder();
        }

        private void DeleteTempFolder()
        {
            if(Directory.Exists(TempFolder))
            {
                Directory.Delete(TempFolder, true);
            }
        }

        private static void Save(NewProjectClass2 project)
        {
            Serializer.ToFile(project, project.FullPath);
            Logger.Log(MessageType.Info, $"Project saved to {project.FullPath}");
        }

        private void SaveToBinary()
        {
            var configName = VisualStudio.GetConfigurationName(StandAloneBuildConfig);
            var bin = $@"{Path}x64\{configName}\game.bin";

            using(var bw = new BinaryWriter(File.Open(bin, FileMode.Create, FileAccess.Write)))
            {
                bw.Write(ActiveScene.GameEntities.Count);
                foreach (var entity in ActiveScene.GameEntities)
                {
                    bw.Write(0);//entity type
                    bw.Write(entity.Components.Count);
                    foreach (var component in entity.Components)
                    {
                        bw.Write((int)component.ToEnumType());
                        component.WriteToBinary(bw);
                    }
                }
            }
        }

        private async Task RunGame(bool debug)
        {
            await Task.Run(() => VisualStudio.BuildSolution(this, StandAloneBuildConfig, debug));
            if(VisualStudio.BuildSucceeded)
            {
                SaveToBinary();
                await Task.Run(() => VisualStudio.Run(this, StandAloneBuildConfig, debug));
            }
        }

        private async Task StopGame() => await Task.Run(() => VisualStudio.Stop());

        private async Task BuildGameCodeDLL(bool showWindow = true)
        {
            try
            {
                UnloadGameCodeDLL();
                await Task.Run( () => VisualStudio.BuildSolution(this, DLLBuildConfig, showWindow));
                if (VisualStudio.BuildSucceeded)
                {
                    LoadGameCodeDLL();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private void LoadGameCodeDLL()
        {
            var configName = VisualStudio.GetConfigurationName(DLLBuildConfig);
            var dll = $@"{Path}x64\{configName}\{Name}.dll";
            AvailableScripts = null;
            if (File.Exists(dll) && AgilisAPI.LoadGameCodeDll(dll) != 0)
            {
                AvailableScripts = AgilisAPI.GetScriptNames();
                ActiveScene.GameEntities.Where(x => x.GetComponent<Script>() != null).ToList().ForEach(x => x.IsActive = true);
                Logger.Log(MessageType.Info, "Game code Dll loaded successfully.");
            }
            else
            {
                Logger.Log(MessageType.Warning, "Failed to load game code Dll file. Try to build the project first.");
            }
        }



        private void UnloadGameCodeDLL()
        {
            ActiveScene.GameEntities.Where(x => x.GetComponent<Script>() != null).ToList().ForEach(x => x.IsActive = false);
            if(AgilisAPI.UnloadGameCodeDll() !=0)
            {
                Logger.Log(MessageType.Info, "Game code Dll unloaded");
                AvailableScripts = null;
            }
        }

        [OnDeserialized]
        private async void OnDeserialized(StreamingContext context)
        {
            if (_scenes != null)
            {
                Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);
                OnPropertyChanged(nameof(Scenes));
            }
            ActiveScene = _scenes.FirstOrDefault(x => x.IsActive);
            Debug.Assert(ActiveScene != null);

            await BuildGameCodeDLL(false);
            SetCommands();


        }
        public NewProjectClass2(string name, string path)
        {
            Name = name;
            Path = path;
            Debug.Assert(File.Exists((Path + Name + Extension).ToLower()));
            OnDeserialized(new StreamingContext());
        }
    }
}
