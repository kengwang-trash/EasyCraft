using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using EasyCraft.Web.JSONCallback;

namespace EasyCraft.Core
{
    class Server
    {
        public readonly int Id;
        public string Name = "EasyCraft Server";
        public int Owner;
        public int Port;
        public string Core;

        public int Maxplayer = 10;
        public int Ram = 1024;

        public bool Running
        {
            get
            {
                try
                {
                    if (process == null) return false;
                    return !process.HasExited;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool Autostart = false;

        public string World = "world";
        public DateTime Expiretime = DateTime.MaxValue;

        Process process = null;
        string lastcore = "";
        string serverdir = "";


        public Dictionary<long, ServerLog> log = new Dictionary<long, ServerLog>();

        Core c;

        public Server(int id)
        {
            this.Id = id;
            RefreshServerConfig();
        }

        public static int CreateServer()
        {
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText =
                "INSERT INTO `server` (`name`,`expiretime`) VALUES ('EasyCraft Server', $1 );select last_insert_rowid();";
            c.Parameters.AddWithValue("$1", DateTime.Now);
            SQLiteDataReader r = c.ExecuteReader();
            if (r.Read())
            {
                return r.GetInt32(0);
            }
            else
            {
                throw new Exception("Failed to create");
            }
        }

        public void SaveServerConfig()
        {
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText =
                "UPDATE `server` SET name = $name , owner = $owner , port = $port , core = $core , maxplayer = $maxplayer , ram = $ram , world = $world , expiretime = $expiretime , autostart = $autostart , lastcore = $lastcore WHERE id = $id ";
            c.Parameters.AddWithValue("$id", Id);
            c.Parameters.AddWithValue("$name", Name);
            c.Parameters.AddWithValue("$owner", Owner);
            c.Parameters.AddWithValue("$port", Port);
            c.Parameters.AddWithValue("$core", Core);
            c.Parameters.AddWithValue("$maxplayer", Maxplayer);
            c.Parameters.AddWithValue("$ram", Ram);
            c.Parameters.AddWithValue("$world", World);
            c.Parameters.AddWithValue("$expiretime", Expiretime);
            c.Parameters.AddWithValue("$autostart", Autostart);
            c.Parameters.AddWithValue("$lastcore", lastcore);
            c.ExecuteNonQuery();
        }

        public void RefreshServerConfig()
        {
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText = "SELECT * FROM server WHERE id = $id ";
            c.Parameters.AddWithValue("$id", Id);
            SQLiteDataReader render = c.ExecuteReader();

            if (render.Read())
            {
                Name = render.GetString(1);
                Owner = render.GetInt32(2);
                Port = render.GetInt32(3);
                Core = render.GetString(4);
                Maxplayer = render.GetInt32(5);
                Ram = render.GetInt32(6);
                World = render.GetString(7);
                Expiretime = render.GetDateTime(8);
                Autostart = render.GetBoolean(9);
                lastcore = render.GetString(10);
            }
            else
            {
                FastConsole.PrintWarning(string.Format(Language.t("服务器 {0} 加载失败."), Id));
            }

            serverdir = Environment.CurrentDirectory + "/data/server/server" + Id.ToString() + "/";

            System.IO.Directory.CreateDirectory(serverdir);

            try
            {
                this.c = new Core(Core);
            }
            catch (Exception)
            {
            }
        }

        public void ClearLog()
        {
            log.Clear();
            PrintLog(Language.t("成功清理日志缓存"));
        }

        private void PrintLog(string message)
        {
            if (message == null) return;
            ServerLog l = new ServerLog();
            l.iserror = false;
            l.message = message;
            l.time = DateTime.Now;
            log.Add(l.id, l);
            if (FastConsole.logLevel == FastConsoleLogLevel.all)
                FastConsole.PrintInfo("[server" + Id + "] " + message);
        }

        private void PrintError(string message)
        {
            if (message == null) return;
            ServerLog l = new ServerLog();
            l.iserror = true;
            l.message = message;
            l.time = DateTime.Now;
            log.Add(l.id, l);
            if (FastConsole.logLevel == FastConsoleLogLevel.all)
                FastConsole.PrintWarning("[server" + Id + "] " + message);
        }

        private string PhraseServerCommand(string cmd)
        {
            if (cmd == null) return "";
            cmd = cmd.Replace("{SERVER_DIR}", serverdir);
            cmd = cmd.Replace("{PORT}", Port.ToString());
            cmd = cmd.Replace("{SERVER_ID}", Id.ToString());
            cmd = cmd.Replace("{WORLD}", World);
            cmd = cmd.Replace("{RAM}", Ram.ToString());
            cmd = cmd.Replace("{PLAYER}", Maxplayer.ToString());
            cmd = cmd.Replace("{COREPATH}", Path.GetFullPath("data/core/" + Core));
            return cmd;
        }

        public void Stop()
        {
            if (process == null || process.HasExited == true) return;
            process.StandardInput.Write("stop\r\n");
        }

        public void Kill()
        {
            if (process == null || process.HasExited == true) return;
            process.Kill(true);
        }

        public void KillAll()
        {
            //这个不建议使用,我只是用着来玩玩哦~
            //具体开不开放给普通用户我也不知道呢~
            //这个功能是把服务器下所有的进程全部结束
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                try
                {
                    if (process.MainModule.FileName.Replace('/', '\\').Contains(serverdir.Replace('/', '\\')))
                    {
                        process.Kill(true);
                        PrintLog(string.Format(Language.t("成功结束进程 {0} (pid:{1})"), process.ProcessName, process.Id));
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        public void Send(string cmd)
        {
            if (process == null || process.HasExited == true) return;
            process.StandardInput.Write(cmd);
        }

        public void Start()
        {
            try
            {
                if ((Expiretime - DateTime.Now).TotalSeconds < 0)
                {
                    PrintError(string.Format(Language.t("服务器已于 {0} 过期, 无法开启服务器"), Expiretime.ToString()));
                    return;
                }

                try
                {
                    c = new Core(Core);
                }
                catch (Exception e)
                {
                    PrintError(string.Format(Language.t("核心 {0} 加载失败: {1}"), Core, e.Message));
                    return;
                }

                if (Core != lastcore)
                {
                    //新核心需要初始化
                    if (c.initcopy)
                    {
                        PrintLog(Language.t("载入核心必须文件"));
                        Functions.CopyDirectory("data/core/" + Core + "/files/", serverdir);
                    }

                    lastcore = Core;
                    SaveServerConfig();
                }

                //更改server.properties
                PrintLog(Language.t("处理 server.properties 中"));
                if (!File.Exists(serverdir + "/server.properties") &&
                    File.Exists("data/core/" + Core + "/server.properties"))
                    File.Copy("data/core/" + Core + "/server.properties", serverdir + "/server.properties");
                if (c.corestruct.serverproperties != null)
                {
                    List<string> lines = new List<string>();
                    List<string> convertedname = new List<string>();
                    if (File.Exists(serverdir + "/server.properties"))
                    {
                        foreach (string line in File.ReadAllLines(serverdir + "/server.properties"))
                        {
                            if (line.StartsWith("#") || line.IndexOf("=") == -1)
                            {
                                lines.Add(line);
                                continue;
                            }

                            string name = line.Substring(0, line.IndexOf("="));
                            convertedname.Add(name);
                            if (c.corestruct.serverproperties.ContainsKey(name))
                            {
                                if (c.corestruct.serverproperties[name].isvar)
                                {
                                    if (c.corestruct.serverproperties[name].what != "{REMOVE}")
                                    {
                                        lines.Add(
                                            name + "=" +
                                            PhraseServerCommand(c.corestruct.serverproperties[name].what));
                                    }

                                    continue;
                                }
                            }

                            lines.Add(line);
                        }
                    }

                    List<string> reconvert = c.corestruct.serverproperties.Keys.Except(convertedname).ToList();
                    foreach (string key in reconvert)
                    {
                        if (c.corestruct.serverproperties.ContainsKey(key))
                        {
                            if (c.corestruct.serverproperties[key].isvar)
                            {
                                lines.Add(key + "=" + PhraseServerCommand(c.corestruct.serverproperties[key].what));
                                continue;
                            }
                            else if (!string.IsNullOrEmpty(c.corestruct.serverproperties[key].defvalue))
                            {
                                lines.Add(key + "=" + c.corestruct.serverproperties[key].defvalue);
                                continue;
                            }
                        }
                    }

                    File.WriteAllLines(serverdir + "/server.properties", lines);
                }

                PrintLog(Language.t("即将开启服务器"));
                process = new Process();
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.WorkingDirectory = serverdir;
                process.StartInfo.RedirectStandardInput = true; // 重定向输入
                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.OutputDataReceived += Process_OutputDataReceived;
                process.StartInfo.CreateNoWindow = true;
                process.Exited += Process_Exited;


                if (c.usecmd || c.multicommand)
                {
                    process.StartInfo.FileName =
                        Environment.OSVersion.Platform == PlatformID.Win32NT ? "cmd.exe" : "bash";
                    process.StartInfo.RedirectStandardInput = true;
                    if (c.multicommand)
                    {
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            if (File.Exists(serverdir + "/start.bat")) File.Delete(serverdir + "/start.bat");
                            File.AppendAllText(serverdir + "start.bat", "@echo off\r\n");
                        }
                        else
                        {
                            if (File.Exists(serverdir + "/start.sh")) File.Delete(serverdir + "/start.sh");

                            File.AppendAllText(serverdir + "start.bash", "#!/bin/bash\r\n");
                        }

                        foreach (string com in c.commands)
                        {
                            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                            {
                                File.AppendAllText(serverdir + "start.bat", PhraseServerCommand(com) + "\r\n");
                            }
                            else
                            {
                                File.AppendAllText(serverdir + "start.sh", PhraseServerCommand(com) + "\r\n");
                            }
                        }
                    }
                    else
                    {
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            if (File.Exists(serverdir + "/start.bat")) File.Delete(serverdir + "/start.bat");
                            File.AppendAllText(serverdir + "start.bat", "@echo off\r\n");
                        }
                        else
                        {
                            if (File.Exists(serverdir + "/start.sh")) File.Delete(serverdir + "/start.sh");

                            File.AppendAllText(serverdir + "start.sh", "#!/bin/bash\r\n");
                        }


                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            File.AppendAllText(serverdir + "start.bat",
                                PhraseServerCommand(c.path) + " " + PhraseServerCommand(c.argument));
                        }
                        else
                        {
                            File.AppendAllText(serverdir + "start.sh",
                                PhraseServerCommand(c.path) + " " + PhraseServerCommand(c.argument));
                        }

                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            process.StartInfo.FileName = serverdir + "/start.bat";
                        }
                        else
                        {
                            process.StartInfo.FileName = "/bin/bash";
                            process.StartInfo.Arguments = serverdir + "/start.sh";
                        }
                    }
                }
            
                else
                {
                    process.StartInfo.FileName = PhraseServerCommand(c.path);
                    process.StartInfo.Arguments = PhraseServerCommand(c.argument);
                }

                try
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    PrintLog(Language.t("服务器已开启"));
                }
                catch (Exception e)
                {
                    PrintError(string.Format(Language.t("无法启动服务器: {0}"), e.Message));
                }
            }
            catch (Exception e)
            {
                PrintError(string.Format(Language.t("无法启动服务器: {0}"), e.Message));
#if DEBUG
                FastConsole.PrintError(e.StackTrace);
#endif
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            PrintLog(Language.t("服务器已停止"));
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            PrintLog(e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            PrintError(e.Data);
        }
    }
}