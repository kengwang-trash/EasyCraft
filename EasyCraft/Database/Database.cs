using System.Collections.Generic;
using EasyCraft.Utils;
using Microsoft.Data.Sqlite;
using Serilog;

namespace EasyCraft.Database
{
    internal class Database
    {
        public static string Password;
        private static SqliteConnection _db = new();

        public static bool IsConnected;

        public static bool Connect()
        {
            try
            {
                _db = new SqliteConnection(new SqliteConnectionStringBuilder
                {
                    DataSource = "data/database.db",
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Password = Password
                }.ToString());
                _db.Open();
                IsConnected = true;
                return true;
            }
            catch (SqliteException e)
            {
                Log.Error("连接到数据库失败: {0}".Translate(e.Message));
                return false;
            }
        }

        public static SqliteCommand CreateCommand(string comm, IEnumerable<SqliteParameter> parameters = null)
        {
            var ans = _db.CreateCommand();
            ans.CommandText = comm;
            if (parameters != null)
                ans.Parameters.AddRange(parameters);
            return ans;
        }
    }
}