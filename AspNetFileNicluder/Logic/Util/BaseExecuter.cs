using Microsoft.VisualStudio.Shell;

namespace AspNetFileNicluder.Logic.Util
{
    public abstract class  BaseExecuter
    {
        protected EnvDTE80.DTE2 Dte => Workspace.SolutionDte;
        protected readonly Settings Settings;

        public BaseExecuter()
        {
            Settings = new Settings();
        }
    }
}
