using EasyCraft.Web.Classes;
using EasyCraft.Web.JSONCallback;
using System;
using System.Collections.Generic;
using System.Text;

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
                        callback.message = "登录失败,尝试次数过多";
                        callback.code = -2;
                        wp.PrintWeb(System.Text.Json.JsonSerializer.Serialize(callback));
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
                            callback.message = "成功登陆";
                            callback.data = new LoginUserInfo();
                            callback.data.username = wp.vars.user.name;
                            callback.data.uid = wp.vars.user.uid;
                            callback.data.email = wp.vars.user.email;
                            wp.PrintWeb(System.Text.Json.JsonSerializer.Serialize(callback));
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
                            callback.message = "登录失败,账号或密码错误";
                            callback.code = -1;
                            wp.PrintWeb(System.Text.Json.JsonSerializer.Serialize(callback));
                        }
                    }
                    else
                    {
                        UserLogin callback = new UserLogin();
                        callback.message = "登录失败,参数不完整";
                        callback.code = -3;
                        wp.PrintWeb(System.Text.Json.JsonSerializer.Serialize(callback));
                    }
                    break;
                case "register":
                    if (wp.POST.ContainsKey("username") && wp.POST.ContainsKey("password"))
                    {

                    }
                    else
                    {
                        Register callback = new Register();
                        callback.code = -3;
                        callback.message = "注册失败,参数不全";
                        wp.PrintWeb(System.Text.Json.JsonSerializer.Serialize(callback));
                    }
                    break;
            }
        }
    }
}
