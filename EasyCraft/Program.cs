using System;
using System.Collections.Generic;
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
using Newtonsoft.Json;
using Serilog;
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

            // 初始化加载
            if (!File.Exists(Directory.GetCurrentDirectory() + "/easycraft.json"))
                InstallEasyCraft();

            Console.WriteLine("正在加载日志组件".Translate());
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Directory.GetCurrentDirectory() + "/logs/log-.log", rollingInterval: RollingInterval.Day)
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
            foreach (var serversValue in ServerManager.Servers.Values) _ = serversValue.Stop();
            Log.Information("正在关闭 EasyCraft".Translate());
            Log.CloseAndFlush();
            Environment.Exit(0);
        }

        #region Install

        private static void InstallEasyCraft()
        {
            var setting = new Dictionary<string, string>();
            Console.WriteLine("这似乎是你第一次使用 EasyCraft, 我们将会带领你进行安装");
            Console.WriteLine("正在为安装做准备......".Translate());
            // 创建必要的路径
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/data");
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/data/cores");
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/data/starters");
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/data/pluigns");
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/data/servers");
            Console.WriteLine("请输入此计算机的 IP地址/域名".Translate());
            Console.WriteLine("此内容将会用于在前端服务器IP显示或其他插件调用".Translate());
            Console.Write("> ".Translate());
            setting["ServerIp"] = Console.ReadLine();

            Console.WriteLine("请输入 EasyCraft 后端 API 将要监听的端口: ".Translate());
            Console.Write("> ".Translate());
            setting["HttpPort"] = Console.ReadLine();

            Console.WriteLine("很棒! 接下来我们来创建数据库吧~".Translate());
            Console.WriteLine("为了保证安全,请输入数据库密码:".Translate());
            Console.Write("> ".Translate());
            var pswd = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(pswd))
            {
                Console.WriteLine("您真的确认不设置数据库密码吗? 我为你自动生成了一个".Translate());
                string autopass = Utils.Utils.CreateRandomString(15, true, true, true);
                Console.WriteLine(autopass);
                Console.WriteLine("回车即可使用这个密码, 如果想自定义, 请输入自定义密码".Translate());
                Console.WriteLine("如果你真的不想使用密码, 请输入 「null」".Translate());
                Console.Write("> ".Translate());
                string inputpass = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(inputpass))
                    pswd = autopass;
                else if (inputpass == "null")
                    pswd = null;
                else
                    pswd = inputpass;
            }

            if (pswd != null)
            {
                Console.WriteLine("请记住你的数据库密码: {0}".Translate(pswd));
                Console.WriteLine("是否自动登录到数据库, 假如您是非个人用途不建议使用".Translate());
                Console.WriteLine("启用请输入 [yes]");
                Console.Write("> ".Translate());
                if (Console.ReadLine() == "yes")
                    setting["Database_Password"] = pswd;
            }

            Console.WriteLine("即将生成数据库,请稍等.");
            Database.Database.Password = pswd;
            if (Database.Database.Connect())
            {
                Database.Database.CreateCommand(InstallSql).ExecuteNonQuery();
            }
            else
            {
                Console.WriteLine("数据库生成失败,请检查目录可写".Translate());
                ExitEasyCraft(null, null);
            }

            Console.Write("请输入 EasyCraft 管理员用户名: ".Translate());
            string username = Console.ReadLine();
            Console.Write("请输入 EasyCraft 管理员密码: ".Translate());
            string password = Console.ReadLine();
            Console.Write("请输入 EasyCraft 管理员邮箱: ".Translate());
            string email = Console.ReadLine();
            Database.Database.CreateCommand(
                "INSERT INTO users (name, password, type, email) VALUES ( $name, $password , $type , $email )",
                new Dictionary<string, object>()
                {
                    { "$name", username },
                    { "$password", password },
                    { "$email", email },
                    { "$type", ((int)UserType.SuperUser).ToString() },
                }).ExecuteNonQuery();
            File.WriteAllText("easycraft.json", JsonConvert.SerializeObject(setting));
            Console.WriteLine("恭喜你, 你现在已经成功安装了 EasyCraft!");
        }

        private static string InstallSql = @"create table permissions
(
	id integer not null
		constraint permissions_pk
			primary key,
	type integer not null
);

create unique index permissions_permissionId_uindex
	on permissions (id);

create table server_start
(
	id integer not null
		constraint server_start_pk
			primary key,
	core text,
	lastcore text,
	world text,
	starter text
);

create unique index server_start_id_uindex
	on server_start (id);

create table servers
(
	id integer default 0 not null
		constraint servers_pk
			primary key autoincrement,
	name text not null,
	owner integer not null,
	expire text not null,
	port integer not null,
	ram integer default 1000,
	autostart integer default 0,
	status integer default 0 not null,
	player integer
);

create unique index servers_id_uindex
	on servers (id);

create table users
(
	id integer not null
		constraint users_pk
			primary key autoincrement,
	name text not null,
	password text,
	type integer,
	email text
);

create unique index users_id_uindex
	on users (id);

create unique index users_name_uindex
	on users (name);

";

        #endregion
    }
}