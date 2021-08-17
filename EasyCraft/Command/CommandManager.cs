using System.Collections.Generic;
using EasyCraft.Base.Server;

namespace EasyCraft.Command
{
    public static class CommandManager
    {
        public delegate void CmdApi(string full);

        public static Dictionary<string, CmdApi> Apis = new()
        {
            { "start", full => { ServerManager.Servers?[int.Parse(full.Replace("start ", ""))].Start(); } },
            { "stop", full => { ServerManager.Servers?[int.Parse(full.Replace("stop ", ""))].Stop(); } },
            {
                "hapi", full =>
                {
                    if (full == "hapi reload") HttpServer.Api.ApiHandler.InitializeApis();
                }
            },
            {
                "fuck", _ =>
                {
                    for (int i = 0; i < 500; i++)
                    {
                        ServerManager.Servers[1].StatusInfo.OnConsoleOutput(i.ToString(), i % 5 == 0);
                    }
                }
            }
        };
    }
}