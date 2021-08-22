using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace EasyCraft.Base.User
{
    [JsonObject(MemberSerialization.OptOut)]
    public class UserInfoBase
    {
        public static UserInfoBase Null = new()
        {
            Id = -1,
            Name = "null",
            Type = UserType.Everyone
        };

        [JsonProperty("email")] public string Email { get; internal set; }
        [JsonProperty("id")] public int Id { get; internal set; }
        [JsonProperty("name")] public string Name { get; internal set; }

        [JsonIgnore] internal string Password;

        [JsonProperty("type")] public UserType Type { get; internal set; }

        public static UserInfoBase CreateFromSqliteReader(SqliteDataReader reader)
        {
            return new UserInfoBase
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Password = reader.GetString(2),
                Type = (UserType)reader.GetInt32(3),
                Email = reader.GetString(4)
            };
        }

        public void SyncToDatabase()
        {
            Database.Database.CreateCommand(
                    "UPDATE users SET (name,password,type,email) = ( $name , $password , $type , $email ) WHERE id = $id",
                    new Dictionary<string, object>
                    {
                        { "$name", Name },
                        { "$password", Password },
                        { "$type", Type },
                        { "$email", Email },
                        { "$id", Id }
                    })
                .ExecuteNonQuery();
        }

        public bool Can(PermissionId pid)
        {
            return Permission.PermissionList[pid] <= Type;
        }
    }

    public enum UserType
    {
        Everyone,
        Registered,
        Technician,
        Admin,
        SuperUser,
        Plugin,
        Nobody
    }
}