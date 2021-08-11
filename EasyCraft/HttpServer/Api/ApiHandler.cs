using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyCraft.Base.User;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EasyCraft.HttpServer.Api
{
    public static class ApiHandler
    {
        public delegate ApiReturnBase HttpApi(HttpContext context);

        public static Dictionary<string, HttpApi> Apis = new();

        public static void InitializeApis()
        {
            Apis = new Dictionary<string, HttpApi>()
            {
                { "/login", HttpApis.ApiLogin },
                { "/login/status", HttpApis.ApiLoginStatus }
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
                await context.Response.WriteAsync(JsonConvert.SerializeObject(Apis[apistr](context)));
            }

            return true;
        }
    }
}