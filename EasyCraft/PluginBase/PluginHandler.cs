using System;
using System.Collections.Generic;
using EasyCraft.Utils;
using Serilog;

namespace EasyCraft.PluginBase
{
    public class PluginHandler
    {
        private delegate object PluginHandleApi(object obj);

        private static Dictionary<string, PluginHandleApi> Apis = new();

        public static void InitPluginHandleApi()
        {
            Apis = new()
            {
                { "FastConsole.PrintInfo", o => PluginApis.FastConsole(2, o.ToString()) },
                { "FastConsole.PrintTrash", o => PluginApis.FastConsole(0, o.ToString()) },
                { "FastConsole.PrintWarning", o => PluginApis.FastConsole(3, o.ToString()) },
                { "FastConsole.PrintError", o => PluginApis.FastConsole(4, o.ToString()) },
                { "FastConsole.PrintFatal", o => PluginApis.FastConsole(5, o.ToString()) },
            };
        }


        public static Dictionary<string, object> Handle(Dictionary<string, object> input)
        {
            try
            {
                if (!PluginController.Plugins[input["id"].ToString()].Auth.ContainsKey(input["func"].ToString()) ||
                    !PluginController.Plugins[input["id"].ToString()].Auth[input["func"].ToString()])
                {
                    return new()
                    {
                        { "status", false },
                        { "code", -401 },
                        { "message", "没有调用此 API 的权限".Translate() },
                        { "data", null }
                    };
                }

                if (input["key"].ToString() != PluginController.Plugins[input["id"].ToString()].Key)
                    return new()
                    {
                        { "status", false },
                        { "code", -400 },
                        { "message", "不正确的 Key".Translate() },
                        { "data", null }
                    };
                if (!Apis.ContainsKey(input["func"].ToString()))
                    return new()
                    {
                        { "status", false },
                        { "code", -501 },
                        { "message", "调用了未知的 API".Translate() },
                        { "data", null }
                    };
                return new()
                {
                    { "status", true },
                    { "code", 200 },
                    { "message", "成功" },
                    { "data", Apis[input["func"].ToString()].Invoke(input["data"]) }
                };
            }
            catch (Exception e)
            {
                return new()
                {
                    { "status", false },
                    { "code", -500 },
                    { "message", e.Message },
                    { "data", null }
                };
            }
        }
    }
}