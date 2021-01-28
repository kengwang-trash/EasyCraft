namespace EasyCraft.Web.JSONCallback
{
    internal class ServerInfo : Callback
    {
        public ServerInfoData data { get; set; }
    }

    internal class ServerInfoData
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