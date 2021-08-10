namespace EasyCraft.Base.User
{
    public struct UserRequestBase
    {
        public bool IsLogin;
        public string Auth;

        public static UserRequestBase Null = new UserRequestBase()
        {
            IsLogin = false,
            Auth = "null"
        };
    }
}