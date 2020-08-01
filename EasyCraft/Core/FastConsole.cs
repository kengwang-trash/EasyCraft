﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace EasyCraft.Core
{
    class FastConsole
    {
        public static void PrintInfo(string message)
        {

            string print = "[INFO] " + message;
            if (Settings.logLevel == LogLevel.noserver || Settings.logLevel == LogLevel.all)
                Console.WriteLine(print);
            File.AppendAllTextAsync("log.log", "[" + DateTime.Now.ToString() + " INFO] " + message + "\r\n");
        }

        public static void PrintTrash(string message)
        {

            string print = "[DEBUG] " + message;
            if (Settings.logLevel >= LogLevel.notrash)
            {
                Console.WriteLine(print);
                File.AppendAllTextAsync("log.log", "[" + DateTime.Now.ToString() + " DEBUG] " + message + "\r\n");
            }
        }

        public static void PrintSuccess(string message)
        {
            ConsoleColor currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            string print = "[INFO] " + message;
            Console.WriteLine(print);
            Console.ForegroundColor = currentForeColor;
            File.AppendAllTextAsync("log.log", "[" + DateTime.Now.ToString() + " INFO] " + message + "\r\n");
        }

        public static void PrintWarning(string message)
        {
            ConsoleColor currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            string print = "[WARN] " + message;
            Console.WriteLine(print);
            Console.ForegroundColor = currentForeColor;
            File.AppendAllTextAsync("log.log", "[" + DateTime.Now.ToString() + " WARN] " + message + "\r\n");
        }

        public static void PrintError(string message)
        {
            ConsoleColor currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            string print = "[ERROR] " + message;
            Console.WriteLine(print);
            Console.ForegroundColor = currentForeColor;
            File.AppendAllTextAsync("log.log", "[" + DateTime.Now.ToString() + " ERROR] " + message + "\r\n");
        }



        public static void PrintFatal(string message)
        {
            ConsoleColor currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            string print = "[FATAL] " + message;
            Console.WriteLine(print);
            Console.ForegroundColor = currentForeColor;
            File.AppendAllTextAsync("log.log", "[" + DateTime.Now.ToString() + " FATAL] " + message + "\r\n");
        }
    }


    class Language
    {
        public static Dictionary<string, string> dic = new Dictionary<string, string>();

        public static void LoadLanguagePack()
        {
            try
            {
                string fi = File.ReadAllText("lang.json");
                dic = JsonSerializer.Deserialize<Dictionary<string, string>>(fi);
            }
            catch (Exception e)
            {

            }

        }

        public static string t(string input)
        {
            File.AppendAllTextAsync("dic.txt", input+"\r\n");
            if (!dic.ContainsKey(input)) return input;
            return dic[input];
        }
    }
}
