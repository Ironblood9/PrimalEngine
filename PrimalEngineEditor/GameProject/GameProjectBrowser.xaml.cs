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
using System.Windows.Shapes;

namespace PrimalEngineEditor.GameProject
{
    /// <summary>
    /// Interaction logic for GameProjectBrowser.xaml
    /// </summary>
    public partial class GameProjectBrowser : Window
    {
        public GameProjectBrowser()
        {
            InitializeComponent();
            Loaded += OnGameProjectBrowserLoaded;
        }

        private void OnGameProjectBrowserLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnGameProjectBrowserLoaded;
            if (!OpenProject.Projects.Any())
            {
                openProjectButton.IsEnabled = false;
                openProjectWindow.Visibility = Visibility.Hidden;
                OnToggleButton_Click(newProjectButton, new RoutedEventArgs());
            }
            
        }



        private void OnToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == openProjectButton)
            {
                if(newProjectButton.IsChecked==true)
                {
                    newProjectButton.IsChecked = false;
                    browser.Margin = new Thickness(0);
                }
                openProjectButton.IsChecked =true;
            }
            else
            { 
               if (openProjectButton.IsChecked == true)
                 {
                  openProjectButton.IsChecked = false;
                  browser.Margin = new Thickness(-800,0,0,0);
                  }
                newProjectButton.IsChecked = true;
            }
        }
    }
}
