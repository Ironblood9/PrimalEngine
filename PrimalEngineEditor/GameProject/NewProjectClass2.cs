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


namespace PrimalEngineEditor.GameProject
{
    [DataContract(Name ="Game")]
    public class NewProjectClass2: ViewModelBase
    {
        
        public static string Extension { get; } = ".primal";
        [DataMember]
        public string Name { get; private set; } = "New Project";
        [DataMember]

        public string  Path { get; private set; }

        public string FullPath => $"{Path}{Name}{Extension}";
        [DataMember(Name ="Scenes")]
        private ObservableCollection<Scene> _scenes = new ObservableCollection<Scene>();

        public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }

        public static NewProjectClass2 Current => Application.Current.MainWindow.DataContext as NewProjectClass2;

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
        public void Unload()
        {

        }
        public static void Save(NewProjectClass2 project)
        {
            Serializer.ToFile(project, project.FullPath);
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
        }
        public NewProjectClass2(string name, string path)
        {


            Name = name;
            Path = path;
            OnDeserialized(new StreamingContext());
        }
    }
}
