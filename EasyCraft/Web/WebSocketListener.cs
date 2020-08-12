using EasyCraft.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace EasyCraft.Web
{
    class WebSocketListener
    {
        public static int port;
        public static HttpListener listener = new HttpListener();
        public static Dictionary<int, WebSocket> sockets = new Dictionary<int, WebSocket>();

        public static void StartListen()
        {
            try
            {
                port = FindNextAvailableTCPPort(25566);
                if (!listener.IsListening)
                {
                    listener.Prefixes.Add("http://+:" + port.ToString() + "/");
                    listener.Start();
                    listener.BeginGetContext(WebSocketRequestHandlerAsync, null);
                    FastConsole.PrintSuccess(string.Format(Language.t("Passive WebSocket Server Started at {0}"), port.ToString()));
                }
            }catch (Exception e)
            {
                FastConsole.PrintError(string.Format(Language.t("WebSocket Server Listen Failed: {0}"), e.Message));
            }

        }

        private static void WebSocketRequestHandlerAsync(IAsyncResult ar)
        {
            FastConsole.PrintSuccess("New WebSocket Connected!");
            try
            {
                listener.BeginGetContext(WebSocketRequestHandlerAsync, null);
                HttpListenerContext context = listener.EndGetContext(ar);
                context.Response.Headers["Server"] = "EasyCraft 1.0.0 over";
                HttpListenerWebSocketContext c = context.AcceptWebSocketAsync(null).Result;
                while (true)
                {
                    if (c.WebSocket.State == WebSocketState.Open)
                    {
                        ArraySegment<byte> buffer = WebSocket.CreateClientBuffer(8192, 8192);
                        WebSocketReceiveResult res = c.WebSocket.ReceiveAsync(buffer, CancellationToken.None).Result;
                        string resule = Encoding.UTF8.GetString(buffer);
                    }
                    else
                    {
                        FastConsole.PrintTrash("A WebSocket Disconnected");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (e.HResult != -2146233088)
                {//客户端的HTTP请求突然掉线,我还是不管吧
                    FastConsole.PrintWarning("WebSocket ERROR: " + e.Message);
                }

            }
        }

        public static int FindNextAvailableTCPPort(int startPort)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                return startPort;
            }
            int port = startPort;
            bool isAvailable = true;
            IPGlobalProperties ipGlobalProperties =
                IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] endPoints =
                ipGlobalProperties.GetActiveTcpListeners();

            do
            {
                if (!isAvailable)
                {
                    port++;
                    isAvailable = true;
                }

                foreach (IPEndPoint endPoint in endPoints)
                {
                    if (endPoint.Port != port) continue;
                    isAvailable = false;
                    break;
                }

            } while (!isAvailable && port < IPEndPoint.MaxPort);

            if (!isAvailable)
                throw new ApplicationException("Not able to find a free TCP port.");

            return port;
        }


    }
}
