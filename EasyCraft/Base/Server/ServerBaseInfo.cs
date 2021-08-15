using System;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace EasyCraft.Base.Server
{
    public class ServerBaseInfo
    {
        [JsonProperty("id")] public int Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("owner")] public int Owner;
        [JsonProperty("player")] public int Player;
        [JsonProperty("expireTime")] public DateTime ExpireTime;
        [JsonProperty("expired")] public bool Expired => ExpireTime < DateTime.Now;

        [JsonProperty("port")] public int Port;
        [JsonProperty("ram")] public int Ram;
        [JsonProperty("autoStart")] public bool AutoStart;
        [JsonProperty("status")] public ServerStatus Status;

        public static ServerBaseInfo CreateFromSqlReader(SqliteDataReader reader)
        {
            return new ServerBaseInfo
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Owner = reader.GetInt32(2),
                ExpireTime = reader.GetDateTime(3),
                Port = reader.GetInt32(4),
                Ram = reader.GetInt32(5),
                AutoStart = reader.GetBoolean(6),
                Status = (ServerStatus)reader.GetInt32(7),
                Player = reader.GetInt32(8)
            };
        }

        public void RefreshInfo()
        {
            var cmd = Database.Database.CreateCommand(
                "SELECT id,name,owner,expire,port,ram,autostart,status,player WHERE id = $id ");
            cmd.Parameters.AddWithValue("$id", Id);
            var reader = cmd.ExecuteReader();
            reader.Read();
            Id = reader.GetInt32(0);
            Name = reader.GetString(1);
            Owner = reader.GetInt32(2);
            ExpireTime = reader.GetDateTime(3);
            Port = reader.GetInt32(4);
            Ram = reader.GetInt32(5);
            AutoStart = reader.GetBoolean(6);
            Status = (ServerStatus)reader.GetInt32(7);
            Player = reader.GetInt32(8);
        }
    }

    public enum ServerStatus
    {
        Normal,
        Paused,
        Deleted
    }
}