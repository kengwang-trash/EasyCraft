using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;

namespace EasyCraft.Core
{
    class Schedule
    {
        public static List<Schedule> schedules = new List<Schedule>();
        private static Timer timer = new Timer(timercallback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        public int sid;
        public int server;
        public int type;
        public int interval;
        public string param;
        public bool enable;
        public DateTime lasttime;
        public DateTime nexttime;

        public static void LoadSchedule()
        {
            SQLiteCommand c = Database.DB.CreateCommand();
            c.CommandText = "SELECT * FROM `schedule`";
            SQLiteDataReader r = c.ExecuteReader();
            while (r.Read())
            {
                Schedule s = new Schedule();
                s.sid = r.GetInt32(0);
                s.server = r.GetInt32(1);
                s.type = r.GetInt32(2);
                s.param = r.GetString(3);
                s.lasttime = Convert.ToDateTime(r.GetString(4));
                s.interval = r.GetInt32(5);
                s.enable = r.GetBoolean(6);
                s.nexttime = DateTime.Now.AddMinutes(s.interval);
                schedules.Add(s);
            }
        }

        public static void StartTrigger()
        {
            timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
            FastConsole.PrintSuccess(Language.t("成功启动计划任务计时器"));
        }

        private static void timercallback(object state)
        {
            FastConsole.PrintTrash("[Schedule] Start Schedule");
            List<Schedule> nowschedules = schedules.Where(s => { return s.nexttime.ToString("yyyy-MM-dd HH:mm") == DateTime.Now.ToString("yyyy-MM-dd HH:mm") && s.enable; }).ToList();
            foreach (Schedule s in nowschedules)
            {
                try
                {
                    switch (s.type)
                    {
                        case 1://执行CMD
                            if (ServerManager.servers[s.server].Running)
                            {
                                ServerManager.servers[s.server].Send(s.param);
                            }
                            break;
                        case 2://开服
                            if (!ServerManager.servers[s.server].Running)
                            {
                                ServerManager.servers[s.server].Start();
                            }
                            break;
                        case 3://关服
                            if (ServerManager.servers[s.server].Running)
                            {
                                ServerManager.servers[s.server].Stop();
                            }
                            break;
                        case 4://重启
                            if (ServerManager.servers[s.server].Running)
                            {
                                ServerManager.servers[s.server].Stop();
                                ServerManager.servers[s.server].Start();
                            }
                            break;
                    }
                    FastConsole.PrintTrash("[Schedule] Server " + s.server.ToString() + " Excute Successful");
                }
                catch (Exception)
                {
                    FastConsole.PrintTrash("[Schedule] Server " + s.server.ToString() + " Excute Failed");
                }
                s.nexttime = DateTime.Now.AddMinutes(s.interval);
            }
            timer.Change(TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan);
        }
    }
}
