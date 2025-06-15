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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PrimalEngineEditor.GameProject
{
    /// <summary>
    /// Interaction logic for GameProjectBrowser.xaml
    /// </summary>
    public partial class GameProjectBrowser : Window
    {
        private readonly CubicEase _easing = new CubicEase() { EasingMode = EasingMode.EaseInOut };
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
                    AnimateToOpenProject();
                    openProjectWindow.IsEnabled = true;
                    newProjectWindow.IsEnabled = false;
                }
                openProjectButton.IsChecked =true;
            }
            else
            { 
               if (openProjectButton.IsChecked == true)
                 {
                  openProjectButton.IsChecked = false;
                    AnimateToCreateProject();
                    openProjectWindow.IsEnabled = false;
                    newProjectWindow.IsEnabled = true;
                  }
                newProjectButton.IsChecked = true;
            }
        }

        private void AnimateToCreateProject()
        {
            var highlightAnimation = new DoubleAnimation(213.7, 413.7, new Duration(TimeSpan.FromSeconds(0.2)));
            highlightAnimation.EasingFunction = _easing;
            highlightAnimation.Completed += (s, e) =>
            {
                var animation = new ThicknessAnimation(new Thickness(0), new Thickness(-1600, 0, 0, 0), new Duration(TimeSpan.FromSeconds(0.5)));
                browser.BeginAnimation(MarginProperty, animation);
            };
            highlightRect.BeginAnimation(Canvas.LeftProperty, highlightAnimation);
        }
        

        private void AnimateToOpenProject()
        {
            var highlightAnimation = new DoubleAnimation(413.7, 213.7, new Duration(TimeSpan.FromSeconds(0.2)));
            highlightAnimation.EasingFunction = _easing;
            highlightAnimation.Completed += (s, e) =>
            {
                var animation = new ThicknessAnimation(new Thickness(-1600, 0, 0, 0), new Thickness(0), new Duration(TimeSpan.FromSeconds(0.5)));
                browser.BeginAnimation(MarginProperty, animation);
            };
            highlightRect.BeginAnimation(Canvas.LeftProperty, highlightAnimation);
        }
    }
}
