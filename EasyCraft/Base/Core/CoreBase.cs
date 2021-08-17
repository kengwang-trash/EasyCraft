using System.Collections.Generic;
using Newtonsoft.Json;

namespace EasyCraft.Base.Core
{
    public class CoreBase
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("info")] public CoreInfoBase Info;
        [JsonIgnore] public CoreStartSimpleInfo Start;
        [JsonIgnore] public Dictionary<string, CoreConfigInfo> ConfigInfo;
    }

    public class CoreStartSimpleInfo
    {
        public string Program;
        public string Parameter;
    }

    public class CoreInfoBase
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("device")] public int Device;
        [JsonProperty("branch")] public string Branch;
        [JsonProperty("name")] public string Name;
    }

    public class CoreConfigInfo
    {
        public string Name;
        public string Type;
        public bool Required;
        public List<CoreConfigKnownItem> Known;
    }

    public class CoreConfigKnownItem
    {
        public string Key;
        public string Name;
        public int Type; // 0 - 布尔值  1 - 文本型  2 - 选择器型
        public List<CoreConfigKnownItemSelection> Selection;
        public bool Visible;
        public bool Force;
        public string Value;
    }

    public class CoreConfigKnownItemSelection
    {
        public string Display;
        public string Value;
    }
}