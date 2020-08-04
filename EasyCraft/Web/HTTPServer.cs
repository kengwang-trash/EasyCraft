using EasyCraft.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace EasyCraft.Web
{
    class HTTPServer
    {
        static HttpListener listener = new HttpListener();
        public static void StartListen()
        {
            if (listener.IsListening) return;
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add("http://+:" + Settings.httpport.ToString() + "/");
                listener.Start();
                listener.BeginGetContext(RequestHandle, null);
                FastConsole.PrintSuccess(string.Format(Language.t("Started HTTP Listen on {0}"), Settings.httpport.ToString()));
            }
            catch (Exception e)
            {
                FastConsole.PrintError(string.Format(Language.t("Cannot Start HTTP Listen on {0}:{1}"), Settings.httpport.ToString(), e.Message));
                FastConsole.PrintError(Language.t("Press [Enter] to ignore this error which is NOT RECOMMENDED"));
                Console.ReadKey();
            }

        }

        public static void RequestHandle(IAsyncResult res)
        {
            listener.BeginGetContext(RequestHandle, null);
            HttpListenerContext context = listener.EndGetContext(res);
            HttpListenerResponse response = context.Response;
            HttpListenerRequest request = context.Request;
            response.ContentEncoding = Encoding.UTF8;
            response.Headers.Set("Server", "EasyCraft 1.0.0 over");
            string responseString = "";
            try
            {
                WebPanelPhraser webp = new WebPanelPhraser();
                webp.PhraseWeb(request, response);
            }
            catch (Exception e)
            {
                string logid = Functions.GetRandomString(15, true, true, true, false, "");
                FastConsole.PrintWarning("Web 500: " + e.Message);
                File.WriteAllText(string.Format("log/weberr/{0}.log", logid), "========= EasyCraft Error Log =========" + "\r\nError Message:" + e.Message + "\r\nTrance:" + e.StackTrace);
                responseString = "<h1>500 Internal Server Error</h1><hr />Log ID: " + logid + "<br />Time:" + DateTime.Now.ToString() + "<br />Server: EasyCraft<br />";
                response.StatusCode = 500;
                byte[] buff = Encoding.UTF8.GetBytes(responseString);
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
        }
    }
}
