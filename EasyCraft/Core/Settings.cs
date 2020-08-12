using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyCraft.Core
{
    class Settings
    {
        public static int httpport  {
            get
            {
                /*
                if (File.Exists("port.conf"))
                {
                    return int.Parse(File.ReadAllText("port.conf"));
                }
                FastConsole.PrintInfo("Please Input the port you want to listen: ");
                string p = Console.ReadLine();
                if (int.Parse(p)!=0)
                {
                    File.WriteAllText("port.conf", p);
                    return int.Parse(p);
                }
                else
                {
                    FastConsole.PrintInfo("Please Input a valid number: ");
                    return httpport;
                }
                */
                return 80;
            }
        }

    }
}
