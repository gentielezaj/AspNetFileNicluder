using Microsoft.VisualStudio.Shell;

namespace AspNetFileNicluder.Logic.Util
{
    public abstract class  BaseExecuter
    {
        protected EnvDTE80.DTE2 Dte => Workspace.SolutionDte;
        protected Settings Settings;

        public BaseExecuter()
        {
            RefreshSettings();
        }

        public void RefreshSettings()
        {
            Settings = new Settings();
        }
    }
}
