using PrimalEngineEditor.Content;
using PrimalEngineEditor.GameProject;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

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

    public static string AgilisPath { get; private set; } 

    private void OnMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext == null)
        {
            e.Cancel = true;
            Application.Current.MainWindow.Hide(); 
            OpenProjectBrowserDialog();
            if(DataContext != null)
            {
                Application.Current.MainWindow.Show();
            }
        }
        else
        {
            Closing -= OnMainWindow_Closing;
            NewProjectClass2.Current?.Unload();
            DataContext = null; 
        }
    }

    private void OnMainWindow_Loaded(object sender,RoutedEventArgs e)
    {
        Loaded -= OnMainWindow_Loaded;
        GetAgilisPath();
        OpenProjectBrowserDialog();
    }

    private void GetAgilisPath()
    {
        var agilisPath = Environment.GetEnvironmentVariable("AGILIS_PATH", EnvironmentVariableTarget.User);
        if(agilisPath==null || !Directory.Exists(Path.Combine(agilisPath, @"MyGameEngineProject\AgilisAPI")))
        {
            var dlg = new AgilisPathDialog();
            if(dlg.ShowDialog()==true)
            {
                AgilisPath = dlg.AgilisPath;
                Environment.SetEnvironmentVariable("AGILIS_PATH", AgilisPath.ToUpper(), EnvironmentVariableTarget.User);
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
        else
        {
            AgilisPath = agilisPath;
        }

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
            var project = projectBrowser.DataContext as NewProjectClass2;
            Debug.Assert(project != null);
            ContentWatcher.Reset(project.ContentPath, project.Path);
            DataContext = project;
        }

    }
}

