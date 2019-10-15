namespace AspNetFileNicluder.Logic.Includers
{
    using EnvDTE;
    using Microsoft.VisualStudio.Shell;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Interaction logic for FileIncluderToolWindowControl.
    /// </summary>
    public partial class FileIncluderToolWindowControl : UserControl
    {
        public ObservableCollection<FileListResultModel> Results { get; set; }
        public readonly FileIncluder fileIncluder;
        /// <summary>
        /// Initializes a new instance of the <see cref="FileIncluderToolWindowControl"/> class.
        /// </summary>
        public FileIncluderToolWindowControl()
        {
            fileIncluder = new FileIncluder();
            Results = GetResults(fileIncluder.GetUnicludedFiles());
            this.InitializeComponent();
            this.DataContext = this;
        }

        public void Refresh()
        {
            fileIncluder.RefreshSettings();
            Results = GetResults(fileIncluder.GetUnicludedFiles());
            CollectionViewSource.GetDefaultView(Results).Refresh();
        }

        public ObservableCollection<FileListResultModel> GetResults(IDictionary<Project, IEnumerable<FileInfo>> data)
        {
            var results = new ObservableCollection<FileListResultModel>();
            foreach (var item in data)
            {
                var model = new FileListResultModel(item.Key);
                model.SetFiles(item.Value);
                results.Add(model);
            }

            return results;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var obj = (CheckBox)sender;
            foreach (var project in Results)
            {
                if(obj.Content.ToString() == project.ProjectName)
                {
                    foreach(var item in project.Files)
                    {
                        item.IsSelected = obj.IsChecked.GetValueOrDefault(false);
                    }

                    CollectionViewSource.GetDefaultView(Results).Refresh();
                    break;
                }
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var filesToInclude = new Dictionary<Project, IEnumerable<FileInfo>>();

            foreach(var item in Results)
            {
                filesToInclude.Add(item.Project, item.Files.Where(f => f.IsSelected).Select(f => f.File));
            }

            fileIncluder.IncludeFiles(filesToInclude);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
    }
}