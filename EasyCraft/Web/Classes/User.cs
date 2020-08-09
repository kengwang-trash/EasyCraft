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
        public readonly int type;
        public readonly string qq;
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
                FastConsole.PrintWarning(string.Format(Language.t("User {0} try to login but faild."), username));
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
                SQLiteCommand co = Database.DB.CreateCommand();
                co.CommandText = "UPDATE user SET auth = $auth WHERE uid = $uid ";
                co.Parameters.AddWithValue("$auth", auth);
                co.Parameters.AddWithValue("$uid", uid);
                co.ExecuteNonQuery();
                FastConsole.PrintSuccess(string.Format(Language.t("User {0} Login Successful."), username));

            }
        }

        public static User Register(string username, string password, string email,string qq)
        {
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText = "INSERT INTO `user` (username, password, email, `type` , `qq` ) VALUES ( $username , $password , $email , 1 , $qq)";
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

        public static bool Exist(string username)
        {
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText = "SELECT * FROM user WHERE username = $username ";
            c.Parameters.AddWithValue("$username", username);
            SQLiteDataReader r = c.ExecuteReader();
            return r.HasRows;
        }
    }
}
