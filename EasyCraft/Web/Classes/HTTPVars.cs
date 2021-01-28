using System.Collections.Generic;
using EasyCraft.Core;

namespace EasyCraft.Web.Classes
{
    internal class HTTPVars
    {
        public List<Core.Core> cores = new List<Core.Core>();
        public Core.Core for_core = null;
        public Server for_server = null;
        public Server server = null;
        public List<Server> servers = new List<Server>();
        public User user = new User("rawobj");
    }
}