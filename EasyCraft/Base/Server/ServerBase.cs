using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyCraft.Base.Core;
using EasyCraft.Base.User;
using EasyCraft.HttpServer.Api;
using EasyCraft.Utils;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace EasyCraft.Base.Server
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ServerBase
    {
        [JsonProperty("baseInfo")] public ServerBaseInfo BaseInfo;
        [JsonProperty("id")] public int Id;
        [JsonProperty("startInfo")] public ServerStartInfo StartInfo;
        [JsonProperty("statusInfo")] public ServerStatusInfo StatusInfo;

        public static List<ServerConfigItem> ConfigItems = new();

        [JsonIgnore] public string ServerDir => Directory.GetCurrentDirectory() + "/data/servers/" + Id + "/";
        [JsonIgnore] public CoreBase Core => CoreManager.Cores[StartInfo.Core];

        public ServerBase(SqliteDataReader reader)
        {
            Id = reader.GetInt32(0);
            BaseInfo = ServerBaseInfo.CreateFromSqlReader(reader);
            StartInfo = ServerStartInfo.CreateFromSqliteById(reader.GetInt32(0));
            StatusInfo = new();
        }

        public void LoadConfigFile()
        {
            foreach (var kvConfigInfo in CoreManager.Cores[StartInfo.Core].ConfigInfo)
            {
                if (!File.Exists(ServerDir + "/" + kvConfigInfo.Key) && kvConfigInfo.Value.Required)
                    File.Create(ServerDir + "/" + kvConfigInfo.Key);
                else
                    continue;
                WriteConfigFile(kvConfigInfo.Key);
            }
        }

        public string PhraseServerVar(string origin)
        {
            return origin
                .Replace("{{SERVERID}}", Id.ToString())
                .Replace("{{SERVERDIR}}", ServerDir)
                .Replace("{{CORE}}", StartInfo.Core)
                .Replace("{{PORT}}", BaseInfo.Port.ToString())
                .Replace("{{PLAYER}}", BaseInfo.Player.ToString())
                .Replace("{{WORLD}}", StartInfo.World);
        }

        public void WriteConfigFile(string filename, Dictionary<string, string> vals = null)
        {
            StringBuilder sb = new();
            var configInfo = Core.ConfigInfo[filename];
            if (configInfo == null)
                return;
            if (vals == null)
            {
                vals = new Dictionary<string, string>();
            }

            switch (configInfo.Type)
            {
                case "properties":
                    var content = File.ReadLines(ServerDir + "/" + filename);
                    foreach (string s in content)
                    {
                        if (s.StartsWith("#"))
                        {
                            sb.AppendLine(s);
                            continue;
                        }

                        var kvp = s.Split('=').Select(t => t.Trim()).ToArray();
                        var knownItem = configInfo.Known.FirstOrDefault(t => t.Key == kvp[0]);
                        if (knownItem != null)
                        {
                            string value = "";
                            if (vals.ContainsKey(kvp[0]))
                            {
                                value = vals[kvp[0]];
                            }

                            if (knownItem.Force)
                            {
                                value = knownItem.Value;
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

            // TODO: 再进行开服器校验
            // Write Your Here

            // 在广播到插件 - 此处事件广播位点可以提出更改
            var ret = (await PluginBase.PluginController.BroadcastEventAsync("OnServerStart", new object[] { Id }))
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
            if (StartInfo.LastCore != StartInfo.Core)
            {
                StatusInfo.OnConsoleOutput("你的核心已更换, 正在加载核心文件".Translate(),
                    false);
                Utils.Utils.DirectoryCopy(Directory.GetCurrentDirectory() + "/data/cores/" + StartInfo.Core + "/files",
                    ServerDir);
            }

            StatusInfo.OnConsoleOutput("正在加载配置项".Translate(),
                false);
            LoadConfigFile();

            StatusInfo.OnConsoleOutput("正在尝试调用开服器".Translate(),
                false);
            // TODO: 调用开服器

            return new ServerStartException()
            {
                Code = 200,
                Message = "成功开服".Translate()
            };
        }

        public async Task<List<ServerConfigItem>> GetServerConfigItems(UserBase user)
        {
            // 首先是 EasyCraft 默认提供的
            var ret = new List<ServerConfigItem>()
            {
                new()
                {
                    Display = "服务器名称",
                    Id = "name",
                    Type = "text",
                    Editable = user.UserInfo.Type > UserType.Technician,
                    Value = BaseInfo.Name
                },
                new()
                {
                    Display = "总玩家数",
                    Id = "player",
                    Type = "number",
                    Editable = user.UserInfo.Type > UserType.Technician,
                    Value = BaseInfo.Player
                },
                new()
                {
                    Display = "到期时间",
                    Id = "expireTime",
                    Type = "date",
                    Editable = user.UserInfo.Type > UserType.Technician,
                    Value = BaseInfo.ExpireTime.ToString("yyyy-MM-dd")
                },
                new()
                {
                    Display = "端口",
                    Id = "port",
                    Type = "number",
                    Editable = user.UserInfo.Type > UserType.Technician,
                    Value = BaseInfo.Port
                },
                new()
                {
                    Display = "最大内存",
                    Id = "ram",
                    Type = "number",
                    Editable = user.UserInfo.Type > UserType.Technician,
                    Value = BaseInfo.Ram
                },
                new()
                {
                    Display = "自动开启",
                    Id = "autoStart",
                    Type = "toggle",
                    Editable = user.UserInfo.Type > UserType.Technician,
                    Value = BaseInfo.AutoStart
                },
                new ()
                {
                    Display = "核心",
                    Id = "core",
                    Type = "select",
                    Editable = true,
                    Value = StartInfo.Core
                },
                new ()
                {
                    Display = "默认世界",
                    Id = "world",
                    Type = "text",
                    Editable = true,
                    Value = StartInfo.World
                }
            };

            // 然后询问插件们有没有要追加的
            // 来了, 又是一个贼长的 LINQ
            ret.AddRange((await PluginBase.PluginController.BroadcastEventAsync("OnGetServerConfigItems",
                new object[] { Id, user.UserInfo.Id , ret })).Values.Cast<Dictionary<string, string>>().Select(t =>
                new ServerConfigItem
                {
                    Display = t["display"],
                    Id = t["id"],
                    Type = t["type"],
                    Editable = t["editable"] == "true"
                }));
            return ret;
        }
    }

    public class ServerConfigItem
    {
        [JsonProperty("display")] public string Display;
        [JsonProperty("id")] public string Id;
        [JsonProperty("type")] public string Type;
        [JsonProperty("editable")] public bool Editable;
        [JsonProperty("value")] public object Value;
    }
}