using EasyCraft.Core;
using EasyCraft.Web.Classes;
using EasyCraft.Web.JSONCallback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static Dictionary<string, Dictionary<string, string>> MultiSessions =
            new Dictionary<string, Dictionary<string, string>>();

        public Dictionary<string, string> session = new Dictionary<string, string>();
        public HTTPVars vars = new HTTPVars();
        public Uri uri = null;
        public Dictionary<string, string> GET = new Dictionary<string, string>();
        public Dictionary<string, string> POST = new Dictionary<string, string>();
        public List<string> urllist = new List<string>();

        public void PhraseWeb(HttpListenerRequest req, HttpListenerResponse res)
        {
            request = req;
            response = res;
            response.ContentType = "charset=utf-8;";
            foreach (Cookie cookie in request.Cookies)
            {
                if (!cookie.Expired)
                {
                    cookies[cookie.Name] = cookie.Value;
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
                    HttpOnly = true,
                    Expires = DateTime.Now.AddDays(3),
                    Path = "/",
                    Expired = false
                };
                response.AppendCookie(sesscookie);
                cookies["SESSDATA"] = sesscookie.Value;
                session = new Dictionary<string, string>();
            }

            if (request.HttpMethod == "POST" && request.ContentType != null &&
                request.ContentType.Contains("application/x-www-form-urlencoded"))
            {
                StreamReader sr = new StreamReader(request.InputStream);
                string postraw = sr.ReadToEnd();
                //FastConsole.PrintTrash("[POSTDATA]" + postraw);
                POST = PhrasePOST(postraw);
            }

            uri = request.Url;
            urllist = uri.AbsolutePath.Split('/').ToList();
            FastConsole.PrintTrash("[" + request.HttpMethod + "]:" + uri.AbsolutePath);

            if (session.ContainsKey("auth"))
            {
                vars.user = new User(session["auth"]);
            }

            if (uri.AbsolutePath.StartsWith("/api/"))
            {
                string absolutepage = uri.AbsolutePath.Substring(5);
                response.ContentType = "application/json";
                Api.PhraseAPI(absolutepage, this);
            }
            else if (uri.AbsolutePath.StartsWith("/page/"))
            {
                response.ContentType = "text/html;charset=utf-8;";
                if (uri.AbsolutePath == "/page/logout")
                {
                    session["auth"] = "";
                    response.StatusCode = 302;
                    response.Headers.Add("Location: /page/login");
                }
                else if (vars.user.islogin || uri.AbsolutePath == "/page/login" || uri.AbsolutePath == "/page/register")
                {
                    if (urllist.Count < 2)
                    {
                        Print404();
                    }

                    if (ThemeController.CheckPagePath(urllist[2]))
                    {
                        PrintWeb(ThemeController.LoadPage(urllist[2], this));
                    }
                    else
                    {
                        Print404();
                    }
                }
                else
                {
                    response.StatusCode = 302;
                    response.Headers.Add("Location: /page/login");
                }
            }
            else if (uri.AbsolutePath == "/")
            {
                response.StatusCode = 302;
                response.Headers.Add("Location: /page/index");
            }
            else if (uri.AbsolutePath.StartsWith("/assets/"))
            {
                string absolutepage = uri.AbsolutePath.Substring(8);
                if (CheckAssetsPath(absolutepage))
                {
                    response.Headers.Add("Cache-Control:max-age=259200"); //设置为三天的缓存
                    if (absolutepage.EndsWith("js"))
                    {
                        response.ContentType = "text/javascript";
                    }
                    else if (absolutepage.EndsWith("css"))
                    {
                        response.ContentType = "text/css";
                    }

                    PrintWeb(File.ReadAllBytes("data/theme/" + ThemeController.themeName + "/assets/" + absolutepage));
                }
                else
                {
                    Print404();
                }
            }

            try
            {
                if (cookies["SESSDATA"] != null && session != null && MultiSessions != null && cookies != null)
                    MultiSessions[cookies["SESSDATA"]] = session;
                response.Close();
            }
            catch (Exception)
            {
                //潜在BUG,可能某些情况不会写session
            }
        }


        private bool CheckAssetsPath(string page)
        {
            if (page.Contains("..") || page.Contains('~') || page.Contains('\'') || page.Contains('\"') ||
                page.Contains('\"'))
            {
                return false;
            }

            if (File.Exists("data/theme/" + ThemeController.themeName + "/assets/" + page))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Print404()
        {
            response.StatusCode = 404;
            PrintWeb("<h1>404 Not Found</h1><hr />Path: " + uri.AbsoluteUri + "<br />Time:" + DateTime.Now.ToString() +
                     "<br />Server: EasyCraft<br />");
        }

        public void PrintWeb(string echo)
        {
            try
            {
                byte[] buff = Encoding.UTF8.GetBytes(echo);
                response.OutputStream.Write(buff, 0, buff.Length);
            }
            catch (Exception)
            {
            }
        }

        public void PrintWeb(byte[] echo)
        {
            response.OutputStream.Write(echo, 0, echo.Length);
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
                ans[key] = value;
            }

            return ans;
        }
    }
}