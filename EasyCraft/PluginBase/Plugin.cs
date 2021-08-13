using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EasyCraft.PluginBase
{
    [JsonObject(MemberSerialization.OptOut)]
    internal class Plugin
    {
        [JsonProperty("Auth")] public Dictionary<string, bool> Auth;
        [JsonIgnore] public Type Dll;
        [JsonProperty("enable")] public bool Enable;
        [JsonProperty("eventHookers")] public Dictionary<string, string> EventHookers;
        [JsonProperty("pluginInfo")] public PluginInfo Info;
        [JsonIgnore] public string Key;
    }

    public struct PluginInfo
    {
        public string Id;
        public string Name;
        public string Version;
        public string Author;
        public string Description;
        public string Link;
    }
}