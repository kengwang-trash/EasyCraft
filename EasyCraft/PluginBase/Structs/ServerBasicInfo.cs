using System;

namespace EasyCraft.PluginBase.Structs
{
    public struct ServerBasicInfo
    {
        public int Id;
        public string Name;
        public string Core;
        public int Owner;
        public bool Running;
        public int Maxplayer;
        public int Port;
        public DateTime Expiretime;
    }
}