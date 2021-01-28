using System;
using System.Collections.Generic;

namespace EasyCraft.Core
{
    internal class ServerManager
    {
        public static Dictionary<int, Server> servers = new Dictionary<int, Server>();

        public static void LoadServers()
        {
            var c = Database.DB.CreateCommand();
            c.CommandText = "SELECT id FROM server";
            var render = c.ExecuteReader();
            while (render.Read())
            {
                var id = render.GetInt32(0);
                try
                {
                    var s = new Server(id);
                    servers[render.GetInt32(0)] = s;
                }
                catch (Exception e)
                {
                    FastConsole.PrintWarning(string.Format(Language.t("服务器 {0} 加载错误: {1}"), id.ToString(), e.Message));
                }
            }
        }
    }
}