﻿using System;
using System.Collections.Generic;
using System.Linq;
using EasyCraft.Core;
using EasyCraft.Web.Classes;
using EasyCraft.Web.JSONCallback;
using Newtonsoft.Json;

namespace EasyCraft.Web
{
    internal class Api
    {
        public static void PhraseAPI(string path, WebPanelPhraser wp)
        {
            switch (path)
            {
                case "login":
                    if (wp.session.ContainsKey("loginfail"))
                        if (int.Parse(wp.session["loginfail"]) >= 5 &&
                            wp.session["lastloginfailday"] == DateTime.Today.ToString())
                        {
                            var callback = new UserLogin();
                            callback.message = Language.t("登录失败,尝试次数过多!");
                            callback.code = -2;
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                            return;
                        }

                    if (wp.POST.ContainsKey("username") && wp.POST.ContainsKey("password"))
                    {
                        wp.vars.user = new User(wp.POST["username"], wp.POST["password"]);
                        if (wp.vars.user.islogin)
                        {
                            wp.vars.user.UpdateAuth();
                            wp.session["auth"] = wp.vars.user.auth;
                            var callback = new UserLogin();
                            callback.code = 9000;
                            callback.message = Language.t("成功登录");
                            callback.data = new LoginUserInfo();
                            callback.data.username = wp.vars.user.name;
                            callback.data.uid = wp.vars.user.uid;
                            callback.data.email = wp.vars.user.email;
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                        else
                        {
                            var callback = new UserLogin();
                            if (wp.session.ContainsKey("loginfail"))
                                wp.session["loginfail"] = (int.Parse(wp.session["loginfail"]) + 1).ToString();
                            else
                                wp.session["loginfail"] = 1.ToString();

                            wp.session["lastloginfailday"] = DateTime.Today.ToString();
                            callback.message = Language.t("登录失败,用户名或密码错误");
                            callback.code = -1;
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        var callback = new UserLogin();
                        callback.message = Language.t("登录错误,参数不完整!");
                        callback.code = -3;
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }

                    break;
                case "register":
                    if (wp.POST.ContainsKey("username") && wp.POST.ContainsKey("password") &&
                        wp.POST.ContainsKey("email"))
                    {
                        if (string.IsNullOrEmpty(wp.POST["username"]) || string.IsNullOrEmpty(wp.POST["password"]) ||
                            string.IsNullOrEmpty(wp.POST["email"]))
                        {
                            var callback = new Callback();
                            callback.code = -2;
                            callback.message = Language.t("注册失败,参数不完整");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                            return;
                        }

                        if (User.GetUid(wp.POST["username"]) != -1)
                        {
                            var callback = new Callback();
                            callback.code = -2;
                            callback.message = Language.t("注册失败,此用户名已存在");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                        else
                        {
                            User user = null;
                            if (wp.POST.ContainsKey("qq") && !string.IsNullOrEmpty(wp.POST["qq"]))
                                user = User.Register(wp.POST["username"], wp.POST["password"], wp.POST["email"],
                                    wp.POST["qq"]);
                            else
                                user = User.Register(wp.POST["username"], wp.POST["password"], wp.POST["email"]);
                            if (user == null)
                            {
                                var callback = new Callback();
                                callback.code = -2;
                                callback.message = Language.t("注册失败");
                                wp.PrintWeb(JsonConvert.SerializeObject(callback));
                            }
                            else
                            {
                                var callback = new Callback();
                                callback.code = 9000;
                                callback.message = Language.t("注册成功");
                                wp.PrintWeb(JsonConvert.SerializeObject(callback));
                            }
                        }
                    }
                    else
                    {
                        var callback = new Callback();
                        callback.code = -3;
                        callback.message = Language.t("注册失败,参数不完整");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }

                    break;
                case "new_server":
                    if (wp.vars.user.CheckUserAbility((int) Permisson.CreateServer))
                    {
                        if (wp.POST.ContainsKey("owner"))
                        {
                            if (User.GetUid(wp.POST["owner"]) != -1)
                            {
                                try
                                {
                                    var sid = Server.CreateServer();
                                    if (sid == 0) throw new Exception("服务器创建失败");
                                    var s = new Server(sid);

                                    if (wp.POST.ContainsKey("name")) s.Name = wp.POST["name"];
                                    if (wp.POST.ContainsKey("owner")) s.Owner = User.GetUid(wp.POST["owner"]);
                                    if (wp.POST.ContainsKey("port")) s.Port = int.Parse(wp.POST["port"]);
                                    if (wp.POST.ContainsKey("core")) s.Core = wp.POST["core"];
                                    if (wp.POST.ContainsKey("maxplayer")) s.Maxplayer = int.Parse(wp.POST["maxplayer"]);
                                    if (wp.POST.ContainsKey("ram")) s.Ram = int.Parse(wp.POST["ram"]);
                                    if (wp.POST.ContainsKey("world")) s.World = wp.POST["world"];
                                    if (wp.POST.ContainsKey("expiretime"))
                                        s.Expiretime = DateTime.Parse(wp.POST["expiretime"]);
                                    s.SaveServerConfig();
                                    ServerManager.servers.Add(s.Id, s);
                                    var callback = new NewServer();
                                    callback.code = 9000;
                                    callback.message = Language.t("服务器创建成功");
                                    callback.data = s.Id;
                                    wp.PrintWeb(JsonConvert.SerializeObject(callback));
                                }
                                catch (Exception)
                                {
                                    var callback = new NewServer();
                                    callback.code = -1;
                                    callback.message = Language.t("创建服务器失败");
                                    wp.PrintWeb(JsonConvert.SerializeObject(callback));
                                }
                            }
                            else
                            {
                                var callback = new NewServer();
                                callback.code = -2;
                                callback.message = Language.t("用户不存在");
                                wp.PrintWeb(JsonConvert.SerializeObject(callback));
                            }
                        }
                        else
                        {
                            var callback = new NewServer();
                            callback.code = -2;
                            callback.message = Language.t("参数不完整");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        var callback = new NewServer();
                        callback.code = -3;
                        callback.message = Language.t("权限不足");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }

                    break;
                case "edit_server":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        var callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("未找到此服务器");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        return;
                    }

                    var server = ServerManager.servers[int.Parse(wp.POST["sid"])];
                    if (wp.vars.user.CheckUserAbility((int) Permisson.EditServer) || server.Owner == wp.vars.user.uid)
                    {
                        try
                        {
                            if (server.Owner == wp.vars.user.uid)
                            {
                                if (wp.POST.ContainsKey("world")) server.World = wp.POST["world"];
                                if (wp.POST.ContainsKey("core")) server.Core = wp.POST["core"];
                                server.SaveServerConfig();
                                ServerManager.servers[int.Parse(wp.POST["sid"])] = server;
                            }

                            if (wp.vars.user.CheckUserAbility((int) Permisson.EditServer))
                            {
                                if (wp.POST.ContainsKey("name")) server.Name = wp.POST["name"];
                                if (wp.POST.ContainsKey("owner")) server.Owner = int.Parse(wp.POST["owner"]);
                                if (wp.POST.ContainsKey("port")) server.Port = int.Parse(wp.POST["port"]);
                                if (wp.POST.ContainsKey("core")) server.Core = wp.POST["core"];
                                if (wp.POST.ContainsKey("maxplayer"))
                                    server.Maxplayer = int.Parse(wp.POST["maxplayer"]);
                                if (wp.POST.ContainsKey("ram")) server.Ram = int.Parse(wp.POST["ram"]);
                                if (wp.POST.ContainsKey("world")) server.World = wp.POST["world"];
                                if (wp.POST.ContainsKey("expiretime"))
                                    server.Expiretime = Convert.ToDateTime(wp.POST["expiretime"]);
                                server.SaveServerConfig();
                                ServerManager.servers[int.Parse(wp.POST["sid"])] = server;
                            }

                            var callback = new Callback();
                            callback.code = 9000;
                            callback.message = Language.t("服务器信息已被保存");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                        catch (Exception)
                        {
                            var callback = new NewServer();
                            callback.code = -2;
                            callback.message = Language.t("编辑服务器信息失败");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        var callback = new NewServer();
                        callback.code = -3;
                        callback.message = Language.t("权限不足");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }

                    break;
                case "start_server":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        var callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("未找到此服务器");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        return;
                    }

                    if (wp.vars.user.CheckUserAbility((int) Permisson.StartServer) ||
                        ServerManager.servers[int.Parse(wp.POST["sid"])].Owner == wp.vars.user.uid)
                    {
                        if (ServerManager.servers[int.Parse(wp.POST["sid"])].Expiretime < DateTime.Now)
                        {
                            var callback = new Callback();
                            callback.code = -3;
                            callback.message = Language.t("服务器已过期");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                            return;
                        }

                        try
                        {
                            ServerManager.servers[int.Parse(wp.POST["sid"])].Start();
                            var callback = new Callback();
                            callback.code = 9000;
                            callback.message = Language.t("服务器已开启");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                        catch (Exception)
                        {
                            var callback = new Callback();
                            callback.code = -2;
                            callback.message = Language.t("服务器开启失败");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        var callback = new NewServer();
                        callback.code = -3;
                        callback.message = Language.t("权限不足");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }

                    break;
                case "stop_server":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        var callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("未找到此服务器");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        return;
                    }

                    if (wp.vars.user.CheckUserAbility((int) Permisson.StopServer) ||
                        ServerManager.servers[int.Parse(wp.POST["sid"])].Owner == wp.vars.user.uid)
                    {
                        try
                        {
                            ServerManager.servers[int.Parse(wp.POST["sid"])].Stop();
                            var callback = new Callback();
                            callback.code = 9000;
                            callback.message = Language.t("服务器已关闭");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                        catch (Exception)
                        {
                            var callback = new Callback();
                            callback.code = -2;
                            callback.message = Language.t("服务器关闭失败");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        var callback = new NewServer();
                        callback.code = -3;
                        callback.message = Language.t("权限不足");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }

                    break;
                case "kill_server":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        var callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("未找到此服务器");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        return;
                    }

                    if (wp.vars.user.CheckUserAbility((int) Permisson.KillServer) ||
                        ServerManager.servers[int.Parse(wp.POST["sid"])].Owner == wp.vars.user.uid)
                    {
                        try
                        {
                            ServerManager.servers[int.Parse(wp.POST["sid"])].Kill();
                            var callback = new Callback();
                            callback.code = 9000;
                            callback.message = Language.t("服务器已强行停止");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                        catch (Exception)
                        {
                            var callback = new Callback();
                            callback.code = -2;
                            callback.message = Language.t("服务器强行停止失败");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        var callback = new NewServer();
                        callback.code = -3;
                        callback.message = Language.t("权限不足");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }

                    break;
                case "kill_server_all":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        var callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("未找到此服务器");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        return;
                    }

                    if (wp.vars.user.CheckUserAbility((int) Permisson.KillServerAll) ||
                        ServerManager.servers[int.Parse(wp.POST["sid"])].Owner == wp.vars.user.uid)
                    {
                        try
                        {
                            ServerManager.servers[int.Parse(wp.POST["sid"])].KillAll();
                            var callback = new Callback();
                            callback.code = 9000;
                            callback.message = Language.t("成功强停服务器");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                        catch (Exception)
                        {
                            var callback = new Callback();
                            callback.code = -2;
                            callback.message = Language.t("服务器强停失败");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        var callback = new NewServer();
                        callback.code = -3;
                        callback.message = Language.t("权限不足");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }

                    break;
                case "log":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        var callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("未找到此服务器");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        return;
                    }

                    if (wp.vars.user.CheckUserAbility((int) Permisson.QueryLog) ||
                        ServerManager.servers[int.Parse(wp.POST["sid"])].Owner == wp.vars.user.uid)
                    {
                        var s = ServerManager.servers[int.Parse(wp.POST["sid"])];
                        List<ServerLog> logs = null;
                        if (wp.POST.ContainsKey("lastlogid") && wp.POST["lastlogid"] != "0")
                        {
                            logs = s.log.Values.Where(log => log.id > int.Parse(wp.POST["lastlogid"])).ToList();
                        }
                        else
                        {
                            var n = 0;
                            logs = s.log.Values.Where(log => ++n <= 100).ToList();
                        }

                        var callback = new LogBack();
                        callback.code = 9000;
                        callback.message = Language.t("成功");
                        callback.data = new LogBackData();
                        callback.data.starting = s.Running;
                        callback.data.logs = logs;
                        if (logs.Count != 0)
                        {
                            callback.data.lastlogid = logs.Last().id;
                        }
                        else
                        {
                            if (wp.POST.ContainsKey("lastlogid") && wp.POST["lastlogid"] != "0")
                                callback.data.lastlogid = int.Parse(wp.POST["lastlogid"]);
                            else
                                callback.data.lastlogid = 0;
                        }

                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }
                    else
                    {
                        var callback = new Callback();
                        callback.code = -3;
                        callback.message = Language.t("权限不足");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }

                    break;
                case "cmd":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        var callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("未找到此服务器");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        return;
                    }

                    if (wp.vars.user.CheckUserAbility((int) Permisson.SendCmd) ||
                        ServerManager.servers[int.Parse(wp.POST["sid"])].Owner == wp.vars.user.uid)
                    {
                        if (wp.POST.ContainsKey("cmd"))
                        {
                            ServerManager.servers[int.Parse(wp.POST["sid"])].Send(wp.POST["cmd"] + "\r\n");
                            var callback = new Callback();
                            callback.code = 9000;
                            callback.message = Language.t("成功");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                        else
                        {
                            var callback = new Callback();
                            callback.code = -2;
                            callback.message = Language.t("参数不全");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        var callback = new Callback();
                        callback.code = -3;
                        callback.message = Language.t("权限不足");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }

                    break;
                case "clear_log":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        var callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("未找到此服务器");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        return;
                    }

                    if (wp.vars.user.CheckUserAbility((int) Permisson.CleanLog) ||
                        ServerManager.servers[int.Parse(wp.POST["sid"])].Owner == wp.vars.user.uid)
                    {
                        ServerManager.servers[int.Parse(wp.POST["sid"])].ClearLog();
                        var callback = new Callback();
                        callback.code = 9000;
                        callback.message = Language.t("成功清理日志缓存");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }
                    else
                    {
                        var callback = new Callback();
                        callback.code = -3;
                        callback.message = Language.t("权限不足");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }

                    break;
                case "server":
                    if (!wp.POST.ContainsKey("sid") || !ServerManager.servers.ContainsKey(int.Parse(wp.POST["sid"])))
                    {
                        var callback = new Callback();
                        callback.code = -1;
                        callback.message = Language.t("未找到此服务器");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        return;
                    }

                    var s_info = ServerManager.servers[int.Parse(wp.POST["sid"])];
                    if (wp.vars.user.CheckUserAbility((int) Permisson.SeeServer) || s_info.Owner == wp.vars.user.uid)
                    {
                        var callback = new ServerInfo();
                        callback.code = 9000;
                        callback.data = new ServerInfoData();
                        callback.data.id = s_info.Id;
                        callback.data.name = s_info.Name;
                        callback.data.port = s_info.Port;
                        callback.data.maxplayer = s_info.Maxplayer;
                        callback.data.ram = s_info.Ram;
                        callback.data.running = s_info.Running;
                        callback.data.expiretime = s_info.Expiretime.ToString();
                        callback.data.core = s_info.Core;
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }
                    else
                    {
                        var callback = new Callback();
                        callback.code = -3;
                        callback.message = Language.t("权限不足");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }

                    break;
                case "edit_announcement":
                    if (wp.vars.user.CheckUserAbility((int) Permisson.EditAnnouncement))
                    {
                        var c = Database.DB.CreateCommand();
                        c.CommandText = "UPDATE `settings` SET `announcement` = $announcement WHERE 1 = 1";
                        c.Parameters.AddWithValue("$announcement", wp.POST["announcement"]);
                        if (c.ExecuteNonQuery() == 0)
                        {
                            var callback = new Callback();
                            callback.code = -1;
                            callback.message = Language.t("公告编辑失败");
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                        else
                        {
                            var callback = new Callback();
                            callback.code = 9000;
                            callback.message = Language.t("公告编辑成功");
                            Settings.LoadDatabase();
                            wp.PrintWeb(JsonConvert.SerializeObject(callback));
                        }
                    }
                    else
                    {
                        var callback = new Callback();
                        callback.code = -3;
                        callback.message = Language.t("权限不足");
                        wp.PrintWeb(JsonConvert.SerializeObject(callback));
                    }

                    break;
            }
        }
    }
}