using AspNetFileNicluder.Logic.Utils;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AspNetFileNicluder.Logic.Util
{
    public class Settings
    {
        public Settings()
        {
#if DEBUG
            var configFilePath = File.Exists(Workspace.ConfigFileFullName) ? Workspace.ConfigFileFullName : Path.Combine(@"D:\Projects\vs extandions\AspNetFileNicluder\AspNetFileNicluder\ExapleData\anfnConfig.json");
#else
            var configFilePath = Workspace.ConfigFileFullName;
#endif
            if (File.Exists(configFilePath))
            {
                var text = File.ReadAllText(configFilePath);
                var obj = JObject.Parse(text);
                Databases = SetDatabaseConnectionString(obj[AppConstants.ConfigFileConstants.DataBase].ToObject<DatabaseModel>());
                IncludFilesToProject = obj[AppConstants.ConfigFileConstants.IncludFilesFrom].ToObject<IncludFilesToProjectModel>();
                ChangeConstant = obj[AppConstants.ConfigFileConstants.ChangeConstants].ToObject<ChangeConstants>();
            }
        }

        public IncludFilesToProjectModel IncludFilesToProject { get; set; }
        public DatabaseModel Databases { get; set; }
        public ChangeConstants ChangeConstant { get; set; }

        #region Methodes

        public string GetProjectFullPath(string project, bool directoryBase = false, string basePath = null, bool asDirectory = true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var path = Path.Combine(basePath ?? Workspace.SolutionPath, directoryBase ? IncludFilesToProject.ProjectAsPath(project) : project);
            if (asDirectory && !path.EndsWith("\\"))
            {
                path += "\\";
            }

            return path;
        }
        private DatabaseModel SetDatabaseConnectionString(DatabaseModel model)
        {
            foreach (var c in model.ConnectionStrings.Where(c => !string.IsNullOrWhiteSpace(c.SameAs)))
            {
                var sameAs = model.ConnectionStrings.Single(cs => cs.Name == c.SameAs);
                c.FilterPattern = sameAs.FilterPattern;
                c.IgnorePattern = sameAs.IgnorePattern;
                c.ReplasePattern = sameAs.ReplasePattern;
                c.SqlCmdPattern = sameAs.SqlCmdPattern;
            }

            return model;
        } 
        #endregion

        #region classes
        public class DatabaseModel
        {
            public string FolderPickerDefaltPath { get; set; } = Workspace.SolutionPath;

            public bool SetDelimiterOnPanesAfterRead { get; set; }

            public IEnumerable<string> Panes { get; set; } = new string[] { "Source Control - Team Foundation" };

            public IEnumerable<DatabaseConnectionString> ConnectionStrings { get; set; }

            public DatabaseConnectionString GetConnectionStringByName(string name)
            {
                return ConnectionStrings.SingleOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public class DatabaseConnectionString
        {
            public string ConnectionString { get; set; }
            public string FilterPattern { get; set; }
            public string ReplasePattern { get; set; }
            public string IgnorePattern { get; set; }

            public string SqlCmdPattern { get; set; }

            public string Name { get; set; }
            public string SameAs { get; set; }

            public string DatabaseName
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(ConnectionString)) return null;
                    var split = ConnectionString.Split(';');
                    var database = split.FirstOrDefault(f => f.StartsWith("Database=", StringComparison.InvariantCultureIgnoreCase));
                    if (string.IsNullOrEmpty(database)) return null;
                    return database.Split('=')[1];
                }
            }

            public string ServerName
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(ConnectionString)) return null;
                    var split = ConnectionString.Split(';');
                    var database = split.FirstOrDefault(f => f.StartsWith("Server=", StringComparison.InvariantCultureIgnoreCase));
                    if (string.IsNullOrEmpty(database)) return null;
                    return database.Split('=')[1];
                }
            }
        }

        public class IncludFilesToProjectModel
        {
            public IEnumerable<string> FileTypes { get; set; }
            public IDictionary<string, IEnumerable<string>> Projects { get; set; }

            public IEnumerable<string> ProjectNames => Projects.Select(p => p.Key);

            public IEnumerable<string> GetFileTypesWithPrefix(string prefix)
            {
                return FileTypes.Select(i => prefix + i);
            }

            public string ProjectAsPath(string project, string prePath = null)
            {
                return Path.Combine(project, Path.Combine(Projects[project].ToArray()));
            }
        }

        public class ChangeConstants
        {
            public IEnumerable<ChangeConstantsConstants> Constants { get; set; } = new List<ChangeConstantsConstants>();
            public IEnumerable<KeyValuePair<string, string>> Files { get; set; } = new List<KeyValuePair<string, string>>();

            public string RowPattern { get; set; }

            public class ChangeConstantsConstants
            {
                public string Key { get; set; }
                public string Value { get; set; }

                public ChangeConstantsConstantsSql Sql { get; set; }

                public class ChangeConstantsConstantsSql
                {
                    public IEnumerable<string> Databases { get; set; } = new List<string>();
                    public string Script { get; set; }
                }
            }
        }
        #endregion
    }
}
