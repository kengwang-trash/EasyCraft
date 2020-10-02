using EasyCraft.Core;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

namespace EasyCraft.Web.Classes
{
    class User
    {
        public readonly bool islogin = false;
        public readonly int uid;
        public readonly string name;
        public readonly string auth;
        public readonly string email;
        public readonly int type = 0;
        public readonly string qq;
        private static Dictionary<int, int> PermissonTable = new Dictionary<int, int>();

        public User(string auth)
        {
            if (auth == "rawobj") return;
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText = "SELECT * FROM user WHERE auth = $auth";
            c.Parameters.AddWithValue("$auth", auth);
            SQLiteDataReader r = c.ExecuteReader();
            if (!r.HasRows)
            {
                islogin = false;
                return;
            }
            else
            {
                islogin = true;
                r.Read();
                uid = r.GetInt32(0);
                name = r.GetString(1);
                email = r.GetString(3);
                this.auth = r.GetString(4);
                type = r.GetInt32(5);
                qq = r.GetString(6);
            }
        }

        public User(string username, string password)
        {
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText = "SELECT * FROM user WHERE username = $username AND password = $password ";
            c.Parameters.AddWithValue("$username", username);
            string pwmd5 = Functions.MD5(password);
            c.Parameters.AddWithValue("$password", pwmd5);
            SQLiteDataReader r = c.ExecuteReader();
            if (!r.HasRows)
            {
                islogin = false;
                FastConsole.PrintWarning(string.Format(Language.t("用户 {0} 登录失败."), username));
                return;
            }
            else
            {
                islogin = true;
                r.Read();
                uid = r.GetInt32(0);
                name = r.GetString(1);
                email = r.GetString(3);
                auth = Functions.MD5(Functions.GetRandomString(15) + pwmd5);
                type = r.GetInt32(5);
                qq = r.GetString(6);
                FastConsole.PrintSuccess(string.Format(Language.t("用户 {0} 成功登录."), username));
            }
        }

        public void UpdateAuth()
        {
            SQLiteCommand co = Database.DB.CreateCommand();
            co.CommandText = "UPDATE user SET auth = $auth WHERE uid = $uid ";
            co.Parameters.AddWithValue("$auth", auth);
            co.Parameters.AddWithValue("$uid", uid);
            co.ExecuteNonQueryAsync();
        }

        public static User Register(string username, string password, string email, string qq)
        {
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText =
                "INSERT INTO `user` (username, password, email, `type` , `qq` ) VALUES ( $username , $password , $email , 1 , $qq)";
            c.Parameters.AddWithValue("$username", username);
            string pwmd5 = Functions.MD5(password);
            c.Parameters.AddWithValue("$password", pwmd5);
            c.Parameters.AddWithValue("$email", email);
            c.Parameters.AddWithValue("$qq", qq);
            if (c.ExecuteNonQuery() != 0)
            {
                return new User(username, password);
            }
            else
            {
                return null;
            }
        }

        public static int GetUid(string username)
        {
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText = "SELECT * FROM `user` WHERE `username` = $username ";
            c.Parameters.AddWithValue("$username", username);
            SQLiteDataReader r = c.ExecuteReader();
            if (r.HasRows)
            {
                r.Read();
                return r.GetInt32(0);
            }
            else
            {
                return -1;
            }
        }

        public bool CheckUserAbility(int PermissonID)
        {
            if (!PermissonTable.ContainsKey(PermissonID)) return false;
            return PermissonTable[PermissonID] <= this.type;
        }

        public static void RefreshPermissonTable()
        {
            PermissonTable.Clear();
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText = "SELECT `pid`,`usertype` FROM `permission`";
            SQLiteDataReader r = c.ExecuteReader();
            while (r.Read())
            {
                PermissonTable[r.GetInt32(0)] = r.GetInt32(1);
            }
        }
    }

    enum UserType
    {
        none,
        registered,
        customer,
        seller,
        admin
    }

    enum Permisson
    {
        CreateServer, //创建服务器
        EditServer, //编辑服务器信息
        StartServer, //开启任意服务器
        StopServer, //停止任意服务器
        QueryLog, //获取任意日志
        SendCmd, //发送指令
        CleanLog, //清除日志
        SeeServer, //查看任意服务器信息
        EditAnnouncement, //编辑公告
        UseAllFTP, //使用全部的FTP
        UseFTP, //使用FTP
        SeeAllServer, //查看全部服务器
        KillServer,//强制关闭服务器
        KillServerAll,//强制关闭服务器目录下的所有进程 (danger)
    }
}