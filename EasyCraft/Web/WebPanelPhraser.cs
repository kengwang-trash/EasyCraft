using EasyCraft.Core;
using EasyCraft.Web.Classes;
using EasyCraft.Web.JSONCallback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyCraft.Web
{
    class WebPanelPhraser
    {
        public Dictionary<string, Cookie> cookiedic = new Dictionary<string, Cookie>();
        public HttpListenerRequest request;
        public HttpListenerResponse response;
        public Dictionary<string, string> cookies = new Dictionary<string, string>();
        public static Dictionary<string, Dictionary<string, string>> MultiSessions = new Dictionary<string, Dictionary<string, string>>();
        public Dictionary<string, string> session = new Dictionary<string, string>();
        public Uri uri = null;
        public User user;
        public Dictionary<string, string> GET = new Dictionary<string, string>();
        public Dictionary<string, string> POST = new Dictionary<string, string>();

        public void PhraseWeb(HttpListenerRequest req, HttpListenerResponse res)
        {
            request = req;
            response = res;
            foreach (Cookie cookie in request.Cookies)
            {
                if (!cookie.Expired)
                {
                    cookies.Add(cookie.Name, cookie.Value);
                }
            }
            if (cookies.ContainsKey("SESSDATA"))
            {
                if (MultiSessions.ContainsKey(cookies["SESSDATA"]))
                {
                    session = MultiSessions[cookies["SESSDATA"]];
                }
                else
                {
                    MultiSessions[cookies["SESSDATA"]] = new Dictionary<string, string>();
                    session = new Dictionary<string, string>();
                }
            }
            else
            {
                Cookie sesscookie = new Cookie
                {
                    Name = "SESSDATA",
                    Value = Functions.GetRandomString(20),
                    HttpOnly = true
                };
                response.AppendCookie(sesscookie);
                cookies.Add("SESSDATA", sesscookie.Value);
                session = new Dictionary<string, string>();
            }
            if (request.HttpMethod == "POST" && request.ContentType == "application/x-www-form-urlencoded")
            {
                StreamReader sr = new StreamReader(request.InputStream);
                string postraw = sr.ReadToEnd();
                //FastConsole.PrintTrash("[POSTDATA]" + postraw);
                POST = PhrasePOST(postraw);
            }

            uri = request.Url;
            FastConsole.PrintTrash("[" + request.HttpMethod + "]:" + uri.AbsolutePath);
            if (session.ContainsKey("auth"))
            {
                user = new User(session["auth"]);
            }

            if (uri.AbsolutePath == "/api/login")
            {
                if (POST.ContainsKey("username") && POST.ContainsKey("password"))
                {
                    user = new User(POST["username"], POST["password"]);
                    if (user.islogin)
                    {
                        session["auth"] = user.auth;
                        UserLogin callback = new UserLogin();
                        callback.code = 0;
                        callback.message = "成功登陆";
                        callback.data = new LoginUserInfo();
                        callback.data.username = user.name;
                        callback.data.uid = user.uid;
                        callback.data.email = user.email;
                        PrintWeb(System.Text.Json.JsonSerializer.Serialize(callback));
                    }
                    else
                    {
                        UserLogin callback = new UserLogin();
                        callback.code = -1;
                    }

                }
            }
            else if (uri.AbsolutePath.StartsWith("/page/"))
            {
                string absolutepage = uri.AbsolutePath.Substring(6);
                if (CheckPagePath(absolutepage))
                {                    
                    PrintWeb(ThemeController.LoadPage(absolutepage, this));
                }
                else
                {
                    Print404();
                }
            }
            if (!MultiSessions.ContainsKey(cookies["SESSDATA"]))
                MultiSessions.Add(cookies["SESSDATA"], session);
            else
                MultiSessions[cookies["SESSDATA"]] = session;
            response.Close();
        }

        private bool CheckPagePath(string page)
        {
            if (page.Contains('.') || page.Contains('~') || page.Contains('\'') || page.Contains('\"') || page.Contains('\"'))
            {
                return false;
            }
            if (File.Exists("theme/" + ThemeController.themeName + "/page/" + page + ".html"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Print404()
        {
            response.StatusCode = 404;
            PrintWeb("<h1>404 Not Found</h1><hr />Path: " + uri.AbsoluteUri + "<br />Time:" + DateTime.Now.ToString() + "<br />Server: EasyCraft<br />");
        }

        private void PrintWeb(string echo)
        {
            byte[] buff = Encoding.UTF8.GetBytes(echo);
            response.OutputStream.Write(buff, 0, buff.Length);
        }

        private static Dictionary<string, string> PhrasePOST(string postraw)
        {
            Dictionary<string, string> ans = new Dictionary<string, string>();
            if (postraw.Length == 0) return ans;
            string[] ps = postraw.Split('&');
            foreach (string pd in ps)
            {
                string key = Uri.UnescapeDataString(pd.Substring(0, pd.IndexOf("=")));
                string value = Uri.UnescapeDataString(pd.Substring(pd.IndexOf("=") + 1));
                ans.Add(key, value);
            }
            return ans;
        }
    }
}
