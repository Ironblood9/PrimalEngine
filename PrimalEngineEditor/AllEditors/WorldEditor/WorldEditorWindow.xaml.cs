using PrimalEngineEditor.Content;
using PrimalEngineEditor.GameDev;
using PrimalEngineEditor.GameProject;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace PrimalEngineEditor.AllEditors
{
    /// <summary>
    /// Interaction logic for WorldEditorWindow.xaml
    /// </summary>
    public partial class WorldEditorWindow : UserControl
    {
        public WorldEditorWindow()
        {
            InitializeComponent();
            Loaded += OnWorldEditorWindowLoaded;
        }
        private void LoggerWindow_Loaded(object sender, RoutedEventArgs e)
        { }
        private void UndoRedoWindow_Loaded(object sender, RoutedEventArgs e)
        { }
        private void OnWorldEditorWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnWorldEditorWindowLoaded;
            Focus();
        }
        private void OnNewScript_Button_Click(object sender, RoutedEventArgs e)
        {
             new NewScriptDialog().ShowDialog();
        }
        private void OnCreatePrimitiveMesh_Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new PrimitiveMeshDialog();
            dlg.ShowDialog();
        }

        private void OnNewProject(object sender, ExecutedRoutedEventArgs e)
        {
            GameProjectBrowser.GotoNewProjectTab = true;
            NewProjectClass2.Current?.Unload();
            Application.Current.MainWindow.DataContext = null;
            Application.Current.MainWindow.Close();
        }

        private void OnOpenProject(object sender, ExecutedRoutedEventArgs e)
        {
            NewProjectClass2.Current?.Unload();
            Application.Current.MainWindow.DataContext = null;  
            Application.Current.MainWindow.Close();

        }

        private void OnEditorClose(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
    }
}

