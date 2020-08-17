using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCraft.Core
{
    class ServerLog
    {
        public static long lastid = DateTime.Now.Ticks;
        public long id;
        public bool iserror = false;
        public string message = "";
        public DateTime time;

        public ServerLog()
        {
            this.id = lastid++;
        }
    }
}
