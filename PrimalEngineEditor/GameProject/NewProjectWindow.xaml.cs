using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PrimalEngineEditor.GameProject
{
    /// <summary>
    /// Interaction logic for NewProjectWindow.xaml
    /// </summary>
    public partial class NewProjectWindow : UserControl
    {
        public NewProjectWindow()
        {
            InitializeComponent();

        }
        private void OnCreate_Button_Click(object sender,RoutedEventArgs e)
        {
            var cp = DataContext as NewProjectClass1;
            var projectPath = cp.CreateProject(templateListBox.SelectedItem as ProjectTemplate);
            bool dialogResult = false;
            var window = Window.GetWindow(this);
            
           

            if (!string.IsNullOrEmpty(projectPath))
            {
                dialogResult = true;
                var project = OpenProject.Open(new ProjectDataSave() { ProjectName = cp.ProjectName, ProjectPath = projectPath });
                window.DataContext = project;
            }
            window.DialogResult = dialogResult;
            window.Close();
        }
    }
}
