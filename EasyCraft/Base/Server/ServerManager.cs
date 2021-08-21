using System;
using System.Collections.Generic;
using Serilog;

namespace EasyCraft.Base.Server
{
    public static class ServerManager
    {
        public static readonly Dictionary<int, ServerBase> Servers = new();

        public static void LoadServers()
        {
            Servers.Clear();
            var cmd = Database.Database.CreateCommand(
                "SELECT id,name,owner,expire,port,ram,autostart,status,player FROM servers");
            var reader = cmd.ExecuteReader();
            while (reader.Read())
                try
                {
                    Servers[reader.GetInt32(0)] = new ServerBase(reader);
                }
                catch (Exception e)
                {
                    Log.Warning("加载服务器 {0} 出错: {1}", reader.GetInt32(0), e.Message);
                }
        }


        public static int AddServer(ServerBaseInfo info)
        {
            var reader = Database.Database.CreateCommand(
                "INSERT INTO servers (name, owner, expire, port, ram, autostart, status, player) VALUES ($name,$owner,$expire,$port,$ram,$autostart,$status,$player); SELECT id, name, owner, expire, port, ram, autostart, status, player FROM servers WHERE id = last_insert_rowid();",
                new Dictionary<string, object>()
                {
                    { "$name", info.Name },
                    { "$owner", info.Owner },
                    { "$expire", info.ExpireTime },
                    { "$port", info.Port },
                    { "$ram", info.Ram },
                    { "$autostart", info.AutoStart },
                    { "$status", info.Status },
                    { "player", info.Player }
                }).ExecuteReader();
            reader.Read();
            Servers[reader.GetInt32(0)] = new ServerBase(reader);
            return reader.GetInt32(0);
        }
    }
}