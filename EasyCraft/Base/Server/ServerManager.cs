using System;
using System.Collections.Generic;
using Serilog;

namespace EasyCraft.Base.Server
{
    public class ServerManager
    {
        public static readonly Dictionary<int, ServerBase> Servers = new();

        public static void LoadServers()
        {
            Servers.Clear();
            var cmd = Database.Database.CreateCommand(
                "SELECT id,name,owner,expire,port,ram,autostart,status FROM servers");
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
    }
}