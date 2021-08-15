using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace EasyCraft.Base.Server
{
    public class ServerStartInfo
    {
        public int Id;
        public string Core;
        public string LastCore;
        public string World;

        public static ServerStartInfo CreateFromSqliteById(int id)
        {
            var reader = Database.Database.CreateCommand(
                "SELECT id, core, lastcore, world FROM server_start WHERE id = $id ", new[]
                {
                    new SqliteParameter("$id", SqliteType.Integer) { Value = id }
                }).ExecuteReader();
            reader.Read();
            return new ServerStartInfo
            {
                Id = reader.GetInt32(0),
                Core = reader.GetString(1),
                LastCore = reader.GetString(2),
                World = reader.GetString(3)
            };
        }

        public void SyncToDatabase()
        {
            Database.Database
                .CreateCommand(
                    "UPDATE server_start SET ( core , lastcore , world ) = ( $core , $lastcore , $world ) WHERE id = $id",
                    new Dictionary<string, object>()
                    {
                        { "$core", Core },
                        { "$lastcore", LastCore },
                        { "$world", World },
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