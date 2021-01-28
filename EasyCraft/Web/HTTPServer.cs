﻿using System;
using System.IO;
using System.Net;
using System.Text;
using EasyCraft.Core;

namespace EasyCraft.Web
{
    internal class HTTPServer
    {
        private static HttpListener listener = new HttpListener();

        public static void StartListen()
        {
            if (listener.IsListening) return;
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add("http://+:" + Settings.httpport + "/");
                listener.Start();
                listener.BeginGetContext(RequestHandle, null);
                FastConsole.PrintSuccess(string.Format(Language.t("成功在 {0} 端口启动 HTTP 服务器"),
                    Settings.httpport.ToString()));
            }
            catch (Exception e)
            {
                FastConsole.PrintError(string.Format(Language.t("无法在 {0} 端口启动 HTTP 服务器: {1}"),
                    Settings.httpport.ToString(), e.Message));
                FastConsole.PrintError(Language.t("按下 [Enter] 忽略此错误"));
                Console.ReadKey();
            }
        }

        public static void StopListen()
        {
            listener.Stop();
        }

        public static void RequestHandle(IAsyncResult res)
        {
            listener.BeginGetContext(RequestHandle, null);
            var context = listener.EndGetContext(res);
            var response = context.Response;
            var request = context.Request;
            response.ContentEncoding = Encoding.UTF8;
            response.Headers.Set("Server", "EasyCraft 1.0.0 over");
            var responseString = "";
            try
            {
                var webp = new WebPanelPhraser();
                webp.PhraseWeb(request, response);
            }
            catch (Exception e)
            {
                try
                {
                    var logid = Functions.GetRandomString(15, true, true, true);
                    FastConsole.PrintWarning("Web 500: " + e.Message);
                    Write500Log(logid, e);
                    if (request.Headers["X-Requested-With"] != "XMLHttpRequest")
                    {
                        responseString = "<h1>500 Internal Server Error</h1><hr />Log ID: " + logid + "<br />Time:" +
                                         DateTime.Now + "<br />Server: EasyCraft<br />";
                        response.StatusCode = 500;
                    }
                    else
                    {
                        responseString = "{\"code\":-5000,\"message\":\"500 Internal Server Error\"}";
                    }

                    var buff = Encoding.UTF8.GetBytes(responseString);
                    response.OutputStream.Write(buff, 0, buff.Length);
                    // 必须关闭输出流
                    try
                    {
                        response.Close();
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }
                catch (Exception)
                {
                    //没救了
                }
            }
        }

        private static void Write500Log(string logid, Exception e)
        {
            FastConsole.PrintError("========= EasyCraft Error Log =========" + "\r\nError Message: " + e.Message +
                                   "\r\nTrance: \r\n" + e.StackTrace + "\r\n\r\n");
            File.AppendAllTextAsync(string.Format("data/log/weberr/{0}.log", logid),
                "========= EasyCraft Error Log =========" + "\r\nError Message: " + e.Message + "\r\nTrance: \r\n" +
                e.StackTrace + "\r\n\r\n");
            if (e.InnerException != null) Write500Log(logid, e.InnerException);
        }
    }
}