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

        private int logid = 0;

        public void OnConsoleOutput(string content, bool error = false, DateTime time = default)
        {
            ConsoleMessages.Add(new ServerConsoleMessage(content, logid++, error, time));
            if (error)
                Log.Warning(content);
            else
                Log.Information(content);
        }
    }

    public class ServerConsoleMessage
    {
        [JsonIgnore] public DateTime Time;
        [JsonProperty("id")] public int Id;
        [JsonProperty("c")] public string Content;
        [JsonProperty("e")] public bool Error;

        public ServerConsoleMessage(string content, int logid, bool error = true, DateTime time = default)
        {
            Time = time == default ? DateTime.Now : time;
            Id = logid;
            Content = content;
            Error = error;
        }
    }
}