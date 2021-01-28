using EasyCraft.Web;

namespace EasyCraft.Core
{
    internal class CommandPhrase
    {
        public static void PhraseCommand(string commandstr)
        {
            if (string.IsNullOrEmpty(commandstr)) return;
            var command = commandstr.Split(' ');
            if (command[0] == "start")
            {
                var s = ServerManager.servers[int.Parse(command[1])];
                s.Start();
            }
            else if (command[0] == "stop")
            {
                var s = ServerManager.servers[int.Parse(command[1])];
                s.Stop();
            }
            else if (command[0] == "theme")
            {
                if (command[1] == "refresh")
                {
                    FastConsole.PrintInfo(Language.t("重载主题中..."));
                    ThemeController.InitComp();
                    ThemeController.InitPage();
                }
            }
            else if (command[0] == "kill")
            {
                var s = ServerManager.servers[int.Parse(command[1])];
                s.Kill();
            }
            else if (command[0] == "killall")
            {
                var s = ServerManager.servers[int.Parse(command[1])];
                s.KillAll();
            }
            else if (command[0] == "version")
            {
                FastConsole.PrintInfo(Settings.BUILDINFO);
            }
            else
            {
                FastConsole.PrintTrash("不支持的指令: " + commandstr);
            }
        }
    }
}