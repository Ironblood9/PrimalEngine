using PrimalEngineEditor.GameProject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace PrimalEngineEditor.Content
{
    /// <summary>
    /// Interaction logic for ContentBrowserWindow.xaml
    /// </summary>
    public partial class ContentBrowserWindow : UserControl
    {
        public ContentBrowserWindow()
        {
            DataContext = null;
            InitializeComponent();
            Loaded += OnContentBrowserLoaded;
        }

        private void OnContentBrowserLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnContentBrowserLoaded;
            if(Application.Current?.MainWindow != null)
            {
                Application.Current.MainWindow.DataContextChanged += OnPropertyChanged;
            }
            OnPropertyChanged(null, new DependencyPropertyChangedEventArgs(DataContextProperty, null, NewProjectClass2.Current));
        }

        private void OnPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (DataContext as ContentBrowser)?.Dispose();
            DataContext = null;
            if (e.NewValue is NewProjectClass2 project)
            {
                Debug.Assert(e.NewValue == NewProjectClass2.Current);
                var contentBrowser = new ContentBrowser(project);
                contentBrowser.PropertyChanged += OnSelectedFolderChanged;
                DataContext = contentBrowser;
            }
        }

        private void OnSelectedFolderChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = sender as ContentBrowser;
            if(e.PropertyName == nameof(vm.SelectedFolder) && !string.IsNullOrEmpty(vm.SelectedFolder))
            {

            }
        }
    }
}
