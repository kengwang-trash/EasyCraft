using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyCraft.Base.Core;
using EasyCraft.Base.Starter;
using EasyCraft.Base.User;
using EasyCraft.HttpServer.Api;
using EasyCraft.PluginBase;
using EasyCraft.Utils;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace EasyCraft.Base.Server
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ServerBase
    {
        [JsonProperty("baseInfo")] public ServerBaseInfo BaseInfo { get; internal set; }
        [JsonProperty("id")] public int Id { get; internal set; }
        [JsonProperty("startInfo")] public ServerStartInfo StartInfo { get; internal set; }
        [JsonProperty("statusInfo")] public ServerStatusInfo StatusInfo { get; internal set; }


        public static List<ServerConfigItem> ConfigItems = new();

        [JsonIgnore] public string ServerDir => AppDomain.CurrentDomain.BaseDirectory + "data/servers/" + Id + "/";
        [JsonIgnore] public CoreBase Core => CoreManager.Cores[StartInfo.Core];

        public ServerBase(SqliteDataReader reader)
        {
            Id = reader.GetInt32(0);
            BaseInfo = ServerBaseInfo.CreateFromSqlReader(reader);
            StartInfo = ServerStartInfo.CreateFromSqliteById(reader.GetInt32(0));
            StatusInfo = new();
            if (!Directory.Exists(ServerDir))
                Directory.CreateDirectory(ServerDir);
        }

        #region 服务器配置文件

        public void WriteConfigFile(string filename, Dictionary<string, string> vals = null)
        {
            StringBuilder sb = new();
            var configInfo = Core.ConfigInfo.FirstOrDefault(t => t.File == filename);
            if (configInfo == null)
                return;
            vals ??= new Dictionary<string, string>();

            switch (configInfo.Type)
            {
                case "properties":
                    var content = File.ReadAllLines(ServerDir + "/" + filename);
                    foreach (string s in content)
                    {
                        if (s.StartsWith("#") || string.IsNullOrWhiteSpace(s))
                        {
                            sb.AppendLine(s);
                            continue;
                        }

                        var kvp = s.Split('=').Select(t => t.Trim()).ToArray();
                        var knownItem = configInfo.Known.FirstOrDefault(t => t.Key == kvp[0]);
                        if (knownItem != null)
                        {
                            string value = string.Join('=', kvp.ToList().GetRange(1, kvp.Length - 1));
                            if (vals.ContainsKey(kvp[0]))
                            {
                                value = PhraseServerVar(vals[kvp[0]]);
                            }

                            if (knownItem.Force)
                            {
                                if (knownItem.Value == "{{REMOVE}}")
                                    continue;
                                value = PhraseServerVar(knownItem.Value);
                            }

                            sb.AppendLine(kvp[0] + "=" + value);
                        }
                        else
                        {
                            sb.AppendLine(s);
                            // ReSharper disable once RedundantJumpStatement
                            continue;
                        }
                    }

                    break;
            }

            File.WriteAllText(ServerDir + "/" + filename, sb.ToString());
        }

        public void LoadConfigFile()
        {
            foreach (var kvConfigInfo in CoreManager.Cores[StartInfo.Core].ConfigInfo)
            {
                if (!File.Exists(ServerDir + "/" + kvConfigInfo.File))
                    switch (kvConfigInfo.Required)
                    {
                        case true:
                            File.Create(ServerDir + "/" + kvConfigInfo.File);
                            break;
                        default:
                            continue;
                    }
                else
                    WriteConfigFile(kvConfigInfo.File);
            }
        }

        public List<Dictionary<string, object>> GetConfigFileContent(string filename)
        {
            var configInfo = Core.ConfigInfo.FirstOrDefault(t => filename == t.File);
            var ret = new List<Dictionary<string, object>>();
            if (configInfo is null) return ret;
            if (!File.Exists(ServerDir + "/" + filename)) return ret;
            switch (configInfo.Type)
            {
                case "properties":
                    var content = File.ReadAllLines(ServerDir + "/" + filename);
                    foreach (string s in content)
                    {
                        if (s.StartsWith("#") || string.IsNullOrWhiteSpace(s))
                        {
                            continue;
                        }

                        var kvp = s.Split('=').Select(t => t.Trim()).ToArray();
                        var knownItem = configInfo.Known.FirstOrDefault(t => t.Key == kvp[0]);
                        var value = string.Join('=', kvp.ToList().GetRange(1, kvp.Length - 1));
                        if (knownItem != null)
                        {
                            if (knownItem.Visible)
                            {
                                if (knownItem.Force) value = PhraseServerVar(knownItem.Value);
                                ret.Add(new Dictionary<string, object>
                                {
                                    { "key", knownItem.Key },
                                    { "display", knownItem.Display },
                                    { "type", knownItem.Type },
                                    { "value", value },
                                    { "selection", knownItem.Type == "select" ? knownItem.Selection : null }
                                });
                            }
                        }
                        else
                        {
                            ret.Add(new Dictionary<string, object>
                            {
                                { "key", kvp[0] },
                                { "display", kvp[0] },
                                { "type", 1 },
                                { "value", value },
                                { "selection", null }
                            });
                        }
                    }

                    break;
            }

            return ret;
        }

        #endregion


        public string PhraseServerVar(string origin)
        {
            if (origin == null) return null;
            if (origin == "{{REMOVE}}") return "";
            return origin
                .Replace("{{SERVERID}}", Id.ToString())
                .Replace("{{SERVERDIR}}", ServerDir)
                .Replace("{{CORE}}", StartInfo.Core)
                .Replace("{{PORT}}", BaseInfo.Port.ToString())
                .Replace("{{PLAYER}}", BaseInfo.Player.ToString())
                .Replace("{{ENV}}", AppDomain.CurrentDomain.BaseDirectory + "/environment/")
                .Replace("{{COREDIR}}",
                    Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "/data/cores/" + StartInfo.Core))
                .Replace("{{WORLD}}", StartInfo.World);
        }


        public async Task<ServerStartException> Start()
        {
            // 开启服务器
            // 首先检查是否到期
            if (BaseInfo.Expired)
            {
                StatusInfo.OnConsoleOutput("服务器已于 {0} 到期.".Translate(BaseInfo.ExpireTime.ToString("s")));
                return new ServerStartException
                {
                    Code = (int)ApiReturnCode.ServerExpired,
                    Message = "服务器已于 {0} 到期.".Translate(BaseInfo.ExpireTime.ToString("s"))
                };
            }

            // 再检查开服核心是否存在
            if (!CoreManager.Cores.ContainsKey(StartInfo.Core))
            {
                StatusInfo.OnConsoleOutput("服务器核心 {0} 不存在.".Translate(StartInfo.Core));
                return new ServerStartException
                {
                    Code = (int)ApiReturnCode.CoreNotFound,
                    Message = "服务器核心 {0} 不存在.".Translate(StartInfo.Core)
                };
            }

            // 再进行开服器校验
            if (!StarterManager.Starters.ContainsKey(StartInfo.Starter))
            {
                StatusInfo.OnConsoleOutput("服务器所需开服器 {0} 不存在.".Translate(StartInfo.Starter));
                return new ServerStartException
                {
                    Code = (int)ApiReturnCode.CoreNotFound,
                    Message = "服务器所需开服器 {0} 不存在.".Translate(StartInfo.Starter)
                };
            }

            if (!Core.Start.ContainsKey(Environment.OSVersion.Platform == PlatformID.Win32NT ? "windows" : "linux"))
            {
                StatusInfo.OnConsoleOutput("核心不支持当前系统版本.".Translate());
                return new ServerStartException
                {
                    Code = (int)ApiReturnCode.SystemNotSupport,
                    Message = "核心不支持当前系统版本.".Translate()
                };
            }

            // 在广播到插件 - 此处事件广播位点可以提出更改
            var ret = (await PluginController.BroadcastEventAsync("OnServerWillStart", new object[] { Id }))
                .Where(t => !(bool)t.Value).ToArray();
            if (ret.Length != 0)
            {
                StatusInfo.OnConsoleOutput("服务器被插件 {0} 拒绝开启.".Translate(ret[0].Key));
                return new ServerStartException
                {
                    Code = (int)ApiReturnCode.PluginReject,
                    Message = "服务器被插件 {0} 拒绝开启.".Translate(ret[0].Key)
                };
            }

            // 检查结束, 先进行开服前准备

            // 先检查是否更换核心
            try
            {
                if (StartInfo.LastCore != StartInfo.Core)
                {
                    StatusInfo.OnConsoleOutput("你的核心已更换, 正在加载核心文件".Translate());
                    Utils.Utils.DirectoryCopy(
                        AppDomain.CurrentDomain.BaseDirectory + "/data/cores/" + StartInfo.Core + "/files",
                        ServerDir);
                    StartInfo.LastCore = StartInfo.Core;
                    StartInfo.SyncToDatabase();
                }
            }
            catch (Exception e)
            {
                StatusInfo.OnConsoleOutput("更新核心文件时错误: {0}".Translate(e.Message), true);
                return new ServerStartException
                {
                    Code = (int)ApiReturnCode.InternalError,
                    Message = e.Message
                };
            }

            try
            {
                StatusInfo.OnConsoleOutput("正在加载配置项".Translate());
                LoadConfigFile();
            }
            catch (Exception e)
            {
                StatusInfo.OnConsoleOutput("加载配置文件错误: {0}".Translate(e.Message), true);
                return new ServerStartException
                {
                    Code = (int)ApiReturnCode.InternalError,
                    Message = e.Message
                };
            }


            StatusInfo.OnConsoleOutput("正在尝试调用开服器".Translate());
            bool? status;
            try
            {
                string system = Environment.OSVersion.Platform == PlatformID.Win32NT ? "windows" : "linux";
                status = (bool?)StarterManager.Starters[StartInfo.Starter].Type.GetMethod("ServerStart")
                    ?.Invoke(null, new object[]
                    {
                        this,
                        Path.GetFullPath(PhraseServerVar(Core.Start[system].Program)),
                        PhraseServerVar(Core.Start[system].Parameter),
                        EncodingConvert(Core.Encoding["input"]),
                        EncodingConvert(Core.Encoding["output"])
                    });
            }
            catch
            {
                status = false;
            }

            if (status is not true)
            {
                StatusInfo.OnConsoleOutput("开服器返回错误, 无法开服".Translate());
                return new ServerStartException
                {
                    Code = (int)ApiReturnCode.RequestFailed,
                    Message = "开服器返回错误, 无法开服".Translate()
                };
            }

            _ = PluginController.BroadcastEventAsync("OnServerStarted", new object[] { Id });
            return new ServerStartException
            {
                Code = 200,
                Message = "成功开服".Translate()
            };
        }

        public Encoding EncodingConvert(string name)
        {
            if (name == "default")
                return Encoding.Default;
            if (name == "utf-8")
                return new UTF8Encoding(false); //无BOM
            return Encoding.GetEncoding(name);
        }

        public async Task<bool> Stop()
        {
            try
            {
                // 在广播到插件 - 此处事件广播位点可以提出更改
                var plugins =
                    (await PluginController.BroadcastEventAsync("OnServerWillStop", new object[] { Id }))
                    .Where(t => !(bool)t.Value).ToArray();
                if (plugins.Length > 0)
                {
                    StatusInfo.OnConsoleOutput("服务器被插件 {0} 拒绝开启.".Translate(plugins[0].Key));
                    return false;
                }

                bool? status = (bool?)StarterManager.Starters[StartInfo.Starter].Type.GetMethod("ServerStop")
                    ?.Invoke(null, new object[]
                    {
                        this
                    });
                if (status is true)
                    _ = PluginController.BroadcastEventAsync("OnServerStopped", new object[] { Id });
                return status is true;
            }
            catch (Exception e)
            {
                StatusInfo.OnConsoleOutput("关闭服务器失败: {0}".Translate(e.Message));
                return false;
            }
        }

        public void RequestInput(string content)
        {
            // 进行开服器校验
            if (!StarterManager.Starters.ContainsKey(StartInfo.Starter))
            {
                StatusInfo.OnConsoleOutput("服务器所需开服器 {0} 不存在.".Translate(StartInfo.Starter));
                return;
            }

            if (StatusInfo.Status != 2)
            {
                StatusInfo.OnConsoleOutput("当前服务器状态不允许输入指令".Translate());
                return;
            }

            bool? status = (bool?)StarterManager.Starters[StartInfo.Starter].Type.GetMethod("OnServerInput")
                ?.Invoke(null, new object[]
                {
                    this,
                    content
                });
            if (status is not true) StatusInfo.OnConsoleOutput("指令输入失败".Translate());
        }

        public List<ServerConfigItem> GetServerConfigItems(UserBase user)
        {
            // 首先是 EasyCraft 默认提供的
            var ret = new List<ServerConfigItem>
            {
                new()
                {
                    Display = "服务器名称",
                    Id = "name",
                    Type = "text",
                    NoEdit = !user.UserInfo.Can(PermissionId.ChangeServerName),
                    Value = BaseInfo.Name
                },
                new()
                {
                    Display = "服务器 IP",
                    Id = "ip",
                    Type = "text",
                    NoEdit = true,
                    Value = Common.Configuration["ServerIP"]
                },
                new()
                {
                    Display = "端口",
                    Id = "port",
                    Type = "number",
                    NoEdit = !user.UserInfo.Can(PermissionId.ChangeServerPort),
                    Value = BaseInfo.Port
                },
                new()
                {
                    Display = "总玩家数",
                    Id = "player",
                    Type = "number",
                    NoEdit = !user.UserInfo.Can(PermissionId.ChangeServerMaxPlayer),
                    Value = BaseInfo.Player
                },
                new()
                {
                    Display = "到期时间",
                    Id = "expireTime",
                    Type = "date",
                    NoEdit = !user.UserInfo.Can(PermissionId.ChangeServerExpire),
                    Value = BaseInfo.ExpireTime.ToString("yyyy-MM-dd")
                },
                new()
                {
                    Display = "最大内存",
                    Id = "ram",
                    Type = "number",
                    NoEdit = !user.UserInfo.Can(PermissionId.ChangeServerRam),
                    Value = BaseInfo.Ram
                },
                new()
                {
                    Display = "自动开启",
                    Id = "autoStart",
                    Type = "toggle",
                    NoEdit = !user.UserInfo.Can(PermissionId.ChangeServerAutoStart),
                    Value = BaseInfo.AutoStart
                },
                new()
                {
                    Display = "默认世界",
                    Id = "world",
                    Type = "text",
                    NoEdit = false,
                    Value = StartInfo.World
                }
            };
            return ret;
        }

        public async Task<dynamic> GetServerPluginItems(UserBase user)
        {
            // 然后询问插件们有没有要追加的
            // 来了, 又是一个贼长的 LINQ
            var ret = (await PluginController.BroadcastEventAsync("OnGetServerPluginItems",
                new object[] { Id, user.UserInfo.Id })).Select(t => new Dictionary<string, object>
            {
                {
                    "Plugin", new Dictionary<string, string>
                    {
                        { "id", t.Key },
                        { "name", PluginController.Plugins[t.Key].Info.Name }
                    }
                },
                {
                    "Values", ((List<Dictionary<string, object>>)t.Value).Select(v => new ServerConfigItem
                    {
                        Display = v["display"].ToString(),
                        Id = v["id"].ToString(),
                        Type = v["type"].ToString(),
                        NoEdit = v["noedit"].ToString() == "true",
                        Value = v["value"].ToString()
                    })
                }
            });
            return ret;
        }
    }

    public class ServerConfigItem
    {
        [JsonProperty("display")] public string Display;
        [JsonProperty("id")] public string Id;
        [JsonProperty("type")] public string Type;
        [JsonProperty("noedit")] public bool NoEdit;
        [JsonProperty("value")] public object Value;
    }
}