using System;
using System.Collections.Generic;
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
        public static List<Plugin> plugins = new List<Plugin>();

        public static void LoadPlugins()
        {
            foreach (string file in Directory.EnumerateFiles("data/plugin/", "*.dll").ToList())
            {
                try
                {

                    Plugin p = new Plugin();
                    p.assembly = Assembly.LoadFrom(file);
                    dynamic info = p.assembly.GetType("EasyCraftPlugin.Plugin").GetMethod("Initialize").Invoke(null, new object[] { Assembly.GetExecutingAssembly().GetType("EasyCraft.Core.FastConsole") });
                    
                    p.id = info.id;
                    p.name = info.name;
                    p.author = info.author;
                    p.link = info.link;
                    p.path = Path.GetFullPath(file);
                    plugins.Add(p);
                    FastConsole.PrintSuccess(string.Format(Language.t("成功加载插件: {0}"), p.name));
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
    }
}