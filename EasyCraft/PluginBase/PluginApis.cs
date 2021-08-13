using Serilog;

namespace EasyCraft.PluginBase
{
    internal class PluginApis
    {
        public static object FastConsole(int type, string message)
        {
            switch (type)
            {
                case 0:
                    Log.Verbose(message);
                    break;
                case 1:
                    Log.Debug(message);
                    break;
                case 2:
                    Log.Information(message);
                    break;
                case 3:
                    Log.Warning(message);
                    break;
                case 4:
                    Log.Error(message);
                    break;
                case 5:
                    Log.Fatal(message);
                    break;
            }

            return null;
        }
    }
}