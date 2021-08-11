using System.Collections.Generic;
using System.Linq;
using EasyCraft.Utils;

namespace EasyCraft.Base.User
{
    public class UserManager
    {
        public static readonly Dictionary<int, UserBase> Users = new();
        
        //空间换时间
        public static readonly Dictionary<string, int> AuthToUid = new();

        public static void LoadUsers()
        {
            Users.Clear();
            var reader = Database.Database.CreateCommand("SELECT id, name, password, type, email FROM users")
                .ExecuteReader();
            while (reader.Read())
            {
                Users[reader.GetInt32(0)] = UserBase.CreateFromSqliteDataReader(reader);
            }
        }

        public static bool CheckUserNameOccupy(string username)
        {
            return Users.Values.All(t => t.UserInfo.Name != username);
        }

        public static int AddUser(UserInfoBase info)
        {
            var reader = Database.Database.CreateCommand(
                "INSERT INTO users (name, password, type, email) VALUES ( $name , $password , $type , $email ); SELECT id, name, password, type, email FROM users WHERE id = last_insert_rowid(); ",
                new Dictionary<string, object>()
                {
                    { "$name", info.Name },
                    { "$password", info.Password },
                    { "$type", info.Password },
                    { "$email", info.Email }
                }.ToSqliteParameter()).ExecuteReader();
            if (!reader.Read())
                return -1;
            Users[reader.GetInt32(0)] = UserBase.CreateFromSqliteDataReader(reader);
            return reader.GetInt32(0);
        }
    }
}