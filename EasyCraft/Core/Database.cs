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
                var connectionString = new SQLiteConnectionStringBuilder()
                {
                    DataSource = Environment.CurrentDirectory + "/db/db.db",
                    Version = 3
                }.ToString();
                DB = new SQLiteConnection(connectionString);
                DB.Open();
            }
            catch (Exception e)
            {
                FastConsole.PrintError("Database Connect Error: " + e.Message);
                if (0 != -2147481601)
                {
                    FastConsole.PrintInfo("Try Create new database");
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
                if (sr.GetInt32(1) != 1)
                {
                    FastConsole.PrintError("Database Not Complecatible. Press [Enter] to overwrite database OR Exit EasyCraft to check & backup your database");
                    Console.ReadKey();
                    CreateNew();
                }
            }
            else
            {
                FastConsole.PrintError("Database Not Complecatible. Press [Enter] to overwrite database OR Exit EasyCraft to check & backup your database");
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
                System.IO.File.Create(Environment.CurrentDirectory + "/db/db.db");
                File.WriteAllText(Environment.CurrentDirectory + "/db/db.db", Resource1.db.ToString());
                var connectionString = new SQLiteConnectionStringBuilder()
                {
                    DataSource = Environment.CurrentDirectory + "/db/db.db"
                }.ToString();
                DB = new SQLiteConnection(connectionString);
                DB.Open();
            }
            catch (Exception e)
            {
                FastConsole.PrintFatal("Database Create Error: " + e.Message);
                FastConsole.PrintFatal("EasyCraft Cannot Run anymore, Press [Enter] to exit");
                Console.ReadKey();
                Environment.Exit(-5);
            }


        }
    }
}
