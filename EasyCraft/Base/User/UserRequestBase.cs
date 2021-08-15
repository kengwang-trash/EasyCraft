using Newtonsoft.Json;

namespace EasyCraft.Base.User
{
    public class UserRequestBase
    {
        public bool IsLogin;
        [JsonProperty("auth")] public string Auth;

        public static UserRequestBase Null = new()
        {
            IsLogin = false,
            Auth = "null"
        };
    }
}