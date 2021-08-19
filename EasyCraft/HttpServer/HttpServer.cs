using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyCraft.HttpServer.Api;
using EasyCraft.Utils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;

namespace EasyCraft.HttpServer
{
    public static class HttpServer
    {
        private static IWebHost _host;

        public static void StartHttpServer(int port = 80)
        {
            if (!int.TryParse(Common.Configuration["HttpPort"], out port)) port = 80;
            try
            {
                _host = WebHost.CreateDefaultBuilder()
                    .SuppressStatusMessages(true)
                    .ConfigureLogging(t => t.SetMinimumLevel(LogLevel.None))
                    .UseKestrel()
                    .ConfigureKestrel(t => t.Listen(IPAddress.Any, port))
                    .UseStartup<HttpServerStartUp>()
                    .Build();
                _host.RunAsync();
                Log.Information("HTTP 服务器成功开启在 {0} 端口".Translate(), port);
            }
            catch (Exception e)
            {
                Log.Information("HTTP 服务器开启在 {0} 端口失败: {1}".Translate(), port, e);
            }
        }
    }

    public class HttpServerStartUp
    {
        public void Configure(IApplicationBuilder app)
        {
            app.Run(Handler);
        }

        public async Task<bool> Handler(HttpContext context)
        {
            context.Response.Headers["Server"] = $"{Common.SoftName} {Common.VersionShort}";
            context.Response.Headers["Access-Control-Allow-Methods"] = "*";
            context.Response.Headers["Access-Control-Allow-Headers"] = "*";
            context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
            context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            try
            {
                //先把版权信息加上
                if (context.Request.Path.StartsWithSegments("/api"))
                    await ApiHandler.HandleApi(context);
                else
                {
                    //await context.Response.Body.WriteAsync(
                    //    File.ReadAllBytes(Directory.GetCurrentDirectory() + "/front/" + context.Request.Path));
                    await context.Response.WriteAsync("欢迎使用 EasyCraft", Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                if (!context.Request.Headers.ContainsKey("ReqJson"))
                    await context.Response.WriteAsync("发生错误: ".Translate() + e, Encoding.UTF8);
                else
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new ApiReturnBase
                    {
                        Status = false,
                        Code = (int)ApiReturnCode.InternalError,
                        Msg = "发生内部错误".Translate(),
                        Data = e.Message
                    }));
            }

            return true;
        }
    }
}