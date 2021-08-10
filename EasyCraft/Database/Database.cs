using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EasyCraft.Utils;
using Microsoft.Data.Sqlite;
using Serilog;

namespace EasyCraft.Database
{
    internal class Database
    {
        public static string password;
        public static SqliteConnection Db = new SqliteConnection();

        public static bool isConnected;

        public static bool Connect()
        {
            try
            {
                Db = new SqliteConnection(new SqliteConnectionStringBuilder
                {
                    DataSource = "data/database.db",
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Password = password,
                }.ToString());
                Db.Open();
                isConnected = true;
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
            var ans = Db.CreateCommand();
            ans.CommandText = comm;
            if (parameters != null)
                ans.Parameters.AddRange(parameters);
            return ans;
        }
    }
}