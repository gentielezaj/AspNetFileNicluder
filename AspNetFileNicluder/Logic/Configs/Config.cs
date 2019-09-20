using AspNetFileNicluder.Logic.Util;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetFileNicluder.Logic.Configs
{
    class Config
    {
        public static bool OpenConfigFile(AsyncPackage package)
        {
            if (!Workspace.IsOpenSolution)
            {
                VsShellUtilities.ShowMessageBox(
                    package,
                    "No solution opend",
                    "Config file",
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return false;
            }

            var file = Workspace.ConfigFileFullName;
            if (!File.Exists(file))
            {
                var fs = File.Create(file);
                fs.Close();
            }

            if (!Workspace.SolutionDte.ItemOperations.IsFileOpen(file))
                Workspace.SolutionDte.ItemOperations.OpenFile(file);

            return true;
        }
    }
}
