using EasyCraft.Core;
using EasyCraft.Web;
using SharpFtpServer;
using System;
using System.Linq;

namespace EasyCraft
{
    class Program
    {
        static void Main(string[] args)
        {
            FastConsole.Init();
            Console.WriteLine(
@" =================================================
   _____                 ____            __ _   
  | ____|__ _ ___ _   _ / ___|_ __ __ _ / _| |_ 
  |  _| / _` / __| | | | |   | '__/ _` | |_| __|
  | |__| (_| \__ \ |_| | |___| | | (_| |  _| |_ 
  |_____\__,_|___/\__, |\____|_|  \__,_|_|  \__|
                  |___/                
 ============= Version : V " + EasyCraftInfo.VersionOut + @" ===============
 ============= Copyright Kengwang ==============
");
            int argc = args.Length;
            FastConsole.logLevel = FastConsoleLogLevel.all;
            for (int i = 0; i < argc; i++)
            {
                if (args[i] == "--loglevel")
                {
                    string l = args[i + 1];
                    switch (l)
                    {
                        case "all":
                            FastConsole.logLevel = FastConsoleLogLevel.all;
                            break;
                        case "noserver":
                            FastConsole.logLevel = FastConsoleLogLevel.noserver;
                            break;
                        case "no":
                            FastConsole.logLevel = FastConsoleLogLevel.no;
                            break;
                        case "notrash":
                            FastConsole.logLevel = FastConsoleLogLevel.notrash;
                            break;
                        default:
                            FastConsole.logLevel = FastConsoleLogLevel.all;
                            break;
                    }
                }
            }
            FastConsole.PrintInfo("Loading Language Pack");
            Language.LoadLanguagePack();
            FastConsole.PrintInfo(Language.t("加载配置表中"));
            Settings.LoadConfig();
            FastConsole.PrintInfo(Language.t("检查更新中"));
            Functions.CheckUpdate();
            FastConsole.PrintWarning(Language.t("您正在使用 EasyCraft 的 Alpha 版本,可能会不稳定"));
            FastConsole.PrintInfo(Language.t("加载权限表中"));
            Functions.InitDirectory();
            FastConsole.PrintInfo(Language.t("加载数据库中"));
            Database.Connect();
            FastConsole.PrintInfo(Language.t("加载设置项中"));
            Settings.LoadDatabase();
            FastConsole.PrintInfo(Language.t("加载服务器中"));
            ServerManager.LoadServers();
            FastConsole.PrintInfo(Language.t("加载主题中"));
            ThemeController.InitComp();
            ThemeController.InitPage();
#if false
            FastConsole.PrintInfo(Language.t("正在开启 WebSocket 服务器"));
            WebSocketListener.StartListen(); //No more WebSocket
#endif
            FastConsole.PrintInfo(Language.t("加载 PluginBase 中"));
            PluginBase.PluginBase.LoadPlugins();
            FastConsole.PrintInfo(Language.t("载入 Docker 中"));
            Settings.docker = new Docker.Docker();
            FastConsole.PrintInfo(Language.t("加载任务中"));
            Schedule.LoadSchedule();
            Schedule.StartTrigger();
            FastConsole.PrintInfo(Language.t("正在开启 FTP 服务器"));
            FtpServer.server = new SharpFtpServer.FtpServer();
            FtpServer.server.Start();
            FastConsole.PrintInfo(Language.t("正在开启 HTTP 服务器"));
            HTTPServer.StartListen();
            Settings.LoadStarted();
            Console.CancelKeyPress += ExitEasyCraft;
            string c = "";
            while ((c = Console.ReadLine()) != "exit")
            {
                try
                {
                    CommandPhrase.PhraseCommand(c);
                }
                catch (Exception e)
                {
                    FastConsole.PrintError("Command Error: " + e.Message);
                }
            }
            ExitEasyCraft(null, null);
        }

        private static void ExitEasyCraft(object sender, ConsoleCancelEventArgs e)
        {
            FastConsole.PrintInfo("关闭 EasyCraft 中");
            FastConsole.PrintTrash("关闭所有服务器中");
            foreach (var server in ServerManager.servers)
            {
                server.Value.Stop();
            }
            FastConsole.PrintTrash("停止定时任务中");
            HTTPServer.StopListen();
            FtpServer.server.Stop();
            Environment.Exit(0);
        }
    }
}
