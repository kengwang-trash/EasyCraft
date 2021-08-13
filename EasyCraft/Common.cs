using System.IO;

namespace EasyCraft
{
    public static class Common
    {
        public static string SoftName = "EasyCraft";
        public static string VersionFull = "1.0.0.0";
        public static string VersionShort = "1.0.0";
        public static string VersionName = "TestFlight"; //待定 发布时确定

        public static string BaseDir = Directory.GetCurrentDirectory();
        public static string DataDir = Directory.GetCurrentDirectory() + "/data";
    }
}