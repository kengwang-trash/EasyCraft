using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace EasyCraft.Base.Server
{
    public class ServerBaseInfo
    {
        [JsonProperty("id")] public int Id { get; internal set; }
        [JsonProperty("name")] public string Name { get; internal set; }
        [JsonProperty("owner")] public int Owner { get; internal set; }
        [JsonProperty("player")] public int Player { get; internal set; }
        [JsonProperty("expireTime")] public DateTime ExpireTime { get; internal set; }
        [JsonProperty("expireTimeRaw")] public string ExpireTimeRaw => ExpireTime.ToString("s");
        [JsonProperty("expired")] public bool Expired => ExpireTime < DateTime.Now;
        [JsonProperty("port")] public int Port { get; internal set; }
        [JsonProperty("ram")] public int Ram { get; internal set; }
        [JsonProperty("autoStart")] public bool AutoStart { get; internal set; }
        [JsonProperty("status")] public ServerStatus Status { get; internal set; }

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

        public void SyncToDatabase()
        {
            Database.Database.CreateCommand(
                "UPDATE servers SET (name,owner,expire,port,ram,autostart,status,player) = ( $name , $owner , $expire , $port , $ram , $autostart , $status , $player ) WHERE id = $id",
                new Dictionary<string, object>()
                {
                    { "$name", Name },
                    { "$owner", Owner },
                    { "$expire", ExpireTime },
                    { "$port", Port },
                    { "$ram", Ram },
                    { "$autostart", AutoStart },
                    { "$status", Status },
                    { "$player", Player },
                    { "$id", Id }
                }).ExecuteNonQuery();
        }
    }

    public enum ServerStatus
    {
        Normal,
        Paused,
        Deleted
    }
}