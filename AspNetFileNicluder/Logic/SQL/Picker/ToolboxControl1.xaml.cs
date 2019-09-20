using AspNetFileNicluder.Logic.Util;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using PickerDialogResult = System.Windows.Forms.DialogResult;

namespace AspNetFileNicluder.Logic.SQL.Picker
{
    /// <summary>
    /// Interaction logic for ToolboxControl1.xaml.
    /// </summary>
    [ProvideToolboxControl("AspNetFileNicluder.Logic.SQL.Picker.ToolboxControl1", true)]
    public partial class ToolboxControl1 : DialogWindow
    {
        private readonly Settings settings;

        public ObservableCollection<string> FolersList { get; set; }

        public ToolboxControl1()
        {
            this.settings = new Settings();
            InitializeComponent();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "We are inside {0}.Button1_Click()", this.ToString()));
        }

        private void OpenFolderPickerClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.InitialDirectory = settings.Databases.FolderPickerDefaltPath; // Use current value for initial dir
            dialog.Title = "Select a Directory"; // instead of default "Save As"
            dialog.Filter = "Directory|*.this.directory"; // Prevents displaying files
            dialog.FileName = "select"; // Filename will then be "select.this.directory"
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                // Remove fake filename from resulting path
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                // If user has changed the filename, create the new directory
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                // Our final value is in path
                ResultText.Text = path;
            }
        }
    }
}
