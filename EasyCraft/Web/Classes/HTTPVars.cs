using System;
using System.Collections.Generic;
using System.Text;
using EasyCraft.Core;

namespace EasyCraft.Web.Classes
{
    class HTTPVars
    {
        public User user=new User("rawobj");
        public Server for_server = new Server();
        public Server server = new Server();
        public List<Server> servers = new List<Server>();
    }
}
