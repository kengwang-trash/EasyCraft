using System;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace EasyCraft.Base.Server
{
    public class ServerStartInfo
    {
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
                Core = reader.GetString(1),
                LastCore = reader.GetString(2),
                World = reader.GetString(3)
            };
        }
    }

    public class ServerStartException
    {
        public int Code;
        public string Message;
    }
}