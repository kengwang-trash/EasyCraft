using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyCraft.Core
{
    class Settings
    {
        static SettinsFile sf = new SettinsFile();
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
                    FastConsole.PrintWarning(Language.t("FTP Remote Address not set! FTP Passive might be error!"));
                    return "0.0.0.0";
                }
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
                    FastConsole.PrintWarning(Language.t("Config File Not Found, Start Install"));
                    FastConsole.PrintWarning(Language.t("Initialize Install... Please Wait"));
                    sf.FTP = new FTPConf();
                    sf.HTTP = new HTTPConf();
                    Console.WriteLine(Language.t("Please input the following forms"));
                    while (true)
                    {
                        Console.WriteLine(Language.t("HTTP Listen Port [80]:"));
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
                            FastConsole.PrintWarning(Language.t("Input Error , Please Retype"));
                        }
                    }
                    while (true)
                    {
                        Console.WriteLine(Language.t("FTP Listen Port [21]:"));
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
                            FastConsole.PrintWarning(Language.t("Input Error , Please Retype"));
                        }
                    }

                    while (true)
                    {
                        Console.WriteLine(Language.t("Server Address <IP that other users can access this server>:"));
                        string line = Console.ReadLine();
                        if (string.IsNullOrEmpty(line))
                        {
                            FastConsole.PrintWarning(Language.t("Input Error , Please Retype"));
                        }
                        else
                        {
                            sf.FTP.remote_addr = line;
                            break;
                        }
                    }

                    FastConsole.PrintSuccess("Install Successfully, Saving Config File");
                    File.WriteAllText("easycraft.conf", Newtonsoft.Json.JsonConvert.SerializeObject(sf));
                    LoadConfig();
                }

            }
            catch (Exception e)
            {
                FastConsole.PrintError(string.Format(Language.t("Config Load Failed: {0}"), e.Message));
            }
        }
    }

    class SettinsFile
    {
        public HTTPConf HTTP { get; set; }
        public FTPConf FTP { get; set; }
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
