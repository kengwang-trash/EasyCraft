using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EasyCraft.Base.Core;
using EasyCraft.Base.Server;
using EasyCraft.Base.User;
using EasyCraft.Utils;
using Microsoft.AspNetCore.Http;

namespace EasyCraft.HttpServer.Api
{
    public static class HttpApis
    {
        private static int Version = 1;

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
                    // ReSharper disable once StringLiteralTypo
                    { "vername", Common.VersionName },
                    // ReSharper disable once StringLiteralTypo
                    { "vershort", Common.VersionShort },
                    { "ApiVer", Version }
                }
            };
        }

        public static ApiReturnBase ApiServers(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            var data = ServerManager.Servers.Values.ToList();
            int page = 0;
            if (context.Request.HasFormContentType)
                int.TryParse(context.Request.Form["page"], out page);
            if (nowUser.UserInfo.Type < UserType.Technician)
                data = data.Where(t => t.BaseInfo.Owner == nowUser.UserInfo.Id).ToList();
            data = data.GetRange(page * 10, Math.Min(10, data.Count - page * 10));
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = data
            };
        }

        public static ApiReturnBase ApiServer(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = ServerManager.Servers[id]
            };
        }
        
        public static ApiReturnBase ApiServerInfoStart(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = ServerManager.Servers[id].StartInfo
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

        public static ApiReturnBase ChangePassword(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["username"]) ||
                string.IsNullOrEmpty(context.Request.Form["newpassword"]) ||
                string.IsNullOrEmpty(context.Request.Form["oldpassword"]))
                return ApiReturnBase.IncompleteParameters;
            var user = UserManager.Users.Values.FirstOrDefault(t =>
                t.UserInfo.Name == context.Request.Form["username"]);
            if (user == null)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "用户名不存在".Translate()
                };
            if (context.Request.Form["oldpassword"].ToString().GetMD5() != user.UserInfo.Password)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.Unauthorized,
                    Msg = "初始密码错误".Translate()
                };

            if (!Regex.IsMatch(context.Request.Form["newpassword"], @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]*).{6,18}$"))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.IncorrectPasswordFormat,
                    Msg = "密码应为6-18位字母,数字,特殊符号的组合".Translate()
                };
            user.UserInfo.Password = context.Request.Form["newpassword"].ToString().GetMD5();
            user.UserInfo.SyncToDatabase();
            _ = ApiLogout(context);
            return new ApiReturnBase()
            {
                Status = true,
                Msg = "成功修改密码".Translate(),
                Code = 200
            };
        }

        public static async Task<ApiReturnBase> ApiServerBaseColumns(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = await ServerManager.Servers[id].GetServerConfigItems(nowUser)
            };
        }

        public static ApiReturnBase ApiCoresBranches(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["device"]))
                return ApiReturnBase.IncompleteParameters;
            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = CoreManager.Cores.Values.Select(t=>t.Info.Branch).ToHashSet()
            };
        }
    }
}