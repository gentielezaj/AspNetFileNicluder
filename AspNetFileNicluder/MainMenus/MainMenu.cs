using System;
using System.ComponentModel.Design;
using AspNetFileNicluder.Logic.ChangeConstant;
using AspNetFileNicluder.Logic.Configs;
using AspNetFileNicluder.Logic.Includers;
using AspNetFileNicluder.Logic.SQL;
using AspNetFileNicluder.Logic.TfsIncluders;
using AspNetFileNicluder.Logic.Util;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
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
        public const int RunSqlFolder = 0x0104;
        public const int ChangeConstant = 0x0105;
        public const int FileIncuderId = 0x0102;
        public const int TfsSqlCommandId = 0x0106;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("ca7faa7a-f352-4253-8cc5-f3d8846e6d00");
        public static readonly Guid ToolbarSet = new Guid("50986476-bba8-435e-a104-81c298efc4ef");

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

            var toolbarComand = new CommandID(ToolbarSet, CommandId);
            var toolbarMenu = new MenuCommand(this.Execute, toolbarComand);
            commandService.AddCommand(toolbarMenu);

            var menuCommandID1 = new CommandID(CommandSet, OpenConfigFile);
            var menuItem1 = new MenuCommand(this.OpenFolderRunner, menuCommandID1);
            commandService.AddCommand(menuItem1);

            var menuCommandRunSqlFolder = new CommandID(CommandSet, RunSqlFolder);
            var menuItemRunSqlFolder = new MenuCommand(this.OpenFolderRunner, menuCommandRunSqlFolder);
            commandService.AddCommand(menuItemRunSqlFolder);

            var toolbarTfsSqlCommand = new CommandID(ToolbarSet, TfsSqlCommandId);
            var toolbarMenuTsSql = new MenuCommand(this.OpenMisingTfsFiles, toolbarTfsSqlCommand);
            commandService.AddCommand(toolbarMenuTsSql);

            var menuCommandTfsSqlCommand = new CommandID(CommandSet, TfsSqlCommandId);
            var menuItemTfsSqlMenu = new MenuCommand(this.OpenMisingTfsFiles, menuCommandTfsSqlCommand);
            commandService.AddCommand(menuItemTfsSqlMenu);

            var menuCommandChangeConstant = new CommandID(CommandSet, ChangeConstant);
            var menuItemChangeConstant = new MenuCommand(this.OpenChangeConstantDialog, menuCommandChangeConstant);
            commandService.AddCommand(menuItemChangeConstant);

            var menuFileIncluder = new CommandID(CommandSet, FileIncuderId);
            var menuItemFileIncuder = new MenuCommand(this.OpenFileDialog, menuFileIncluder);
            commandService.AddCommand(menuItemFileIncuder);
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

            OpenExecuteResultDialogMessage(executed);
        }

        private void OpenCofigFile(object sender, EventArgs e)
        {
            Config.OpenConfigFile(this.package);
            //var tool = new ToolboxControl1();
            //tool.ShowDialog();
        }

        private void OpenFileDialog(object sender, EventArgs e)
        {
            OpenDialog<FileIncluderToolWindowControl>();
        }

        private void OpenChangeConstantDialog(object sender, EventArgs e)
        {
            OpenDialog<ChangeConstantToolBoxControl>();
        }

        private void OpenDialog<T>()
            where T : DialogWindow
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!Logic.Util.Workspace.IsOpenSolution)
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    "No soluton opend",
                    "Execute sql files",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            IVsUIShell uiShell = (IVsUIShell)ServiceProvider.GetServiceAsync(typeof(SVsUIShell)).Result;

            Func<bool, bool> callback = OpenExecuteResultDialogMessage;
            var popup = Activator.CreateInstance(typeof(T), callback) as T;

            popup.IsCloseButtonEnabled = true;
            IntPtr hwnd;
            uiShell.GetDialogOwnerHwnd(out hwnd);
            popup.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            uiShell.EnableModeless(0);
            try
            {
                WindowHelper.ShowModal(popup, hwnd);
            }
            finally
            {
                // This will take place after the window is closed.
                uiShell.EnableModeless(1);
            }

        }

        private void OpenFolderRunner(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var settings = new Settings();
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.InitialDirectory = settings.Databases.FolderPickerDefaltPath; // Use current value for initial dir
            dialog.Title = "Select a Directory"; // instead of default "Save As"
            dialog.Filter = "Directory|*.this.directory"; // Prevents displaying files
            dialog.FileName = "select"; // Filename will then be "select.this.directory"
            dialog.ShowDialog();
            string path = dialog.FileName;
            // Remove fake filename from resulting path
            path = path.Replace("\\select.this.directory", "");
            path = path.Replace(".this.directory", "");
            // Our final value is in path

            var exexuted = new SqlRuner().ExecuteFromDirectoryPath(path);
            OpenExecuteResultDialogMessage(exexuted);
        }

        private void OpenMisingTfsFiles(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!Logic.Util.Workspace.IsOpenSolution)
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    "No soluton opend",
                    "Execute sql files",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }
            var tfsIncluder = new TfsIncluder();
            tfsIncluder.Execute();
        }

        private bool OpenExecuteResultDialogMessage(bool executeResult)
        {
            OpenExecuteResultDialogMessage(executeResult ? 0 : 1);
            return true;
        }

        private void OpenExecuteResultDialogMessage(int executeResult)
        {
            string message;
            OLEMSGICON type;

            switch (executeResult)
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
                    message = executeResult + " files head errors";
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
    }
}
