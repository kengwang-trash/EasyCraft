using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Numerics;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using EasyCraft.Core;
using EasyCraft.Web.Classes;

namespace SharpFtpServer
{
    public class ClientConnection : IDisposable
    {
        private class DataConnectionOperation
        {
            public Func<NetworkStream, string, string> Operation { get; set; }
            public string Arguments { get; set; }
        }

        #region Copy Stream Implementations

        private static long CopyStream(Stream input, Stream output, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int count = 0;
            long total = 0;

            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, count);
                total += count;
            }

            return total;
        }

        private static long CopyStreamAscii(Stream input, Stream output, int bufferSize)
        {
            char[] buffer = new char[bufferSize];
            int count = 0;
            long total = 0;

            using (StreamReader rdr = new StreamReader(input, Encoding.GetEncoding(936)))
            {
                using (StreamWriter wtr = new StreamWriter(output, Encoding.GetEncoding(936)))
                {
                    while ((count = rdr.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        wtr.Write(buffer, 0, count);
                        total += count;
                    }
                }
            }

            return total;
        }

        private long CopyStream(Stream input, Stream output)
        {
            try
            {
                Stream limitedStream = output; // new RateLimitingStream(output, 131072, 0.5);

                if (_connectionType == TransferType.Image)
                {
                    return CopyStream(input, limitedStream, 4096);
                }
                else
                {
                    return CopyStreamAscii(input, limitedStream, 4096);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion

        #region Enums

        private enum TransferType
        {
            Ascii,
            Ebcdic,
            Image,
            Local,
        }

        private enum FormatControlType
        {
            NonPrint,
            Telnet,
            CarriageControl,
        }

        private enum DataConnectionType
        {
            Passive,
            Active,
        }

        private enum FileStructureType
        {
            File,
            Record,
            Page,
        }

        #endregion

        private bool _disposed = false;

        private TcpListener _passiveListener;

        private TcpClient _controlClient;
        private TcpClient _dataClient;

        private NetworkStream _controlStream;
        private StreamReader _controlReader;
        private StreamWriter _controlWriter;

        private TransferType _connectionType = TransferType.Ascii;
        private FormatControlType _formatControlType = FormatControlType.NonPrint;
        private DataConnectionType _dataConnectionType = DataConnectionType.Passive;
        private FileStructureType _fileStructureType = FileStructureType.File;

        private string _username;
        private int _sid;
        private string _root;
        private string _currentDirectory;
        private IPEndPoint _dataEndpoint;
        private IPEndPoint _remoteEndPoint;

        private X509Certificate _cert = null;
        private SslStream _sslStream;

        private string _clientIP;

        private User _currentUser;

        private List<string> _validCommands;

        private Encoding _writerEncoding = Encoding.ASCII;
        private Encoding _readerEncoding = Encoding.ASCII;


        public ClientConnection(TcpClient client)
        {
            _controlClient = client;

            _validCommands = new List<string>();
        }

        private string CheckUser()
        {
            if (_currentUser == null || _currentUser.islogin == false)
            {
                return "530 Not logged in";
            }

            return null;
        }

        public void HandleClient(object obj)
        {
            _remoteEndPoint = (IPEndPoint) _controlClient.Client.RemoteEndPoint;

            _clientIP = _remoteEndPoint.Address.ToString();

            _controlStream = _controlClient.GetStream();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            //设置成UTF8无BOM
            _readerEncoding = new UTF8Encoding(false);
            _writerEncoding = new UTF8Encoding(false);

            _controlReader = new StreamReader(_controlStream, _readerEncoding);
            _controlWriter = new StreamWriter(_controlStream, _writerEncoding);

            FastConsole.PrintTrash("[FTP Connect] " + _clientIP + " using " + _writerEncoding.EncodingName);

            _controlWriter.WriteLine("220 EasyCraft 易开服 1.0.0 FTP Server");
            _controlWriter.Flush();

            _validCommands.AddRange(new string[] {"AUTH", "USER", "PASS", "QUIT", "HELP", "NOOP"});

            string line;

            _dataClient = new TcpClient();

            string renameFrom = null;

            try
            {
                while ((line = _controlReader.ReadLine()) != null)
                {
                    FastConsole.PrintTrash("[FTP Recieved] " + line);
                    string response = null;
                    string cmd, arguments;
                    int spaceidx = line.IndexOf(" ");
                    if (spaceidx != -1)
                    {
                        cmd = line.Substring(0, spaceidx).ToUpperInvariant();
                        arguments = line.Substring(spaceidx + 1);
                    }
                    else
                    {
                        cmd = line.ToUpperInvariant();
                        arguments = null;
                    }


                    if (arguments != null && arguments.Trim().Length == 0)
                    {
                        arguments = null;
                    }

                    LogEntry logEntry = new LogEntry
                    {
                        Date = DateTime.Now,
                        CIP = _clientIP,
                        CSUriStem = arguments
                    };

                    if (!_validCommands.Contains(cmd))
                    {
                        response = CheckUser();
                    }

                    if (cmd != "RNTO")
                    {
                        renameFrom = null;
                    }

                    if (response == null)
                    {
                        switch (cmd)
                        {
                            case "USER":
                                response = User(arguments);
                                break;
                            case "PASS":
                                response = Password(arguments);
                                logEntry.CSUriStem = "******";
                                break;
                            case "CWD":
                                response = ChangeWorkingDirectory(arguments);
                                break;
                            case "CDUP":
                                response = ChangeWorkingDirectory("..");
                                break;
                            case "QUIT":
                                response = "221 Service closing control connection";
                                break;
                            case "REIN":
                                _currentUser = null;
                                _username = null;
                                _passiveListener = null;
                                _dataClient = null;

                                response = "220 Service ready for new user";
                                break;
                            case "PORT":
                                response = Port(arguments);
                                logEntry.CPort = _dataEndpoint.Port.ToString();
                                break;
                            case "PASV":
                                response = Passive();
                                logEntry.SPort = ((IPEndPoint) _passiveListener.LocalEndpoint).Port.ToString();
                                break;
                            case "TYPE":
                                string[] command = line.Split(' ');
                                response = Type(command[1], command.Length == 3 ? command[2] : null);
                                logEntry.CSUriStem = command[1];
                                break;
                            case "STRU":
                                response = Structure(arguments);
                                break;
                            case "MODE":
                                response = Mode(arguments);
                                break;
                            case "RNFR":
                                renameFrom = arguments;
                                response = "350 Requested file action pending further information";
                                break;
                            case "RNTO":
                                response = Rename(renameFrom, arguments);
                                break;
                            case "DELE":
                                response = Delete(arguments);
                                break;
                            case "RMD":
                                response = RemoveDir(arguments);
                                break;
                            case "MKD":
                                response = CreateDir(arguments);
                                break;
                            case "PWD":
                            case "XPWD":
                                response = PrintWorkingDirectory();
                                break;
                            case "RETR":
                                response = Retrieve(arguments);
                                logEntry.Date = DateTime.Now;
                                break;
                            case "STOR":
                                response = Store(arguments);
                                logEntry.Date = DateTime.Now;
                                break;
                            case "STOU":
                                response = StoreUnique();
                                logEntry.Date = DateTime.Now;
                                break;
                            case "APPE":
                                response = Append(arguments);
                                logEntry.Date = DateTime.Now;
                                break;
                            case "LIST":
                                response = List(arguments ?? _currentDirectory);
                                logEntry.Date = DateTime.Now;
                                break;
                            case "SYST":
                                response = "215 UNIX Type: L8";
                                break;
                            case "NOOP":
                                response = "200 OK";
                                break;
                            case "ACCT":
                                response = "200 OK";
                                break;
                            case "ALLO":
                                response = "200 OK";
                                break;
                            case "NLST":
                                response = "502 Command not implemented";
                                break;
                            case "SITE":
                                response = "502 Command not implemented";
                                break;
                            case "STAT":
                                response = "502 Command not implemented";
                                break;
                            case "HELP":
                                response = "502 Command not implemented";
                                break;
                            case "SMNT":
                                response = "502 Command not implemented";
                                break;
                            case "REST":
                                response = "502 Command not implemented";
                                break;
                            case "ABOR":
                                response = "502 Command not implemented";
                                break;

                            // Extensions defined by rfc 2228
                            case "AUTH":
                                //TODO : SSL over FTP
                                response = "502 Command not implemented";
                                //response = Auth(arguments);
                                break;

                            // Extensions defined by rfc 2389
                            case "FEAT":
                                response = FeatureList();
                                break;
                            case "OPTS":
                                response = Options(arguments);
                                break;

                            // Extensions defined by rfc 3659
                            case "MDTM":
                                response = FileModificationTime(arguments);
                                break;
                            case "SIZE":
                                response = FileSize(arguments);
                                break;

                            // Extensions defined by rfc 2428
                            case "EPRT":
                                response = EPort(arguments);
                                logEntry.CPort = _dataEndpoint.Port.ToString();
                                break;
                            case "EPSV":
                                response = EPassive();
                                logEntry.SPort = ((IPEndPoint) _passiveListener.LocalEndpoint).Port.ToString();
                                break;

                            default:
                                response = "502 Command not implemented";
                                break;
                        }
                    }

                    logEntry.CSMethod = cmd;
                    logEntry.CSUsername = _username;
                    logEntry.SCStatus = response.Substring(0, response.IndexOf(' '));

                    //_log.Info(logEntry);

                    if (_controlClient == null || !_controlClient.Connected)
                    {
                        break;
                    }
                    else
                    {
                        _controlWriter.WriteLine(response);
                        _controlWriter.Flush();
                        FastConsole.PrintTrash("[FTP Send] " + response);

                        if (response.StartsWith("221"))
                        {
                            break;
                        }

                        if (cmd == "AUTH")
                        {
                            response = "502 Command not implemented";
                            _controlWriter.WriteLine(response);
                            _controlWriter.Flush();
                            /*
                             * TODO: SSL over FTP
                            _cert = new X509Certificate("config/ftp.cer");

                            _sslStream = new SslStream(_controlStream);

                            _sslStream.AuthenticateAsServer(_cert);

                            _controlReader = new StreamReader(_sslStream);
                            _controlWriter = new StreamWriter(_sslStream);
                            */
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FastConsole.PrintError("[FTP ERROR] " + ex.Message);
                try
                {
                    if (_controlStream.CanWrite)
                    {
                        string response = "502 Command not implemented";
                        _controlWriter.WriteLine(response);
                        _controlWriter.Flush();
                    }
                }
                catch (Exception)
                {
                }
            }

            Dispose();
        }

        private bool IsPathValid(string path)
        {
            return path.StartsWith(_root);
        }

        private string NormalizeFilename(string path)
        {
            if (path == null)
            {
                path = string.Empty;
            }

            if (path == "/")
            {
                return _root;
            }
            else if (path.StartsWith("/") && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                path = new FileInfo(Path.Combine(_root, path.Substring(1))).FullName;
            }
            else
            {
                path = new FileInfo(Path.Combine(_currentDirectory, path)).FullName;
            }

            return IsPathValid(path) ? path : null;
        }

        #region FTP Commands

        private string FeatureList()
        {
            _controlWriter.WriteLine("211- Extensions supported:");
            _controlWriter.WriteLine(" MDTM");
            _controlWriter.WriteLine(" SIZE");
            return "211 End";
        }

        private string Options(string arguments)
        {
            if (arguments.ToUpper() == "UTF8 ON")
            {
                //设置成UTF8无BOM
                _readerEncoding = new UTF8Encoding(false);
                _writerEncoding = new UTF8Encoding(false);
                _controlReader = new StreamReader(_controlStream, _readerEncoding);
                _controlWriter = new StreamWriter(_controlStream, _writerEncoding);
                return "200 Changed to UTF-8";
            }
            else
            {
                //设置成UTF8无BOM
                _readerEncoding = Encoding.GetEncoding(936);
                _writerEncoding = Encoding.GetEncoding(936);
                _controlReader = new StreamReader(_controlStream, _readerEncoding);
                _controlWriter = new StreamWriter(_controlStream, _writerEncoding);
                return "200 Changed to ASCII";
            }
        }

        private string Auth(string authMode)
        {
            if (authMode.ToUpper() == "TLS")
            {
                return "234 Enabling TLS Connection";
            }
            else
            {
                return "504 Unrecognized AUTH mode";
            }
        }

        private string User(string username)
        {
            try
            {
                int dotpos = username.LastIndexOf('.');
                if (dotpos != -1)
                {
                    string tmpusername = username.Substring(0, dotpos);
                    int tmpserver = int.Parse(username.Substring(dotpos + 1));
                    bool res = CheckUserName(tmpusername, tmpserver);
                    if (res)
                    {
                        _username = tmpusername;
                        _sid = tmpserver;
                        return "331 Username ok need password";
                    }
                    else
                    {
                        return "530 Username Error, Retype";
                    }
                }
                else
                {
                    return "530 Username Error Retype";
                }
            }
            catch (Exception)
            {
                return "530 Username Error Retype";
            }
        }

        private static bool CheckUserName(string username, int sid)
        {
            int uid = EasyCraft.Web.Classes.User.GetUid(username);
            if (uid != -1)
            {
                if (ServerManager.servers.ContainsKey(sid) && ServerManager.servers[sid].owner == uid)
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

        private string Password(string password)
        {
            _currentUser = new User(_username, password);
            if (_currentUser.islogin)
            {
                if (_currentUser.CheckUserAbility((int) Permisson.UseAllFTP) ||
                    (ServerManager.servers[_sid].owner == _currentUser.uid &&
                     _currentUser.CheckUserAbility((int) Permisson.UseFTP)))
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        _root = (Environment.CurrentDirectory + "\\data\\server\\server" + _sid.ToString() + "\\");
                    }
                    else
                    {
                        _root = (Environment.CurrentDirectory + "/data/server/server" + _sid.ToString() + "/");
                    }

                    _currentDirectory = _root;
                    return "230 User logged in";
                }
                else
                {
                    return "530 Not logged in";
                }
            }
            else
            {
                _currentUser = null;
                return "530 Not logged in";
            }
        }

        private string ChangeWorkingDirectory(string pathname)
        {
            if (pathname == "/")
            {
                _currentDirectory = _root;
            }
            else
            {
                string newDir;

                if (pathname.StartsWith("/"))
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        pathname = pathname.Substring(1).Replace('/', '\\');
                    }
                    else
                    {
                        pathname = pathname.Substring(1).Replace('\\', '/');
                    }

                    newDir = Path.Combine(_root, pathname);
                }
                else
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        pathname = pathname.Replace('/', '\\');
                    }
                    else
                    {
                        pathname = pathname.Replace('\\', '/');
                    }

                    newDir = Path.Combine(_currentDirectory, pathname);
                }

                if (Directory.Exists(newDir))
                {
                    _currentDirectory = new DirectoryInfo(newDir).FullName;

                    if (!IsPathValid(_currentDirectory))
                    {
                        _currentDirectory = _root;
                    }
                }
                else
                {
                    _currentDirectory = _root;
                }
            }

            return "250 Changed to new directory";
        }

        private string Port(string hostPort)
        {
            _dataConnectionType = DataConnectionType.Active;

            string[] ipAndPort = hostPort.Split(',');

            byte[] ipAddress = new byte[4];
            byte[] port = new byte[2];

            for (int i = 0; i < 4; i++)
            {
                ipAddress[i] = Convert.ToByte(ipAndPort[i]);
            }

            for (int i = 4; i < 6; i++)
            {
                port[i - 4] = Convert.ToByte(ipAndPort[i]);
            }

            if (BitConverter.IsLittleEndian)
                Array.Reverse(port);

            _dataEndpoint = new IPEndPoint(new IPAddress(ipAddress), BitConverter.ToInt16(port, 0));

            return "200 Data Connection Established";
        }

        private string EPort(string hostPort)
        {
            _dataConnectionType = DataConnectionType.Active;

            char delimiter = hostPort[0];

            string[] rawSplit = hostPort.Split(new char[] {delimiter}, StringSplitOptions.RemoveEmptyEntries);

            char ipType = rawSplit[0][0];

            string ipAddress = rawSplit[1];
            string port = rawSplit[2];

            _dataEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), int.Parse(port));

            return "200 Data Connection Established";
        }

        private string Passive()
        {
            _dataConnectionType = DataConnectionType.Passive;

            //IPAddress ipAddress = ((IPEndPoint)_controlClient.Client.LocalEndPoint).Address;
            //IPHostEntry hostInfo = Dns.GetHostEntry("192.168.0.102");
            //IPAddress ipAddress = hostInfo.AddressList[0];
            IPAddress ipAddress = IPAddress.Parse(Settings.remoteip); //TODO
            if (_passiveListener == null)
            {
                _passiveListener = new TcpListener(IPAddress.Any, 0);
                _passiveListener.Start();
            }

            IPEndPoint passiveListenerEndpoint = (IPEndPoint) _passiveListener.LocalEndpoint;

            byte[] address = ipAddress.GetAddressBytes();
            short port = (short) passiveListenerEndpoint.Port;

            byte[] portArray = BitConverter.GetBytes(port);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(portArray);

            return string.Format("227 Entering Passive Mode ({0},{1},{2},{3},{4},{5})", address[0], address[1],
                address[2], address[3], portArray[0], portArray[1]);
        }

        private string EPassive()
        {
            _dataConnectionType = DataConnectionType.Passive;

            IPAddress localIp = ((IPEndPoint) _controlClient.Client.LocalEndPoint).Address;

            _passiveListener = new TcpListener(localIp, 0);
            _passiveListener.Start();

            IPEndPoint passiveListenerEndpoint = (IPEndPoint) _passiveListener.LocalEndpoint;

            return string.Format("229 Entering Extended Passive Mode (|||{0}|)", passiveListenerEndpoint.Port);
        }

        private string Type(string typeCode, string formatControl)
        {
            switch (typeCode.ToUpperInvariant())
            {
                case "A":
                    _connectionType = TransferType.Ascii;
                    /*
                    _readerEncoding = Encoding.GetEncoding(936);
                    _writerEncoding = Encoding.GetEncoding(936);
                    _controlReader = new StreamReader(_controlStream, _readerEncoding);
                    _controlWriter = new StreamWriter(_controlStream, _writerEncoding);
                    */
                    break;
                case "I":
                    _connectionType = TransferType.Image;
                    break;
                default:
                    return "504 Parameter Error";
            }

            if (!string.IsNullOrWhiteSpace(formatControl))
            {
                switch (formatControl.ToUpperInvariant())
                {
                    case "N":
                        _formatControlType = FormatControlType.NonPrint;
                        break;
                    default:
                        return "504 Parameter Error";
                }
            }

            return string.Format("200 Type is {0}", _connectionType);
        }

        private string Delete(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                if (File.Exists(pathname))
                {
                    File.Delete(pathname);
                }
                else
                {
                    return "550 File Not Found";
                }

                return "250 Requested file action okay, completed";
            }

            return "550 File Not Found";
        }

        private string RemoveDir(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                if (Directory.Exists(pathname))
                {
                    Directory.Delete(pathname);
                }
                else
                {
                    return "550 Directory Not Found";
                }

                return "250 Requested file action okay, completed";
            }

            return "550 Directory Not Found";
        }

        private string CreateDir(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                if (!Directory.Exists(pathname))
                {
                    Directory.CreateDirectory(pathname);
                }
                else
                {
                    return "550 Directory already exists";
                }

                return "250 Requested file action okay, completed";
            }

            return "550 Directory Not Found";
        }

        private string FileModificationTime(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                if (File.Exists(pathname))
                {
                    return string.Format("213 {0}", File.GetLastWriteTime(pathname).ToString("yyyyMMddHHmmss.fff"));
                }
            }

            return "550 File Not Found";
        }

        private string FileSize(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                if (File.Exists(pathname))
                {
                    long length = 0;

                    using (FileStream fs = File.Open(pathname, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        length = fs.Length;
                    }

                    return string.Format("213 {0}", length);
                }
            }

            return "550 File Not Found";
        }

        private string Retrieve(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                if (File.Exists(pathname))
                {
                    var state = new DataConnectionOperation {Arguments = pathname, Operation = RetrieveOperation};

                    SetupDataConnectionOperation(state);

                    return string.Format("150 Opening {0} mode data transfer for RETR", _dataConnectionType);
                }
            }

            return "550 File Not Found";
        }

        private string Store(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                var state = new DataConnectionOperation {Arguments = pathname, Operation = StoreOperation};

                SetupDataConnectionOperation(state);

                return string.Format("150 Opening {0} mode data transfer for STOR", _dataConnectionType);
            }

            return "450 Requested file action not taken";
        }

        private string Append(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                var state = new DataConnectionOperation {Arguments = pathname, Operation = AppendOperation};

                SetupDataConnectionOperation(state);

                return string.Format("150 Opening {0} mode data transfer for APPE", _dataConnectionType);
            }

            return "450 Requested file action not taken";
        }

        private string StoreUnique()
        {
            string pathname = NormalizeFilename(new Guid().ToString());

            var state = new DataConnectionOperation {Arguments = pathname, Operation = StoreOperation};

            SetupDataConnectionOperation(state);

            return string.Format("150 Opening {0} mode data transfer for STOU", _dataConnectionType);
        }

        private string PrintWorkingDirectory()
        {
            string current = _currentDirectory.Replace(_root, string.Empty).Replace('\\', '/');
            if (current.Length == 0)
            {
                current = "/";
            }

            return string.Format("257 \"{0}\" is current directory.", current);
            ;
        }

        private string List(string pathname)
        {
            if (pathname.StartsWith("-"))
            {
                //垃圾ES还tm带参数 要不是有人说想要ES来管理我直接tm就给你抛500
                string[] cmds = pathname.Split(' ');
                if (cmds.Length == 1 || cmds[1] == ".")
                {
                    //当前目录
                    pathname = "./";
                }
                else
                {
                    pathname = "/";
                }
            }

            pathname = NormalizeFilename(pathname);

            if (pathname != null)
            {
                var state = new DataConnectionOperation {Arguments = pathname, Operation = ListOperation};

                SetupDataConnectionOperation(state);

                return string.Format("150 Opening {0} mode data transfer for LIST", _dataConnectionType);
            }

            return "450 Requested file action not taken";
        }

        private string Structure(string structure)
        {
            switch (structure.ToUpper())
            {
                case "F":
                    _fileStructureType = FileStructureType.File;
                    break;
                case "R":
                case "P":
                    return string.Format("504 STRU not implemented for \"{0}\"", structure);
                default:
                    return string.Format("501 Parameter {0} not recognized", structure);
            }

            return "200 Command OK";
        }

        private string Mode(string mode)
        {
            if (mode.ToUpperInvariant() == "S")
            {
                return "200 OK";
            }
            else
            {
                return "504 Command not implemented for that parameter";
            }
        }

        private string Rename(string renameFrom, string renameTo)
        {
            if (string.IsNullOrWhiteSpace(renameFrom) || string.IsNullOrWhiteSpace(renameTo))
            {
                return "450 Requested file action not taken";
            }

            renameFrom = NormalizeFilename(renameFrom);
            renameTo = NormalizeFilename(renameTo);

            if (renameFrom != null && renameTo != null)
            {
                if (File.Exists(renameFrom))
                {
                    File.Move(renameFrom, renameTo);
                }
                else if (Directory.Exists(renameFrom))
                {
                    Directory.Move(renameFrom, renameTo);
                }
                else
                {
                    return "450 Requested file action not taken";
                }

                return "250 Requested file action okay, completed";
            }

            return "450 Requested file action not taken";
        }

        #endregion

        #region DataConnection Operations

        private void HandleAsyncResult(IAsyncResult result)
        {
            if (_dataConnectionType == DataConnectionType.Active)
            {
                _dataClient.EndConnect(result);
            }
            else
            {
                _dataClient = _passiveListener.EndAcceptTcpClient(result);
            }
        }

        private void SetupDataConnectionOperation(DataConnectionOperation state)
        {
            if (_dataConnectionType == DataConnectionType.Active)
            {
                _dataClient = new TcpClient(_dataEndpoint.AddressFamily);
                _dataClient.BeginConnect(_dataEndpoint.Address, _dataEndpoint.Port, DoDataConnectionOperation, state);
            }
            else
            {
                FastConsole.PrintTrash("[FTP Passive Start] Begin Accept Passive Client at " +
                                       ((IPEndPoint) _passiveListener.LocalEndpoint).Address + ":" +
                                       ((IPEndPoint) _passiveListener.LocalEndpoint).Port);
                _passiveListener.BeginAcceptTcpClient(DoDataConnectionOperation, state);
            }
        }

        private void DoDataConnectionOperation(IAsyncResult result)
        {
            HandleAsyncResult(result);

            FastConsole.PrintTrash("[FTP Passive Connect] " + ((IPEndPoint) _dataClient.Client.RemoteEndPoint).Address);


            DataConnectionOperation op = result.AsyncState as DataConnectionOperation;

            string response;

            using (NetworkStream dataStream = _dataClient.GetStream())
            {
                response = op.Operation(dataStream, op.Arguments);
            }

            _dataClient.Close();
            _dataClient = null;
            if (_controlWriter.BaseStream.CanWrite)
            {
                _controlWriter.WriteLine(response);
                _controlWriter.Flush();
                FastConsole.PrintTrash("[FTP Passive Send] " + response);
            }
        }

        private string RetrieveOperation(NetworkStream dataStream, string pathname)
        {
            long bytes = 0;

            using (FileStream fs = new FileStream(pathname, FileMode.Open, FileAccess.Read))
            {
                bytes = CopyStream(fs, dataStream);
            }

            return "226 Closing data connection, file transfer successful";
        }

        private string StoreOperation(NetworkStream dataStream, string pathname)
        {
            long bytes = 0;

            using (FileStream fs = new FileStream(pathname, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                4096, FileOptions.SequentialScan))
            {
                bytes = CopyStream(dataStream, fs);
            }

            LogEntry logEntry = new LogEntry
            {
                Date = DateTime.Now,
                CIP = _clientIP,
                CSMethod = "STOR",
                CSUsername = _username,
                SCStatus = "226",
                CSBytes = bytes.ToString()
            };

            //_log.Info(logEntry);

            return "226 Closing data connection, file transfer successful";
        }

        private string AppendOperation(NetworkStream dataStream, string pathname)
        {
            long bytes = 0;

            using (FileStream fs = new FileStream(pathname, FileMode.Append, FileAccess.Write, FileShare.None, 4096,
                FileOptions.SequentialScan))
            {
                bytes = CopyStream(dataStream, fs);
            }

            LogEntry logEntry = new LogEntry
            {
                Date = DateTime.Now,
                CIP = _clientIP,
                CSMethod = "APPE",
                CSUsername = _username,
                SCStatus = "226",
                CSBytes = bytes.ToString()
            };

            //_log.Info(logEntry);

            return "226 Closing data connection, file transfer successful";
        }

        private string ListOperation(NetworkStream dataStream, string pathname)
        {
            StreamWriter dataWriter = new StreamWriter(dataStream, _controlWriter.Encoding);

            IEnumerable<string> directories = Directory.EnumerateDirectories(pathname);

            foreach (string dir in directories)
            {
                DirectoryInfo d = new DirectoryInfo(dir);
                string owner = "unknown";
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    owner = ((NTAccount) d.GetAccessControl().GetOwner(typeof(NTAccount))).Value.Split('\\')[1];
                string date = d.LastWriteTime < DateTime.Now - TimeSpan.FromDays(180)
                    ? d.LastWriteTime.ToString("MMM dd  yyyy", CultureInfo.CreateSpecificCulture("en-US"))
                    : d.LastWriteTime.ToString("MMM dd HH:mm", CultureInfo.CreateSpecificCulture("en-US"));
                string line;
                if (_controlWriter.Encoding.CodePage == 936)
                {
                    //本来就是GBK,不用转换
                    line = string.Format("drwxr-xr-x    2 {3}     {3}     {0,8}  {1} {2}", "4096", date, d.Name, owner);
                }
                else
                {
                    string filenameconv = _writerEncoding.GetString(Encoding.Convert(Encoding.GetEncoding(936),
                        _writerEncoding, Encoding.GetEncoding(936).GetBytes(d.Name)));
                    line = string.Format("drwxr-xr-x    2 {3}     {3}     {0,8}  {1} {2}", "4096", date, filenameconv,
                        owner);
                }

                FastConsole.PrintTrash("[FTP Passive Send] " + line);
                dataWriter.WriteLine(line);
                dataWriter.Flush();
            }

            IEnumerable<string> files = Directory.EnumerateFiles(pathname);

            foreach (string file in files)
            {
                FileInfo f = new FileInfo(file);
                string owner = "unknown";
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    owner = ((NTAccount) f.GetAccessControl().GetOwner(typeof(NTAccount))).Value.Split('\\')[1];
                string date = f.LastWriteTime < DateTime.Now - TimeSpan.FromDays(180)
                    ? f.LastWriteTime.ToString("MMM dd  yyyy", CultureInfo.CreateSpecificCulture("en-US"))
                    : f.LastWriteTime.ToString("MMM dd HH:mm", CultureInfo.CreateSpecificCulture("en-US"));
                string line;
                if (_controlWriter.Encoding.CodePage == 936)
                {
                    line = string.Format("-rw-r--r--    2 {3}     {3}     {0,8}  {1} {2}", f.Length, date, f.Name,
                        owner);
                }
                else
                {
                    string filenameconv = _writerEncoding.GetString(Encoding.Convert(Encoding.GetEncoding(936),
                        _writerEncoding, Encoding.GetEncoding(936).GetBytes(f.Name)));
                    line = string.Format("-rw-r--r--    2 {3}     {3}     {0,8}  {1} {2}", f.Length, date, filenameconv,
                        owner);
                }

                FastConsole.PrintTrash("[FTP Passive Send] " + line);

                dataWriter.WriteLine(line);
                dataWriter.Flush();
            }

            LogEntry logEntry = new LogEntry
            {
                Date = DateTime.Now,
                CIP = _clientIP,
                CSMethod = "LIST",
                CSUsername = _username,
                SCStatus = "226"
            };

            //_log.Info(logEntry);

            return "226 Transfer complete";
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_controlClient != null)
                    {
                        _controlClient.Close();
                    }

                    if (_dataClient != null)
                    {
                        _dataClient.Close();
                    }

                    if (_controlStream != null)
                    {
                        _controlStream.Close();
                    }

                    if (_controlReader != null)
                    {
                        _controlReader.Close();
                    }

                    if (_controlWriter != null)
                    {
                        _controlWriter.Close();
                    }
                }
            }

            _disposed = true;
        }

        #endregion
    }
}