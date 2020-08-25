using EasyCraft.Core;
using EasyCraft.Web.Classes;
using EasyCraft.Web.JSONCallback;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyCraft.Web
{
    class Api
    { 
        public static void PhraseAPI(string path, WebPanelPhraser wp)
        {
            switch (path)
            {
                case "login":
                    if (wp.session.ContainsKey("loginfail") && int.Parse(wp.session["loginfail"]) >= 5 && wp.session["lastloginfailday"] == DateTime.Today.ToString())
                    {
                        UserLogin callback = new UserLogin();
                        wp.session["loginfail"] += 1;
                        callback.message = Language.t("Login failed, too many attempts!");
                        callback.code = -2;
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        return;
                    }
                    if (wp.POST.ContainsKey("username") && wp.POST.ContainsKey("password"))
                    {
                        wp.vars.user = new User(wp.POST["username"], wp.POST["password"]);
                        if (wp.vars.user.islogin)
                        {
                            wp.session["auth"] = wp.vars.user.auth;
                            UserLogin callback = new UserLogin();
                            callback.code = 9000;
                            callback.message = Language.t("Login Successful");
                            callback.data = new LoginUserInfo();
                            callback.data.username = wp.vars.user.name;
                            callback.data.uid = wp.vars.user.uid;
                            callback.data.email = wp.vars.user.email;
                            wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        }
                        else
                        {
                            UserLogin callback = new UserLogin();
                            if (wp.session.ContainsKey("loginfail"))
                            {
                                wp.session["loginfail"] = (int.Parse(wp.session["loginfail"]) + 1).ToString();
                            }
                            else
                            {
                                wp.session["loginfail"] = 1.ToString();
                            }

                            wp.session["lastloginfailday"] = DateTime.Today.ToString();
                            callback.message = Language.t("Login failed, account or password is wrong");
                            callback.code = -1;
                            wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        UserLogin callback = new UserLogin();
                        callback.message = Language.t("Login failed, parameters are incomplete!");
                        callback.code = -3;
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                    }
                    break;
                case "register":
                    if (wp.POST.ContainsKey("username") && wp.POST.ContainsKey("password") && wp.POST.ContainsKey("email"))
                    {
                        if (User.Exist(wp.POST["username"]))
                        {
                            Callback callback = new Callback();
                            callback.code = -2;
                            callback.message = Language.t("Registration failed, username already exists");
                            wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        }
                        else
                        {
                            User user = User.Register(wp.POST["username"], wp.POST["password"], wp.POST["email"], wp.POST["qq"]);
                            if (user == null)
                            {
                                Callback callback = new Callback();
                                callback.code = -2;
                                callback.message = Language.t("Registration failed");
                                wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                            }
                            else
                            {
                                Callback callback = new Callback();
                                callback.code = 9000;
                                callback.message = Language.t("Registration Success");
                                wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                            }
                        }
                    }
                    else
                    {
                        Callback callback = new Callback();
                        callback.code = -3;
                        callback.message = Language.t("Registration failed with incomplete parameters");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                    }
                    break;
                case "new_server":
                    if (wp.vars.user.type >= 3)
                    {
                        if (wp.POST.ContainsKey("owner"))
                        {
                            try
                            {
                                int sid = Server.CreateServer();
                                Server s = new Server(sid);

                                if (s == null) throw new Exception("Create Failed");

                                if (wp.POST.ContainsKey("name")) s.name = wp.POST["name"];
                                if (wp.POST.ContainsKey("owner")) s.owner = int.Parse(wp.POST["owner"]);
                                if (wp.POST.ContainsKey("port")) s.port = int.Parse(wp.POST["port"]);
                                if (wp.POST.ContainsKey("core")) s.core = wp.POST["core"];
                                if (wp.POST.ContainsKey("maxplayer")) s.maxplayer = int.Parse(wp.POST["maxplayer"]);
                                if (wp.POST.ContainsKey("ram")) s.ram = int.Parse(wp.POST["ram"]);
                                if (wp.POST.ContainsKey("world")) s.world = wp.POST["world"];
                                if (wp.POST.ContainsKey("expiretime")) s.expiretime = DateTime.Parse(wp.POST["expiretime"]);
                                s.SaveServerConfig();
                                ServerManager.servers.Add(s.id, s);
                                NewServer callback = new NewServer();
                                callback.code = 9000;
                                callback.message = Language.t("Server Successfully Create");
                                callback.data = s.id;
                                wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                            }
                            catch (Exception)
                            {
                                NewServer callback = new NewServer();
                                callback.code = -1;
                                callback.message = Language.t("Failed to Create Server");
                                wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                            }

                        }
                        else
                        {
                            NewServer callback = new NewServer();
                            callback.code = -2;
                            callback.message = Language.t("Param not completed");
                            wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        NewServer callback = new NewServer();
                        callback.code = -3;
                        callback.message = Language.t("Permission Denied");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                    }
                    break;
                case "edit_server":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        Callback callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("Server Not Found");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        return;
                    }
                    Server server = ServerManager.servers[int.Parse(wp.POST["sid"])];
                    if (wp.vars.user.type >= 2 || server.owner == wp.vars.user.uid)
                    {
                        try
                        {
                            if (server.owner == wp.vars.user.uid)
                            {
                                if (wp.POST.ContainsKey("world")) server.world = wp.POST["world"];
                                if (wp.POST.ContainsKey("core")) server.core = wp.POST["core"];
                                server.SaveServerConfig();
                                ServerManager.servers[int.Parse(wp.POST["sid"])] = server;
                            }
                            else
                            {

                                if (wp.POST.ContainsKey("name")) server.name = wp.POST["name"];
                                if (wp.POST.ContainsKey("owner")) server.owner = int.Parse(wp.POST["owner"]);
                                if (wp.POST.ContainsKey("port")) server.port = int.Parse(wp.POST["port"]);
                                if (wp.POST.ContainsKey("core")) server.core = wp.POST["core"];
                                if (wp.POST.ContainsKey("maxplayer")) server.maxplayer = int.Parse(wp.POST["maxplayer"]);
                                if (wp.POST.ContainsKey("ram")) server.ram = int.Parse(wp.POST["ram"]);
                                if (wp.POST.ContainsKey("world")) server.world = wp.POST["world"];
                                if (wp.POST.ContainsKey("expiretime")) server.expiretime = DateTime.Parse(wp.POST["expiretime"]);
                                server.SaveServerConfig();
                                ServerManager.servers[int.Parse(wp.POST["sid"])] = server;
                            }

                            Callback callback = new Callback();
                            callback.code = 9000;
                            callback.message = Language.t("Server Edited");
                            wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                            return;
                        }
                        catch (Exception)
                        {
                            NewServer callback = new NewServer();
                            callback.code = -2;
                            callback.message = Language.t("Edit Server Config Failed");
                            wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        }

                    }
                    else
                    {
                        NewServer callback = new NewServer();
                        callback.code = -3;
                        callback.message = Language.t("Permission Denied");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                    }
                    break;
                case "start_server":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        Callback callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("Server Not Found");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        return;
                    }
                    if (wp.vars.user.type >= 2 || ServerManager.servers[int.Parse(wp.POST["sid"])].owner == wp.vars.user.uid)
                    {
                        if (ServerManager.servers[int.Parse(wp.POST["sid"])].expiretime < DateTime.Now)
                        {
                            Callback callback = new Callback();
                            callback.code = -3;
                            callback.message = Language.t("Server Expired");
                            wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                            return;
                        }
                        try
                        {
                            ServerManager.servers[int.Parse(wp.POST["sid"])].Start();
                            Callback callback = new Callback();
                            callback.code = 9000;
                            callback.message = Language.t("Server Started");
                            wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                            return;
                        }
                        catch (Exception)
                        {
                            Callback callback = new Callback();
                            callback.code = -2;
                            callback.message = Language.t("Failed to Start Server");
                            wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        NewServer callback = new NewServer();
                        callback.code = -3;
                        callback.message = Language.t("Permission Denied");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                    }
                    break;
                case "stop_server":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        Callback callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("Server Not Found");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        return;
                    }
                    if (wp.vars.user.type >= 2 || ServerManager.servers[int.Parse(wp.POST["sid"])].owner == wp.vars.user.uid)
                    {
                        try
                        {
                            ServerManager.servers[int.Parse(wp.POST["sid"])].Stop();
                            Callback callback = new Callback();
                            callback.code = 9000;
                            callback.message = Language.t("Server Stopped");
                            wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                            return;
                        }
                        catch (Exception)
                        {
                            Callback callback = new Callback();
                            callback.code = -2;
                            callback.message = Language.t("Failed to Stop Server");
                            wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        NewServer callback = new NewServer();
                        callback.code = -3;
                        callback.message = Language.t("Permission Denied");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                    }
                    break;
                case "log":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        Callback callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("Server Not Found");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        return;
                    }
                    if (wp.vars.user.type >= 2 || ServerManager.servers[int.Parse(wp.POST["sid"])].owner == wp.vars.user.uid)
                    {
                        Server s = ServerManager.servers[int.Parse(wp.POST["sid"])];
                        List<ServerLog> logs = null;
                        if (wp.POST.ContainsKey("lastlogid") && wp.POST["lastlogid"] != "0")
                        {
                            logs = s.log.Values.Where(log => log.id > int.Parse(wp.POST["lastlogid"])).ToList();
                        }
                        else
                        {
                            int n = 0;
                            logs = s.log.Values.Where(log => ++n <= 100).ToList();
                        }
                        LogBack callback = new LogBack();
                        callback.code = 9000;
                        callback.message = Language.t("Success");
                        callback.data = new LogBackData();
                        callback.data.logs = logs;
                        if (logs.Count != 0)
                        {
                            callback.data.lastlogid = logs.Last().id;
                        }
                        else
                        {
                            if (wp.POST.ContainsKey("lastlogid") && wp.POST["lastlogid"] != "0")
                            {
                                callback.data.lastlogid = int.Parse(wp.POST["lastlogid"]);
                            }
                            else
                            {
                                callback.data.lastlogid = 0;
                            }
                        }
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                    }
                    else
                    {
                        Callback callback = new Callback();
                        callback.code = -3;
                        callback.message = Language.t("Permission Denied");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                    }
                    break;
                case "cmd":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        Callback callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("Server Not Found");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        return;
                    }
                    if (wp.vars.user.type >= 2 || ServerManager.servers[int.Parse(wp.POST["sid"])].owner == wp.vars.user.uid)
                    {
                        if (wp.POST.ContainsKey("cmd"))
                        {
                            ServerManager.servers[int.Parse(wp.POST["sid"])].Send(wp.POST["cmd"] + "\r\n");
                            Callback callback = new Callback();
                            callback.code = 9000;
                            callback.message = Language.t("Success");
                            wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        }
                        else
                        {
                            Callback callback = new Callback();
                            callback.code = -2;
                            callback.message = Language.t("Param not completed");
                            wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        Callback callback = new Callback();
                        callback.code = -3;
                        callback.message = Language.t("Permission Denied");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                    }
                    break;
                case "clear_log":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        Callback callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("Server Not Found");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                        return;
                    }
                    if (wp.vars.user.type >= 2 || ServerManager.servers[int.Parse(wp.POST["sid"])].owner == wp.vars.user.uid)
                    {
                        ServerManager.servers[int.Parse(wp.POST["sid"])].ClearLog();
                        Callback callback = new Callback();
                        callback.code = 9000;
                        callback.message = Language.t("Successfully to clear logs on server");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                    }
                    else
                    {
                        Callback callback = new Callback();
                        callback.code = -3;
                        callback.message = Language.t("Permission Denied");
                        wp.PrintWeb(Newtonsoft.Json.JsonConvert.SerializeObject(callback));
                    }
                    break;
            }
        }
    }
}
