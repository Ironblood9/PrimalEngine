using System;
using System.Collections.Generic;
using System.IO;
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


namespace PrimalEngineEditor
{
    /// <summary>
    /// Interaction logic for AgilisPathDialog.xaml
    /// </summary>
    public partial class AgilisPathDialog : Window
    {
        public string AgilisPath { get; private set; }
        public AgilisPathDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void OnOK_Button_Click(object sender, RoutedEventArgs e)
        {
            var path = pathTextBox.Text.Trim();
            messageTextBlock.Text = string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                messageTextBlock.Text = "Invalid path.";
            }
            else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                messageTextBlock.Text = "Invalid character(s) used in path.";
            }
            else if(!Directory.Exists(Path.Combine(path, @"MyGameEngineProject\AgilisAPI\")))
            {
                messageTextBlock.Text = "Unable to find engine at the specified location.";
            }

            if(string.IsNullOrEmpty(messageTextBlock.Text))
            {
                if (!Path.EndsInDirectorySeparator(path)) path += @"\";
                AgilisPath = path;
                DialogResult = true;
                Close();
            }
        }
    }
}
