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
        public Settings(string configFilePath = null)
        {
#if DEBUG
            configFilePath = configFilePath ?? (File.Exists(Workspace.ConfigFileFullName) ? Workspace.ConfigFileFullName : Path.Combine(@"C:\Users\gogi_\source\AspNetFileNicluder\AspNetFileNicluder\ExapleData\anfnConfig.json"));
#else
            configFilePath = configFilePath ?? Workspace.ConfigFileFullName;
#endif
            if (File.Exists(configFilePath))
            {
                var text = File.ReadAllText(configFilePath);
                var obj = JObject.Parse(text);
                Databases = SetDatabaseConnectionString(obj[AppConstants.ConfigFileConstants.DataBase].ToObject<DatabaseModel>());
                IncludFilesToProject = obj[AppConstants.ConfigFileConstants.IncludFilesFrom].ToObject<IncludFilesToProjectModel>();
                ChangeConstants = CreateChangeConstants(obj[AppConstants.ConfigFileConstants.ChangeConstants] as JArray); // obj[AppConstants.ConfigFileConstants.ChangeConstants].ToObject<List<ChangeConstant>>();
            }
        }

        public IncludFilesToProjectModel IncludFilesToProject { get; set; }
        public DatabaseModel Databases { get; set; }
        public List<ChangeConstant> ChangeConstants { get; set; }

        #region Methodes

        #region CreateChangeConstants 
        private List<ChangeConstant> CreateChangeConstants(JArray array)
        {
            if (array == null) return new List<ChangeConstant>();

            var result = new List<ChangeConstant>();

            foreach (var item in array)
            {
                var changeConstant = new ChangeConstant
                {
                    Name = item["name"].ToString(),
                    Description = item["decription"]?.ToString(),
                    FormatValues = item["formatValues"]?.ToObject<List<string>>(),
                    Sql = item["sql"]?.ToObject<ChangeConstant.ChangeConstantsConstantsSql>()
                };

                foreach (JObject file in item["files"])
                {
                    changeConstant.Files.AddRange(CreateChangeConstantsFiles(file, changeConstant.FormatValues));
                }

                result.Add(changeConstant);
            }

            return result;
        }

        private List<ChangeConstant.ProjectFiles> CreateChangeConstantsFiles(JObject jObject, IEnumerable<string> formatValues)
        {
            var result = new List<ChangeConstant.ProjectFiles>();

            string projectName = null;
            if (jObject.ContainsKey("projectName")) projectName = jObject["projectName"]?.ToString();
            else if (jObject.ContainsKey("projectNames"))
            {
                foreach (var token in jObject["projectNames"])
                {
                    jObject["projectName"] = token;
                    result.AddRange(CreateChangeConstantsFiles(jObject, formatValues));
                }
                return result;
            }

            string fileName;
            if (!jObject.ContainsKey("file"))
            {
                foreach (var token in jObject["files"])
                {
                    jObject["file"] = token;
                    result.AddRange(CreateChangeConstantsFiles(jObject, formatValues));
                }
                return result;
            }
            else fileName = jObject["file"]?.ToString();

            string pattern;
            if (!jObject.ContainsKey("pattern"))
            {
                foreach (var token in jObject["patterns"])
                {
                    jObject["pattern"] = token;
                    result.AddRange(CreateChangeConstantsFiles(jObject, formatValues));
                }
                return result;
            }
            else pattern = jObject["pattern"]?.ToString();

            var value = jObject["value"]?.ToString();

            if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(pattern) && !string.IsNullOrEmpty(value))
            {
                if(formatValues?.Any() == true)
                {
                    value = string.Format(value, formatValues.ToArray());
                }

                result.Add(new ChangeConstant.ProjectFiles
                {
                    File = fileName,
                    Pattern = pattern,
                    ProjectName = projectName ?? "No Project",
                    Value = value
                });
            }

            return result;
        }

        #endregion

        public string GetProjectFullPath(string project, bool directoryBase = false, string basePath = null, bool asDirectory = true)
        {
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
            public DatabaseModel()
            {
                FolderPickerDefaltPath = UnitTestDetector.IsRunningFromNUnit ? null : Workspace.SolutionPath;
            }

            public string FolderPickerDefaltPath { get; set; }

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

        public class ChangeConstant
        {
            public string Name { get; set; }
            public string Description { get; set; }

            public List<string> FormatValues { get; set; }

            public List<ProjectFiles> Files { get; set; } = new List<ProjectFiles>();

            public ChangeConstantsConstantsSql Sql { get; set; }

            public class ChangeConstantsConstantsSql
            {
                public IEnumerable<string> Databases { get; set; } = new List<string>();
                public string Script { get; set; }
            }

            public class ProjectFiles
            {
                public string ProjectName { get; set; }
                public string File { get; set; }
                public string Pattern { get; set; }
                public string Value { get; set; }
            }
        }
        #endregion
    }
}
