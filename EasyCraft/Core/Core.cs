using System.Collections.Generic;
using System.IO;
using EasyCraft.Web.JSONCallback;
using Newtonsoft.Json;

namespace EasyCraft.Core
{
    internal class Core
    {
        public string argument;
        public List<string> commands;
        private readonly string coreconfig;
        public CoreStruct corestruct;
        public string id;
        public bool initcopy;
        public bool multicommand;
        public string name;
        public string os;
        public string path;
        public bool usecmd;

        public Core(string id)
        {
            this.id = id;
            coreconfig = File.ReadAllText("data/core/" + id + "/manifest.json");
            corestruct = JsonConvert.DeserializeObject<CoreStruct>(coreconfig);
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
}