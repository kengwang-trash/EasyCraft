using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
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
            string bak = w.DownloadString("https://api.easycraft.top/version.php");
            VersionCallback b = System.Text.Json.JsonSerializer.Deserialize<VersionCallback>(bak);
            if (b.version != System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString())
            {
                FastConsole.PrintWarning(string.Format(Language.t("The Newst Version Of EasyCraft is {0}"), b.version));
                FastConsole.PrintInfo(string.Format(Language.t("Update Log: {0}"), b.log));
                FastConsole.PrintInfo(string.Format(Language.t("You can go to https://www.easycraft.top to update")));
                FastConsole.PrintWarning(string.Format(Language.t("Press [Enter] to continue which is NOT RECOMMENDED")));
                Console.ReadKey();
            }
        }

        public static string MD5(string str)
        {
            string pwd = "";
            MD5 md5 = System.Security.Cryptography.MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符

                pwd = pwd + s[i].ToString("x2");

            }
            return pwd;
        }

        public static void CopyDirectory(string sourceDirPath, string SaveDirPath)
        {
            //如果指定的存储路径不存在，则创建该存储路径
            if (!Directory.Exists(SaveDirPath))
            {
                //创建
                Directory.CreateDirectory(SaveDirPath);
            }
            //获取源路径文件的名称
            string[] files = Directory.GetFiles(sourceDirPath);
            //遍历子文件夹的所有文件
            foreach (string file in files)
            {
                string pFilePath = SaveDirPath + "\\" + Path.GetFileName(file);
                if (File.Exists(pFilePath))
                    continue;
                File.Copy(file, pFilePath, true);
            }
            string[] dirs = Directory.GetDirectories(sourceDirPath);
            //递归，遍历文件夹
            foreach (string dir in dirs)
            {
                CopyDirectory(dir, SaveDirPath + "\\" + Path.GetFileName(dir));
            }
        }
    }
    public class VersionCallback
    {
        public string version { get; set; }
        public string log { get; set; }
    }
}
