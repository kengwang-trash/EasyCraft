using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Text.Json.Serialization;

namespace EasyCraft.Web
{
    class ThemeController
    {
        static Dictionary<string, string> component = new Dictionary<string, string>();
        static Dictionary<string, string> themeConfig = new Dictionary<string, string>();
        public static string themeName = "MDefault";
        HttpListenerRequest request;


        public static void LoadComponent()
        {
            component.Clear();
            string[] files = Directory.GetFiles("panel/themes/" + themeName + "/components/", "*.html");
            foreach (string file in files)
            {
                component.Add(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file));
            }
        }

        public ThemeController(HttpListenerRequest request)
        {
            this.request = request;

        }

        public static void LoadThemeConfig()
        {
            themeConfig = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("panel/themes/" + themeName + "/config/config.json"));
        }

        public static void SaveThemeConfig()
        {
            File.WriteAllText("panel/themes/" + themeName + "/config/config.json", System.Text.Json.JsonSerializer.Serialize(themeConfig));
        }

        public static string LoadPage(string name, WebPanelPhraser wp)
        {
            string pagetext = File.ReadAllText("theme/" + ThemeController.themeName + "/page/" + name + ".html");

            //if 判断
            while (true)
            {
                int ifidx = pagetext.IndexOf("{if:");
                if (ifidx != -1)
                {
                    int iflidx = pagetext.IndexOf("}", ifidx) + 1;
                    int endifidx = pagetext.IndexOf("{endif}", iflidx);
                    int elseidx = pagetext.IndexOf("{else}", iflidx);
                    string varname = pagetext.Substring(ifidx + 4, pagetext.IndexOf("}", ifidx) - 4 - ifidx);
                    if (pagetext.IndexOf("{if:", iflidx) != -1 && endifidx > pagetext.IndexOf("{if:", iflidx))
                    {//是否为嵌套IF md还要写
                        return "<h1>The combined IF statement is not supported temporarily, please re-write. Thanks!<h1> Error throw in page: " + name;
                    }
                    else
                    {//不嵌套,万岁~
                        if (PhraseVarName.isBool(varname, wp))
                        {
                            if (elseidx <= endifidx && elseidx != -1)
                            {//有else,输出else前的内容
                                pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(iflidx, pagetext.Length - 1 - elseidx) + pagetext.Substring(endifidx + 7, pagetext.Length - 1 - endifidx - 7);
                            }
                            else
                            {//删掉逻辑判断标签
                                pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(iflidx, endifidx - iflidx) + pagetext.Substring(endifidx + 7, pagetext.Length - 1 - endifidx - 7);
                            }
                            continue;
                        }
                        else
                        {
                            if (elseidx <= endifidx && elseidx != -1)
                            {//有else,输出else的内容
                                pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(elseidx + 6, endifidx - elseidx - 6) + pagetext.Substring(endifidx + 7, pagetext.Length - 1 - endifidx - 7);
                            }
                            else
                            {//删掉逻辑判断标签
                                pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(endifidx + 7, pagetext.Length - 1 - endifidx - 7);
                            }
                            continue;
                        }
                    }
                }
                else
                {
                    break;
                }
            }


            //Include 判断
            while (true)
            {
                int includeidx = pagetext.IndexOf("{include:");
                if (includeidx != -1)
                {
                    int includelidx = pagetext.IndexOf("}", includeidx);
                    string comname = pagetext.Substring(includeidx + 9, pagetext.IndexOf(" ", includeidx) - 9 - includeidx);
                    pagetext = pagetext.Substring(0, includeidx) + PhraseComponent(comname, wp) + pagetext.Substring(includelidx + 1, pagetext.Length - 1 - includelidx);
                }
                else
                {
                    break;
                }
            }
            return pagetext;
        }

        public static string PhraseComponent(string component,/*Dictionary<string,string> vars,*/ WebPanelPhraser wp)
        {
            if (CheckComponentPath(component))
            {
                string pagetext = File.ReadAllText("theme/" + ThemeController.themeName + "/component/" + component + ".html");

                //if 判断
                while (true)
                {
                    int ifidx = pagetext.IndexOf("{if:");
                    if (ifidx != -1)
                    {
                        int iflidx = pagetext.IndexOf("}", ifidx) + 1;
                        int endifidx = pagetext.IndexOf("{endif}", iflidx);
                        int elseidx = pagetext.IndexOf("{else}", iflidx);
                        string varname = pagetext.Substring(ifidx + 4, pagetext.IndexOf("}", ifidx) - 4 - ifidx);
                        if (pagetext.IndexOf("{if:", iflidx) != -1 && endifidx > pagetext.IndexOf("{if:", iflidx))
                        {//是否为嵌套IF md还要写
                            return "<h1>The combined IF statement is not supported temporarily, please re-write. Thanks!<h1>";
                        }
                        else
                        {//不嵌套,万岁~
                            if (PhraseVarName.isBool(varname, wp))
                            {
                                if (elseidx <= endifidx && elseidx != -1)
                                {//有else,输出else前的内容
                                    pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(iflidx, pagetext.Length - 1 - elseidx) + pagetext.Substring(endifidx + 7, pagetext.Length - 1 - endifidx - 7);
                                }
                                else
                                {//删掉逻辑判断标签
                                    pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(iflidx, endifidx - iflidx) + pagetext.Substring(endifidx + 7, pagetext.Length - 1 - endifidx - 7);
                                }
                                continue;
                            }
                            else
                            {
                                if (elseidx <= endifidx && elseidx != -1)
                                {//有else,输出else的内容
                                    pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(elseidx + 6, endifidx - elseidx - 6) + pagetext.Substring(endifidx + 7, pagetext.Length - 1 - endifidx - 7);
                                }
                                else
                                {//删掉逻辑判断标签
                                    pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(endifidx + 7, pagetext.Length - 1 - endifidx - 7);
                                }
                                continue;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }


                //Include 判断
                while (true)
                {
                    int includeidx = pagetext.IndexOf("{include:");
                    if (includeidx != -1)
                    {
                        int includelidx = pagetext.IndexOf("}", includeidx);
                        string comname = pagetext.Substring(includeidx + 9, pagetext.IndexOf(" ", includeidx) - 9 - includeidx);
                        pagetext = pagetext.Substring(0, includeidx) + PhraseComponent(comname, wp) + pagetext.Substring(includelidx + 1, pagetext.Length - 1 - includelidx);
                    }
                    else
                    {
                        break;
                    }
                }
                return pagetext;

            }
            else
            {
                return "Theme Error: Component " + component + " Not Found";
            }
        }

        private static bool CheckComponentPath(string page)
        {
            if (page.Contains('.') || page.Contains('~') || page.Contains('\'') || page.Contains('\"') || page.Contains('\"'))
            {
                return false;
            }
            if (File.Exists("theme/" + ThemeController.themeName + "/component/" + page + ".html"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    class PhraseVarName
    {
        public static bool isBool(string var, WebPanelPhraser wp)
        {
            switch (var)
            {
                case "user.login":
                    return wp.user.islogin;
                    break;
                default:
                    return false;
                    break;
            }
        }
    }
}
