using AspNetFileNicluder.Logic.Utils;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetFileNicluder.Logic.Util
{
    static class Workspace
    {
        public static IVsSolution Solution => GetSolution<IVsSolution>();

        public static EnvDTE80.DTE2 SolutionDte => GetSolution<SDTE, EnvDTE80.DTE2>();

        public static string SolutionPath => GetSolutionPath();

        public static string ConfigFileFullName => string.IsNullOrEmpty(SolutionPath) ? null : Path.Combine(SolutionPath, AppConstants.ConfigFileConstants.Name);

        public static IVsOutputWindow Output => GetSolution<IVsOutputWindow>();

        public static bool IsOpenSolution => !string.IsNullOrWhiteSpace(SolutionPath);


        public static Settings Settings => new Settings();

        public static List<Project> Projects => GetProjects();

        private static T GetSolution<T>() where T : class
        {
            return GetSolution<T, T>();
        }

        private static TR GetSolution<T, TR>() where TR : class
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(T)) as TR;
        }

        private static string GetSolutionPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Solution.GetSolutionInfo(out var path, out var file, out var userOptions);
            return path;
        }

        private static List<Project> GetProjects(Project parent = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var list = new List<Project>();
            if (parent != null && parent.Kind != ProjectKinds.vsProjectKindSolutionFolder && !string.IsNullOrWhiteSpace(parent.FileName))
            {
                list.Add(parent);
            }
            else if(parent != null)
            {
                if (parent.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    foreach (ProjectItem projectItem in parent.ProjectItems)
                    {
                        var project = projectItem.SubProject;
                        if (project == null) continue;

                        if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                        {
                            list.AddRange(GetProjects(project));
                        }
                        else if (!string.IsNullOrWhiteSpace(project.FileName))
                        {
                            list.Add(project);
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(parent.FileName))
                {
                    list.Add(parent);
                }
            }
            else
            {
                foreach (Project project in SolutionDte.Solution.Projects)
                {
                    if (project == null) continue;

                    if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                    {
                        list.AddRange(GetProjects(project));
                    }
                    else if(!string.IsNullOrWhiteSpace(project.FileName))
                    {
                        list.Add(project);
                    }
                }
            }

            return list;
        }
    }

}
