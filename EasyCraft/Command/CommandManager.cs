using System.Collections.Generic;
using EasyCraft.Base.Server;

namespace EasyCraft.Command
{
    public static class CommandManager
    {
        public delegate void CmdApi(string full);

        public static Dictionary<string, CmdApi> Apis = new ()
        {
            {"start", full =>
            {
                ServerManager.Servers?[int.Parse(full.Replace("start ", ""))].Start();
            }}
        };
    }
}