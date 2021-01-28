namespace EasyCraft.Web.JSONCallback
{
    internal class UserLogin : Callback
    {
        public LoginUserInfo data { get; set; }
    }

    internal class LoginUserInfo
    {
        public int uid { get; set; }

        public string username { get; set; }

        public string email { get; set; }
    }
}