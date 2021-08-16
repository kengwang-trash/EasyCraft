using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace EasyCraft.Base.Server
{
    public class ServerStartInfo
    {
        [JsonProperty("id")] public int Id;
        [JsonProperty("core")] public string Core;
        [JsonIgnore] public string LastCore;
        [JsonProperty("world")] public string World;
        [JsonProperty("starter")] public string Starter;

        public static ServerStartInfo CreateFromSqliteById(int id)
        {
            var reader = Database.Database.CreateCommand(
                "SELECT id, core, lastcore, world, starter FROM server_start WHERE id = $id ", new[]
                {
                    new SqliteParameter("$id", SqliteType.Integer) { Value = id }
                }).ExecuteReader();
            reader.Read();
            return new ServerStartInfo
            {
                Id = reader.GetInt32(0),
                Core = reader.GetString(1),
                LastCore = reader.GetString(2),
                World = reader.GetString(3),
                Starter = reader.GetString(4)
            };
        }

        public void SyncToDatabase()
        {
            Database.Database
                .CreateCommand(
                    "UPDATE server_start SET ( core , lastcore , world , starter ) = ( $core , $lastcore , $world , $starter ) WHERE id = $id",
                    new Dictionary<string, object>()
                    {
                        { "$core", Core },
                        { "$lastcore", LastCore },
                        { "$world", World },
                        { "$starter", Starter },
                        { "$id", Id }
                    })
                .ExecuteNonQuery();
        }
    }

    public class ServerStartException
    {
        public int Code;
        public string Message;
    }
}