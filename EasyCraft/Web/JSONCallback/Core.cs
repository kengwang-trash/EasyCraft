using System.Collections.Generic;

namespace EasyCraft.Web.JSONCallback
{
    internal class CoreStruct
    {
        public string id { get; set; }
        public string name { get; set; }

        public CoreStartConfig startconfig { get; set; }
        public CoreFirstStartConfig init { get; set; }
        public Dictionary<string, CoreServerPropertiesArgument> serverproperties { get; set; }
    }

    internal class CoreStartConfig
    {
        public string os { get; set; }
        public bool usecmd { get; set; }
        public bool multicommand { get; set; }
        public List<string> commands { get; set; }
        public string path { get; set; }
        public string argument { get; set; }
    }

    internal class CoreFirstStartConfig
    {
        public bool copyfiles { get; set; }
    }

    internal class CoreServerPropertiesArgument
    {
        public string name { get; set; }
        public bool show { get; set; }
        public string type { get; set; }
        public string defvalue { get; set; }
        public bool isvar { get; set; }
        public string what { get; set; }
    }
}