using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCraft.Web.JSONCallback
{
    class UserLogin : Callback
    {        public LoginUserInfo data { get; set; }
    }
    class LoginUserInfo
    {
        public int uid { get; set; }

        public string username { get; set; }

        public string email { get; set; }
    }
}
