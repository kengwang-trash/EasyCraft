using System;
using System.IO;
using EasyCraft.Base.Core;
using EasyCraft.Base.Server;
using EasyCraft.Base.Starter;
using EasyCraft.Base.User;
using EasyCraft.Command;
using EasyCraft.HttpServer.Api;
using EasyCraft.PluginBase;
using EasyCraft.Utils;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace EasyCraft
{
    public static class Program
    {
        // ReSharper disable once UnusedParameter.Local
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

            Common.Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("easycraft.json", true, true)
#if DEBUG
                .AddJsonFile("easycraft.development.json", true, true)
#else
                .AddJsonFile("easycraft.production.json", true, true)
#endif
                .Build();

            Database.Database.Password = Common.Configuration["Database_Password"];
            Database.Database.Connect();
            while (!Database.Database.IsConnected)
            {
                Console.Write("请输入数据库密码: ".Translate());
                Database.Database.Password = Console.ReadLine();
                Log.Information("正在尝试连接数据库".Translate());
                Database.Database.Connect();
            }

            Log.Information("连接到数据库成功!".Translate());

            Log.Information("加载权限表中".Translate());
            Permission.LoadPermissions();

            Log.Information("加载用户中".Translate());
            UserManager.LoadUsers();

            Log.Information("加载核心中".Translate());
            CoreManager.LoadCores();
            Log.Information("加载核心完成, 共 {0} 个".Translate(), CoreManager.Cores.Count);

            Log.Information("加载开服器中".Translate());
            StarterManager.InitializeStarters();
            Log.Information("加载开服器完成, 共 {0} 个".Translate(), StarterManager.Starters.Count);

            Log.Information("加载服务器中".Translate());
            ServerManager.LoadServers();
            Log.Information("加载核心成功, 共 {0} 个".Translate(), ServerManager.Servers.Count);

            Log.Information("加载 API 中".Translate());
            ApiHandler.InitializeApis();
            Log.Information("加载 API 完成, 共 {0} 条".Translate(), ApiHandler.Apis.Count);

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
                if (input == null)
                    continue;
                var l = input.Split(' ');
                if (CommandManager.Apis.ContainsKey(l[0]))
                    CommandManager.Apis[l[0]].Invoke(input);
            }

            ExitEasyCraft(null, null);
        }

        private static void ExitEasyCraft(object sender, ConsoleCancelEventArgs e)
        {
            Log.Information("正在关闭所有服务器".Translate());
            foreach (var serversValue in ServerManager.Servers.Values) serversValue.Stop();
            Log.Information("正在关闭 EasyCraft".Translate());
            Log.CloseAndFlush();
            Environment.Exit(0);
        }
    }
}