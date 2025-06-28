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

        private void OnWorldEditorWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnWorldEditorWindowLoaded;
            Focus();
        }

        private void UndoRedoWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void OnNewScript_Button_Click(object sender, RoutedEventArgs e)
        {
             new NewScriptDialog().ShowDialog();
        }

        private void LoggerWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}

