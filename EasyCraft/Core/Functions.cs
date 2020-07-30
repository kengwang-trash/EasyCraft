using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace EasyCraft.Core
{
    class Functions
    {
        ///<summary>
        ///生成随机字符串 
        ///</summary>
        ///<param name="length">目标字符串的长度</param>
        ///<param name="useNum">是否包含数字，1=包含，默认为包含</param>
        ///<param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
        ///<param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
        ///<param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
        ///<param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
        ///<returns>指定长度的随机字符串</returns>
        public static string GetRandomString(int length = 15, bool useNum = true, bool useLow = true, bool useUpp = false, bool useSpe = false, string custom = "")
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }

        public static void InitDirectory()
        {
            string[] dirs =
            {
                "log/weberr",
                "db",
                "core",
                "server"
            };
            foreach (string dir in dirs)
            {
                Directory.CreateDirectory(dir);
            }

        }

        public static void CheckUpdate()
        {
            WebClient w = new WebClient();
            string bak = w.DownloadString("https://www.easycraft.top/version.php");
            VersionCallback b = System.Text.Json.JsonSerializer.Deserialize<VersionCallback>(bak);
            if (b.version != System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString())
            {
                FastConsole.PrintWarning("The Newst Version Of EasyCraft is " + b.version);
                FastConsole.PrintInfo("Update Log: " + b.log);
                FastConsole.PrintInfo("You can go to xxxxxxxxxx to update");
                FastConsole.PrintInfo("Press [Enter] to continue which is NOT RECOMMENDED");
                Console.ReadKey();
            }
        }
    }
    public class VersionCallback
    {
        public string version { get; set; }
        public string log { get; set; }
    }
}
