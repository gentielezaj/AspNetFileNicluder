using AspNetFileNicluder.Logic.Util;
using EnvDTE;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Settings = AspNetFileNicluder.Logic.Util.Settings;
using System.Diagnostics;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Text;
using System.Threading;
using AspNetFileNicluder.Logic.Utils;
using System.Collections;
using System.Collections.Generic;
using static AspNetFileNicluder.Logic.Util.Settings;
using Microsoft.VisualStudio.Shell;

namespace AspNetFileNicluder.Logic.SQL
{
    public class SqlRuner : BaseExecuter
    {
        public SqlRuner() : base()
        {
        }

        #region ExecuteFromDirectoryPath
        public int ExecuteFromDirectoryPath(string path)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!Directory.Exists(path)) return -1;

            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);  //Directory.GetFileSystemEntries(path, "*", ).ToArray();

            return ExecuteFiles(files);
        } 
        #endregion

        public int Execute()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
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
                        var text = sel.Text;
                        if (Settings.Databases.SetDelimiterOnPanesAfterRead)
                        {
                            var indexOfDelimiter = text.LastIndexOf(AppConstants.DelimiterOnPanesAfterRead);
                            if (indexOfDelimiter > 0)
                                text = text.Substring(indexOfDelimiter);

                            pane.OutputString(Environment.NewLine + AppConstants.DelimiterOnPanesAfterRead + Environment.NewLine);
                        }
                        result += Environment.NewLine + text + Environment.NewLine;
                    }
                    catch (Exception e)
                    {
                        AppOutput.ConsoleWriteException(e);
                        throw;
                    }
                }
            }

            var rows = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            return ExecuteFiles(rows);
        }

        public int ExecuteString(string sqlScript, IEnumerable<DatabaseConnectionString> connectionStrings = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (string.IsNullOrWhiteSpace(sqlScript))
            {
                return 0;
            }

            var errorCount = -1;
            foreach (var connectionString in connectionStrings ?? Settings.Databases.ConnectionStrings)
            {
                errorCount = 0;
                System.Data.SqlClient.SqlConnection sqlConnection = new System.Data.SqlClient.SqlConnection(connectionString.ConnectionString);
                ServerConnection svrConnection = new ServerConnection(sqlConnection);
                Server server = new Server(svrConnection);
                AppOutput.ConsoleWriteLine("------ Database: " + sqlConnection.Database + " -------");
                try
                {
                    var a = server.ConnectionContext.ExecuteNonQuery(sqlScript) > -1;
                    AppOutput.ConsoleWriteLine("Executed script: " + sqlScript);
                }
                catch (Exception e)
                {
                    AppOutput.ConsoleWriteException(e, "Error script at: " + sqlScript);
                    throw;
                }

                sqlConnection.Close();
            }

            return errorCount;
        }

        public int ExecuteFiles(string[] files, IEnumerable<DatabaseConnectionString> connectionStrings = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var havFilesToExecute = false;
            var errorCount = 0;
            foreach (var connectionString in connectionStrings ?? Settings.Databases.ConnectionStrings)
            {
                System.Data.SqlClient.SqlConnection sqlConnection = new System.Data.SqlClient.SqlConnection(connectionString.ConnectionString);
                ServerConnection svrConnection = new ServerConnection(sqlConnection);
                Server server = new Server(svrConnection);
                AppOutput.ConsoleWriteLine(Environment.NewLine, "------ Database: " + sqlConnection.Database + " -------");
                foreach (var row in files)
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
                        System.Diagnostics.Process proc = new System.Diagnostics.Process();
                        try
                        {
                            var args = $" -S \"{sqlConnection.DataSource}\" -E -d \"{sqlConnection.Database}\" -i \"{filePath}\"";
                            ProcessStartInfo info = new ProcessStartInfo("sqlcmd", args);
                            info.UseShellExecute = false;
                            info.CreateNoWindow = true;
                            info.WindowStyle = ProcessWindowStyle.Hidden;
                            info.RedirectStandardOutput = true;
                            info.RedirectStandardError = true;
                            proc.StartInfo = info;

                            var output = new StringBuilder();
                            var error = new StringBuilder();
                            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                            {
                                proc.ErrorDataReceived += (sender, e) =>
                                {
                                    if (e.Data == null)
                                    {
                                        errorWaitHandle.Set();
                                    }
                                    else
                                    {
                                        error.AppendLine(e.Data);
                                    }
                                };

                                proc.OutputDataReceived += (sender, e) =>
                                {
                                    if (e.Data == null)
                                    {
                                        outputWaitHandle.Set();
                                    }
                                    else
                                    {
                                        output.AppendLine(e.Data);
                                    }
                                };

                                proc.Start();
                                proc.BeginErrorReadLine();
                                proc.BeginOutputReadLine();

                                proc.WaitForExit();
                            }

                            var errorString = error.ToString();
                            var outputString = output.ToString()?.ToLower();

                            var isSuccess = string.IsNullOrEmpty(errorString)
                                && (string.IsNullOrEmpty(outputString) || outputString.Contains("commands completed successfully") || Regex.IsMatch(outputString, "[(][0-9]+ row[s]* affected[)]", RegexOptions.Multiline));


                            if (isSuccess) AppOutput.ConsoleWriteLine("Executed sqlcmd: " + filePath);
                            else
                            {
                                errorCount++;
                                AppOutput.ConsoleWriteLine("-----------", "----- Error sqlcmd: " + filePath, $"{error.ToString()}{Environment.NewLine}{output.ToString()}", "-------------");
                            }
                        }
                        catch (Exception e)
                        {
                            AppOutput.ConsoleWriteException(e, "Error at: " + filePath);
                            throw;
                        }
                        finally
                        {
                            proc.Close();
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

            return havFilesToExecute ? errorCount : -1;
        }
    }
}
