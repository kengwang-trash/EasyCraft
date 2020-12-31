using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCraft.Docker.Structure.Info
{
    public class Root
    {
        public string ID { get; set; }
        public int Containers { get; set; }
        public int ContainersRunning { get; set; }
        public int ContainersPaused { get; set; }
        public int ContainersStopped { get; set; }
        public int Images { get; set; }
        public string KernelVersion { get; set; }
        public string OperatingSystem { get; set; }
        public string OSVersion { get; set; }
        public string OSType { get; set; }
        public string Architecture { get; set; }
        public int NCPU { get; set; }
        public long MemTotal { get; set; }
        public string GenericResources { get; set; }
        public string Name { get; set; }
        public string ExperimentalBuild { get; set; }
        public string ServerVersion { get; set; }
    }

}
