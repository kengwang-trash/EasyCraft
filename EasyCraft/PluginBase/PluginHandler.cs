using System.Collections.Generic;
using EasyCraft.Core;
using EasyCraft.PluginBase.Structs;

namespace EasyCraft.PluginBase
{
    public class PluginHandler
    {
        public static object Handle(dynamic input)
        {
            string pluginid = input.pluginid;
            if (PluginBase.plugins.ContainsKey(pluginid) && PluginBase.plugins[pluginid].key == input.key &&
                PluginBase.plugins[pluginid].enabled)
                return Process(input.pluginid, input.type, input.data);
            return null;
        }

        private static object Process(string pluginid, string method, Dictionary<string, string> data)
        {
            switch (method)
            {
                case "FastConsole.PrintInfo":
                    FastConsole.PrintInfo("[" + pluginid + "] " + data["message"]);
                    break;
                case "FastConsole.PrintFatal":
                    FastConsole.PrintFatal("[" + pluginid + "] " + data["message"]);
                    break;
                case "FastConsole.PrintWarning":
                    FastConsole.PrintWarning("[" + pluginid + "] " + data["message"]);
                    break;
                case "FastConsole.PrintSuccess":
                    FastConsole.PrintSuccess("[" + pluginid + "] " + data["message"]);
                    break;
                case "FastConsole.PrintTrash":
                    FastConsole.PrintTrash("[" + pluginid + "] " + data["message"]);
                    break;
                case "FastConsole.PrintError":
                    FastConsole.PrintError("[" + pluginid + "] " + data["message"]);
                    break;
                case "Server.GetBasicInfo":
                    if (PluginBase.CheckPluginAuth(pluginid, "Server.GetBasicInfo"))
                    {
                        if (ServerManager.servers.ContainsKey(int.Parse(data["sid"])))
                        {
                            var s = ServerManager.servers[int.Parse(data["sid"])];
                            return new ServerBasicInfo
                            {
                                Core = s.Core,
                                Expiretime = s.Expiretime,
                                Id = s.Id,
                                Maxplayer = s.Maxplayer,
                                Name = s.Name,
                                Owner = s.Owner,
                                Port = s.Port,
                                Running = s.Running
                            };
                        }

                        return null;
                    }

                    break;
                case "Server.SendCommand":
                    if (PluginBase.CheckPluginAuth(pluginid, "Server.SendCommand"))
                    {
                        if (ServerManager.servers.ContainsKey(int.Parse(data["sid"])))
                        {
                            var s = ServerManager.servers[int.Parse(data["sid"])];
                            if (s.process == null || s.process.HasExited) return false;
                            s.Send(data["cmd"]);
                            return true;
                        }

                        return true;
                    }

                    break;
            }

            return null;
        }
    }
}