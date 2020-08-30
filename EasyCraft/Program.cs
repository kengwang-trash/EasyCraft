﻿using EasyCraft.Core;
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
            Console.WriteLine(
@" =================================================
   _____                 ____            __ _   
  | ____|__ _ ___ _   _ / ___|_ __ __ _ / _| |_ 
  |  _| / _` / __| | | | |   | '__/ _` | |_| __|
  | |__| (_| \__ \ |_| | |___| | | (_| |  _| |_ 
  |_____\__,_|___/\__, |\____|_|  \__,_|_|  \__|
                  |___/                
 ============= Version : V " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + @" ===============
 ============== Copyright Kengwang ===============
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
            FastConsole.PrintInfo(Language.t("加载权限中"));
            Functions.InitDirectory();
            FastConsole.PrintInfo(Language.t("加载数据库中"));
            Database.Connect();
            FastConsole.PrintInfo(Language.t("加载服务器中"));
            ServerManager.LoadServers();
            FastConsole.PrintInfo(Language.t("加载主题中"));
            ThemeController.InitComp();
            ThemeController.InitPage();
            //FastConsole.PrintInfo(Language.t("正在开启 WebSocket 服务器"));
            //WebSocketListener.StartListen(); //No more WebSocket
            FastConsole.PrintInfo(Language.t("正在开启 FTP 服务器"));
            FtpServer.server = new SharpFtpServer.FtpServer();
            FtpServer.server.Start();
            FastConsole.PrintInfo(Language.t("正在开启 HTTP 服务器"));
            HTTPServer.StartListen();
            string c = "";
            while ((c = Console.ReadLine()) != "exit")
            {
                try
                {
                    CommandPhrase.PhraseCommand(c);

                }catch (Exception e)
                {
                    FastConsole.PrintError("Command Error: " + e.Message);
                }


            }
            FastConsole.PrintInfo("关闭 EasyCraft 中");
            HTTPServer.StopListen();
            FtpServer.server.Stop();
        }
    }
}
