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
    enum BuildConfiguraiton
    {
        Debug,
        DebugEditor,
        Release,
        ReleaseEditor,
    }
    [DataContract(Name = "Game")]
    class NewProjectClass2 : ViewModelBase
    {

        public static string Extension { get; } = ".primal";
        [DataMember]
        public string Name { get; private set; } = "New Project";
        [DataMember]

        public string Path { get; private set; }

        public string FullPath => $@"{Path}{Name}{Extension}";
        public string Solution => $@"{Path}{Name}.sln";

        private static readonly string[] _buildConfigurationNames = new string[] { "Debug", "DebugEditor", "Release", "ReleaseEditor" };

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
        public BuildConfiguraiton DllBuildConfig => BuildConfig == 0 ? BuildConfiguraiton.DebugEditor : BuildConfiguraiton.ReleaseEditor;

        private string[] _availableScripts;
        public string[] AvailableScripts
        {
            get => _availableScripts;
            set
            {
                if (_availableScripts != value)
                {
                    _availableScripts = value;
                    OnPropertyChanged(nameof(AvailableScripts));
                }
            }
        }

        [DataMember(Name = "Scenes")]
        private ObservableCollection<Scene> _scenes = new ObservableCollection<Scene>();

        public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }

        public static NewProjectClass2 Current => Application.Current.MainWindow.DataContext as NewProjectClass2;
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
        public ICommand BuildCommand { get; private set; }

        private void SetCommands()
        {
            AddNewSceneCommand = new RelayCommand<object>(x =>
            {
                AddNewSceneInternal($" New Scene{_scenes.Count}");
                var newScene = _scenes.Last();
                var IndexScene = _scenes.Count - 1;

                UndoRedo.Add(new UndoRedoActions(
                    () => RemoveSceneInternal(newScene),
                   () => _scenes.Insert(IndexScene, newScene),
                   $"Add{newScene.Name}"
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
            BuildCommand = new RelayCommand<bool>(async x => await BuildGameCodeDll(x), x => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);

            OnPropertyChanged(nameof(AddNewSceneCommand));
            OnPropertyChanged(nameof(RemoveSceneCommand));
            OnPropertyChanged(nameof(UndoCommand));
            OnPropertyChanged(nameof(RedoCommand));
            OnPropertyChanged(nameof(SaveCommand));
            OnPropertyChanged(nameof(BuildCommand));
        }

        private static string GetConfigurationName(BuildConfiguraiton config) => _buildConfigurationNames[(int)config];

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

        public void Unload()
        {
            VisualStudio.CloseVS();
            UndoRedo.Reset();
        }
        public static void Save(NewProjectClass2 project)
        {
            Serializer.ToFile(project, project.FullPath);
            Logger.Log(MessageType.Info, $"Project saved to {project.FullPath}");
        }

        public static NewProjectClass2 Load(string file)
        {
            Debug.Assert(File.Exists(file));
            return Serializer.FromFile<NewProjectClass2>(file);
        }

        private async Task BuildGameCodeDll(bool showWindow = true)
        {
            try
            {
                UnloadGameCodeDll();
                await Task.Run( () => VisualStudio.BuildSolution(this, GetConfigurationName(DllBuildConfig), showWindow));
                if (VisualStudio.BuildSucceeded)
                {
                    LoadGameCodeDll();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private void LoadGameCodeDll()
        {
            var configName = GetConfigurationName(DllBuildConfig);
            var projectPath = this.Path; // Senin önceden tanımladığın 'Path' değişkeni buysa kullan
            var name = this.Name;

            var sourceDll = $@"{projectPath}x64\{configName}\{name}.dll";
            var tempDll = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", $"{name}_temp.dll");

            AvailableScripts = null;

            if (File.Exists(sourceDll))
            {
                var tempDir = System.IO.Path.GetDirectoryName(tempDll);
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }

                File.Copy(sourceDll, tempDll, overwrite: true);
            }

            if (File.Exists(tempDll) && AgilisAPI.LoadGameCodeDll(tempDll) != 0)
            {
                AvailableScripts = AgilisAPI.GetScriptNames();
                Logger.Log(MessageType.Info, "Game code DLL loaded successfully.");
            }
            else
            {
                Logger.Log(MessageType.Warning, "Failed to load game code DLL file. Try to build the project first.");
            }
        }



        private void UnloadGameCodeDll()
        {
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
            ActiveScene = Scenes.FirstOrDefault(x => x.IsActive);

            await BuildGameCodeDll(false);
            SetCommands();


        }
        public NewProjectClass2(string name, string path)
        {
            Name = name;
            Path = path;
            OnDeserialized(new StreamingContext());
        }
    }
}
