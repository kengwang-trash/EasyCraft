using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EasyCraft.Core;

namespace EasyCraft.PluginBase
{
    public class PluginBase
    {
        public static Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();
        public static Dictionary<string, List<string>> hooks = new Dictionary<string, List<string>>();

        public static void LoadPlugins()
        {
            var plugindb = new Dictionary<string, bool>();
            var c = Database.DB.CreateCommand();
            c.CommandText = "SELECT id,enable FROM plugin";
            var render = c.ExecuteReader();
            while (render.Read()) plugindb.Add(render.GetString(0), render.GetBoolean(1));

            foreach (var file in Directory.EnumerateFiles("data/plugin/", "*.dll").ToList())
                try
                {
                    var p = new Plugin();
                    if (File.Exists(Path.ChangeExtension(file, "pdb")))
                        //可载入调试文件
                        p.assembly = Assembly.Load(File.ReadAllBytes(file),
                            File.ReadAllBytes(Path.ChangeExtension(file, "pdb")));
                    else
                        p.assembly = Assembly.LoadFrom(file);
                    var key = Functions.GetRandomString(20, true, true, true, true);
                    dynamic info = p.assembly.GetType("EasyCraftPlugin.Plugin").GetMethod("Initialize").Invoke(null,
                        new object[]
                            {Assembly.GetExecutingAssembly().GetType("EasyCraft.PluginBase.PluginHandler"), key});
                    p.id = info.id;
                    p.name = info.name;
                    p.author = info.author;
                    p.link = info.link;
                    p.key = key;
                    p.auth = info.auth;
                    p.hooks = info.hooks;
                    foreach (var pHook in p.hooks)
                    {
                        if (!hooks.ContainsKey(pHook)) hooks[pHook] = new List<string>();
                        hooks[pHook].Add(p.id);
                    }

                    p.enabled = plugindb.ContainsKey(p.id) && plugindb[p.id];
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

        public static bool BroadcastEvent(string eventid, object[] paratmers)
        {
            if (!hooks.ContainsKey(eventid)) return true;
            var finalret = true;
            foreach (var s in hooks[eventid])
                try
                {
                    var ret = (bool) plugins[s].assembly.GetType("EasyCraftPlugin.Plugin").GetMethod(eventid)
                        .Invoke(null, paratmers);
                    finalret = ret && finalret;
                }
                catch (Exception e)
                {
                    FastConsole.PrintWarning(string.Format(Language.t("插件 [{0}] 执行 {1} 时出错: {2}"), s, eventid,
                        e.Message));
                }

            return finalret;
        }

        public static bool CheckPluginAuth(string pid, string authid)
        {
            return plugins[pid].enabled && plugins[pid].auth.Contains(authid);
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
        public string[] hooks;
        public string[] auth;
        public bool enabled;
    }
}