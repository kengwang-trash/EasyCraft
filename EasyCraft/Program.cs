using EasyCraft.Core;
using System;

namespace EasyCraft
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(
@" _____                 ____            __ _   
| ____|__ _ ___ _   _ / ___|_ __ __ _ / _| |_ 
|  _| / _` / __| | | | |   | '__/ _` | |_| __|
| |__| (_| \__ \ |_| | |___| | | (_| |  _| |_ 
|_____\__,_|___/\__, |\____|_|  \__,_|_|  \__|
                |___/                
 ===== Version : V 1.0.0 Alpha =====
 ====== Copyright Kengwang ======
");
            FastConsole.PrintWarning("You are running the alpha version of EasyCraft, it's not stable");
            FastConsole.PrintInfo("Loading Database");
            Database.Connect();
        }
    }
}
