using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrimalEngineEditor.GameProject
{
    [DataContract(Name ="Game")]
    public class NewProjectClass2: ViewModelBase
    {
        
        public static string Extension { get; } = ".primal";
        [DataMember]
        public string Name { get; private set; }
        [DataMember]

        public string  Path { get; private set; }

        public string FullPath => $"{Path}{Name}{Extension}";
        [DataMember(Name ="Scenes")]
        private ObservableCollection<Scene> _scenes = new ObservableCollection<Scene>();

        public ReadOnlyObservableCollection<Scene> Scenes { get; }

        public NewProjectClass2(string name, string path)
        {
            Name = name;
            Path = path;
            _scenes.Add(new Scene(this, "Default Scene"));
        }
    }
}
