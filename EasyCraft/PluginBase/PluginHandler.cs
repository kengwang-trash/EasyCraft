using EasyCraft.Core;
using System;
using System.Runtime.InteropServices;

namespace EasyCraft.PluginBase
{
    public class PluginHandler
    {
        public static dynamic Handle(dynamic input)
        {
            string pluginid = input.pluginid;
            if (PluginBase.plugins.ContainsKey(pluginid) && PluginBase.plugins[pluginid].key == input.key && PluginBase.plugins[pluginid].enabled)
            {
                return Process(input.pluginid, input.type, input.data);
            }
            else
            {
                return null;
            }
            return null;
        }

        private static dynamic Process(string pluginid, string method, string data)
        {
            switch (method)
            {
                case "FastConsole.PrintInfo":
                    FastConsole.PrintInfo("[" + pluginid + "] " + data);
                    break;
                case "FastConsole.PrintFatal":
                    FastConsole.PrintFatal("[" + pluginid + "] " + data);
                    break;
                case "FastConsole.PrintWarning":
                    FastConsole.PrintWarning("[" + pluginid + "] " + data);
                    break;
                case "FastConsole.PrintSuccess":
                    FastConsole.PrintSuccess("[" + pluginid + "] " + data);
                    break;
                case "FastConsole.PrintTrash":
                    FastConsole.PrintTrash("[" + pluginid + "] " + data);
                    break;
                case "FastConsole.PrintError":
                    FastConsole.PrintError("[" + pluginid + "] " + data);
                    break;
            }
            return null;
        }
    }
}