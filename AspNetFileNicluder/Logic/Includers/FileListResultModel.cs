using EnvDTE;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetFileNicluder.Logic.Includers
{
    public class FileListResultModel
    {
        public FileListResultModel() { }

        public FileListResultModel(Project project) : this()
        {
            Project = project;
        }

        public string ProjectName => this.Project?.Name?.ToString() ?? "Unkown";
        public Project Project { get; set; }
        public bool IsSelected { get; set; } = true;

        public IList<ProjectFileInfo> Files { get; set; } = new List<ProjectFileInfo>();

        public void SetFiles(IEnumerable<string> filesPaths)
        {
            foreach(var f in filesPaths)
            {
                Files.Add(new ProjectFileInfo(f));
            }
        }

        public void SetFiles(IEnumerable<FileInfo> filesPaths)
        {
            foreach (var f in filesPaths)
            {
                Files.Add(new ProjectFileInfo(f));
            }
        }
    }

    public class ProjectFileInfo
    {
        public ProjectFileInfo() { }

        public ProjectFileInfo(string path)
        {
            if(System.IO.File.Exists(path))
                File = new FileInfo(path);
        }

        public ProjectFileInfo(FileInfo path)
        {
            File = path;
        }

        public FileInfo File { get; set; }

        public string Name => File?.Name;

        public string FullName => File?.FullName;

        public bool IsSelected { get; set; } = true;
    }

    //public class ProjectData
    //{
    //    public string Name { get; set; }
    //}
}
