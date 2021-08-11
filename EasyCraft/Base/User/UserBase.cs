using Microsoft.Data.Sqlite;

namespace EasyCraft.Base.User
{
    public class UserBase
    {
        public UserInfoBase UserInfo;
        public UserRequestBase UserRequest;

        public static UserBase CreateFromSqliteDataReader(SqliteDataReader reader)
        {
            return new UserBase()
            {
                UserInfo = UserInfoBase.CreateFromSqliteReader(reader)
            };
        }

        public static UserBase Null = new UserBase()
        {
            UserInfo = UserInfoBase.Null,
            UserRequest = UserRequestBase.Null
        };
    }
}