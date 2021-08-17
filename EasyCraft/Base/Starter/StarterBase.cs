using System;
using Newtonsoft.Json;

namespace EasyCraft.Base.Starter
{
    public class StarterBase
    {
        [JsonIgnore] public Type Type;
        [JsonProperty("name")] public string Name;
        [JsonProperty("id")] public string Id;
        [JsonProperty("version")] public string Version;
        [JsonProperty("description")] public string Description;
        [JsonProperty("author")] public string Author;
    }
}