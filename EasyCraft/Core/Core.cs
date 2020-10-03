using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyCraft.Core
{
    class Core
    {
        private CoreStruct corestruct;
        private string coreconfig;
        public string id;
        public string name;
        public string os;
        public bool usecmd;
        public bool multicommand;
        public List<string> commands;
        public string path;
        public string argument;
        public bool initcopy;

        public Core(string id)
        {
            this.id = id;
            coreconfig = File.ReadAllText("core/" + id + "/manifest.json");
            corestruct = Newtonsoft.Json.JsonConvert.DeserializeObject<CoreStruct>(coreconfig);
            name = corestruct.name;
            os = corestruct.startconfig.os;
            usecmd = corestruct.startconfig.usecmd;
            multicommand = corestruct.startconfig.multicommand;
            initcopy = corestruct.init.copyfiles;
            if (multicommand)
            {
                commands = corestruct.startconfig.commands;
            }
            else
            {
                path = corestruct.startconfig.path;
                argument = corestruct.startconfig.argument;
            }
        }
    }
    
    
    
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
