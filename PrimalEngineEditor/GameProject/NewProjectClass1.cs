using PrimalEngineEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;


namespace PrimalEngineEditor.GameProject
{
    [DataContract]
    public class ProjectTemplate
    {
        [DataMember]
        public string ProjectType { get; set; }
        [DataMember]
        public string ProjectFile { get; set; }
        [DataMember]
        public List<string> Folders { get; set; }

        public byte[] Icon { get; set; }
        public byte[] Screenshot { get; set; }
        public string IconPath { get; set; }
        public string ScreenshotPath { get; set; }
        public string ProjectFilePath { get; set; }

    }
    class NewProjectClass1 : ViewModelBase
    {
        private const string V = "C:/Users/Msı/source/repos/PrimalEngine/PrimalEngineEditor/ProjectTemplates";
        private readonly string _templatePath = V;
        private string _projectName = "NewProject";
        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName != value)
                {
                    _projectName = value;
                    OnPropertyChanged(nameof(ProjectName));
                }
            }
        }
        private string _projectPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\PrimalProject\";
        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                if (_projectPath != value)
                {
                    _projectPath = value;
                    OnPropertyChanged(nameof(ProjectPath));
                }
            }
        }

        private ObservableCollection<ProjectTemplate> _projectTemplates = new ObservableCollection<ProjectTemplate>();
        public ReadOnlyObservableCollection<ProjectTemplate> ProjectTemplates { get; }


        public NewProjectClass1()
        {
            ProjectTemplates = new ReadOnlyObservableCollection<ProjectTemplate>(_projectTemplates);
            try
            {
                
                var templatesFiles = Directory.GetFiles(_templatePath, "template.txt", SearchOption.AllDirectories);
                Debug.Assert(templatesFiles.Any());
                foreach (var file in templatesFiles)
                {
                   var template= Serializer.FromFile<ProjectTemplate>(file);
                    _projectTemplates.Add(template);
                    template.IconPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), "Icon.png"));
                    template.Icon = File.ReadAllBytes(template.IconPath);
                    template.ScreenshotPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), "Screenshot.png"));
                    template.Screenshot = File.ReadAllBytes(template.ScreenshotPath);
                    template.ProjectFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file),template.ProjectFile));
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
