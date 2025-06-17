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
    /// Interaction logic for TransformWindow.xaml
    /// </summary>
    public partial class TransformWindow : UserControl
    {
        private Action _undoAction = null;
        private bool _propertyChanged = false;
        public TransformWindow()
        {
            InitializeComponent();
            Loaded += OnTransformWindowLoaded;
        }

        private void OnTransformWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnTransformWindowLoaded;
            (DataContext as MSTransform).PropertyChanged += (s, e) => _propertyChanged = true;
        }

        private void OnPosition_VectorBox_Mouse_LBD(object sender, MouseButtonEventArgs e)
        {
            _propertyChanged = false;
            if(!(DataContext is MSTransform cp))
            {
                return;
            }
            var selection = cp.SelectedComponents.Select(transform => (transform, transform.Position)).ToList();
            _undoAction = new Action(() => 
            {
                selection.ForEach(item => item.transform.Position = item.Position);
                (GameEntityWindow.Instance.DataContext as MSEntity)?.GetMSComponent<MSTransform>().Refresh();
            });
        }

        private void OnPosition_VectorBox_Mouse_LBU(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is MSTransform cp))
            {
                return;
            }
            if (_propertyChanged)
            {
                _propertyChanged = false;
                var selection = cp.SelectedComponents.Select(transform => (transform, transform.Position)).ToList();
               var _redoAction= new Action(() =>
                {
                    selection.ForEach(item => item.transform.Position = item.Position);
                    (GameEntityWindow.Instance.DataContext as MSEntity)?.GetMSComponent<MSTransform>().Refresh();
                });
                NewProjectClass2.UndoRedo.Add(new UndoRedoActions(_undoAction,_redoAction,"Position Changed"));
            }
        }

        private void OnPosition_VectorBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

        }
    }
}
