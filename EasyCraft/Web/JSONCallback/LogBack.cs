using EasyCraft.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCraft.Web.JSONCallback
{
    class LogBack : Callback
    {
        public LogBackData data { get; set; }
    }

    class LogBackData
    {
        public long lastlogid { get; set; }
        public List<ServerLog> logs { get; set; }
    }
}
