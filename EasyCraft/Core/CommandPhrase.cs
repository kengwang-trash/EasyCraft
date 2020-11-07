using EasyCraft.Web;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCraft.Core
{
    class CommandPhrase
    {
        public static void PhraseCommand(string commandstr)
        {
            if (string.IsNullOrEmpty(commandstr))
            {
                return;
            }
            string[] command = commandstr.Split(' ');
            if (command[0] == "start")
            {
                Server s = ServerManager.servers[int.Parse(command[1])];
                s.Start();
            }
            else if (command[0] == "stop")
            {
                Server s = ServerManager.servers[int.Parse(command[1])];
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
                Server s = ServerManager.servers[int.Parse(command[1])];
                s.Kill();
            }
            else if (command[0] == "killall")
            {
                Server s = ServerManager.servers[int.Parse(command[1])];
                s.KillAll();
            }
            else if (command[0] == "version")
            {
                FastConsole.PrintInfo(Settings.BUILDINFO);
            }
            else if (command[0] == "docker")
            {
                if (command[1] == "list")
                {
                    FastConsole.PrintInfo(Language.t("以下是 Docker 容器"));
                    Settings.docker.ListContainer(true).ForEach(container => { FastConsole.PrintInfo(container.Names[0]+"("+ container.Id+") "+container.State); });
                }
            }
            else
            {
                FastConsole.PrintTrash("不支持的指令: " + commandstr);
            }
        }
    }
}
