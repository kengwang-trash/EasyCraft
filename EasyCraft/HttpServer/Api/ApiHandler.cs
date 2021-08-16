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

        public delegate ApiReturnBase Func(HttpContext context);

        public delegate Task<ApiReturnBase> FuncAsync(HttpContext context);


        public Func ApiFunc;

        public FuncAsync ApiFuncAsync;

        public bool IsAsync;

        public static HttpApi Create(Func func, UserType minUserType = UserType.Everyone)
        {
            return new()
            {
                ApiFunc = func,
                MinUserType = minUserType,
                IsAsync = false
            };
        }

        public static HttpApi CreateAsync(FuncAsync func, UserType minUserType = UserType.Everyone)
        {
            return new()
            {
                ApiFuncAsync = func,
                MinUserType = minUserType,
                IsAsync = true
            };
        }
    }

    public static class ApiHandler
    {
        public static Dictionary<string, HttpApi> Apis = new();

        public static void InitializeApis()
        {
            Apis = new Dictionary<string, HttpApi>
            {
                { "/login", HttpApi.Create(HttpApis.ApiLogin) },
                { "/login/status", HttpApi.Create(HttpApis.ApiLoginStatus) },
                { "/logout", HttpApi.Create(HttpApis.ApiLogout) },
                { "/register", HttpApi.Create(HttpApis.ApiRegister) },
                { "/user/password/change", HttpApi.Create(HttpApis.ChangePassword) },
                { "/version", HttpApi.Create(HttpApis.ApiVersion) },
                { "/servers", HttpApi.Create(HttpApis.ApiServers, UserType.Registered) },
                { "/server", HttpApi.Create(HttpApis.ApiServer, UserType.Registered) },
                { "/server/base/columns", HttpApi.CreateAsync(HttpApis.ApiServerBaseColumns) },
                { "/server/info/start", HttpApi.Create(HttpApis.ApiServerInfoStart, UserType.Registered) },
                { "/cores/branches", HttpApi.Create(HttpApis.ApiCoresBranches) }
            };
        }

        public static async Task<bool> HandleApi(HttpContext context)
        {
            var apiStr = context.Request.Path.ToString().Substring(4);
            context.Response.ContentType = "application/json; charset=utf-8;";
            if (!Apis.ContainsKey(apiStr))
            {
                await context.Response.WriteAsync(JsonConvert.SerializeObject(ApiReturnBase.ApiNotFound));
            }
            else
            {
                var api = Apis[apiStr];
                if (GetCurrentUser(context).UserInfo.Type >= api.MinUserType)
                    if (api.IsAsync)
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(await api.ApiFuncAsync(context),
                            new JsonSerializerSettings() { DateFormatString = "yyyy年MM月dd日 HH:mm:ss".Translate() }));
                    else
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(api.ApiFunc(context),
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
            var nowUser = !UserManager.AuthToUid.ContainsKey(auth)
                ? UserBase.Null
                : UserManager.Users[UserManager.AuthToUid[auth]];
            return nowUser;
        }
    }
}