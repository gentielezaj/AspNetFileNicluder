using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetFileNicluder.Logic.Utils
{
    static class AppConstants
    {
        public const string AppName = "AspNetFileIncluder";
        public const string AppNameView = "Asp.net file includer";
        public const string DelimiterOnPanesAfterRead = " ====== Asp net file Inclider delimiter ======";

        public static class ConfigFileConstants
        {
            public const string IncludFilesFrom = "includFilesToProject";
            public const string FileTypes = "fileTypes";
            public const string Projects = "projects";
            public const string Name = "anfnConfig.json";
            public const string DataBase = "database";
        }
    }
}
