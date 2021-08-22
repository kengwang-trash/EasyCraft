using System.Collections.Generic;
using Newtonsoft.Json;

namespace EasyCraft.Base.Core
{
    public class CoreBase
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("info")] public CoreInfoBase Info;
        [JsonIgnore] public Dictionary<string, CoreStartSimpleInfo> Start;
        [JsonIgnore] public string CoreConfig;
        public List<CoreConfigInfo> ConfigInfo => CoreManager.ConfigInfos[CoreConfig];
    }

    public class CoreStartSimpleInfo
    {
        [JsonProperty("program")] public string Program;
        [JsonProperty("param")] public string Parameter;
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
        [JsonProperty("file")] public string File;
        [JsonProperty("display")] public string Display;
        [JsonProperty("type")] public string Type;
        [JsonIgnore] public bool Required;
        [JsonIgnore] public List<CoreConfigKnownItem> Known;
    }

    public class CoreConfigKnownItem
    {
        public string Key;
        public string Display;
        public string Type; // `toggle` - 布尔值  `text` - 文本型  `select` - 选择器型
        public List<CoreConfigKnownItemSelection> Selection;
        public bool Visible;
        public bool Force;
        public string Value;
    }

    public class CoreConfigKnownItemSelection
    {
        [JsonProperty("display")] public string Display;
        [JsonProperty("value")] public string Value;
    }
}