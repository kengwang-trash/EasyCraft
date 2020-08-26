using EasyCraft.Web.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;

namespace EasyCraft.Core
{
    class FtpServer
    {
        private static TcpListener listener;
        public static int port = 21;
        private static List<TcpClient> clients = new List<TcpClient>();

        public static void StartListen()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                listener.BeginAcceptTcpClient(AcceptClient, listener);
                FastConsole.PrintSuccess(string.Format(Language.t("Successfully to Start FTP Server on {0}"), port));
            }
            catch (Exception e)
            {
                FastConsole.PrintFatal(string.Format(Language.t("Failed to Start FTP Server {0}: {1}"), port, e.Message));
            }

        }

        public static void CleanClient()
        {
            foreach (TcpClient client in clients)
            {
                if (!client.Connected)
                {
                    clients.Remove(client);
                    client.Close();
                    client.Dispose();
                }
            }
        }

        private static void AcceptClient(IAsyncResult result)
        {
            TcpClient client = listener.EndAcceptTcpClient(result);
            clients.Add(client);
            listener.BeginAcceptTcpClient(AcceptClient, listener);
            NetworkStream stream = client.GetStream();
            StreamWriter writer = new StreamWriter(stream);
            StreamReader reader = new StreamReader(stream);
            if (true)
            {
                writer.WriteLine("220 EasyCraft FTP Server");
                writer.Flush();

                string line = null;
                string username = "";
                int server = 0;
                int contype = 0; // 0  - 默认   1 - Passive
                bool logined = false;
                TcpListener _passiveListener = null;
                string realdir = "";
                string serverroot = "";
                string ftpdir = "";
                while (client.Connected && !string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    FastConsole.PrintTrash("FTP Recived: " + line);
                    string response = null;
                    int spaceidx = line.IndexOf(" ");
                    string cmd = "", arguments = "";
                    if (spaceidx != -1)
                    {
                        cmd = line.Substring(0, spaceidx);
                        arguments = line.Substring(spaceidx + 1);
                    }
                    else
                    {
                        cmd = line;
                        arguments = "";
                    }

                    if (string.IsNullOrWhiteSpace(arguments))
                        arguments = null;

                    if (response == null)
                    {
                        switch (cmd)
                        {
                            case "USER":
                                bool res = false;
                                try
                                {
                                    string[] arr = arguments.Split('.');
                                    string tmpusername = arr[0];
                                    int tmpserver = int.Parse(arr[1]);
                                    res = checkUserName(tmpusername, tmpserver);
                                    if (res)
                                    {
                                        username = tmpusername;
                                        server = tmpserver;
                                        response = "331 Username ok, need password";
                                    }
                                    else
                                    {
                                        response = "530 Username Error, Retype";
                                    }
                                }
                                catch (Exception)
                                {
                                    response = "530 Username Error, Retype";
                                }
                                break;
                            case "PASS":
                                logined = checkPassword(username, arguments, server);
                                if (logined)
                                {
                                    realdir = (Environment.CurrentDirectory + "/server/server" + server.ToString() + "/").Replace("\\", "/");
                                    serverroot = (Environment.CurrentDirectory + "/server/server" + server.ToString() + "/").Replace("\\", "/");
                                    ftpdir = "/";
                                }
                                response = logined ? "230 User logged in" : "530 Not logged in";
                                break;
                            case "CWD":
                                if (logined)
                                {
                                    string tempdir = Path.GetFullPath(realdir + arguments).Replace("\\", "/");
                                    if (tempdir.Contains(serverroot))
                                    {
                                        if (Directory.Exists(tempdir))
                                        {
                                            realdir = tempdir;
                                            ftpdir = "/" + tempdir.Replace(serverroot, "");
                                            response = "250 Changed to \"" + ftpdir + "\"";
                                        }
                                        else
                                        {
                                            response = "550 No such directory.";
                                        }
                                    }
                                    else
                                    {
                                        response = "257 \"" + ftpdir + "\" is current directory.";
                                    }
                                }
                                else
                                {
                                    response = "530 Not logged in";
                                }
                                break;
                            case "CDUP":
                                if (logined)
                                {
                                    arguments = "../";
                                    string tempdir = Path.GetFullPath(realdir + arguments).Replace("\\", "/");
                                    if (tempdir.Contains(serverroot))
                                    {
                                        if (Directory.Exists(tempdir))
                                        {
                                            realdir = tempdir;
                                            ftpdir = "/" + tempdir.Replace(serverroot, "");
                                            response = "250 Changed to \"" + ftpdir + "\"";
                                        }
                                        else
                                        {
                                            response = "550 No such directory.";
                                        }
                                    }
                                    else
                                    {
                                        response = "257 \"" + ftpdir + "\" is current directory.";
                                    }
                                }
                                else
                                {
                                    response = "530 Not logged in";
                                }
                                break;
                            case "QUIT":
                                response = "221 Service closing control connection";
                                break;
                            case "XPWD":
                            case "PWD":
                                if (logined)
                                {
                                    response = "257 \"" + ftpdir + "\" is current directory.";
                                }
                                else
                                {
                                    response = "530 Not logged in";
                                }
                                break;
                            case "TYPE":
                                if (arguments == "A")
                                {
                                    writer = new StreamWriter(stream, Encoding.ASCII);
                                    reader = new StreamReader(stream, Encoding.ASCII);
                                    response = "200 Type now is: ASCII.";
                                }
                                else if (arguments == "B")
                                {
                                    writer = new StreamWriter(stream);
                                    reader = new StreamReader(stream);
                                    response = "200 Type now is: Binary.";
                                }
                                else
                                {
                                    writer = new StreamWriter(stream);
                                    reader = new StreamReader(stream);
                                    response = "200 Not Support, Now is Binary";
                                }
                                break;
                            case "PORT":
                                response = "200 I don't know what is PORT MODE.";
                                break;
                            case "PASV":
                                contype = 1;
                                IPAddress localIp = ((IPEndPoint)client.Client.LocalEndPoint).Address;
                                _passiveListener = new TcpListener(localIp, 0);
                                _passiveListener.Start();
                                IPEndPoint passiveListenerEndpoint = (IPEndPoint)_passiveListener.LocalEndpoint;

                                byte[] address = passiveListenerEndpoint.Address.GetAddressBytes();
                                short port = (short)passiveListenerEndpoint.Port;

                                byte[] portArray = BitConverter.GetBytes(port);

                                if (BitConverter.IsLittleEndian)
                                    Array.Reverse(portArray);
                                response = string.Format("227 Entering Passive Mode ({0},{1},{2},{3},{4},{5})", address[0], address[1], address[2], address[3], portArray[0], portArray[1]);
                                break;
                            case "LIST":

                                break;
                            default:
                                response = "502 Command not implemented";
                                break;
                        }
                    }

                    if (client == null || !client.Connected)
                    {
                        break;
                    }
                    else
                    {
                        FastConsole.PrintTrash("FTP Send: " + response);
                        writer.WriteLine(response);
                        writer.Flush();

                        if (response.StartsWith("221"))
                        {
                            break;
                        }
                    }
                }
            }
        }

        private static void PASV()
        {
            throw new NotImplementedException();
        }

        private static bool checkUserName(string username, int sid)
        {
            if (User.Exist(username))
            {
                if (ServerManager.servers.ContainsKey(sid))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private static bool checkPassword(string username, string password, int sid)
        {
            User user = new User(username, password);
            if (user.islogin)
            {
                if (user.type >= 2 || ServerManager.servers[sid].owner == user.uid)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public static void Stop()
        {
            if (listener != null)
            {
                listener.Stop();
            }
        }
    }
}
