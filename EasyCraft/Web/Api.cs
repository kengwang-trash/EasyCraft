using EasyCraft.Core;
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
                        callback.message = Language.t("Login failed, too many attempts!");
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
                            callback.message = Language.t("Login Successful");
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
                            callback.message = Language.t("Login failed, account or password is wrong");
                            callback.code = -1;
                            wp.PrintWeb(System.Text.Json.JsonSerializer.Serialize(callback));
                        }
                    }
                    else
                    {
                        UserLogin callback = new UserLogin();
                        callback.message = Language.t("Login failed, parameters are incomplete!");
                        callback.code = -3;
                        wp.PrintWeb(System.Text.Json.JsonSerializer.Serialize(callback));
                    }
                    break;
                case "register":
                    if (wp.POST.ContainsKey("username") && wp.POST.ContainsKey("password") && wp.POST.ContainsKey("email"))
                    {
                        if (User.Exist(wp.POST["username"]))
                        {
                            Register callback = new Register();
                            callback.code = -2;
                            callback.message = Language.t("Registration failed, username already exists");
                            wp.PrintWeb(System.Text.Json.JsonSerializer.Serialize(callback));
                        }
                        else
                        {
                            User user = User.Register(wp.POST["username"], wp.POST["password"], wp.POST["email"], wp.POST["qq"]);
                            if (user == null)
                            {
                                Register callback = new Register();
                                callback.code = -2;
                                callback.message = Language.t("Registration failed");
                                wp.PrintWeb(System.Text.Json.JsonSerializer.Serialize(callback));
                            }
                            else
                            {
                                Register callback = new Register();
                                callback.code = 9000;
                                callback.message = Language.t("Registration Success");
                                wp.PrintWeb(System.Text.Json.JsonSerializer.Serialize(callback));
                            }
                        }
                    }
                    else
                    {
                        Register callback = new Register();
                        callback.code = -3;
                        callback.message = Language.t("Registration failed with incomplete parameters");
                        wp.PrintWeb(System.Text.Json.JsonSerializer.Serialize(callback));
                    }
                    break;
            }
        }
    }
}
