using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EasyCraft.Utils;
using Serilog;

namespace EasyCraft.Base.Starter
{
    public class StarterManager
    {
        public static readonly Dictionary<string, StarterBase> Starters = new();

        public static void InitializeStarters()
        {
            foreach (string file in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + "/data/starters",
                "*.dll"))
            {
                try
                {
                    Type type = null;
                    if (!File.Exists(Path.ChangeExtension(file, "pdb")))
                    {
                        type = Assembly.LoadFile(file).GetType("EasyCraftStarter.Starter");
                    }
                    else
                    {
                        type = Assembly.Load(File.ReadAllBytes(file),
                                File.ReadAllBytes(Path.ChangeExtension(file, "pdb")))
                            .GetType("EasyCraftStarter.Starter");
                    }

                    Dictionary<string, string> ret = (Dictionary<string, string>)type
                        ?.GetMethod("InitializeStarter")
                        ?.Invoke(null, null);
                    if (ret == null)
                    {
                        Log.Warning("开服器 {0} 加载失败: {1}".Translate(), Path.GetFileName(file), "Get Starter Info Failed");
                        continue;
                    }

                    Starters[ret["id"]] = new StarterBase
                    {
                        Type = type,
                        Name = ret["name"],
                        Id = ret["id"],
                        Version = ret["version"],
                        Description = ret["description"],
                        Author = ret["author"]
                    };
                }
                catch (Exception e)
                {
                    Log.Warning("开服器 {0} 加载失败: {1}".Translate(), Path.GetFileName(file), e.Message);
                }
            }
        }
    }
}