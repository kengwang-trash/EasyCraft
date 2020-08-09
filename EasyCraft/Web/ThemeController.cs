using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection.Emit;
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
                component[Path.GetFileNameWithoutExtension(file)] = File.ReadAllText(file);
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
            return PhraseStatment(pagetext, new Dictionary<string, string>(), wp, "page." + name);
        }

        public static string PhraseComponent(string component, Dictionary<string, string> postvars, WebPanelPhraser wp)
        {
            if (CheckComponentPath(component))
            {
                string pagetext = File.ReadAllText("theme/" + ThemeController.themeName + "/component/" + component + ".html");
                return PhraseStatment(pagetext, postvars, wp, "comp." + component);

            }
            else
            {
                return "Theme Error: Component " + component + " Not Found";
            }
        }

        private static string PhraseStatment(string text, Dictionary<string, string> postvars, WebPanelPhraser wp, string pageidf = "unknown")
        {
            string rettext = "";
            string[] lines = text.Split("\r\n");
            bool canprint = true;
            bool inifcond = false;
            for (int lid = 0; lid < lines.Length; lid++)
            {
                rettext += "\r\n";
                string line = lines[lid];
                int phraseidx = 0;
                int lastphraseidx = 0;
                goto linephrase;
            linephrase:
                try
                {
                    phraseidx = line.IndexOf("{", lastphraseidx);
                    if (phraseidx == -1)
                    {//这行不需要编译
                        if (inifcond && !canprint) continue; //在if中不允许输出
                        rettext += line.Substring(lastphraseidx);
                    }
                    else
                    {
                        string phrasecod = line.Substring(phraseidx, 4);
                        if ((inifcond && !canprint) && (phrasecod != "{end" && phrasecod != "{els"))
                        {//假如说if不允许就真滴不允许了,后面放心大胆写
                            lastphraseidx = line.IndexOf('}', phraseidx);
                            goto linephrase;
                        }

                        if (!(inifcond && !canprint))
                        {
                            rettext += line.Substring(lastphraseidx, phraseidx - lastphraseidx);
                        }
                        if (phrasecod == "{end")
                        {//if结束,允许print
                            inifcond = false;
                            canprint = true;
                            lastphraseidx = phraseidx + 7;
                            goto linephrase;
                        }
                        if (phrasecod == "{els")
                        {
                            if (!inifcond) throw new Exception("Unexpected endif without if statement start");
                            canprint = !canprint;
                            lastphraseidx = phraseidx + 6;
                            goto linephrase;
                        }
                        //正式开始啦~ ^_^
                        //////////////////////// INCLUDE 语法开始 /////////////////////////
                        if (phrasecod == "{inc")
                        {
                            int includelidx = line.IndexOf("}", phraseidx);
                            int spacestart = line.IndexOf(" ", phraseidx);
                            int kstart = line.IndexOf("}", phraseidx);
                            if (spacestart == -1)
                            {
                                spacestart = kstart;
                            }
                            int stringend = Math.Min(spacestart, kstart);
                            string comname = line.Substring(phraseidx + 9, stringend - 9 - phraseidx);
                            Dictionary<string, string> vars = new Dictionary<string, string>();
                            if (spacestart < kstart)
                            {//有空格 有参数
                                string varsstring = line.Substring(spacestart, kstart - spacestart).Trim();
                                string[] varitems = varsstring.Split(',');
                                foreach (string varitem in varitems)
                                {
                                    string[] kv = varitem.Split('=');
                                    if (kv.Length != 2) throw new Exception("Include parameter passing error");
                                    if (kv[1].StartsWith("\""))
                                    {//直接哦~
                                        vars[kv[0]] = kv[1].Trim('\"');
                                    }
                                    else
                                    {
                                        vars[kv[0]] = PhraseVarName.VarString(kv[1], wp);
                                    }
                                }
                            }
                            rettext += PhraseComponent(comname, vars, wp);
                            lastphraseidx = kstart + 1;
                            goto linephrase;
                        }
                        //////////////////////// INCLUDE 语法结束 /////////////////////////

                        ///////////////////////   IF    语法开始  ////////////////////////

                        if (phrasecod == "{if:")
                        {
                            if (inifcond) throw new Exception("Currently does not support nested IF statements");
                            inifcond = true;
                            int iflidx = line.IndexOf("}", phraseidx);
                            string varname = line.Substring(phraseidx + 4, iflidx - 4 - phraseidx);
                            if (PhraseVarName.isBool(varname, wp, postvars))
                            {
                                canprint = true;
                            }
                            else
                            {
                                canprint = false;
                            }
                            lastphraseidx = iflidx + 1;
                            goto linephrase;
                        }
                        ///////////////////////   IF    语法结束  ////////////////////////

                        /////////////////////  VAR 变量输出语法开始 ////////////////////////
                        if (phrasecod == "{var")
                        {
                            int varlidx = line.IndexOf("}", phraseidx) + 1;
                            string varname = line.Substring(phraseidx + 1, varlidx - phraseidx - 2);
                            string varval = PhraseVarName.VarString(varname, wp, postvars);
                            rettext += varval;
                            lastphraseidx = varlidx;
                            goto linephrase;
                        }
                        /////////////////////  VAR 变量输出语法结束 ////////////////////////

                        //////////////////////  SET 定义变量开始   /////////////////////////
                        if (phrasecod == "{set")
                        {
                            int eqidx = line.IndexOf("=", phraseidx);
                            int setlidx = line.IndexOf("}", phraseidx) + 1;
                            string varname = line.Substring(phraseidx + 5, eqidx - phraseidx - 5).Trim(' ');
                            string varval = line.Substring(eqidx + 1, setlidx - eqidx - 2).Trim(' ');
                            if (varval.StartsWith("\""))
                            {
                                postvars[varname] = varval.Trim('"');
                            }
                            else
                            {
                                postvars[varname] = PhraseVarName.VarString(varval, wp, postvars);
                            }
                            lastphraseidx = setlidx;
                            goto linephrase;
                        }
                        //////////////////////  SET 定义变量结束   /////////////////////////

                        if (true)
                        {//啥都不是
                            rettext += "{";
                            lastphraseidx = phraseidx + 1;
                            goto linephrase;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Theme Phrase Error: at \"" + pageidf + "\" Line " + (lid + 1).ToString() + " Position: " + (phraseidx + 1).ToString(), e);
                }

            }

            return rettext;
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
        public static bool isBool(string varname, WebPanelPhraser wp, Dictionary<string, string> postvar = null)
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
                result = VarString(varname, wp, postvar) == "true";
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
                    case "var.user.qq":
                        return wp.vars.user.qq;
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
