using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace EasyCraft.Base.Server
{
    public class ServerBase
    {
        [JsonProperty("baseInfo")] public ServerBaseInfo BaseInfo;
        [JsonProperty("id")] public int Id;

        [JsonProperty("startInfo")] public ServerStartInfo StartInfo;

        public ServerBase(SqliteDataReader reader)
        {
            Id = reader.GetInt32(0);
            BaseInfo = ServerBaseInfo.CreateFromSqlReader(reader);
        }
    }
}