using AspNetFileNicluder.Logic.Utils;
using EnvDTE;
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

        private static T GetSolution<T>() where T: class
        {
            return GetSolution<T, T>();
        }

        private static TR GetSolution<T, TR>() where TR : class
        {
            return Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(T)) as TR;
        }

        private static string GetSolutionPath()
        {
            Solution.GetSolutionInfo(out var path, out var file, out var userOptions);
            return path;
        }
    }

}
