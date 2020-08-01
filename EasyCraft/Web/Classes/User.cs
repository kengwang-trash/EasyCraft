﻿using EasyCraft.Core;
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
        public User(string auth)
        {
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
                SQLiteCommand co = Database.DB.CreateCommand();
                co.CommandText = "UPDATE user SET auth = $auth WHERE uid = $uid ";
                co.Parameters.AddWithValue("$auth", auth);
                co.Parameters.AddWithValue("$uid", uid);
                co.ExecuteNonQuery();
            }
        }
    }
}
