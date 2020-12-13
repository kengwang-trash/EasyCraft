using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using EasyCraft.Core;
using EasyCraft.Web;

namespace EasyCraft.PluginBase
{
    public class PluginBase
    {
        public static Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();


        public static void LoadPlugins()
        {
            Dictionary<string, bool> plugindb = new Dictionary<string, bool>();
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText = "SELECT id,enable FROM plugin";
            SQLiteDataReader render = c.ExecuteReader();
            while (render.Read())
            {
                plugindb.Add(render.GetString(0), render.GetBoolean(1));
            }
            foreach (string file in Directory.EnumerateFiles("data/plugin/", "*.dll").ToList())
            {
                try
                {
                    Plugin p = new Plugin();
                    p.assembly = Assembly.LoadFrom(file);
                    string key = Functions.GetRandomString(20, true, true, true, true);
                    dynamic info = p.assembly.GetType("EasyCraftPlugin.Plugin").GetMethod("Initialize").Invoke(null, new object[] { Assembly.GetExecutingAssembly().GetType("EasyCraft.PluginBase.PluginHandler"), key });
                    p.id = info.id;
                    p.name = info.name;
                    p.author = info.author;
                    p.link = info.link;
                    p.key = key;
                    p.enabled = (plugindb.ContainsKey(p.id) && plugindb[p.id]);
                    p.path = Path.GetFullPath(file);
                    plugins[p.id] = p;
                    if (p.enabled)
                    {
                        p.assembly.GetType("EasyCraftPlugin.Plugin").GetMethod("OnEnable").Invoke(null, null);
                        FastConsole.PrintSuccess(string.Format(Language.t("成功加载插件: {0}"), p.name));
                    }
                }
                catch (Exception e)
                {
                    FastConsole.PrintWarning(string.Format(Language.t("加载插件 {0} 失败: {1}"), file, e.Message));
                }
            }
        }
    }

    public struct Plugin
    {
        public Assembly assembly;
        public string id;
        public string name;
        public string author;
        public string description;
        public string link;
        public string path;
        public string key;
        public bool enabled;
    }
}