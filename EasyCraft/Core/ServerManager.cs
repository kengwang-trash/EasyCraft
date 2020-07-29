using EasyCraft.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace EasyCraft.Core
{
    class ServerManager
    {
        public static List<Server> servers = new List<Server>();
        string rawjson = "";
        ServerManagerStruct sms;

        void LoadServersFromConfig()
        {
        }
    }
}
