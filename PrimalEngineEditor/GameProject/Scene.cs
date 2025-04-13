using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalEngineEditor.GameProject
{
     public class Scene : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
    }

    public NewProjectClass2 Project { get; private set; }





}
