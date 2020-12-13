using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EasyCraft.Web.JSONCallback;

namespace EasyCraft.Core
{
    class Core
    {
        public CoreStruct corestruct;
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
            coreconfig = File.ReadAllText("data/core/" + id + "/manifest.json");
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



}