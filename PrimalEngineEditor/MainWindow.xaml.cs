using PrimalEngineEditor.GameProject;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PrimalEngineEditor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnMainWindow_Loaded;
        Closing += OnMainWindow_Closing;
    }

    public static string AgilisPath { get; private set; } = @"C:\Users\Msı\source\repos\PrimalEngine";

    private void OnMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Closing -= OnMainWindow_Closing;
        NewProjectClass2.Current?.Unload();
    }

    private void OnMainWindow_Loaded(object sender,RoutedEventArgs e)
    {
        Loaded -= OnMainWindow_Loaded;
        OpenProjectBrowserDialog();
    }
    private void OpenProjectBrowserDialog()
    {
        var projectBrowser = new GameProjectBrowser();
        if (projectBrowser.ShowDialog() == false || projectBrowser.DataContext==null)
        {
            Application.Current.Shutdown();
        }
        else
        {
            NewProjectClass2.Current?.Unload();
            DataContext = projectBrowser.DataContext;
        }

    }
}

