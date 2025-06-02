using PrimalEngineEditor.Components;
using PrimalEngineEditor.GameProject;
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

namespace PrimalEngineEditor.AllEditors
{
    /// <summary>
    /// Interaction logic for ProjectLayoutWindow.xaml
    /// </summary>
    public partial class ProjectLayoutWindow : UserControl
    {
        public ProjectLayoutWindow()
        {
            InitializeComponent();
            
        }

        private void OnAddGameEntity_Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var cp = btn.DataContext as Scene;
            cp.AddGameEntityCommand.Execute(new GameEntity(cp) { Name = "Empty Game Entity" });
        }

        private void OnGameEntities_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var entity = (sender as ListBox).SelectedItems[0];
            GameEntityWindow.Instance.DataContext = entity;
        }
    }
}
