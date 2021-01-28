using System;

namespace EasyCraft.Web.JSONCallback
{
    internal class ServerLog
    {
        private static int _lastid = 1;

        public ServerLog()
        {
            iserror = false;
            message = "";
            time = DateTime.Now;
            id = lastid;
        }

        public static int lastid => _lastid++;

        public long id { get; set; }
        public bool iserror { get; set; }
        public string message { get; set; }
        public DateTime time { get; set; }
    }
}