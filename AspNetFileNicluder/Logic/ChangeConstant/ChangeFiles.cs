using AspNetFileNicluder.Logic.SQL;
using AspNetFileNicluder.Logic.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AspNetFileNicluder.Logic.Util.StaticExtensions;

namespace AspNetFileNicluder.Logic.ChangeConstant
{
    public class ChangeFiles
    {
        private readonly Settings settings;

        protected Action<string[]> consoleWriteLine;

        private readonly IDictionary<string, String> projects;
        private readonly Action<Exception, string[]> consoleWriteException;
        private readonly Action<string, string> WriteAllText;

        public ChangeFiles(Settings settings, IDictionary<string, string> projects, Action<string[]> consoleWriteLine, Action<Exception, string[]> consoleWriteException)
            : this(settings, consoleWriteLine, consoleWriteException, File.WriteAllText)
        {
            this.projects = projects;
        }

        public ChangeFiles(Settings settings, Action<string[]> consoleWriteLine, Action<Exception, string[]> consoleWriteException, Action<string, string> writeAllText)
        {
            this.settings = settings;
            this.consoleWriteLine = consoleWriteLine;
            this.consoleWriteException = consoleWriteException;
            this.WriteAllText = writeAllText;
        }

        private void ConsoleWriteLine(params string[] messages) => consoleWriteLine(messages);
        private void ConsoleWriteException(Exception e, params string[] messages) => consoleWriteException(e, messages);

        public bool Execute(IEnumerable<Settings.ChangeConstant> changeConstants)
        {
            return changeConstants.All(c => Execute(c));
        }

        public bool Execute(Settings.ChangeConstant changeConstants)
        {
            try
            {
                ConsoleWriteLine($"Changeing {changeConstants.Name}");
                foreach (var fileDiconary in changeConstants.Files)
                {
                    ConsoleWriteLine($"value: {fileDiconary.Value}");
                    string filepath = fileDiconary.File;
                    if (!File.Exists(filepath))
                    {
                        if (string.IsNullOrEmpty(fileDiconary.ProjectName)) continue;

                        string projectDirectory = projects.ContainsKey(fileDiconary.ProjectName) ? Path.GetDirectoryName(projects[fileDiconary.ProjectName]) : null;
                        if (string.IsNullOrEmpty(projectDirectory))
                        {
                            ConsoleWriteLine($"no project by name {fileDiconary.ProjectName} on open solution");
                            continue;
                        }
                        
                        filepath = Path.Combine(projectDirectory, fileDiconary.File);

                        if (!File.Exists(filepath))
                        {
                            ConsoleWriteLine($"no File {fileDiconary.File} in project {fileDiconary.ProjectName}");
                            continue;
                        }
                    }

                    var fileText = File.ReadAllText(filepath);
                    if (!Regex.IsMatch(fileText, fileDiconary.Pattern))
                    {
                        ConsoleWriteLine($"no match for file {fileDiconary.Pattern} in project {fileDiconary.ProjectName}");
                        continue;
                    }

                    fileText = Regex.Replace(fileText, fileDiconary.Pattern, fileDiconary.Value);
                    WriteAllText(filepath, fileText);

                    ConsoleWriteLine($"File {fileDiconary.File} in project {fileDiconary.ProjectName} rewrited");
                }

                if (!string.IsNullOrEmpty(changeConstants.Sql?.Script) && changeConstants.Sql?.Databases?.Any() == true)
                {
                    var sqlScript = File.Exists(changeConstants.Sql?.Script) ? File.ReadAllText(changeConstants.Sql.Script) : changeConstants.Sql.Script;

                    var sqlRunner = new SqlRuner();
                    var databases = settings.Databases.ConnectionStrings.Where(d => changeConstants.Sql.Databases.Contains(d.Name));
                    sqlRunner.ExecuteString(changeConstants.Sql.Script, databases);
                }

                return true;
            }
            catch (System.Exception e)
            {
                ConsoleWriteException(e, new string[] { });
                return false;
            }
        }
    }
}
