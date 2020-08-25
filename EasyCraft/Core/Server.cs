using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using EasyCraft.Web.JSONCallback;

namespace EasyCraft.Core
{
    class Server
    {
        public int id;
        public string name = "EasyCraft Server";
        public int owner;
        public int port;
        public string core;

        public int maxplayer = 10;
        public int ram = 1024;
        public bool running
        {
            get
            {
                if (process == null) return false;
                return !process.HasExited;
            }
        }

        public bool autostart = false;

        public string world = "world";
        public DateTime expiretime = DateTime.MaxValue;

        Process process = null;
        string lastcore = "";
        string serverdir = "";


        public Dictionary<long, ServerLog> log = new Dictionary<long, ServerLog>();

        Core c;
        public Server(int id)
        {
            this.id = id;
            RefreshServerConfig();
        }

        public static int CreateServer()
        {
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText = "INSERT INTO `server` (`name`,`expiretime`) VALUES ('EasyCraft Server', $1 );select last_insert_rowid();";
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
            c.CommandText = "UPDATE `server` SET name = $name , owner = $owner , port = $port , core = $core , maxplayer = $maxplayer , ram = $ram , world = $world , expiretime = $expiretime , autostart = $autostart , lastcore = $lastcore WHERE id = $id ";
            c.Parameters.AddWithValue("$id", id);
            c.Parameters.AddWithValue("$name", name);
            c.Parameters.AddWithValue("$owner", owner);
            c.Parameters.AddWithValue("$port", port);
            c.Parameters.AddWithValue("$core", core);
            c.Parameters.AddWithValue("$maxplayer", maxplayer);
            c.Parameters.AddWithValue("$ram", ram);
            c.Parameters.AddWithValue("$world", world);
            c.Parameters.AddWithValue("$expiretime", expiretime);
            c.Parameters.AddWithValue("$autostart", autostart);
            c.Parameters.AddWithValue("$lastcore", lastcore);
            c.ExecuteNonQuery();
        }

        public void RefreshServerConfig()
        {
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText = "SELECT * FROM server WHERE id = $id ";
            c.Parameters.AddWithValue("$id", id);
            SQLiteDataReader render = c.ExecuteReader();

            if (render.Read())
            {
                name = render.GetString(1);
                owner = render.GetInt32(2);
                port = render.GetInt32(3);
                core = render.GetString(4);
                maxplayer = render.GetInt32(5);
                ram = render.GetInt32(6);
                world = render.GetString(7);
                expiretime = render.GetDateTime(8);
                autostart = render.GetBoolean(9);
                lastcore = render.GetString(10);
            }
            else
            {
                FastConsole.PrintWarning(string.Format(Language.t("Server {0} Load Failed."), id));
            }

            serverdir = Environment.CurrentDirectory + "/server/server" + id.ToString() + "/";

            System.IO.Directory.CreateDirectory(serverdir);
        }

        public void ClearLog()
        {
            log.Clear();
            PrintLog(Language.t("Successfully to clear logs on server"));
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
                FastConsole.PrintInfo("[server" + id + "] " + message);
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
                FastConsole.PrintWarning("[server" + id + "] " + message);
        }

        private string PhraseServerCommand(string cmd)
        {
            if (cmd == null) return "";
            cmd = cmd.Replace("{SERVER_DIR}", serverdir);
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
            process.Kill();
        }

        public void Send(string cmd)
        {
            if (process == null || process.HasExited == true) return;
            process.StandardInput.Write(cmd);
        }

        public void Start()
        {
            if ((expiretime - DateTime.Now).TotalSeconds < 0)
            {
                PrintError(string.Format(Language.t("Server Expired at {0}, Cannot strat server"), expiretime.ToString()));
                return;
            }
            try
            {
                c = new Core(core);

            }
            catch (Exception e)
            {
                PrintError(string.Format(Language.t("Core {0} Load Failed: {1}"), core, e.Message));
                return;
            }

            if (core != lastcore)
            {//新核心需要初始化
                if (c.initcopy)
                {
                    PrintLog("Copying Core Required Files");
                    Functions.CopyDirectory("core/" + core + "/files/", serverdir);
                }
                lastcore = core;
                SaveServerConfig();
            }

            process = new Process();
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;  // 重定向输入
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.StartInfo.CreateNoWindow = true;
            process.Exited += Process_Exited;


            if (c.usecmd || c.multicommand)
            {
                process.StartInfo.FileName = Environment.OSVersion.Platform == PlatformID.Win32NT ? "cmd.exe" : "bash";
                process.StartInfo.RedirectStandardInput = true;
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
                if (c.multicommand)
                {
                    foreach (string com in c.commands)
                    {
                        process.StandardInput.WriteLineAsync(PhraseServerCommand(com));
                    }
                }
            }
            catch (Exception e)
            {
                PrintError(string.Format(Language.t("Cannot Start Server: {0}"), e.Message));
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            PrintLog("Server Stopped");
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
