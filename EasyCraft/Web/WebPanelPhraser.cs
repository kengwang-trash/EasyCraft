using EasyCraft.Core;
using EasyCraft.Web.Classes;
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
        Dictionary<string, Cookie> cookiedic = new Dictionary<string, Cookie>();
        HttpListenerRequest request;
        HttpListenerResponse response;
        Dictionary<string, string> cookies = new Dictionary<string, string>();
        static Dictionary<string, Dictionary<string, string>> MultiSessions = new Dictionary<string, Dictionary<string, string>>();
        Dictionary<string, string> session = new Dictionary<string, string>();
        Uri uri = null;
        bool islogin = false;
        User user;
        Dictionary<string, string> GET = new Dictionary<string, string>();
        Dictionary<string, string> POST = new Dictionary<string, string>();

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
                session = new Dictionary<string, string>();
            }
            if (request.HttpMethod == "POST" && request.ContentType == "application/x-www-form-urlencoded")
            {
                StreamReader sr = new StreamReader(request.InputStream);
                string postraw = sr.ReadToEnd();
                //FastConsole.PrintTrash("[POSTDATA]" + postraw);
                POST=PhrasePOST(postraw);
            }

            uri = request.Url;
            FastConsole.PrintTrash("[" + request.HttpMethod + "]:" + uri.AbsolutePath);
            if (session.ContainsKey("auth"))
            {
                user = new User(session["auth"]);
            }

            if (uri.AbsolutePath == "/api")
            {
                if (req.QueryString["fun"] == "login")
                {
                    if (POST.ContainsKey("username") && POST.ContainsKey("password"))
                    {
                        user = new User(POST["username"], POST["password"]);
                    }
                }
            }
            response.Close();
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
