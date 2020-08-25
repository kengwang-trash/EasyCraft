using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCraft.Web.JSONCallback
{
    class ServerLog
    {
        private static int _lastid = 0;
        public static int lastid
        {
            get
            {
                return _lastid++;
            }
        }
        public long id { get; set; }
        public bool iserror { get; set; }
        public string message { get; set; }
        public DateTime time { get; set; }

        public ServerLog()
        {
            iserror = false;
            message = "";
            time = DateTime.Now;
            this.id = lastid;
        }
    }
}
