using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Text.Json;

namespace EasyCraft.Core
{
    class ServerManager
    {
        public static Dictionary<int, Server> servers = new Dictionary<int, Server>();

        public static void LoadServers()
        {
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText = "SELECT id FROM server";
            SQLiteDataReader render = c.ExecuteReader();
            while (render.Read())
            {
                Server s = new Server(render.GetInt32(0));
                servers[render.GetInt32(0)] = s;
            }
        }
    }
}
