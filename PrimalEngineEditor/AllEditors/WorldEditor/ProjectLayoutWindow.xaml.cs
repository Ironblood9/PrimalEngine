using PrimalEngineEditor.Components;
using PrimalEngineEditor.GameProject;
using PrimalEngineEditor.Utilities;
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
            GameEntityWindow.Instance.DataContext = null;
            var listbox = sender as ListBox;
            if (e.AddedItems.Count > 0)
            {
                GameEntityWindow.Instance.DataContext = listbox.SelectedItems[0];
            }
            var NewSelection = listbox.SelectedItems.Cast<GameEntity>().ToList();
            var previousSelection = NewSelection.Except(e.AddedItems.Cast<GameEntity>()).Concat(e.RemovedItems.Cast<GameEntity>()).ToList();

            NewProjectClass2.UndoRedo.Add(new UndoRedoActions(
                () =>
                {
                    listbox.UnselectAll();
                    previousSelection.ForEach(x => (listbox.ItemContainerGenerator.ContainerFromItem(x) as ListBoxItem).IsSelected = true);
                },
                () =>
                {
                    listbox.UnselectAll();
                    NewSelection.ForEach(x => (listbox.ItemContainerGenerator.ContainerFromItem(x) as ListBoxItem).IsSelected = true);
                },
                "Selection Changed"
                ));
        }
    }
}
