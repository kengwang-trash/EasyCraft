using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCraft.Struct
{
    class CoreStruct
    {
        public string id { get; set; }
        public string name { get; set; }

        public CoreStartConfig startconfig { get; set; }
        public CoreFirstStartConfig init { get; set; }

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
}
