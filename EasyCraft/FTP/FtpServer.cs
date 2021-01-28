using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EasyCraft.Core;

namespace SharpFtpServer
{
    public class FtpServer : IDisposable
    {
        public static FtpServer server = null;
        private List<ClientConnection> _activeConnections;
        private bool _disposed;

        private TcpListener _listener;
        private bool _listening;

        private readonly IPEndPoint _localEndPoint;

        public FtpServer()
            : this(IPAddress.Any, Settings.ftpport)
        {
        }

        public FtpServer(IPAddress ipAddress, int port)
        {
            _localEndPoint = new IPEndPoint(ipAddress, port);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Start()
        {
            try
            {
                _listener = new TcpListener(_localEndPoint);

                _listening = true;
                _listener.Start();

                _activeConnections = new List<ClientConnection>();

                _listener.BeginAcceptTcpClient(HandleAcceptTcpClient, _listener);
                FastConsole.PrintSuccess(string.Format(Language.t("成功在 {0} 端口开启 FTP 服务器"),
                    _localEndPoint.Port.ToString()));
            }
            catch (Exception e)
            {
                FastConsole.PrintFatal(string.Format(Language.t("未能在 {0} 端口开启服务器: {1}"), _localEndPoint.Port.ToString(),
                    e.Message));
            }
        }

        public void Stop()
        {
            FastConsole.PrintInfo("关闭 FTP 服务器");

            _listening = false;
            _listener.Stop();

            _listener = null;
        }

        private void HandleAcceptTcpClient(IAsyncResult result)
        {
            if (_listening)
            {
                _listener.BeginAcceptTcpClient(HandleAcceptTcpClient, _listener);

                var client = _listener.EndAcceptTcpClient(result);

                var connection = new ClientConnection(client);

                _activeConnections.Add(connection);

                ThreadPool.QueueUserWorkItem(connection.HandleClient, client);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                {
                    Stop();

                    foreach (var conn in _activeConnections) conn.Dispose();
                }

            _disposed = true;
        }
    }
}