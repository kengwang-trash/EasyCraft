using System;
using Microsoft.Extensions.Configuration;

namespace EasyCraft
{
    public static class Common
    {
        public static string SoftName = "EasyCraft";
        public static string VersionFull = "1.0.0.0";
        public static string VersionShort = "1.0.0";
        public static string VersionName = "TestFlight"; //待定 发布时确定
        public static IConfiguration Configuration;

        public static string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
        public static string DataDir = AppDomain.CurrentDomain.BaseDirectory + "/data";
    }
}