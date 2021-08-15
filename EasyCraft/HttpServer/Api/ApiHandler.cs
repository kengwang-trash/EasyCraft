using System.Collections.Generic;
using System.Threading.Tasks;
using EasyCraft.Base.User;
using EasyCraft.Utils;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EasyCraft.HttpServer.Api
{
    public class HttpApi
    {
        public UserType MinUserType;

        public delegate Task<ApiReturnBase> Func(HttpContext context);

        public Func ApiFunc;

        public HttpApi(Func func, UserType minuser = UserType.Everyone)
        {
            MinUserType = minuser;
            ApiFunc = func;
        }
    }

    public static class ApiHandler
    {
        public static Dictionary<string, HttpApi> Apis = new();

        public static void InitializeApis()
        {
            Apis = new Dictionary<string, HttpApi>
            {
                { "/login", new HttpApi(HttpApis.ApiLogin) },
                { "/login/status", new HttpApi(HttpApis.ApiLoginStatus) },
                { "/logout", new HttpApi(HttpApis.ApiLogout) },
                { "/register", new HttpApi(HttpApis.ApiRegister) },
                { "/user/password/change", new HttpApi(HttpApis.ChangePassword) },
                { "/version", new HttpApi(HttpApis.ApiVersion) },
                { "/servers", new HttpApi(HttpApis.ApiServers, UserType.Registered) },
                { "/server", new HttpApi(HttpApis.ApiServer, UserType.Registered) },
                {"/server/base/columns" , new HttpApi(HttpApis.ApiServerBaseColumns)}
            };
        }

        public static async Task<bool> HandleApi(HttpContext context)
        {
            var apistr = context.Request.Path.ToString().Substring(4);
            context.Response.ContentType = "application/json; charset=utf-8;";
            if (!Apis.ContainsKey(apistr))
            {
                await context.Response.WriteAsync(JsonConvert.SerializeObject(ApiReturnBase.ApiNotFound));
            }
            else
            {
                var api = Apis[apistr];
                if (GetCurrentUser(context).UserInfo.Type >= api.MinUserType)
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(await api.ApiFunc(context),
                        new JsonSerializerSettings() { DateFormatString = "yyyy年MM月dd日 HH:mm:ss".Translate() }));
                else
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(ApiReturnBase.PermissionDenied));
            }

            return true;
        }

        public static UserBase GetCurrentUser(HttpContext context)
        {
            var auth = context.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(auth)) return UserBase.Null;
            UserBase nowUser;
            if (!UserManager.AuthToUid.ContainsKey(auth))
                nowUser = UserBase.Null;
            else
                nowUser = UserManager.Users[UserManager.AuthToUid[auth]];
            return nowUser;
        }
    }
}