using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AspNetFileNicluder.Logic.Util;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Shell;
using TeamFoundationWorkspace = Microsoft.TeamFoundation.VersionControl.Client.Workspace;

namespace AspNetFileNicluder.Logic.TfsIncluders
{
    public class TfsIncluder
    {
        private readonly Settings settings;

        private readonly string tfsDirectory;
        private readonly string serverUrl;
        private readonly string fileMatchPattern;
        private readonly IAsyncServiceProvider serviceProvider;
        
        public TfsIncluder() : this(new Settings())
        {
        }

        public TfsIncluder(Settings settings)
        {
            this.settings = settings;
            tfsDirectory = settings.ResolvePattern(settings.TfsIncluder?.LocalMapPath);
            serverUrl = settings.TfsIncluder?.ServerUrl;
            fileMatchPattern = settings.ResolvePattern(settings.TfsIncluder.FileMatchPattern);
        }

        public void Execute()
        {
            TfsTeamProjectCollection tpc = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(serverUrl));
            VersionControlServer tfServer = tpc.GetService<VersionControlServer>();
            var ws = tfServer.GetWorkspace(tfsDirectory);

            Execute(ws);
        }

        public void Execute(TeamFoundationWorkspace ws)
        {
            if (ws == null)
            {
                AppOutput.ConsoleWriteLine("---- Invalide ws config");
            }

            var files = GetLocalFiles();
            AppOutput.ConsoleWriteLine("Tfs inclider: filesFound: " + files.Count());

            var paddingChanges = ws.GetPendingChanges().Select(p => p.LocalItem);
            foreach (var file in files)
            {
                var serverPath = ws.TryGetServerItemForLocalItem(file);
                var serverItemExists = ws.VersionControlServer.ServerItemExists(serverPath, ItemType.File);
                if (!serverItemExists && !paddingChanges.Contains(file))
                {
                    ws.PendAdd(file);
                    AppOutput.ConsoleWriteLine("    Exluded file: " + file);
                }
            }

            AppOutput.ConsoleWriteLine("Finished");
        }

        private IEnumerable<string> GetLocalFiles()
        {
            var directoryInfo = new DirectoryInfo(tfsDirectory);
            var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories).Where(f => Regex.IsMatch(f.FullName, fileMatchPattern));
            return files.Select(f => f.FullName);
        }
    }
}
