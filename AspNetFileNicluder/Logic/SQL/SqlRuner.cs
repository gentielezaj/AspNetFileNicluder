using AspNetFileNicluder.Logic.Util;
using EnvDTE;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.VisualStudio.Shell;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Settings = AspNetFileNicluder.Logic.Util.Settings;
using System.Diagnostics;

namespace AspNetFileNicluder.Logic.SQL
{
    public class SqlRuner : BaseExecuter
    {
        public SqlRuner() : base()
        {
        }

        public bool Execute()
        {
            var text = GetOutput(); // File.ReadAllText(@"C:\Users\gogi_\Desktop\source control.txt"); // GetOutput();

            var rows = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var havFilesToExecute = false;

            foreach (var connectionString in Settings.Databases.ConnectionStrings)
            {
                SqlConnection sqlConnection = new SqlConnection(connectionString.ConnectionString);
                ServerConnection svrConnection = new ServerConnection(sqlConnection);
                Server server = new Server(svrConnection);
                foreach (var row in rows)
                {
                    if (!string.IsNullOrWhiteSpace(connectionString.FilterPattern) && !Regex.IsMatch(row, connectionString.FilterPattern))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(connectionString.IgnorePattern) && Regex.IsMatch(row, connectionString.IgnorePattern))
                    {
                        continue;
                    }

                    havFilesToExecute = true;

                    var filePath = string.IsNullOrWhiteSpace(connectionString.ReplasePattern)
                        ? connectionString.ReplasePattern
                        : Regex.Replace(row, connectionString.ReplasePattern, string.Empty);

                    if (!string.IsNullOrWhiteSpace(connectionString.SqlCmdPattern) && Regex.IsMatch(row, connectionString.SqlCmdPattern))
                    {
                        try
                        {
                            ProcessStartInfo info = new ProcessStartInfo("sqlcmd", $" -S \"{connectionString.ConnectionString}\" -i \"{filePath}\"");
                            info.UseShellExecute = false;
                            info.CreateNoWindow = true;
                            info.WindowStyle = ProcessWindowStyle.Hidden;
                            info.RedirectStandardOutput = true;
                            System.Diagnostics.Process proc = new System.Diagnostics.Process();
                            proc.StartInfo = info;
                            proc.Start();
                            //proc.WaitForExit();

                            AppOutput.ConsoleWriteLine("Executed sqlcmd: " + filePath);
                        }
                        catch (Exception e)
                        {
                            AppOutput.ConsoleWriteException(e, "Error at: " + filePath);
                            throw;
                        }
                        continue;
                    }

                    if (File.Exists(filePath))
                    {
                        var sqlScript = File.ReadAllText(filePath);
                        try
                        {
                            var a = server.ConnectionContext.ExecuteNonQuery(sqlScript) > -1;
                            AppOutput.ConsoleWriteLine("Executed: " + filePath);
                        }
                        catch (Exception e)
                        {
                            AppOutput.ConsoleWriteException(e, "Error at: " + filePath);
                            throw;
                        }
                    }
                    else
                    {
                        AppOutput.ConsoleWriteLine("File dont exists: " + filePath);
                    }
                }

                sqlConnection.Close();
            }

            return havFilesToExecute;
        }

        private string GetOutput()
        {
            EnvDTE.OutputWindowPanes panes = Dte.ToolWindows.OutputWindow.OutputWindowPanes;

            var result = string.Empty;
            foreach (OutputWindowPane pane in panes)
            {
                if (Settings.Databases.Panes.Contains(pane.Name))
                {
                    try
                    {
                        pane.Activate();
                        var sel = pane.TextDocument.Selection;
                        sel.StartOfDocument(false);
                        sel.EndOfDocument(true);
                        result += sel.Text + "\n";
                    }
                    catch (Exception e)
                    {
                        //AppOutput.ConsoleWriteException(e);
                        //throw;
                        result = File.ReadAllText("C:\\Users\\gogi_\\Desktop\\source control.txt");
                    }
                }
            }

            return result;
        }
    }
}
