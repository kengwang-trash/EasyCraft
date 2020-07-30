using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCraft.Core
{
    class Settings
    {
        public static int httpport = 80;
        public static LogLevel logLevel;
    }

    enum LogLevel
    {
        no,//只输出Fatal和Error
        noserver,//不输出服务器内的日志
        notrash,
        all//所有
    }
}
