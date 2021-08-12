using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace EasyCraft.Base.User
{
    [JsonObject(MemberSerialization.OptOut)]
    public class UserInfoBase
    {
        [JsonProperty("id")]
        public int Id;
        [JsonProperty("name")]
        public string Name;

        [JsonIgnore] public string Password;

        [JsonProperty("type")]
        public UserType Type;
        [JsonProperty("email")]
        public string Email;

        public static UserInfoBase CreateFromSqliteReader(SqliteDataReader reader)
        {
            return new UserInfoBase()
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Password = reader.GetString(2),
                Type = (UserType) reader.GetInt32(3),
                Email = reader.GetString(4)
            };
        }

        public static UserInfoBase Null = new UserInfoBase()
        {
            Id = -1,
            Name = "null",
            Type = UserType.Everyone
        };
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