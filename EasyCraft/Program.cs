using System;
using EasyCraft.Base.Server;
using EasyCraft.Base.User;
using EasyCraft.HttpServer.Api;
using EasyCraft.Utils;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace EasyCraft
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CancelKeyPress += ExitEasyCraft;
            Console.WriteLine(@"
 _____                 ____            __ _
| ____|__ _ ___ _   _ / ___|_ __ __ _ / _| |_
|  _| / _` / __| | | | |   | '__/ _` | |_| __|
| |__| (_| \__ \ |_| | |___| | | (_| |  _| |_
|_____\__,_|___/\__, |\____|_|  \__,_|_|  \__|
                |___/");
            Console.WriteLine(@"
== Version " + Common.VERSIONFULL + "  (" + Common.VERSIONNAME + @") ==
 == Developed by EasyCraft Team ==
  ==    Under GPL v3 Licence   ==");
            Console.WriteLine("Loading Language Pack");
            Translation.LoadTranslation();
            Console.WriteLine("正在加载日志组件".Translate());
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("/logs/log-.log", LogEventLevel.Information, rollingInterval: RollingInterval.Day)
                .WriteTo.Console(theme: SystemConsoleTheme.Colored)
                .CreateLogger();
            while (!Database.Database.isConnected)
            {
                Console.Write("请输入数据库密码: ".Translate());
                Database.Database.password = Console.ReadLine();
                Log.Information("正在尝试连接数据库".Translate());
                Database.Database.Connect();
            }

            Log.Information("连接到数据库成功!".Translate());
            Log.Information("加载用户中".Translate());
            UserManager.LoadUsers();
            Log.Information("加载服务器中".Translate());
            ServerManager.LoadServers();

            Log.Information("加载 API 中".Translate());
            ApiHandler.InitializeApis();
            Log.Information("加载 API 成功, 共 {0} 条".Translate(), ApiHandler.Apis.Count);

            Log.Information("正在开启 HTTP 服务器".Translate());
            HttpServer.HttpServer.StartHttpServer();
            string input = String.Empty;
            while ((input = Console.ReadLine()) != "exit")
            {
            }

            ExitEasyCraft(null, null);
        }

        private static void ExitEasyCraft(object sender, ConsoleCancelEventArgs e)
        {
            Log.Information("正在关闭 EasyCraft".Translate());
            Log.CloseAndFlush();
            Environment.Exit(0);
        }
    }
}