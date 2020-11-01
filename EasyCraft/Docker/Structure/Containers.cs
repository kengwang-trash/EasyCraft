using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCraft.Docker.Structure
{
    public class PortsItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PrivatePort { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PublicPort { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }
    }

    public class HostConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public string NetworkMode { get; set; }
    }

    public class Bridge
    {
        /// <summary>
        /// 
        /// </summary>
        public string IPAMConfig { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Links { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Aliases { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string NetworkID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string EndpointID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Gateway { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IPAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int IPPrefixLen { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IPv6Gateway { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GlobalIPv6Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int GlobalIPv6PrefixLen { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MacAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DriverOpts { get; set; }
    }

    public class Networks
    {
        /// <summary>
        /// 
        /// </summary>
        public Bridge bridge { get; set; }
    }

    public class NetworkSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public Networks Networks { get; set; }
    }

    public class MountsItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Mode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RW { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Propagation { get; set; }
    }

    public class Container
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Names { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ImageID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Command { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Created { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<PortsItem> Ports { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public HostConfig HostConfig { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public NetworkSettings NetworkSettings { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<MountsItem> Mounts { get; set; }
    }
}
