namespace AspNetFileNicluder.Logic.Includers
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("d9eda215-6ab6-480c-953b-6a3d7c35e1af")]
    public class FileIncluderToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileIncluderToolWindow"/> class.
        /// </summary>
        public FileIncluderToolWindow() : base(null)
        {
            this.Caption = "FileIncluderToolWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new FileIncluderToolWindowControl();
        }
    }
}
