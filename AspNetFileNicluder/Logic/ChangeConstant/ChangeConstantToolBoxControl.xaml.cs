using AspNetFileNicluder.Logic.Core;
using AspNetFileNicluder.Logic.SQL;
using AspNetFileNicluder.Logic.Util;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace AspNetFileNicluder.Logic.ChangeConstant
{
    /// <summary>
    /// Interaction logic for ChangeConstantToolBoxControl.xaml.
    /// </summary>
    [ProvideToolboxControl("AspNetFileNicluder.Logic.ChangeConstant.ChangeConstantToolBoxControl", true)]
    public partial class ChangeConstantToolBoxControl : DialogWindow
    {
        private readonly Settings settings;
        private readonly Func<bool, bool> callBack;
        public ObservableCollection<Settings.ChangeConstant> Results { get; set; }

        public delegate void OnChangeFilesResults(bool success);

        public ChangeConstantToolBoxControl(Func<bool, bool> callBack = null)
        {
            this.callBack = callBack;
            this.settings = new Settings();
            SetData();
            InitializeComponent();
            this.DataContext = this;
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (ReferencesNew.SelectedValue == null)
            {
                this.Close();
            }

            var item = (Settings.ChangeConstant)ReferencesNew.SelectedItem;

            var result = ChangeFiles(item);

            callBack(result);
            
            this.Close();
        }

        private bool ChangeFiles(Settings.ChangeConstant changeConstants)
        {
            var projects = Workspace.Projects.ToDictionary(p => p.Name, p => p.FullName);
            return new ChangeFiles(settings, projects, AppOutput.ConsoleWriteLine, AppOutput.ConsoleWriteException).Execute(changeConstants);
        }

        private void SetData()
        {
            Results = new ObservableCollection<Settings.ChangeConstant>();
            foreach (var changeConstants in settings.ChangeConstants)
            {
                Results.Add(changeConstants);
            }
        }
    }
}
