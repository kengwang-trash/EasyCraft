using System.Linq;
using EasyCraft.Base.User;
using EasyCraft.Utils;
using Microsoft.AspNetCore.Http;

namespace EasyCraft.HttpServer.Api
{
    public static class HttpApis
    {
        public static ApiReturnBase ApiLogin(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (!context.Request.Form.ContainsKey("username") || !context.Request.Form.ContainsKey("password"))
                return ApiReturnBase.IncompleteParameters;
            var user =
                UserManager.Users.Values.FirstOrDefault((t => t.UserInfo.Name == context.Request.Form["username"]));
            if (user == null)
                return new ApiReturnBase()
                {
                    Status = false,
                    Code = (int)ApiErrorCode.UserNotFound,
                    Msg = "用户不存在".Translate()
                };
            if (user.UserInfo.Password !=
                context.Request.Form["password"].ToString().GetMD5())
                return new ApiReturnBase()
                {
                    Status = false,
                    Code = (int)ApiErrorCode.Unauthorized,
                    Msg = "登录失败, 密码错误"
                };
            if (user.UserRequest.Auth != null)
                UserManager.AuthToUid.Remove(user.UserRequest.Auth);
            user.UserRequest.Auth = Utils.Utils.CreateRandomString();
            UserManager.AuthToUid[user.UserRequest.Auth] = user.UserInfo.Id;
            return new ApiReturnBase()
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
            {
                return new ApiReturnBase()
                {
                    Status = false,
                    Code = (int)ApiErrorCode.Unauthorized,
                    Msg = "未登录".Translate()
                };
            }

            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "成功登录".Translate(),
                Data = UserManager.Users[UserManager.AuthToUid[auth]]
            };
        }
    }
}