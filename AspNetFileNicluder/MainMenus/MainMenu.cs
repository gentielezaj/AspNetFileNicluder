using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using AspNetFileNicluder.Logic.Includers;
using AspNetFileNicluder.Logic.SQL;
using AspNetFileNicluder.Logic.Util;
using AspNetFileNicluder.Logic.Utils;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace AspNetFileNicluder.MainMenus
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class MainMenu
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;
        public const int OpenConfigFile = 0x0099;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("ca7faa7a-f352-4253-8cc5-f3d8846e6d00");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainMenu"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private MainMenu(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);

            var menuCommandID1 = new CommandID(CommandSet, OpenConfigFile);
            var menuItem1 = new MenuCommand(this.OpenCofigFile, menuCommandID1);
            commandService.AddCommand(menuItem1);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static MainMenu Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in MainMenu's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new MainMenu(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var executed = new SqlRuner().Execute();

            ////var window = new FIleIncluderToolboxControl();

            //var message = string.Join(Environment.NewLine, files.Select(d => string.Join(Environment.NewLine, d.Value.Select(f => f.FullName))));

            string message;
            OLEMSGICON type;

            switch (executed)
            {
                case 0:
                    message = "Files executed";
                    type = OLEMSGICON.OLEMSGICON_INFO;
                    break;

                case -1:
                    message = "No files to execute";
                    type = OLEMSGICON.OLEMSGICON_INFO;
                    break;
                default:
                    message = executed + " files head errors";
                    type = OLEMSGICON.OLEMSGICON_CRITICAL;
                    break;
            }

            //// Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                "Execute sql files",
                type,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private void OpenCofigFile(object sender, EventArgs e)
        {
            var file = Workspace.ConfigFileFullName;
            if (!File.Exists(file))
            {
                var fs = File.Create(file);
                fs.Close();
            }

            if (!Workspace.SolutionDte.ItemOperations.IsFileOpen(file))
                Workspace.SolutionDte.ItemOperations.OpenFile(file);
        }
    }
}
