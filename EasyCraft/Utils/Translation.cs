using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace EasyCraft.Utils
{
    public static class Translation
    {
        public static Dictionary<string, string> TransList = new Dictionary<string, string>();

        public static void LoadTranslation()
        {
            try
            {
                TransList = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("lang.json"));
            }
            catch
            {
                Console.WriteLine("语言包加载失败,默认为 简体中文 (zh-cn)");
            }
        }

        public static string Translate(this string str, params object[] reps)
        {
            if (reps.Length != 0) return string.Format(TransList.GetValueOrDefault(str, str), reps);
            return TransList.GetValueOrDefault(str, str);
        }
    }
}