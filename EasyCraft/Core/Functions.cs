using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
            InitPermisson();
        }

        public static void InitPermisson()
        {
            // 1 - 创建用户组
            if (UserGroup.IsGroupExist("easycraft"))
            {
                FastConsole.PrintWarning(Language.t("没有找到用户组,正在创建"));
            }
        }

        public static void CheckUpdate()
        {
#if !DEBUG
            try
            {
                if (Settings.release == "Personal") Settings.key = "none";
                Uri u = new Uri("https://api.easycraft.top/version.php");
                HttpWebRequest req = HttpWebRequest.Create(u) as HttpWebRequest;
                req.ServerCertificateValidationCallback += CertificateValidation;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";                
                File.Copy(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + ".bak", true);//拷贝一份防止占用
                string filemd5 = GetMD5HashFromFile(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + ".bak");
                File.Delete(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + ".bak");
                string type = "exe";
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    type = "lin";
                }
                else
                {
                    type = System.Reflection.Assembly.GetExecutingAssembly().Location.EndsWith(".dll") ? "dll" : "exe";

                }
                if (Process.GetProcessesByName("ollydbg").Length != 0)
                {
                    throw new Exception("Please Exit Debugger");
                }
                int salt = new Random().Next(111111, 999999);
                string poststring = "version=" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version +
                    "&checksum=" + filemd5 +
                    "&type=" + type +
                    "&branch="+Settings.release+
                    "&key=" + MD5(filemd5 + salt + Settings.key) +
                    "&salt=" + salt.ToString();
                byte[] post = Encoding.UTF8.GetBytes(poststring);
                req.ContentLength = post.Length;
                Stream reqStream = req.GetRequestStream();
                reqStream.Write(post, 0, post.Length);
                reqStream.Close();
                WebResponse wr = req.GetResponse();
                System.IO.Stream respStream = wr.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(respStream,Encoding.UTF8);
                string t = reader.ReadToEnd();
                VersionCallback b = Newtonsoft.Json.JsonConvert.DeserializeObject<VersionCallback>(t);
                FastConsole.PrintInfo(b.log);

                while (b == null || b.runkey == null || b.runkey != MD5(Settings.release + filemd5 + salt.ToString() + Settings.key + salt.ToString() + "VeriflcationChrcked"))
                {
                    FastConsole.PrintFatal(Language.t("授权检测失败, 按 [Enter] 退出"));
                    if (b != null && b.log != null)
                    {
                        FastConsole.PrintWarning(b.log);
                    }
                    int i = -1;
                    while (true)
                    {
                        while (true)
                        {
                            Environment.Exit(i--);
                        }
                    }
                    Exception e = new Exception("Licence Check Failed");
                    e.HResult = -20240628;
                    throw (e);
                }
                //FastConsole.PrintSuccess(Language.t("授权检测通过,感谢支持!"));

                if (b.version != System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString())
                {
                    FastConsole.PrintWarning(string.Format(Language.t("最新版本 {0} 已经发布"), b.version));
                    FastConsole.PrintInfo(string.Format(Language.t("更新日志: {0}"), b.log));
                    FastConsole.PrintInfo(string.Format(Language.t("你可以访问 https://www.easycraft.top 获取更新")));
                    FastConsole.PrintWarning(string.Format(Language.t("按下 [Enter] 继续 (不推荐)")));
                    Console.ReadKey();
                }
            }
            catch (Exception e)
            {
                if (e.HResult == -20240628)
                {
                    FastConsole.PrintFatal(Language.t("授权检测失败!"));
                    int i = -1;
                    while (true)
                    {
                        while (true)
                        {
                            Environment.Exit(i--);
                        }
                    }
                }
                else
                {
                    FastConsole.PrintFatal(Language.t("版本检测失败! 按 [Enter] 退出"));
                    Console.ReadKey();
                    Environment.Exit(-25);
                }
            }
#endif
        }

        private static bool CertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return (sslPolicyErrors == SslPolicyErrors.None && certificate.Subject == "CN=api.easycraft.top" && chain.ChainElements[1].Certificate.Thumbprint == "E6A3B45B062D509B3382282D196EFE97D5956CCB");
        }

        public static string MD5(string str)
        {
            if (str == null) str = "";
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
                string pFilePath = SaveDirPath + "/" + Path.GetFileName(file);
                if (File.Exists(pFilePath))
                    continue;
                File.Copy(file, pFilePath, true);
            }
            string[] dirs = Directory.GetDirectories(sourceDirPath);
            //递归，遍历文件夹
            foreach (string dir in dirs)
            {
                CopyDirectory(dir, SaveDirPath + "/" + Path.GetFileName(dir));
            }
        }


        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="fileName">文件绝对路径</param>
        /// <returns>MD5值</returns>
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Hash File Error:" + ex.Message);
            }
        }
    }
    public class VersionCallback
    {
        public string version { get; set; }
        public string log { get; set; }
        public string runkey { get; set; }
    }
}
