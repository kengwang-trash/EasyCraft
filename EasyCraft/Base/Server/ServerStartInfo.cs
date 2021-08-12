using Newtonsoft.Json;

namespace EasyCraft.Base.Server
{
    public struct ServerStartInfo
    {
        [JsonProperty("isRunning")] public bool IsRunning;
    }
}