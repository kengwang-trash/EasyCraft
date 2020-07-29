using EasyCraft.Core;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCraft
{
    class Database
    {
        public static SqliteConnection DB = null;

        public static void Connect()
        {
            try
            {
                var connectionString = new SqliteConnectionStringBuilder()
                {
                    DataSource = Environment.CurrentDirectory + "/db/db.db",
                    Mode = SqliteOpenMode.ReadWriteCreate
                }.ToString();
                DB = new SqliteConnection(connectionString);
                DB.Open();
            }
            catch (Exception e)
            {
                FastConsole.PrintError("Database Connect Error: " + e.Message);
            }
        }
    }
}
