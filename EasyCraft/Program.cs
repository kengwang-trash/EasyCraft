using EasyCraft.Core;
using EasyCraft.Web;
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
            Settings.logLevel = LogLevel.all;
            for (int i = 0; i < argc; i++)
            {
                if (args[i] == "--loglevel")
                {
                    string l = args[i + 1];
                    switch (l)
                    {
                        case "all":
                            Settings.logLevel = LogLevel.all;
                            break;
                        case "noserver":
                            Settings.logLevel = LogLevel.noserver;
                            break;
                        case "no":
                            Settings.logLevel = LogLevel.no;
                            break;
                        case "notrash":
                            Settings.logLevel = LogLevel.notrash;
                            break;
                        default:
                            Settings.logLevel = LogLevel.all;
                            break;
                    }
                }
            }
            FastConsole.PrintInfo("Loading Language Pack");
            Language.LoadLanguagePack();
            FastConsole.PrintInfo(Language.t("Checking Update"));
            Functions.CheckUpdate();
            FastConsole.PrintWarning(Language.t("You are running the alpha version of EasyCraft, it's not stable"));
            FastConsole.PrintInfo(Language.t("Initialize Directories"));
            Functions.InitDirectory();
            FastConsole.PrintInfo(Language.t("Loading Database"));
            Database.Connect();
            FastConsole.PrintInfo(Language.t("Loading Servers"));
            ServerManager.LoadServers();
            FastConsole.PrintInfo(Language.t("Starting HTTP Server"));
            HTTPServer.StartListen();
            string c = "";
            while ((c = Console.ReadLine()) != "exit")
            {
                FastConsole.PrintTrash("You Inputed " + c);
            }
        }
    }
}
