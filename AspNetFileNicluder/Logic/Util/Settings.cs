using AspNetFileNicluder.Logic.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetFileNicluder.Logic.Util
{
    public class Settings
    {
        public Settings()
        {
            var configFilePath = Path.Combine(Workspace.SolutionPath, AppConstants.ConfigFileConstants.Name);
            if (File.Exists(configFilePath))
            {
                var text = File.ReadAllText(configFilePath);
                var obj = JObject.Parse(text);
                Databases = obj[AppConstants.ConfigFileConstants.DataBase].ToObject<DatabaseModel>();
                IncludFilesToProject = obj[AppConstants.ConfigFileConstants.IncludFilesFrom].ToObject<IncludFilesToProjectModel>();
            }
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
        }

        public class DatabaseConnectionString
        {
            public string ConnectionString { get; set; }
            public string FilterPattern { get; set; }
            public string ReplasePattern { get; set; }
            public string IgnorePattern { get; set; }

            public string SqlCmdPattern { get; set; }
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
