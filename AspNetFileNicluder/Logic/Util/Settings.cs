using AspNetFileNicluder.Logic.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AspNetFileNicluder.Logic.Util
{
    public class Settings
    {
        public Settings()
        {
#if DEBUG
            var configFilePath = string.IsNullOrWhiteSpace(Workspace.SolutionPath)
                    ? Path.Combine(@"D:\Projects\vs extandions\AspNetFileNicluder\AspNetFileNicluder\ExapleData\anfnConfig.json")
                    : Path.Combine(Workspace.SolutionPath, AppConstants.ConfigFileConstants.Name);
#else
            var configFilePath = Path.Combine(Workspace.SolutionPath, AppConstants.ConfigFileConstants.Name);
#endif
            if (File.Exists(configFilePath))
            {
                var text = File.ReadAllText(configFilePath);
                var obj = JObject.Parse(text);
                Databases = SetDatabaseConnectionString(obj[AppConstants.ConfigFileConstants.DataBase].ToObject<DatabaseModel>());
                IncludFilesToProject = obj[AppConstants.ConfigFileConstants.IncludFilesFrom].ToObject<IncludFilesToProjectModel>();
            }
        }

        private DatabaseModel SetDatabaseConnectionString(DatabaseModel model)
        {
            foreach(var c in model.ConnectionStrings.Where(c => !string.IsNullOrWhiteSpace(c.SameAs)))
            {
                var sameAs = model.ConnectionStrings.Single(cs => cs.Name == c.SameAs);
                c.FilterPattern = sameAs.FilterPattern;
                c.IgnorePattern = sameAs.IgnorePattern;
                c.ReplasePattern = sameAs.ReplasePattern;
                c.SqlCmdPattern = sameAs.SqlCmdPattern;
            }

            return model;
        }

        public IncludFilesToProjectModel IncludFilesToProject { get; set; }
        public DatabaseModel Databases { get; set; }

        public string GetProjectFullPath(string project, bool directoryBase = false, string basePath = null, bool asDirectory = true)
        {
            var path = Path.Combine(basePath ?? Workspace.SolutionPath, directoryBase ? IncludFilesToProject.ProjectAsPath(project) : project);
            if (asDirectory && !path.EndsWith("\\"))
            {
                path += "\\";
            }

            return path;
        }

        public class DatabaseModel
        {
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
    }
}
