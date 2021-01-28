using System.Collections.Generic;

namespace EasyCraft.Web.JSONCallback
{
    internal class LogBack : Callback
    {
        public LogBackData data { get; set; }
    }

    internal class LogBackData
    {
        public bool starting { get; set; }
        public long lastlogid { get; set; }
        public List<ServerLog> logs { get; set; }
    }
}