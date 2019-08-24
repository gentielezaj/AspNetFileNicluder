using AspNetFileNicluder.Logic.Util;
using EnvDTE;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using System.Threading.Tasks;

namespace AspNetFileNicluder.Logic.Includers
{
    public class FileIncluder : BaseExecuter
    {
        public FileIncluder() : base()
        {
        }

        public bool IncludeFiles(IDictionary<Project, IEnumerable<FileInfo>> filesToInclude)
        {
            foreach (var project in filesToInclude)
            {
                foreach (var file in project.Value)
                {
                    AppOutput.ConsoleWriteLine($"including file: {file.Name} in {project.Key.Name}");
                    project.Key.ProjectItems.AddFromFile(file.FullName);
                    AppOutput.ConsoleWriteLine("File included");
                }
            }

            return true;
        }

        public IDictionary<Project, IEnumerable<FileInfo>> GetUnicludedFiles()
        {
            var result = new Dictionary<Project, IEnumerable<FileInfo>>();
            foreach (Project project in Dte.Solution.Projects)
            {
                GetUnicludedFiles(project, ref result);

                //if (Settings.IncludFilesToProject.ProjectNames.Contains(project.Name))
                //{
                //    var list = new List<FileInfo>();
                //    newfiles = GetFilesNotInProject(project);
                //    foreach (var file in newfiles)
                //    {
                //        list.Add(new FileInfo(file));
                //        //AppOutput.ConsoleWriteLine("including file: " + file);
                //        //project.ProjectItems.AddFromFile(file);
                //    }

                //    result.Add(project, list);
                //}
            }

            return result;
        }

        public void GetUnicludedFiles(Project project, ref Dictionary<Project, IEnumerable<FileInfo>> results)
        {
            if(string.IsNullOrWhiteSpace(project.FullName))
            {
                foreach(ProjectItem pi in project.ProjectItems)
                {
                    GetUnicludedFiles(pi.SubProject, ref results);
                }
            }

            if (Settings.IncludFilesToProject.ProjectNames.Contains(project.Name))
            {
                var list = new List<FileInfo>();
                var newfiles = GetFilesNotInProject(project);
                foreach (var file in newfiles)
                {
                    list.Add(new FileInfo(file));
                    //AppOutput.ConsoleWriteLine("including file: " + file);
                    //project.ProjectItems.AddFromFile(file);
                }

                results.Add(project, list);
            }
        }

        private List<string> GetFilesNotInProject(Project project)
        {
            List<string> returnValue = new List<string>();
            string startPath = Path.Combine(Path.GetDirectoryName(project.FullName), Path.Combine(Settings.IncludFilesToProject.Projects[project.Name].ToArray()));
            startPath += startPath.EndsWith("\\") ? string.Empty : "\\";
            List<string> projectFiles = GetAllProjectFiles(project.ProjectItems, startPath);

            foreach (var filter in Settings.IncludFilesToProject.GetFileTypesWithPrefix("*."))
                foreach (var file in Directory.GetFiles(startPath, filter, SearchOption.AllDirectories))
                    if (!projectFiles.Contains(file)) returnValue.Add(file);

            return returnValue;
        }

        private List<string> GetAllProjectFiles(ProjectItems projectItems, string projectPath, IEnumerable<string> extensions = null)
        {
            List<string> returnValue = new List<string>();
            extensions = extensions ?? Settings.IncludFilesToProject.GetFileTypesWithPrefix(".");

            foreach (ProjectItem projectItem in projectItems)
            {
                var projectItemPath = projectItem.Properties.Item("FullPath").Value.ToString();
                var projectItemPathToCompare = projectItemPath.Substring(0, projectItemPath.Length <= projectPath.Length ? projectItemPath.Length : projectPath.Length);
                if (!projectPath.StartsWith(projectItemPathToCompare))
                {
                    continue;
                }
                if (extensions.Contains(Path.GetExtension(projectItemPath).ToLower()))
                    returnValue.Add(projectItemPath);
                //for (short i = 1; i <= projectItems.Count; i++)
                //{
                //    string fileName = projectItem.FileNames[i];
                //    if (extensions.Contains(Path.GetExtension(fileName).ToLower()))
                //        returnValue.Add(fileName);
                //}
                returnValue.AddRange(GetAllProjectFiles(projectItem.ProjectItems, projectPath, extensions));
            }

            return returnValue;
        }
    }
}
