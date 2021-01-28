using System;
using System.Diagnostics;
using System.IO;
using EasyCraft.Web.Classes;
using Newtonsoft.Json;

namespace EasyCraft.Core
{
    internal class EasyCraftInfo
    {
        public static readonly string SoftName = "EasyCraft";
        public static readonly string SoftNameZh = "易开服";
        public static readonly string VersionFull = "1.0.0.0";
        public static readonly string VersionOut = "1.0.0";
        public static readonly string CommitID = "{COMMITID}";
        public static readonly string Builder = "{BUILDER}";
        public static readonly string BuildID = "{BUILDID}";
        public static readonly string BuildOS = "{BUILDOS}";
        public static readonly string BuildTime = "{BUILDTIME}";
        public static readonly string Copyright = "EasyCraft Team 2021,Made with Love by Kengwang";
    }

    internal class Settings
    {
        private static SettingsFile sf = new SettingsFile();

        public static readonly string release = "Personal";

        //$info="Build by Azure DevOps on $(Agent.OS)\r\nBuild Time: $(Get-Date)\r\nBuild ID: $(Build.BuildNumber)\r\nCommit: $(Build.SourceVersion)\r\nCopyright Kengwang $(Get-Date -Format 'yyyy')"
        public static readonly string BUILDINFO = string.Format(
            "{0} {1} V{2}\r\nBuild by {3} on {4}\r\nBuild Time: {5}\r\nBuild ID: {6}\r\nCommit: {7}\r\nCopyright {8}",
            EasyCraftInfo.SoftName, EasyCraftInfo.SoftNameZh, EasyCraftInfo.VersionFull, EasyCraftInfo.Builder,
            EasyCraftInfo.BuildOS, EasyCraftInfo.BuildTime, EasyCraftInfo.BuildID, EasyCraftInfo.CommitID,
            EasyCraftInfo.Copyright);

        public static int httpport
        {
            get
            {
                if (sf.HTTP != null && sf.HTTP.port != 0)
                    return sf.HTTP.port;
                return 80;
            }
        }

        public static int ftpport
        {
            get
            {
                if (sf.FTP != null && sf.FTP.port != 0)
                    return sf.FTP.port;
                return 21;
            }
        }

        public static string remoteip
        {
            get
            {
                if (sf.FTP != null && !string.IsNullOrEmpty(sf.FTP.remote_addr)) return sf.FTP.remote_addr;

                FastConsole.PrintWarning(Language.t("FTP 远端地址未设置! FTP 被动模式可能无法运行!"));
                return "0.0.0.0";
            }
        }

        public static string key
        {
            get
            {
                if (sf != null && !string.IsNullOrEmpty(sf.key))
                    return sf.key;
                return "No KEY";
            }
            set
            {
                sf.key = key;
                File.WriteAllText("easycraft.conf", JsonConvert.SerializeObject(sf));
            }
        }

        public static void LoadConfig()
        {
            try
            {
                if (File.Exists("easycraft.conf"))
                {
                    sf = JsonConvert.DeserializeObject<SettingsFile>(File.ReadAllText("easycraft.conf"));
                }
                else
                {
                    #if WINDOWS
                    FastConsole.PrintWarning(Language.t("未找到配置文件,请运行 install.exe 进行安装"));
                    #else
                    FastConsole.PrintWarning(Language.t("未找到配置文件,请运行 `bash install.sh` 进行安装"));
                    #endif
                    Environment.Exit(-5);
                }
            }
            catch (Exception e)
            {
                FastConsole.PrintError(string.Format(Language.t("配置文件加载错误: {0}"), e.Message));
            }
        }

        public static void LoadDatabase()
        {
            var c = Database.DB.CreateCommand();
            c.CommandText = "SELECT * FROM settings";
            var render = c.ExecuteReader();
            while (render.Read()) SettingsDatabase.annoucement = render.GetString(0);

            User.RefreshPermissonTable();
        }

        public static void LoadStarted()
        {
            if (File.Exists("data/tools/startup.list"))
            {
                foreach (var cmd in File.ReadAllLines("data/tools/startup.list"))
                {
                    var process = new Process();
                    process.StartInfo.FileName = "cmd";
                    process.StartInfo.WorkingDirectory = "data/tools/";
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.OutputDataReceived += StandardOutput;
                    process.ErrorDataReceived += StandardOutput;
                    process.Start();
                    process.StandardInput.Write(cmd + "\r\n");
                }

                FastConsole.PrintSuccess("Successful Load Startup Event");
            }
        }

        private static void StandardOutput(object sender, DataReceivedEventArgs e)
        {
            FastConsole.PrintTrash("[STARTUP] " + e.Data);
        }
    }

    internal class SettingsDatabase
    {
        public static string annoucement = "";
    }

    internal class SettingsFile
    {
        public HTTPConf HTTP { get; set; }
        public FTPConf FTP { get; set; }
        public AdvancedConfig Advanced { get; set; }
        public string key { get; set; }
    }

    internal class HTTPConf
    {
        public int port { get; set; }
        public bool https { get; set; } //Not Support yet
    }

    internal class AdvancedConfig
    {
        public bool usesandboxie { get; set; } //Not Support yet
    }

    internal class FTPConf
    {
        public int port { get; set; }
        public string remote_addr { get; set; }
    }
}