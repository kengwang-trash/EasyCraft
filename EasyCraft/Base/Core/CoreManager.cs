using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyCraft.Utils;
using Newtonsoft.Json.Linq;
using Serilog;

namespace EasyCraft.Base.Core
{
    public static class CoreManager
    {
        public static readonly Dictionary<string, CoreBase> Cores = new();
        public static readonly Dictionary<string, List<CoreConfigInfo>> ConfigInfos = new();

        public static void LoadCores()
        {
            LoadPublicConfigs();
            foreach (string directory in
                Directory.EnumerateDirectories(AppDomain.CurrentDomain.BaseDirectory + "/data/cores"))
            {
                try
                {
                    if (Path.GetFileName(directory) != "configs")
                    {
                        var json = JObject.Parse(File.ReadAllText(directory + "/core.json"));
                        if (json["id"]?.ToString() != Path.GetFileName(directory))
                        {
                            Log.Warning("核心路径 {0} 与核心 ID {1} 不匹配".Translate(), Path.GetDirectoryName(directory),
                                json["id"]?.ToString());
                            continue;
                        }

                        // 别看了, 这个 lambda 我也看晕了
                        if (json != null && json["info"] != null)
                        {
                            var core = new CoreBase
                            {
                                Id = json["id"].ToString(),
                                Info = new()
                                {
                                    Id = json["id"].ToString(),
                                    Device = json["info"]["device"]?.ToObject<int>() ?? 0,
                                    Branch = json["info"]["branch"]?.ToString() ?? "未知分支".Translate(),
                                    Name = json["info"]["name"]?.ToString() ?? "未知核心".Translate(),
                                },
                                Start = json["startinfo"].ToObject<Dictionary<string, CoreStartSimpleInfo>>()
                            };
                            string config = Path.GetFileName(directory);
                            if (json["configs"].Type != JTokenType.String)
                            {
                                LoadPublicConfig(config, json["configs"].ToString());
                                core.CoreConfig = config;
                            }
                            else
                            {
                                core.CoreConfig = json["configs"].ToString();
                            }

                            Cores[config] = core;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Warning("核心 {0} 加载失败: {1}".Translate(), Path.GetFileName(directory), e.Message);
                }
            }
        }

        public static void LoadPublicConfigs()
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/data/cores/configs"))
                foreach (string file in Directory.EnumerateFiles(
                    AppDomain.CurrentDomain.BaseDirectory + "/data/cores/configs",
                    "*.json"))
                {
                    LoadPublicConfig(Path.GetFileNameWithoutExtension(file));
                }
        }

        public static void LoadPublicConfig(string name, string content = null)
        {
            if (content == null)
                content = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/data/cores/configs/" + name +
                                           ".json");
            var json = JArray.Parse(content);
            ConfigInfos[name] =
                (json.ToObject<List<JToken>>() ?? new List<JToken>()).Select(
                    keyValue =>
                    {
                        return new CoreConfigInfo
                        {
                            File = keyValue["file"]?.ToString(),
                            Display = keyValue["display"]?.ToString(),
                            Type = keyValue["type"]?.ToString(),
                            Required = keyValue["required"]?.ToObject<bool>() ?? false,
                            Known = (keyValue["known"])!.Select(t =>
                                new CoreConfigKnownItem
                                {
                                    Key = t["key"].ToString(),
                                    Display = t["display"].ToString(),
                                    Type = t["type"].ToString(),
                                    Visible = t["visible"]?.ToObject<bool>() ?? true,
                                    Force = t["force"]?.ToObject<bool>() ?? false,
                                    Value = t["value"]?.ToObject<string>() ?? String.Empty,
                                    Selection = (t["selection"]?.ToObject<List<JToken>>() ??
                                                 new List<JToken>())
                                        .Select(
                                            token => new CoreConfigKnownItemSelection
                                            {
                                                Display = token["display"].ToString(),
                                                Value = token["value"].ToString()
                                            }).ToList()
                                }
                            ).ToList()
                        };
                    }).ToList();
        }
    }
}