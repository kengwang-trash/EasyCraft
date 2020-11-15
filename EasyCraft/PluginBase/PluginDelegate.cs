using System;
using System.Runtime.InteropServices;

namespace EasyCraft.PluginBase
{
    public class PluginDelegate
    {
        public delegate string Initialize([MarshalAs(UnmanagedType.LPWStr)] String Handler, [MarshalAs(UnmanagedType.LPWStr)]String AuthKey);
    }
}