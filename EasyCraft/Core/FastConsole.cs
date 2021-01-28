using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace EasyCraft.Core
{
    internal class FastConsole
    {
        private static StreamWriter logfile;
        public static FastConsoleLogLevel logLevel;

        public static void PrintInfo(string message)
        {
            var print = "[INFO] " + message;
            if (logLevel >= FastConsoleLogLevel.noserver)
                Console.WriteLine(print);
            logfile.WriteLine("[" + DateTime.Now + " INFO] " + message);
            //File.AppendAllTextAsync("log.log", "[" + DateTime.Now.ToString() + " INFO] " + message + "\r\n");
        }

        public static void Init()
        {
            //设定书写的开始位置为文件的末尾 
            var files = File.OpenWrite("log.log");
            files.Position = files.Length;
            logfile = new StreamWriter(files, Encoding.UTF8);
            logfile.AutoFlush = true;
        }

        public static void PrintTrash(string message)
        {
            var print = "[DEBUG] " + message;
            if (logLevel > FastConsoleLogLevel.notrash)
            {
                Console.WriteLine(print);
                logfile.WriteLine("[" + DateTime.Now + " DEBUG] " + message);

                //File.AppendAllTextAsync("log.log", "[" + DateTime.Now.ToString() + " DEBUG] " + message + "\r\n");
            }
        }

        public static void PrintSuccess(string message)
        {
            var currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            var print = "[INFO] " + message;
            Console.WriteLine(print);
            Console.ForegroundColor = currentForeColor;
            logfile.WriteLine("[" + DateTime.Now + " INFO] " + message);

            //File.AppendAllTextAsync("log.log", "[" + DateTime.Now.ToString() + " INFO] " + message + "\r\n");
        }

        public static void PrintWarning(string message)
        {
            var currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            var print = "[WARN] " + message;
            Console.WriteLine(print);
            Console.ForegroundColor = currentForeColor;
            //File.AppendAllTextAsync("log.log", "[" + DateTime.Now.ToString() + " WARN] " + message + "\r\n");
            logfile.WriteLine("[" + DateTime.Now + " WARN] " + message);
        }

        public static void PrintError(string message)
        {
            var currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            var print = "[ERROR] " + message;
            Console.WriteLine(print);
            Console.ForegroundColor = currentForeColor;
            //File.AppendAllTextAsync("log.log", "[" + DateTime.Now.ToString() + " ERROR] " + message + "\r\n");
            logfile.WriteLine("[" + DateTime.Now + " ERROR] " + message);
        }


        public static void PrintFatal(string message)
        {
            var currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            var print = "[FATAL] " + message;
            Console.WriteLine(print);
            Console.ForegroundColor = currentForeColor;
            //File.AppendAllTextAsync("log.log", "[" + DateTime.Now.ToString() + " FATAL] " + message + "\r\n");
            logfile.WriteLine("[" + DateTime.Now + " FATAL] " + message);
        }
    }


    internal enum FastConsoleLogLevel
    {
        no, //只输出Fatal和Error
        noserver, //不输出服务器内的日志
        notrash,
        all //所有
    }

    internal class Language
    {
        public static Dictionary<string, string> dic = new Dictionary<string, string>();

        public static void LoadLanguagePack()
        {
            try
            {
                var fi = File.ReadAllText("lang.json");
                dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(fi);
            }
            catch (Exception)
            {
            }
        }

        public static string t(string input)
        {
            if (!dic.ContainsKey(input)) return input;
            return dic[input];
        }
    }
}