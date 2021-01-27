using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCraft.Web.JSONCallback
{
    class CoreStruct
    {
        public string id { get; set; }
        public string name { get; set; }

        public CoreStartConfig startconfig { get; set; }
        public CoreFirstStartConfig init { get; set; }
        public Dictionary<string, CoreServerPropertiesArgument> serverproperties { get; set; }
    }

    class CoreStartConfig
    {
        public string os { get; set; }
        public bool usecmd { get; set; }
        public bool multicommand { get; set; }
        public List<string> commands { get; set; }
        public string path { get; set; }
        public string argument { get; set; }
    }

    class CoreFirstStartConfig
    {
        public bool copyfiles { get; set; }
    }

    class CoreServerPropertiesArgument
    {
        public string name { get; set; }
        public bool show { get; set; }
        public string type { get; set; }
        public string defvalue { get; set; }
        public bool isvar { get; set; }
        public string what { get; set; }
    }
}
