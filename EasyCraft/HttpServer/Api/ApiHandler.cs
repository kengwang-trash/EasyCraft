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

    internal static class ApiHandler
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
                { "/server/base/columns", HttpApi.Create(HttpApis.ApiServerBaseColumns, UserType.Registered) },
                { "/server/plugin/columns", HttpApi.CreateAsync(HttpApis.ApiServerPluginColumns, UserType.Registered) },
                { "/server/info/start", HttpApi.Create(HttpApis.ApiServerInfoStart, UserType.Registered) },
                { "/cores/branches", HttpApi.Create(HttpApis.ApiCoresBranches, UserType.Registered) },
                { "/cores/branch/item", HttpApi.Create(HttpApis.ApiCoresBranchItems, UserType.Registered) },
                { "/starters", HttpApi.Create(HttpApis.ApiStarters, UserType.Registered) },
                { "/server/console", HttpApi.Create(HttpApis.ApiServerConsole, UserType.Registered) },
                { "/server/start", HttpApi.CreateAsync(HttpApis.ApiServerStart, UserType.Registered) },
                { "/server/stop", HttpApi.CreateAsync(HttpApis.ApiServerStop, UserType.Registered) },
                { "/server/status", HttpApi.Create(HttpApis.ApiServerStatus, UserType.Registered) },
                { "/server/base/info/update", HttpApi.Create(HttpApis.ApiServerBaseInfoUpdate, UserType.Registered) },
                { "/server/start/info/update", HttpApi.Create(HttpApis.ApiServerStartInfoUpdate, UserType.Registered) },
                { "/server/console/clean", HttpApi.Create(HttpApis.ApiServerConsoleClean, UserType.Registered) },
                { "/server/console/input", HttpApi.Create(HttpApis.ApiServerConsoleInput, UserType.Registered) },
                { "/server/configs/list", HttpApi.Create(HttpApis.ApiServerConfigsList, UserType.Registered) },
                { "/server/config/content", HttpApi.Create(HttpApis.ApiServerConfigContent, UserType.Registered) },
                {
                    "/server/config/content/update",
                    HttpApi.Create(HttpApis.ApiServerConfigContentUpdate, UserType.Registered)
                },
                { "/create/server", HttpApi.Create(HttpApis.ApiCreateServer, UserType.Registered) }
            };
        }

        public static async Task HandleApi(HttpContext context)
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
                var nowUser = GetCurrentUser(context);
                if (nowUser.UserInfo.Type >= api.MinUserType)
                    if (api.IsAsync)
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(await api.ApiFuncAsync(context),
                            new JsonSerializerSettings() { DateFormatString = "yyyy年MM月dd日 HH:mm:ss".Translate() }));
                    else
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(api.ApiFunc(context),
                            new JsonSerializerSettings() { DateFormatString = "yyyy年MM月dd日 HH:mm:ss".Translate() }));
                else if (nowUser.UserRequest.IsLogin)
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(ApiReturnBase.PermissionDenied));
                else
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(ApiReturnBase.NotLogin));
            }
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