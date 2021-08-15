using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EasyCraft.Base.Server;
using EasyCraft.Base.User;
using EasyCraft.Utils;
using Microsoft.AspNetCore.Http;

namespace EasyCraft.HttpServer.Api
{
    public static class HttpApis
    {
        public static int Version = 1;

        public static ApiReturnBase ApiLogin(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["username"]) ||
                string.IsNullOrEmpty(context.Request.Form["password"]))
                return ApiReturnBase.IncompleteParameters;
            var user =
                UserManager.Users.Values.FirstOrDefault(t => t.UserInfo.Name == context.Request.Form["username"]);
            if (user == null)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.UserNotFound,
                    Msg = "用户不存在".Translate()
                };
            if (user.UserInfo.Password !=
                context.Request.Form["password"].ToString().GetMD5())
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.Unauthorized,
                    Msg = "登录失败, 密码错误"
                };
            if (user.UserRequest.Auth != null)
                UserManager.AuthToUid.Remove(user.UserRequest.Auth);
            user.UserRequest.Auth = Utils.Utils.CreateRandomString();
            UserManager.AuthToUid[user.UserRequest.Auth] = user.UserInfo.Id;
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功登录".Translate(),
                Data = user
            };
        }

        public static ApiReturnBase ApiLoginStatus(HttpContext context)
        {
            var auth = context.Request.Headers["Authorization"].ToString();
            if (auth == null || auth == "null" || !UserManager.AuthToUid.ContainsKey(auth))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.Unauthorized,
                    Msg = "未登录".Translate()
                };

            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功登录".Translate(),
                Data = UserManager.Users[UserManager.AuthToUid[auth]]
            };
        }

        public static ApiReturnBase ApiLogout(HttpContext context)
        {
            var auth = context.Request.Headers["Authorization"].ToString();
            if (auth is null or "null" || !UserManager.AuthToUid.ContainsKey(auth))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.Unauthorized,
                    Msg = "未登录".Translate()
                };

            UserManager.Users[UserManager.AuthToUid[auth]].UserRequest.Auth = "null";
            UserManager.AuthToUid.Remove(auth);
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功登出"
            };
        }

        public static ApiReturnBase ApiVersion(HttpContext _)
        {
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = new Dictionary<string, object>
                {
                    { "version", Common.VersionFull },
                    { "vername", Common.VersionName },
                    { "vershort", Common.VersionShort },
                    { "ApiVer", Version }
                }
            };
        }

        public static ApiReturnBase ApiServers(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = ServerManager.Servers.Values.Where(t => t.BaseInfo.Owner == nowUser.UserInfo.Id)
            };
        }

        public static ApiReturnBase ApiRegister(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["username"]) ||
                string.IsNullOrEmpty(context.Request.Form["password"]) ||
                string.IsNullOrEmpty(context.Request.Form["repassword"]) ||
                string.IsNullOrEmpty(context.Request.Form["email"]))
                return ApiReturnBase.IncompleteParameters;
            if (UserManager.CheckUserNameOccupy(context.Request.Form["username"]))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.UserNameOccupied,
                    Msg = "用户名被占用".Translate()
                };
            if (context.Request.Form["password"] != context.Request.Form["repassword"])
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PasswordNotIdentical,
                    Msg = "两次密码不一致".Translate()
                };
            if (!Regex.IsMatch(context.Request.Form["password"], @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]*).{6,18}$"))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.IncorrectPasswordFormat,
                    Msg = "密码应为6-18位字母,数字,特殊符号的组合".Translate()
                };
            int ret = UserManager.AddUser(new UserInfoBase
            {
                Email = context.Request.Form["email"],
                Name = context.Request.Form["username"],
                Password = context.Request.Form["password"].ToString().GetMD5(),
                Type = UserType.Registered
            });
            if (ret == -1)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.RequestFailed,
                    Msg = "注册失败"
                };
            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "注册成功".Translate(),
                Data = UserManager.Users[ret]
            };
        }
    }
}