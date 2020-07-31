using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
            c.CommandText = "SELECT * FROM user WHERE auth = [auth]";
            c.Parameters.AddWithValue("[auth]", auth);
            SQLiteDataReader r = c.ExecuteReader();
            if (!r.HasRows)
            {
                islogin = false;
                return;
            }
            else
            {
                islogin = true;
                uid = r.GetInt32(0);
                name = r.GetString(1);
                email = r.GetString(3);
                this.auth = r.GetString(4);
                type = r.GetInt32(5);
            }
        }

        public User(string username,string password)
        {

        }
    }
}
