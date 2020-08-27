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
                if (sf.HTTP != null && sf.HTTP.port != null && sf.HTTP.port != 0)
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
                if (sf.FTP != null && sf.FTP.port != null && sf.FTP.port != 0)
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
                sf = Newtonsoft.Json.JsonConvert.DeserializeObject<SettinsFile>(File.ReadAllText("easycraft.conf"));

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
