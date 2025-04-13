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
    [DataContract]
    public class NewProjectClass2: ViewModelBase
    {
        public static string Extension { get; } = ".primal";
        public string Name { get; private set; }
       
        public string  Path { get; private set; }

        public string FullPath => $"{Path}{Name}{Extension}";
        private ObservableCollection<Scene> _scenes = new ObservableCollection<Scene>();

        public ReadOnlyObservableCollection<Scene> Scenes { get; }

    }
}
