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
            else
            {
                FastConsole.PrintTrash("不支持的指令: " + commandstr);
            }
        }
    }
}
