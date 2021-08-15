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

        public static void LoadCores()
        {
            foreach (string directory in
                Directory.EnumerateDirectories(Directory.GetCurrentDirectory() + "/data/cores"))
            {
                try
                {
                    var json = JObject.Parse(File.ReadAllText(directory + "/core.json"));
                    if (json["id"]?.ToString() != Path.GetFileName(directory))
                    {
                        Log.Warning("核心路径 {0} 与核心 ID {1} 不匹配".Translate(), Path.GetDirectoryName(directory),
                            json["id"]?.ToString());
                        continue;
                    }

                    // 别看了, 这个 lambda 我也看晕了
                    var core = new CoreBase
                    {
                        Id = json["id"].ToString(),
                        Info = new()
                        {
                            Id = json["id"].ToString(),
                            Device = json["info"]["device"].ToObject<int>(),
                            Branch = json["info"]["branch"].ToString(),
                            Name = json["info"]["name"].ToString(),
                        },
                        Start = new CoreStartInfo()
                        {
                            Type = json["startinfo"]["type"].ToObject<int>(),
                            SimpleInfo = new CoreStartSimpleInfo()
                            {
                                Program = json["startinfo"]["program"].ToString(),
                                Parameter = json["startinfo"]["param"].ToString()
                            }
                        },
                        ConfigInfo =
                            (json["configs"]?.ToObject<Dictionary<string, JToken>>() ??
                             new Dictionary<string, JToken>()).ToDictionary(k => k.Key,
                                keyValuePair => new CoreConfigInfo()
                                {
                                    Name = keyValuePair.Value["name"]?.ToString(),
                                    Type = keyValuePair.Value["type"]?.ToString(),
                                    Required = keyValuePair.Value["required"]?.ToObject<bool>() ?? false,
                                    Known = (keyValuePair.Value["known"])!.Select(t =>
                                        new CoreConfigKnownItem
                                        {
                                            Key = t["key"].ToString(),
                                            Name = t["name"].ToString(),
                                            Type = t["type"].ToObject<int>(),
                                            Visible = t["visible"]?.ToObject<bool>() ?? true,
                                            Force = t["force"]?.ToObject<bool>() ?? false,
                                            Value = t["value"]?.ToObject<string>() ?? String.Empty,
                                            Selection = (t["selection"]?.ToObject<List<JToken>>() ?? new List<JToken>())
                                                .Select(
                                                    token => new CoreConfigKnownItemSelection
                                                    {
                                                        Display = token["display"].ToString(),
                                                        Value = token["value"].ToString()
                                                    }).ToList()
                                        }
                                    ).ToList()
                                })
                    };


                    Cores[json["id"].ToString()] = core;
                }
                catch (Exception e)
                {
                    Log.Warning("核心 {0} 加载失败: {1}".Translate(), Path.GetFileName(directory), e.Message);
                }
            }
        }
    }
}