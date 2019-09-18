﻿using AspNetFileNicluder.Logic.Util;
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

namespace AspNetFileNicluder.Logic.SQL
{
    public class SqlRuner : BaseExecuter
    {
        public SqlRuner() : base()
        {
        }

        public int Execute()
        {
            var text = GetOutput();

#if DEBUG
            text += Environment.NewLine + File.ReadAllText(@"D:\Projects\vs extandions\AspNetFileNicluder\AspNetFileNicluder\ExapleData\source control.txt");
#endif

            var rows = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var havFilesToExecute = false;
            var errorCount = 0;
            foreach (var connectionString in Settings.Databases.ConnectionStrings)
            {
                System.Data.SqlClient.SqlConnection sqlConnection = new System.Data.SqlClient.SqlConnection(connectionString.ConnectionString);
                ServerConnection svrConnection = new ServerConnection(sqlConnection);
                Server server = new Server(svrConnection);
                AppOutput.ConsoleWriteLine(Environment.NewLine, "------ Database: " + sqlConnection.Database + " -------");
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
