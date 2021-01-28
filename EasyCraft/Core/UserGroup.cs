using System;
using System.Diagnostics;

namespace EasyCraft.Core
{
    internal class UserGroup
    {
        private static string path = "WinNT://" + Environment.MachineName;

        public static bool AddGroup()
        {
            return true;
        }

        public static bool IsGroupExist(string groupname)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                return ExcuteCommand("net localgroup").Contains("*" + groupname + "\r\n");
            return ExcuteCommand("/usr/bin/groups").Contains(groupname);
        }

        private static string ExcuteCommand(string path, string argument)
        {
            var process = new Process();
            process.StartInfo.FileName = path;
            process.StartInfo.Arguments = argument;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();
            return output;
        }

        private static string ExcuteCommand(string command)
        {
            var spaceidx = command.IndexOf(" ");

            if (spaceidx == -1)
                return ExcuteCommand(command, "");
            return ExcuteCommand(command.Substring(0, spaceidx), command.Substring(spaceidx + 1));
        }
    }
}