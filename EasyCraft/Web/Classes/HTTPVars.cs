using System;
using System.Collections.Generic;
using System.Text;
using EasyCraft.Core;

namespace EasyCraft.Web.Classes
{
    class HTTPVars
    {
        public User user=new User("rawobj");
        public Server for_server = null;
        public Server server = null;
        public List<Server> servers = new List<Server>();
    }
}
