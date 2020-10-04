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
                FastConsole.PrintError(string.Format(Language.t("数据库连接错误: {0}"), e.Message));
                if (0 != -2147481601)
                {
                    FastConsole.PrintInfo(Language.t("尝试创建新的数据库"));
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
                    FastConsole.PrintError(Language.t("数据库错误或不匹配当前版本. 按下 [Enter] 覆盖数据库 (危险) 或者退出 EasyCraft 手动备份并检查数据库"));
                    Console.ReadKey();
                    CreateNew();
                }
            }
            else
            {
                FastConsole.PrintError(Language.t("数据库错误或不匹配当前版本. 按下 [Enter] 覆盖数据库 (危险) 或者退出 EasyCraft 手动备份并检查数据库"));
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
                throw new Exception("No Database File Founded");
                /*
                var connectionString = new SQLiteConnectionStringBuilder()
                {
                    DataSource = Environment.CurrentDirectory + "/db/db.db"
                }.ToString();
                DB = new SQLiteConnection(connectionString);
                DB.Open();
                */
            }
            catch (Exception e)
            {
                FastConsole.PrintFatal(string.Format(Language.t("数据库创建失败: {0}"), e.Message));
                FastConsole.PrintFatal(Language.t("EasyCraft 无法运行, 按 [Enter] 退出"));
                Console.ReadKey();
                Environment.Exit(-5);
            }
        }
    }
}
