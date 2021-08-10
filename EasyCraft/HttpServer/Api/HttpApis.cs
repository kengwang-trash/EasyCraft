using System.Linq;
using EasyCraft.Base.User;
using EasyCraft.Utils;
using Microsoft.AspNetCore.Http;

namespace EasyCraft.HttpServer.Api
{
    public class HttpApis
    {
        public static ApiReturnBase ApiLogin(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (!context.Request.Form.ContainsKey("username") || !context.Request.Form.ContainsKey("password"))
                return ApiReturnBase.IncompleteParameters;
            var userIdx =
                UserManager.Users.Values.ToList().FindIndex((t => t.UserInfo.Name == context.Request.Form["username"]));
            if (userIdx == -1)
                return new ApiReturnBase()
                {
                    Status = false,
                    Code = (int) ApiErrorCode.UserNotFound,
                    Msg = "用户不存在".Translate()
                };
            var user = UserManager.Users.Values.ToArray()[userIdx];
            if (user.UserInfo.Password != context.Request.Form["password"].ToString().GetMD5())
                return new ApiReturnBase()
                {
                    Status = false,
                    Code = (int) ApiErrorCode.Unauthorized,
                    Msg = "登录失败, 密码错误"
                };
            user.UserRequest.IsLogin = true;
            user.UserRequest.Auth = Utils.Utils.CreateRandomString();
            context.Session.SetString("auth", user.UserRequest.Auth);
            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "成功登录".Translate(),
                Data = user.UserInfo
            };
        }
    }
}