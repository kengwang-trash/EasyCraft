using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Serilog;

namespace EasyCraft.Base.Server
{
    public class ServerStatusInfo
    {
        /// <summary>
        /// 开服状态
        /// 0 - Stopped    1 - Starting     2 - Started     3 - Stopping
        /// </summary>
        [JsonProperty("status")] public int Status;
        [JsonIgnore] public readonly List<ServerConsoleMessage> ConsoleMessages = new();

        public void OnConsoleOutput(string content, bool error = false, DateTime time = default)
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
}