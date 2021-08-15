using System;
using EasyCraft.Base.Core;
using EasyCraft.Base.Server;
using EasyCraft.Base.User;
using EasyCraft.HttpServer.Api;
using EasyCraft.PluginBase;
using EasyCraft.Utils;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace EasyCraft
{
    internal class Program
    {
        private static void Main(string[] args)
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
== Version " + Common.VersionFull + "  (" + Common.VersionName + @") ==
 == Developed by EasyCraft Team ==
  ==    Under GPL v3 Licence   ==");
            Console.WriteLine("Loading Language Pack");
            Translation.LoadTranslation();
            Console.WriteLine("正在加载日志组件".Translate());
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("/logs/log-.log", LogEventLevel.Information, rollingInterval: RollingInterval.Day)
                .WriteTo.Console(theme: SystemConsoleTheme.Colored)
                .CreateLogger();
            Database.Database.Connect();
            while (!Database.Database.IsConnected)
            {
                Console.Write("请输入数据库密码: ".Translate());
                Database.Database.Password = Console.ReadLine();
                Log.Information("正在尝试连接数据库".Translate());
                Database.Database.Connect();
            }

            Log.Information("连接到数据库成功!".Translate());
            Log.Information("加载用户中".Translate());
            UserManager.LoadUsers();

            Log.Information("加载核心中".Translate());
            CoreManager.LoadCores();
            Log.Information("加载核心成功, 共 {0} 个".Translate(), CoreManager.Cores.Count);

            Log.Information("加载服务器中".Translate());
            ServerManager.LoadServers();

            Log.Information("加载 API 中".Translate());
            ApiHandler.InitializeApis();
            Log.Information("加载 API 成功, 共 {0} 条".Translate(), ApiHandler.Apis.Count);

            Log.Information("加载插件中".Translate());
            PluginController.LoadPlugins();
            Log.Information("加载插件完成, 共 {0} 个".Translate(), PluginController.Plugins.Count);

            Log.Information("启用插件中".Translate());
            PluginController.EnablePlugins();

            Log.Information("正在开启 HTTP 服务器".Translate());
            HttpServer.HttpServer.StartHttpServer();
            string input;
            while ((input = Console.ReadLine()) != "exit")
            {
                Log.Information("你输入了 {0}", input);
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