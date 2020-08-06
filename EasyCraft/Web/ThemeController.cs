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
                                pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(iflidx, elseidx - iflidx) + pagetext.Substring(endifidx + 7);
                            }
                            else
                            {//删掉逻辑判断标签
                                pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(iflidx + 1, endifidx - iflidx - 1) + pagetext.Substring(endifidx + 7);
                            }
                            continue;
                        }
                        else
                        {
                            if (elseidx <= endifidx && elseidx != -1)
                            {//有else,输出else的内容
                                pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(elseidx + 6, endifidx - elseidx - 6) + pagetext.Substring(endifidx + 7);
                            }
                            else
                            {//删掉逻辑判断标签
                                pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(endifidx + 7);
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
                    int spacestart = pagetext.IndexOf(" ", includeidx);
                    int kstart = pagetext.IndexOf("}", includeidx);
                    if (spacestart == -1)
                    {
                        spacestart = kstart;
                    }
                    int stringend = Math.Min(spacestart, kstart);
                    string comname = pagetext.Substring(includeidx + 9, stringend - 9 - includeidx);
                    Dictionary<string, string> vars = new Dictionary<string, string>();
                    if (spacestart < kstart)
                    {//有空格 有参数
                        string varsstring = pagetext.Substring(spacestart, kstart - spacestart).Trim();
                        string[] varitems = varsstring.Split(',');
                        foreach (string varitem in varitems)
                        {
                            string[] kv = varitem.Split('=');
                            if (kv[1].StartsWith("\""))
                            {//直接哦~
                                vars.Add(kv[0], kv[1].Trim('\"'));
                            }
                            else
                            {
                                vars.Add(kv[0], PhraseVarName.VarString(kv[1], wp));
                            }
                        }
                    }
                    pagetext = pagetext.Substring(0, includeidx) + PhraseComponent(comname, vars, wp) + pagetext.Substring(includelidx + 1, pagetext.Length - 1 - includelidx);
                }
                else
                {
                    break;
                }
            }
            //变量替换
            while (true)
            {
                int varidx = pagetext.IndexOf("{var.");
                if (varidx != -1)
                {
                    int varlidx = pagetext.IndexOf("}", varidx) + 1;
                    string varname = pagetext.Substring(varidx + 1, varlidx - varidx - 2);
                    string varval = PhraseVarName.VarString(varname, wp);
                    pagetext = pagetext.Substring(0, varidx) + varval + pagetext.Substring(varlidx, pagetext.Length - 1 - varlidx);
                }
                else
                {
                    break;
                }
            }
            return pagetext;
        }

        public static string PhraseComponent(string component, Dictionary<string, string> postvars, WebPanelPhraser wp)
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
                            return "<h1>The combined IF statement is not supported temporarily, please re-write. Thanks!<h1> Error throw in component: " + component;
                        }
                        else
                        {//不嵌套,万岁~
                            if (PhraseVarName.isBool(varname, wp))
                            {
                                if (elseidx <= endifidx && elseidx != -1)
                                {//有else,输出else前的内容
                                    pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(iflidx, elseidx - iflidx) + pagetext.Substring(endifidx + 7);
                                }
                                else
                                {//删掉逻辑判断标签
                                    pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(iflidx + 1, endifidx - iflidx - 1) + pagetext.Substring(endifidx + 7);
                                }
                                continue;
                            }
                            else
                            {
                                if (elseidx <= endifidx && elseidx != -1)
                                {//有else,输出else的内容
                                    pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(elseidx + 6, endifidx - elseidx - 6) + pagetext.Substring(endifidx + 7);
                                }
                                else
                                {//删掉逻辑判断标签
                                    pagetext = pagetext.Substring(0, ifidx) + pagetext.Substring(endifidx + 7);
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


                while (true)
                {
                    int includeidx = pagetext.IndexOf("{include:");
                if (includeidx != -1)
                {
                    int includelidx = pagetext.IndexOf("}", includeidx);
                    int spacestart = pagetext.IndexOf(" ", includeidx);
                    int kstart = pagetext.IndexOf("}", includeidx);
                    if (spacestart == -1)
                    {
                        spacestart = kstart;
                    }
                    int stringend = Math.Min(spacestart, kstart);
                    string comname = pagetext.Substring(includeidx + 9, stringend - 9 - includeidx);
                    Dictionary<string, string> vars = new Dictionary<string, string>();
                    if (spacestart < kstart)
                    {//有空格 有参数
                            string varsstring = pagetext.Substring(spacestart, kstart - spacestart).Trim();
                            string[] varitems = varsstring.Split(',');
                            foreach (string varitem in varitems)
                            {
                                string[] kv = varitem.Split('=');
                                if (kv[1].StartsWith("\""))
                                {//直接哦~
                                    vars.Add(kv[0], kv[1].Trim('\"'));
                                }
                                else
                                {
                                    vars.Add(kv[0], PhraseVarName.VarString(kv[1], wp));
                                }
                            }
                        }
                        pagetext = pagetext.Substring(0, includeidx) + PhraseComponent(comname, vars, wp) + pagetext.Substring(includelidx + 1, pagetext.Length - 1 - includelidx);
                    }
                    else
                    {
                        break;
                    }
                }
                //变量
                while (true)
                {
                    int varidx = pagetext.IndexOf("{var.");
                    if (varidx != -1)
                    {
                        int varlidx = pagetext.IndexOf("}", varidx) + 1;
                        string varname = pagetext.Substring(varidx + 1, varlidx - varidx - 2);
                        string varval = PhraseVarName.VarString(varname, wp, postvars);
                        pagetext = pagetext.Substring(0, varidx) + varval + pagetext.Substring(varlidx, pagetext.Length - 1 - varlidx);
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
        public static bool isBool(string varname, WebPanelPhraser wp)
        {
            bool result = false;
            bool reverse = false;
            if (varname.StartsWith('!'))
            {
                reverse = true;
                varname = varname.TrimStart('!');
            }
            try
            {
                switch (varname)
                {
                    case "var.user.login":
                        result = VarString("var.user.login", wp) == "true";
                        break;
                    default:
                        result = false;
                        break;
                }

                return reverse ? !result : result;
            }
            catch (Exception)
            {
                return reverse ? !result : result;
            }
        }

        public static string VarString(string varname, WebPanelPhraser wp, Dictionary<string, string> postvar = null)
        {
            try
            {
                switch (varname)
                {
                    case "var.user.login":
                        return wp.vars.user.islogin ? "true" : "false";
                    case "var.user.username":
                        return wp.vars.user.name;
                    case "var.user.email":
                        return wp.vars.user.email;
                    case "var.user.uid":
                        return wp.vars.user.uid.ToString();
                    default:
                        if (postvar == null || !postvar.ContainsKey(varname))
                        {
                            return "null";
                        }
                        else
                        {
                            return postvar[varname];
                        }
                }
            }
            catch (Exception)
            {
                return "null";
            }

        }
    }
}
