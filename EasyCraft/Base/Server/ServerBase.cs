using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyCraft.Base.Core;
using EasyCraft.HttpServer.Api;
using EasyCraft.Utils;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Serilog;

namespace EasyCraft.Base.Server
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ServerBase
    {
        [JsonProperty("baseInfo")] public ServerBaseInfo BaseInfo;
        [JsonProperty("id")] public int Id;
        [JsonProperty("startInfo")] public ServerStartInfo StartInfo;
        [JsonIgnore] public string ServerDir => Directory.GetCurrentDirectory() + "/data/servers/" + Id + "/";
        [JsonIgnore] public CoreBase Core => CoreManager.Cores[StartInfo.Core];

        public ServerBase(SqliteDataReader reader)
        {
            Id = reader.GetInt32(0);
            BaseInfo = ServerBaseInfo.CreateFromSqlReader(reader);
            StartInfo = ServerStartInfo.CreateFromSqliteById(reader.GetInt32(0));
        }

        public void LoadConfigFile()
        {
            foreach (var kvConfigInfo in CoreManager.Cores[StartInfo.Core].ConfigInfo)
            {
                if (!File.Exists(ServerDir + "/" + kvConfigInfo.Key) && kvConfigInfo.Value.Required)
                    File.Create(ServerDir + "/" + kvConfigInfo.Key);
                else
                    continue;
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
                            if (knownItem.Force)
                            {
                                var list = kvp.ToList();
                                list.RemoveAt(0);
                                sb.AppendLine(kvp[0] + "=" + PhraseServerVar(string.Join('=', list)));
                            }
                        }
                        else
                        {
                            sb.AppendLine(s);
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
                Log.Warning("服务器已于 {0} 到期.".Translate(), BaseInfo.ExpireTime);
                return new ServerStartException
                {
                    Code = (int)ApiErrorCode.ServerExpired,
                    Message = "服务器已于 {0} 到期.".Translate(BaseInfo.ExpireTime)
                };
            }

            // 再检查开服核心是否存在
            if (!CoreManager.Cores.ContainsKey(StartInfo.Core))
            {
                Log.Warning("服务器核心 {0} 不存在.".Translate(), StartInfo.Core);
                return new ServerStartException
                {
                    Code = (int)ApiErrorCode.CoreNotFound,
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
                Log.Warning("服务器被插件 {0} 拒绝开启.".Translate(), ret[0].Key);
                return new ServerStartException
                {
                    Code = (int)ApiErrorCode.PluginReject,
                    Message = "服务器被插件 {0} 拒绝开启.".Translate(ret[0].Key)
                };
            }

            // 检查结束, 先进行开服前准备
            LoadConfigFile();

            return new ServerStartException()
            {
                Code = 200,
                Message = "成功开服".Translate()
            };
        }
    }
}