using System;
using Microsoft.Data.Sqlite;

namespace EasyCraft.Base.Server
{
    public struct ServerBaseInfo
    {
        public int Id;
        public string Name;
        public int Owner;
        public DateTime Expire;
        public int Port;
        public int Ram;
        public bool AutoStart;
        public ServerStatus Status;

        public static ServerBaseInfo CreateFromSqlReader(SqliteDataReader reader)
        {
            return new ServerBaseInfo
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Owner = reader.GetInt32(2),
                Expire = reader.GetDateTime(3),
                Port = reader.GetInt32(4),
                Ram = reader.GetInt32(5),
                AutoStart = reader.GetBoolean(6),
                Status = (ServerStatus) reader.GetInt32(7)
            };
        }

        public void RefreshInfo()
        {
            var cmd = Database.Database.CreateCommand("SELECT id,name,owner,expire,port,ram,autostart,status WHERE id = $id ");
            cmd.Parameters.AddWithValue("$id", Id);
            var reader = cmd.ExecuteReader();
            reader.Read();
            Id = reader.GetInt32(0);
            Name = reader.GetString(1);
            Owner = reader.GetInt32(2);
            Expire = reader.GetDateTime(3);
            Port = reader.GetInt32(4);
            Ram = reader.GetInt32(5);
            AutoStart = reader.GetBoolean(6);
            Status = (ServerStatus) reader.GetInt32(7);
        }
    }

    public enum ServerStatus
    {
        Normal,
        Paused,
        Deleted
    }
}