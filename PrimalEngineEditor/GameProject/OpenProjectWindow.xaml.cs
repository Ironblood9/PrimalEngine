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
    /// Interaction logic for OpenProjectWindow.xaml
    /// </summary>
    public partial class OpenProjectWindow : UserControl
    {
        public OpenProjectWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                var item = projectsListBox.ItemContainerGenerator.ContainerFromIndex(projectsListBox.SelectedIndex) as ListBoxItem;
                item?.Focus();
            };
        }

        private void OpenProjectWindow_Loaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void projectsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OnOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedProject();
        }
        private void OnListBoxItem_Mouse_DoubleClick(object sender, RoutedEventArgs e)
        {
            OpenSelectedProject();
        }
        private void OpenSelectedProject()
        {

        
   
        var project = OpenProject.Open(projectsListBox.SelectedItem as ProjectDataSave);
        bool dialogResult = false;
        var window = Window.GetWindow(this);
            
           

            if (project !=null)
            {
                dialogResult = true;
                window.DataContext = project;

            }
            window.DialogResult = dialogResult;
            window.Close();
        }
    }
}
