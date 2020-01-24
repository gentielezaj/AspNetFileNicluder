using AspNetFileNicluder.Logic.Utils;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;

namespace AspNetFileNicluder.Logic.Util
{
    public abstract class Console
    {
        public abstract void ConsoleWriteException(Exception exception, params string[] text);
        public abstract void ConsoleWriteLine(params string[] text);
    }

    public class AppOutput
    {
        protected static IVsOutputWindow Output;
        protected static IVsOutputWindowPane OutputPane;
        protected static Guid paneGuid;

        private static void SetAppOutput()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            paneGuid = new Guid("18a63fba-e292-443a-95c8-7834e25da9a4");
            Output = Workspace.Output;
            Output.CreatePane(paneGuid, AppConstants.AppNameView, 1, 1);
            Output.GetPane(ref paneGuid, out OutputPane);
        }

        public static void ConsoleWriteLine(params string[] text)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (OutputPane == null)
            {
                SetAppOutput();
            }

            OutputPane.Activate();

            if (text?.Any() != true)
            {
                text = new string[1] { Environment.NewLine };
            }

            foreach (var t in text)
            {
                OutputPane.OutputString(t + Environment.NewLine);
            }
        }

        public static void ConsoleWriteException(Exception exception, params string[] text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var exInner = string.Empty;
            var exText = text.ToList();
            while (exception != null)
            {
                exText.Add(exInner + "Error:");
                exText.Add(exception.Message);
                exception = exception.InnerException;
                exInner += " => ";
            }

            ConsoleWriteLine(exText.ToArray());
        }
    }
}
