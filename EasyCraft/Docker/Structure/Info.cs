using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCraft.Docker.Structure.Info
{
    public class Plugins
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> Volume { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Network { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Authorization { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Log { get; set; }
    }

    public class RegistryConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> AllowNondistributableArtifactsCIDRs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> AllowNondistributableArtifactsHostnames { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> InsecureRegistryCIDRs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Mirrors { get; set; }
    }

    public class Runc
    {
        /// <summary>
        /// 
        /// </summary>
        public string path { get; set; }
    }

    public class Runtimes
    {
        /// <summary>
        /// 
        /// </summary>
        public Runc runc { get; set; }
    }

    public class Swarm
    {
        /// <summary>
        /// 
        /// </summary>
        public string NodeID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string NodeAddr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LocalNodeState { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ControlAvailable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RemoteManagers { get; set; }
    }

    public class ContainerdCommit
    {
        /// <summary>
        /// 
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Expected { get; set; }
    }

    public class RuncCommit
    {
        /// <summary>
        /// 
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Expected { get; set; }
    }

    public class InitCommit
    {
        /// <summary>
        /// 
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Expected { get; set; }
    }

    public class Root
    {
        /// <summary>
        /// 
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Containers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ContainersRunning { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ContainersPaused { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ContainersStopped { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Images { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Driver { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<List<string>> DriverStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SystemStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Plugins Plugins { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MemoryLimit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SwapLimit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string KernelMemory { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string KernelMemoryTCP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CpuCfsPeriod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CpuCfsQuota { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CPUShares { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CPUSet { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PidsLimit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IPv4Forwarding { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BridgeNfIptables { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BridgeNfIp6tables { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Debug { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NFd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OomKillDisable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NGoroutines { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SystemTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LoggingDriver { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CgroupDriver { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NEventsListener { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string KernelVersion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OperatingSystem { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OSType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Architecture { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IndexServerAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public RegistryConfig RegistryConfig { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NCPU { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int MemTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GenericResources { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DockerRootDir { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string HttpProxy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string HttpsProxy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string NoProxy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Labels { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ExperimentalBuild { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ServerVersion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ClusterStore { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ClusterAdvertise { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Runtimes Runtimes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DefaultRuntime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Swarm Swarm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LiveRestoreEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Isolation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string InitBinary { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ContainerdCommit ContainerdCommit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public RuncCommit RuncCommit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public InitCommit InitCommit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> SecurityOptions { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ProductLicense { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Warnings { get; set; }
    }

}
