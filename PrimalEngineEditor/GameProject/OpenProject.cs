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

namespace PrimalEngineEditor.GameProject
{
    [DataContract]
    public class ProjectDataSave
    {
        [DataMember]
        public string ProjectName { get; set; }
        [DataMember]
        public string ProjectPath { get; set; }
        [DataMember]
        public DateTime Date { get; set; }

        public string FullPath { get => $"{ProjectPath}{ProjectName}{NewProjectClass2.Extension}"; }

        public byte[] Icon { get; set; }
        public byte[] Screenshot { get; set; }

    }
    [DataContract]
    public class ProjectDataSaveList
    {
        [DataMember]
        public List<ProjectDataSave> Projects { get; set; }
    }
    class OpenProject
    {
        private static readonly string _applicationDataPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\PrimalEditor\";
        private static readonly string _ProjectDataPath;
        private static readonly ObservableCollection<ProjectDataSave> _projects = new ObservableCollection<ProjectDataSave>();
        public static ReadOnlyObservableCollection<ProjectDataSave> Projects { get; }

      
        private static void ReadProjectDataSave()
        {
            if (File.Exists(_ProjectDataPath))
            {
                var projects = Serializer.FromFile<ProjectDataSaveList>(_ProjectDataPath).Projects.OrderByDescending(x => x.Date);
                _projects.Clear();
                foreach (var project in projects)
                {
                    if (File.Exists(project.FullPath))
                    {
                        project.Icon = File.ReadAllBytes($@"{project.ProjectPath}\.Primal\Icon.png");
                        project.Screenshot = File.ReadAllBytes($@"{project.ProjectPath}\.Primal\Screenshot.png");
                        _projects.Add(project);
                    }
                }
            }

        }
        private static void WriteProjectDataSave()
        {
            var projects = _projects.OrderBy(x => x.Date).ToList();
            Serializer.ToFile(new ProjectDataSaveList() {Projects=projects},_ProjectDataPath);
        }

        public NewProjectClass2  Open(ProjectDataSave data)
        {
            ReadProjectDataSave();
            var project = _projects.FirstOrDefault(x => x.FullPath == data.FullPath);
            if (project != null)
            {
                project.Date = DateTime.Now;


            }
            else
            {
                project = data;
                project.Date = DateTime.Now;
                _projects.Add(project);
            }
            WriteProjectDataSave();


            return null;
             
            
        }


        static OpenProject()
        {
            try
            {
                if (Directory.Exists(_applicationDataPath)) Directory.CreateDirectory(_ProjectDataPath);
                _ProjectDataPath = $@"{_applicationDataPath}ProjectDataSave.xml";
                Projects = new ReadOnlyObservableCollection<ProjectDataSave>(_projects);
                ReadProjectDataSave();
            }
            catch (Exception exception)
            {

                Debug.WriteLine(exception.Message);
            }

        }
    }
}
