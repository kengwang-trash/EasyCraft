using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EasyCraft.Core
{
    class Server
    {
        public int id;
        public string name = "EasyCraft Server";
        public int owner;
        public int port;
        public string core;

        public int maxplayer;
        public int ram;

        public bool autostart = false;

        public string world = "world";
        DateTime expiretime = DateTime.MaxValue;

        Process process = null;
        string log;

        Core c;
        public Server(int id)
        {
            this.id = id;
            RefreshServerConfig();
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
            }
            else
            {
                FastConsole.PrintWarning(string.Format(Language.t("Server {0} Load Failed."), id));
            }

        }

        private void PrintLog(string message)
        {
            log += message;
            if (Settings.logLevel == LogLevel.all)
                FastConsole.PrintInfo("[server" + id + "] " + message);
        }

        private void PrintError(string message)
        {
            log += message;
            if (Settings.logLevel == LogLevel.all)
                FastConsole.PrintWarning("[server" + id + "] " + message);
        }

        public void Start()
        {
            try
            {
                c = new Core(core);

            }
            catch (Exception e)
            {
                PrintError(string.Format(Language.t("Core {0} Load Failed: {1}"), core, e.Message));
                return;
            }

            process = new Process();
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.StartInfo.CreateNoWindow = true;


            if (c.usecmd || c.multicommand)
            {
                process.StartInfo.FileName = Environment.OSVersion.Platform == PlatformID.Win32NT ? "cmd.exe" : "bash";
                process.StartInfo.RedirectStandardInput = true;
            }
            else
            {
                process.StartInfo.FileName = c.path;
                process.StartInfo.Arguments = c.argument;
            }

            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                PrintError(string.Format(Language.t("Cannot Start Server: {0}"), e.Message));
            }
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
