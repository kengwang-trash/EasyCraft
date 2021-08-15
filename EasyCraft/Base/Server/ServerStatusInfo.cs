using System;
using System.Collections.Generic;
using Serilog;

namespace EasyCraft.Base.Server
{
    public class ServerStatusInfo
    {
        public ServerStatusCode IsRunning;
        public readonly List<ServerConsoleMessage> ConsoleMessages = new();

        public void OnConsoleOutput(string content, bool error = true, DateTime time = default)
        {
            ConsoleMessages.Add(new ServerConsoleMessage(content, error, time));
            if (error)
                Log.Warning(content);
            else
                Log.Information(content);
        }
    }

    public class ServerConsoleMessage
    {
        public DateTime Time;
        public string Content;
        public bool Error;

        public ServerConsoleMessage(string content, bool error = true, DateTime time = default)
        {
            Time = time == default ? DateTime.Now : time;
            Content = content;
            Error = error;
        }
    }

    public enum ServerStatusCode
    {
        Stopped,
        Starting,
        Started,
        Stopping
    }
}