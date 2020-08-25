using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace EasyCraft.Web.JSONCallback
{
    class UserLogin : Callback
    {
        [JsonPropertyName("data")]
        public LoginUserInfo data { get; set; }
    }
    class LoginUserInfo
    {
        [JsonPropertyName("uid")]
        public int uid { get; set; }

        [JsonPropertyName("username")]
        public string username { get; set; }

        [JsonPropertyName("email")]
        public string email { get; set; }
    }
}
