using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EasyCraft.Utils;
using Serilog;
#pragma warning disable 1998

namespace EasyCraft.PluginBase
{
    public static class PluginController
    {
        internal static Dictionary<string, Plugin> Plugins = new();
        private static readonly Dictionary<string, Dictionary<string, int>> EventHookers = new();

        public static void LoadPlugins()
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/data/plugins")) return;
            foreach (var file in
                Directory.EnumerateFiles(Directory.GetCurrentDirectory() + "/data/plugins", "*.dll"))
                try
                {
                    // 1 - 载入 Assembly , 我们不用文件方便后期动态加载
                    var ass = Assembly.Load(File.ReadAllBytes(file));
                    // 2 - 定时载入插件 , 如若超时不加载也罢 - 加载限时暂未设计
                    var type = ass.GetType("EasyCraftPlugin.Plugin");
                    var key = Utils.Utils.CreateRandomString();
                    var ret = (Dictionary<string, Dictionary<string, string>>)
                        type?
                            .GetMethod("OnLoad")?
                            .Invoke(null, new object[]
                            {
                                Assembly.GetExecutingAssembly().GetType("EasyCraft.PluginBase.PluginHandler"), key
                            });

                    Plugins[ret["PluginInfo"]["id"]] = new Plugin
                    {
                        Info = new PluginInfo
                        {
                            Id = ret["PluginInfo"]["id"],
                            Name = ret["PluginInfo"]["name"],
                            Version = ret["PluginInfo"]["version"],
                            Author = ret["PluginInfo"]["author"],
                            Description = ret["PluginInfo"]["description"],
                            Link = ret["PluginInfo"]["link"]
                        },
                        Enable = false,
                        Type = type,
                        Key = key
                    };
                    // 3 - 加载相关 EventHooker
                    Plugins[ret["PluginInfo"]["id"]].EventHookers = ret["Hooks"];
                    foreach (var plh in ret["Hooks"])
                    {
                        if (!EventHookers.ContainsKey(plh.Key))
                            EventHookers[plh.Key] = new Dictionary<string, int>();
                        EventHookers[plh.Key][ret["PluginInfo"]["id"]] = int.Parse(plh.Value);
                    }

                    // 4 - 载入申请权限 - 目前完全允许
                    Plugins[ret["PluginInfo"]["id"]].Auth = ret["Request"].ToDictionary(t => t.Key, _ => true);
                }
                catch (Exception e)
                {
                    Log.Warning("插件 {0} 加载失败: {1}".Translate(), Path.GetFileName(file), e.Message);
                }

            // 5 - 对 EventHooker 进行排序方便调用
            foreach (var evtkey in EventHookers.Keys)
                EventHookers[evtkey] =
                    EventHookers[evtkey].OrderBy(t => t.Value).ToDictionary(t => t.Key, t => t.Value);

            // 6 - 这时才加载插件 API
            PluginHandler.InitPluginHandleApi();
        }


        public static async Task<Dictionary<string, object>> BroadcastEventAsync(string eventId, object[] parameters)
        {
            // 不要回答! 不要回答! 不要回答!
            var ret = new Dictionary<string, object>();
            if (!EventHookers.ContainsKey(eventId)) return ret;
            foreach (var kvp in EventHookers[eventId])
                try
                {
                    ret[kvp.Key] = Plugins[kvp.Key].Type.GetMethod(eventId)?.Invoke(null, parameters);
                }
                catch (Exception e)
                {
                    Log.Warning("调用插件 {0} 中 {1} 出错: {3}".Translate(), kvp.Key, eventId, e.Message);
                }

            return ret;
        }

        public static void EnablePlugins()
        {
            _ = BroadcastEventAsync("OnEnable", null);
        }
    }
}