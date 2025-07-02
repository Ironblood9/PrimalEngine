using PrimalEngineEditor.Components;
using PrimalEngineEditor.GameProject;
using PrimalEngineEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PrimalEngineEditor.AllEditors
{
    public class NullableBoolToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b == true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b == true;
        }
    }
    /// <summary>
    /// Interaction logic for GameEntityWindow.xaml
    /// </summary>
    public partial class GameEntityWindow : UserControl
    {
        private Action _undoAction;
        private string _propertyName;
        public static GameEntityWindow Instance { get; private set; }
        public GameEntityWindow()
        {
            InitializeComponent();
            DataContext = null;
            Instance = this;
            DataContextChanged += (_, __) =>
            {
                if (DataContext != null)
                {
                    (DataContext as MSEntity).PropertyChanged += (s, e) => _propertyName = e.PropertyName;
                }
            };
        }

        private Action GetRenameAction()
        {
            var cp = DataContext as MSEntity;
            var selection = cp.SelectedEntities.Select(entity => (entity, entity.Name)).ToList();
            return new Action(() =>
            {
                selection.ForEach(item => item.entity.Name = item.Name);
                (DataContext as MSEntity).Refresh();
            });
        }

        private Action GetIsEnabledAction()
        {
            var cp = DataContext as MSEntity;
            var selection = cp.SelectedEntities.Select(entity => (entity, entity.IsEnabled)).ToList();
            return new Action(() =>
            {
                selection.ForEach(item => item.entity.IsEnabled = item.IsEnabled);
                (DataContext as MSEntity).Refresh();
            });
        }

        private void OnName_TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _propertyName = string.Empty;
            _undoAction = GetRenameAction();
        }

        private void OnName_TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(_propertyName == nameof(MSEntity.Name) && _undoAction != null)
            {
                var redoAction = GetRenameAction();
                NewProjectClass2.UndoRedo.Add(new UndoRedoActions(_undoAction, redoAction, "Rename game entity"));
                _propertyName = null;
            }
            _undoAction = null;
        }


        private void OnIsEnabled_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var undoAction = GetIsEnabledAction();
            var cp = DataContext as MSEntity;
            cp.IsEnabled = (sender as CheckBox).IsChecked == true;
            var redoAction = GetIsEnabledAction();
            NewProjectClass2.UndoRedo.Add(new UndoRedoActions(undoAction, redoAction, cp.IsEnabled == true ? "Enable game entity" : "Disable game entity"));
        }

        private void OnAddComponent_Button_PreviewMouse_LBD(object sender, MouseButtonEventArgs e)
        {
            var menu = FindResource("addComponentMenu") as ContextMenu;
            var btn = sender as ToggleButton;
            btn.IsChecked = true;
            menu.Placement = PlacementMode.Bottom;
            menu.PlacementTarget = btn;
            menu.MinWidth = btn.ActualWidth;
            menu.IsOpen = true;
        }

        private void AddComponent(ComponentType componentType, object data)
        {
            var creationFunction = ComponentFactory.GetCreationFunction(componentType);
            var chandedEntities = new List<(GameEntity entity, Component component)>();
            var vm = DataContext as MSEntity;
            foreach (var entity in vm.SelectedEntities)
            {
                var component = creationFunction(entity, data);
                if(entity.AddComponent(component))
                {
                    chandedEntities.Add((entity, component));
                }
            }

            if(chandedEntities.Any())
            {
                vm.Refresh();
                NewProjectClass2.UndoRedo.Add(new UndoRedoActions(
                    () =>
                    {
                        chandedEntities.ForEach(x => x.entity.RemoveComponent(x.component));
                        (DataContext as MSEntity).Refresh();
                    },
                    () =>
                    {
                        chandedEntities.ForEach(x => x.entity.AddComponent(x.component));
                        (DataContext as MSEntity).Refresh();
                    },
                    $"Add {componentType} component"
                    ));
            }
        }
        private void OnAddScriptComponent(object sender, RoutedEventArgs e)
        {
            AddComponent(ComponentType.Script, (sender as MenuItem).Header.ToString());
        }
    }
}
