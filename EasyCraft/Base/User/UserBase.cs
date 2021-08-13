using Microsoft.Data.Sqlite;

namespace EasyCraft.Base.User
{
    public class UserBase
    {
        public static UserBase Null = new()
        {
            UserInfo = UserInfoBase.Null,
            UserRequest = UserRequestBase.Null
        };

        public UserInfoBase UserInfo;
        public UserRequestBase UserRequest;

        public static UserBase CreateFromSqliteDataReader(SqliteDataReader reader)
        {
            return new UserBase
            {
                UserInfo = UserInfoBase.CreateFromSqliteReader(reader)
            };
        }
    }
}