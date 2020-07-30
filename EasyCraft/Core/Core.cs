using EasyCraft.Struct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;

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

        public Core(string id)
        {
            this.id = id;
            coreconfig = File.ReadAllText("core/" + id + "/manifest.yaml");
            Deserializer des = new Deserializer();
            TextReader input = new StringReader(coreconfig);
            corestruct = des.Deserialize<CoreStruct>(input);
            name = corestruct.name;
            os = corestruct.os;
            usecmd = corestruct.startconfig.usecmd;
            multicommand = corestruct.startconfig.multicommand;
            commands = corestruct.startconfig.commands;
            path = corestruct.startconfig.path;
            argument = corestruct.startconfig.argument;
        }
    }




}
