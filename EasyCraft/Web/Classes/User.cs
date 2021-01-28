using System.Collections.Generic;
using EasyCraft.Core;

namespace EasyCraft.Web.Classes
{
    internal class User
    {
        private static readonly Dictionary<int, int> PermissonTable = new Dictionary<int, int>();
        public readonly string auth;
        public readonly string email;
        public readonly bool islogin;
        public readonly string name;
        public readonly string qq;
        public readonly int type;
        public readonly int uid;

        public User(string auth)
        {
            if (auth == "rawobj") return;
            var c = Database.DB.CreateCommand();
            c.CommandText = "SELECT * FROM user WHERE auth = $auth";
            c.Parameters.AddWithValue("$auth", auth);
            var r = c.ExecuteReader();
            if (!r.HasRows)
            {
                islogin = false;
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
            var c = Database.DB.CreateCommand();
            c.CommandText = "SELECT * FROM user WHERE username = $username AND password = $password ";
            c.Parameters.AddWithValue("$username", username);
            var pwmd5 = Functions.MD5(password);
            c.Parameters.AddWithValue("$password", pwmd5);
            var r = c.ExecuteReader();
            if (!r.HasRows)
            {
                islogin = false;
                FastConsole.PrintWarning(string.Format(Language.t("用户 {0} 登录失败."), username));
            }
            else
            {
                islogin = true;
                r.Read();
                uid = r.GetInt32(0);
                name = r.GetString(1);
                email = r.GetString(3);
                auth = Functions.MD5(Functions.GetRandomString() + pwmd5);
                type = r.GetInt32(5);
                qq = r.GetString(6);
                FastConsole.PrintSuccess(string.Format(Language.t("用户 {0} 成功登录."), username));
            }
        }

        public void UpdateAuth()
        {
            var co = Database.DB.CreateCommand();
            co.CommandText = "UPDATE user SET auth = $auth WHERE uid = $uid ";
            co.Parameters.AddWithValue("$auth", auth);
            co.Parameters.AddWithValue("$uid", uid);
            co.ExecuteNonQueryAsync();
        }

        public static User Register(string username, string password, string email, string qq = null)
        {
            var c = Database.DB.CreateCommand();
            if (!string.IsNullOrEmpty(qq))
            {
                c.CommandText =
                    "INSERT INTO `user` (username, password, email, `type` , `qq` ) VALUES ( $username , $password , $email , 1 , $qq)";
                c.Parameters.AddWithValue("$qq", qq);
            }
            else
            {
                c.CommandText =
                    "INSERT INTO `user` (username, password, email, `type` ) VALUES ( $username , $password , $email , 1 )";
            }

            c.Parameters.AddWithValue("$username", username);
            var pwmd5 = Functions.MD5(password);
            c.Parameters.AddWithValue("$password", pwmd5);
            c.Parameters.AddWithValue("$email", email);

            if (c.ExecuteNonQuery() != 0)
                return new User(username, password);
            return null;
        }

        public static int GetUid(string username)
        {
            var c = Database.DB.CreateCommand();
            c.CommandText = "SELECT * FROM `user` WHERE `username` = $username ";
            c.Parameters.AddWithValue("$username", username);
            var r = c.ExecuteReader();
            if (r.HasRows)
            {
                r.Read();
                return r.GetInt32(0);
            }

            return -1;
        }

        public bool CheckUserAbility(int PermissonID)
        {
            if (!PermissonTable.ContainsKey(PermissonID)) return false;
            return PermissonTable[PermissonID] <= type;
        }

        public static void RefreshPermissonTable()
        {
            PermissonTable.Clear();
            var c = Database.DB.CreateCommand();
            c.CommandText = "SELECT `pid`,`usertype` FROM `permission`";
            var r = c.ExecuteReader();
            while (r.Read()) PermissonTable[r.GetInt32(0)] = r.GetInt32(1);
        }
    }

    internal enum UserType
    {
        none,
        registered,
        customer,
        seller,
        admin
    }

    internal enum Permisson
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
        KillServer, //强制关闭服务器
        KillServerAll //强制关闭服务器目录下的所有进程 (danger)
    }
}