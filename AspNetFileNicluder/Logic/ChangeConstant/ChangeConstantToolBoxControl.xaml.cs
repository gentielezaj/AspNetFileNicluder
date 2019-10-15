using AspNetFileNicluder.Logic.SQL;
using AspNetFileNicluder.Logic.Util;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using static AspNetFileNicluder.Logic.Util.Settings.ChangeConstants;

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
        public ObservableCollection<ChangeConstantsConstants> Results { get; set; }

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

            var item = (ChangeConstantsConstants)ReferencesNew.SelectedItem;

            var result = ChangeFiles(item);

            callBack(result);

            this.Close();
        }

        private bool ChangeFiles(ChangeConstantsConstants item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                foreach (var settingsProject in settings.ChangeConstant.Files)
                {
                    var solutionProject = Workspace.Projects.FirstOrDefault(p => p.Name == settingsProject.Key);
                    if (solutionProject == null)
                    {
                        AppOutput.ConsoleWriteLine($"no project by name {settingsProject.Key} on open solution");
                        continue;
                    }

                    var projectDirectory = Path.GetDirectoryName(solutionProject.FullName);

                    var filepath = Path.Combine(projectDirectory, settingsProject.Value);
                    if (!File.Exists(filepath))
                    {
                        AppOutput.ConsoleWriteLine($"no File {settingsProject.Value} in project {settingsProject.Key}");
                        continue;
                    }

                    var fileText = File.ReadAllText(filepath);
                    if (!Regex.IsMatch(fileText, settings.ChangeConstant.RowPattern)) {
                        AppOutput.ConsoleWriteLine($"no match for file {settingsProject.Value} in project {settingsProject.Key}");
                        continue;
                    }

                    fileText = Regex.Replace(fileText, settings.ChangeConstant.RowPattern, item.Value);
                    File.WriteAllText(filepath, fileText);

                    AppOutput.ConsoleWriteLine($"File {settingsProject.Value} in project {settingsProject.Key} rewrited");
                }

                if (item.Sql != null)
                {
                    var sqlRunner = new SqlRuner();
                    var databases = item.Sql.Databases?.Any() != true ? null : settings.Databases.ConnectionStrings.Where(d => item.Sql.Databases.Contains(d.Name));
                    sqlRunner.ExecuteString(item.Sql.Script, databases);
                }

                return true;
            }
            catch (System.Exception e)
            {
                AppOutput.ConsoleWriteException(e);
                return false;
            }
        }

        private void SetData()
        {
            Results = new ObservableCollection<ChangeConstantsConstants>();
            foreach (var c in settings.ChangeConstant.Constants)
            {
                Results.Add(c);
            }
        }
    }
}
