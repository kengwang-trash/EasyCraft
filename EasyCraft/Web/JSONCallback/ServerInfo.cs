using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCraft.Web.JSONCallback
{
    class ServerInfo : Callback
    {
        public ServerInfoData data  { get; set; }
    }

    class ServerInfoData
    {
        public int id { get; set; }
        public string name { get; set; }
        public int port { get; set; }
        public string core { get; set; }
        public int maxplayer { get; set; }
        public int ram { get; set; }
        public bool running { get; set; }
        public string expiretime { get; set; }
    }
}
