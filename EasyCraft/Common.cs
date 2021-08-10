using System.IO;
using Serilog.Core;

namespace EasyCraft
{
    public static class Common
    {
        public static string SOFT_NAME = "EasyCraft";
        public static string VERSIONFULL = "1.0.0.0";
        public static string VERSIONSHORT = "1.0.0";
        public static string VERSIONNAME = "TestFlight"; //待定 发布时确定

        public static string BASE_DIR = Directory.GetCurrentDirectory();
        public static string DATA_DIR = Directory.GetCurrentDirectory() + "/data";
        
    }
}