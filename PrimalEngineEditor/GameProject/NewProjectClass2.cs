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
    [DataContract(Name ="Game")]
     class NewProjectClass2: ViewModelBase
    {
        
        public static string Extension { get; } = ".primal";
        [DataMember]
        public string Name { get; private set; } = "New Project";
        [DataMember]

        public string  Path { get; private set; }

        public string FullPath => $@"{Path}{Name}\{Name}{Extension}";
        public string Solution => $@"{Path}{Name}.sln";
        [DataMember(Name ="Scenes")]
        private ObservableCollection<Scene> _scenes = new ObservableCollection<Scene>();

        public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }

        public static NewProjectClass2 Current => Application.Current.MainWindow.DataContext as NewProjectClass2;
        public  static UndoRedo UndoRedo { get; } = new UndoRedo();

        private Scene _activeScene;
      
        public Scene ActiveScene
        {
            get => _activeScene;
            set
            {
                if (_activeScene!=value)
                {
                    _activeScene = value;
                    OnPropertyChanged(nameof(ActiveScene));
                }
            }
        }
        public static  NewProjectClass2 Load(string file)
        {
            Debug.Assert(File.Exists(file));
            return Serializer.FromFile<NewProjectClass2>(file);
        }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand AddNewSceneCommand { get; private set; }
        public ICommand RemoveSceneCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

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
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_scenes != null)
            {
                Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);
                OnPropertyChanged(nameof(Scenes));
            }
            ActiveScene = Scenes.FirstOrDefault(x => x.IsActive);

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

            }, x=> !x.IsActive
            );

            UndoCommand = new RelayCommand<object>(x => UndoRedo.Undo());
            RedoCommand = new RelayCommand<object>(x => UndoRedo.Redo());
            SaveCommand = new RelayCommand<object>(x => Save(this));

        }
        public NewProjectClass2(string name, string path)
        { 
            Name = name;
            Path = path;
            OnDeserialized(new StreamingContext());
        }
    }
}
