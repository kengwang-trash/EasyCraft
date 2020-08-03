using EasyCraft.Core;
using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EasyCraft
{
    class Database
    {
        public static SQLiteConnection DB = null;

        public static void Connect()
        {
            try
            {
                if (!File.Exists(Environment.CurrentDirectory + "/db/db.db"))
                    throw new Exception("Database File Not Found");
                var connectionString = new SQLiteConnectionStringBuilder()
                {
                    DataSource = Environment.CurrentDirectory + "/db/db.db",
                    Version = 3
                }.ToString();
                DB = new SQLiteConnection(connectionString);
                DB.Open();
                Check();
            }
            catch (Exception e)
            {
                FastConsole.PrintError(string.Format(Language.t("Database Connect Error: {0}"), e.Message));
                if (0 != -2147481601)
                {
                    FastConsole.PrintInfo(Language.t("Try Create new database"));
                    CreateNew();
                }

            }
        }

        public static void Check()
        {
            SQLiteCommand c = DB.CreateCommand();
            c.CommandText = "SELECT version FROM version";
            SQLiteDataReader sr = c.ExecuteReader();
            if (sr.HasRows)
            {
                sr.Read();
                if (sr.GetInt32(0) != 1)
                {
                    FastConsole.PrintError(Language.t("Database Not Complecatible. Press [Enter] to overwrite database (dangerous) OR Exit EasyCraft to check & backup your database"));
                    Console.ReadKey();
                    CreateNew();
                }
            }
            else
            {
                FastConsole.PrintError(Language.t("Database Not Complecatible. Press [Enter] to overwrite database (dangerous) OR Exit EasyCraft to check & backup your database"));
                Console.ReadKey();
                sr.Close();
                DB.Close();
                CreateNew();
            }
            sr.Close();
        }

        public static void CreateNew()
        {
            try
            {
                if (!File.Exists(Environment.CurrentDirectory + "/db/db.db"))
                    System.IO.File.Create(Environment.CurrentDirectory + "/db/db.db");
                if (DB != null)
                    DB.Close();
                File.WriteAllBytes(Environment.CurrentDirectory + "/db/db.db", Resource1.db);
                var connectionString = new SQLiteConnectionStringBuilder()
                {
                    DataSource = Environment.CurrentDirectory + "/db/db.db"
                }.ToString();
                DB = new SQLiteConnection(connectionString);
                DB.Open();
            }
            catch (Exception e)
            {
                FastConsole.PrintFatal(string.Format(Language.t("Database Create Error: {0}"), e.Message));
                FastConsole.PrintFatal(Language.t("EasyCraft Cannot Run anymore, Press [Enter] to exit"));
                Console.ReadKey();
                Environment.Exit(-5);
            }


        }
    }
}
