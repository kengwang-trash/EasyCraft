using Microsoft.Data.Sqlite;

namespace EasyCraft.Base.Server
{
    public class ServerBase
    {
        public int Id;
        public ServerBaseInfo BaseInfo;

        public ServerBase(SqliteDataReader reader)
        {
            Id = reader.GetInt32(0);
            BaseInfo = ServerBaseInfo.CreateFromSqlReader(reader);
        }
    }
}