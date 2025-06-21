
using PrimalEngineEditor.GameProject;
using PrimalEngineEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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


namespace PrimalEngineEditor.GameDev
{
    /// <summary>
    /// Interaction logic for NewScriptDialog.xaml
    /// </summary>
    public partial class NewScriptDialog : Window
    {
        public NewScriptDialog()
        {
            InitializeComponent();
        }

        bool Validate()
        {
            bool isValid = false;
            var name = scriptName.Text.Trim();
            var path = scriptPath.Text.Trim();
            string errorMessage = string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                errorMessage = "Please, type in a script name.";

            }
            else if (name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 || name.Any(x=> char.IsWhiteSpace(x)))
            {
                errorMessage = "Invalid character(s) used in script name.";
            }

            if (string.IsNullOrEmpty(path))
            {
                errorMessage = "Please, select a valid script folder.";

            }
            else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1 )
            {
                errorMessage = "Invalid character(s) used in script path.";
            }
            else if(!Path.GetFullPath(Path.Combine(NewProjectClass2.Current.Path, path)).Contains(Path.Combine(NewProjectClass2.Current.Path, @"GameCode\")))
            {
                errorMessage = "Script must be added to (a sub-folder of) GameCode.";
            }
            else if(File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(NewProjectClass2.Current.Path, path), $"{name}.cpp"))) ||
                    File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(NewProjectClass2.Current.Path, path), $"{name}.h"))))
            {
                errorMessage = $"script {name} already exists in this folder.";
            }
            else
            {
                isValid = true;
            }
            if(!isValid)
            {
                messageTextBlock.Foreground = FindResource("Editor.RedBrush") as Brush;

            }
            else
            {
                messageTextBlock.Foreground = FindResource("Editor.FontBrush") as Brush;
            }
            messageTextBlock.Text = errorMessage;
            return isValid;
        }
        private void OnScriptName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Validate()) return;
            var name = scriptName.Text.Trim();
            messageTextBlock.Text = $"{name}.h and {name}.cpp will be added to {NewProjectClass2.Current.Name}";
        }

        private void OnScriptPath_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Validate();
        }

        private async void OnCreate_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;
            IsEnabled = false;

            try
            {
                await Task.Run(() => CreateScript(name, path, solution, projectName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to create script {scriptName.Text}");
                
            }
        }

        private void CreateScript(string name, string path, string solution, string projectName)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            var cpp = Path.GetFullPath(Path.Combine(path, $"{name}.cpp"));
            var headerFile = Path.GetFullPath(Path.Combine(path, $"{name}.h"));

            using(var sw = File.CreateText(cpp))
            { }
            using (var sw = File.CreateText(headerFile))
            { }
        }
    }
}
