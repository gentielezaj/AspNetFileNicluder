namespace AspNetFileNicluder.Logic.Includers
{
    using AspNetFileNicluder.Logic.Core;
    using EnvDTE;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell;
    using System;
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
    public partial class FileIncluderToolWindowControl : DialogWindow, IDialogWindow
    {
        public ObservableCollection<FileListResultModel> Results { get; set; }
        public readonly FileIncluder fileIncluder;
        private readonly Func<bool, bool> callBack;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileIncluderToolWindowControl"/> class.
        /// </summary>
        public FileIncluderToolWindowControl(Func<bool, bool> callBack = null)
        {
            fileIncluder = new FileIncluder();
            Results = GetResults(fileIncluder.GetUnicludedFiles());
            this.InitializeComponent();
            this.DataContext = this;
            this.callBack = callBack;
        }
        
        public DialogWindow Create(Func<bool, bool> callback = null)
        {
            return new FileIncluderToolWindowControl(callback);
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
            var executed = true;
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var filesToInclude = new Dictionary<Project, IEnumerable<FileInfo>>();

                foreach (var item in Results)
                {
                    filesToInclude.Add(item.Project, item.Files.Where(f => f.IsSelected).Select(f => f.File));
                }

                fileIncluder.IncludeFiles(filesToInclude);
            }
            catch (Exception)
            {
                executed = false;
                throw;
            } 
            finally
            {
                this.Close();
                if (callBack != null) callBack.Invoke(executed);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
    }
}