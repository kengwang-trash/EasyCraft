using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace EasyCraft.Core
{
    class Settings
    {
        static SettinsFile sf = new SettinsFile();

        public static string release = "Personal";

        public static int httpport
        {
            get
            {
                if (sf.HTTP != null && sf.HTTP.port != 0)
                {
                    return sf.HTTP.port;
                }
                else
                {
                    return 80;
                }
            }
        }

        public static int ftpport
        {
            get
            {
                if (sf.FTP != null && sf.FTP.port != 0)
                {
                    return sf.FTP.port;
                }
                else
                {
                    return 80;
                }
            }
        }

        public static string remoteip
        {
            get
            {
                if (sf.FTP != null && !string.IsNullOrEmpty(sf.FTP.remote_addr))
                {
                    return sf.FTP.remote_addr;
                }
                else
                {
                    FastConsole.PrintWarning(Language.t("FTP 远端地址未设置! FTP 被动模式可能无法运行!"));
                    return "0.0.0.0";
                }
            }
        }

        public static string key
        {
            get
            {
                if (sf != null && !string.IsNullOrEmpty(sf.key))
                {
                    return sf.key;
                }
                else
                {
                    return "No KEY";
                }
            }
            set
            {
                sf.key = key;
                File.WriteAllText("easycraft.conf", Newtonsoft.Json.JsonConvert.SerializeObject(sf));
            }
        }

        public static void LoadConfig()
        {
            try
            {
                if (File.Exists("easycraft.conf"))
                {
                    sf = Newtonsoft.Json.JsonConvert.DeserializeObject<SettinsFile>(File.ReadAllText("easycraft.conf"));

                }
                else
                {
                    FastConsole.PrintWarning(Language.t("未找到配置文件,正在启动安装进程"));
                    FastConsole.PrintWarning(Language.t("正在准备安装... 请稍候"));
                    sf.FTP = new FTPConf();
                    sf.HTTP = new HTTPConf();
                    Console.WriteLine(Language.t("请填写以下信息"));
                    while (true)
                    {
                        Console.WriteLine(Language.t("HTTP 监听端口 [80]:"));
                        string line = Console.ReadLine();
                        if (string.IsNullOrEmpty(line)) line = "80";
                        int port = 0;
                        if (int.TryParse(line, out port))
                        {
                            sf.HTTP.port = port;
                            break;
                        }
                        else
                        {
                            FastConsole.PrintWarning(Language.t("输入错误,请重新输入"));
                        }
                    }
                    while (true)
                    {
                        Console.WriteLine(Language.t("FTP 监听端口 [21]:"));
                        string line = Console.ReadLine();
                        if (string.IsNullOrEmpty(line)) line = "21";
                        int port = 0;
                        if (int.TryParse(line, out port))
                        {
                            sf.FTP.port = port;
                            break;
                        }
                        else
                        {
                            FastConsole.PrintWarning(Language.t("输入错误,请重新输入"));
                        }
                    }

                    while (true)
                    {
                        Console.WriteLine(Language.t("服务器远端IP <用户可以通过此 IP 访问服务器>:"));
                        string line = Console.ReadLine();
                        if (string.IsNullOrEmpty(line))
                        {
                            FastConsole.PrintWarning(Language.t("输入错误,请重新输入"));
                        }
                        else
                        {
                            sf.FTP.remote_addr = line;
                            break;
                        }
                    }

                    while (true)
                    {
                        Console.WriteLine(Language.t("授权密钥 [若无请留空]:"));
                        string line = Console.ReadLine();
                        if (string.IsNullOrEmpty(line))
                        {
                            sf.key = "none";
                            break;
                        }
                        else
                        {
                            sf.key = line;
                            break;
                        }
                    }

                    FastConsole.PrintSuccess(Language.t("安装完成,正在保存配置文件"));
                    File.WriteAllText("easycraft.conf", Newtonsoft.Json.JsonConvert.SerializeObject(sf));
                    LoadConfig();
                }

            }
            catch (Exception e)
            {
                FastConsole.PrintError(string.Format(Language.t("配置文件加载错误: {0}"), e.Message));
            }
        }

        public static void LoadDatabase()
        {
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText = "SELECT * FROM settings";
            SQLiteDataReader render = c.ExecuteReader();
            while (render.Read())
            {
                SettingsDatabase.annoucement = render.GetString(0);
            }
        }
    }

    class SettingsDatabase
    {
        public static string annoucement = "";
    }

    class SettinsFile
    {
        public HTTPConf HTTP { get; set; }
        public FTPConf FTP { get; set; }
        public string key { get; set; }
    }

    class HTTPConf
    {
        public int port { get; set; }
        public bool https { get; set; }//Not Support yet
    }

    class FTPConf
    {
        public int port { get; set; }
        public string remote_addr { get; set; }
    }
}
