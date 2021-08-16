using System;
using System.Collections.Generic;
using EasyCraft.Utils;

namespace EasyCraft.PluginBase
{
    // 请不要将此类设置为 internal - 插件们还要用
    public static class PluginHandler
    {
        private static Dictionary<string, PluginHandleApi> _apis = new();

        public static void InitPluginHandleApi()
        {
            _apis = new Dictionary<string, PluginHandleApi>
            {
                { "FastConsole.PrintInfo", o => PluginApis.FastConsole(2, o.ToString()) },
                { "FastConsole.PrintTrash", o => PluginApis.FastConsole(0, o.ToString()) },
                { "FastConsole.PrintWarning", o => PluginApis.FastConsole(3, o.ToString()) },
                { "FastConsole.PrintError", o => PluginApis.FastConsole(4, o.ToString()) },
                { "FastConsole.PrintFatal", o => PluginApis.FastConsole(5, o.ToString()) }
            };
        }


        public static Dictionary<string, object> Handle(Dictionary<string, object> input)
        {
            try
            {
                if (!PluginController.Plugins[input["id"].ToString() ?? throw new Exception("参数不全".Translate())].Auth.ContainsKey(input["func"].ToString() ?? throw new Exception("参数不全".Translate())) ||
                    !PluginController.Plugins[input["id"].ToString() ?? throw new Exception("参数不全".Translate())].Auth[input["func"].ToString() ?? throw new Exception("参数不全".Translate())])
                    return new Dictionary<string, object>
                    {
                        { "status", false },
                        { "code", -401 },
                        { "message", "没有调用此 API 的权限".Translate() },
                        { "data", null }
                    };

                if (input["key"].ToString() != PluginController.Plugins[input["id"].ToString()].Key)
                    return new Dictionary<string, object>
                    {
                        { "status", false },
                        { "code", -400 },
                        { "message", "不正确的 Key".Translate() },
                        { "data", null }
                    };
                if (!_apis.ContainsKey(input["func"].ToString() ?? throw new Exception("参数不全".Translate())))
                    return new Dictionary<string, object>
                    {
                        { "status", false },
                        { "code", -501 },
                        { "message", "调用了未知的 API".Translate() },
                        { "data", null }
                    };
                return new Dictionary<string, object>
                {
                    { "status", true },
                    { "code", 200 },
                    { "message", "成功" },
                    { "data", _apis[input["func"].ToString() ?? throw new Exception("参数不全".Translate())].Invoke(input["data"]) }
                };
            }
            catch (Exception e)
            {
                return new Dictionary<string, object>
                {
                    { "status", false },
                    { "code", -500 },
                    { "message", e.Message },
                    { "data", null }
                };
            }
        }

        private delegate object PluginHandleApi(object obj);
    }
}