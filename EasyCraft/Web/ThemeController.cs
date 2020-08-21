using EasyCraft.Core;
using EasyCraft.Web.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
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
        static Dictionary<string, string> page = new Dictionary<string, string>();
        static Dictionary<string, string> themeConfig = new Dictionary<string, string>();
        public static string themeName = "MDefault";


        public static void InitComp()
        {
            component.Clear();
            string[] files = Directory.GetFiles("theme/" + themeName + "/component/", "*.html");
            foreach (string file in files)
            {
                component[Path.GetFileNameWithoutExtension(file)] = File.ReadAllText(file);
            }
        }

        public static void InitPage()
        {
            page.Clear();
            string[] files = Directory.GetFiles("theme/" + themeName + "/page/", "*.html");
            foreach (string file in files)
            {
                page[Path.GetFileNameWithoutExtension(file)] = File.ReadAllText(file);
            }
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
            //string pagetext = page[name];
            return PhraseStatment(pagetext, new Dictionary<string, string>(), wp, "page." + name);
        }

        public static string PhraseComponent(string comname, Dictionary<string, string> postvars, WebPanelPhraser wp)
        {
            if (CheckComponentPath(comname))
            {
                string pagetext = File.ReadAllText("theme/" + ThemeController.themeName + "/component/" + comname + ".html");
                //string pagetext = component[comname];
                return PhraseStatment(pagetext, postvars, wp, "comp." + comname);

            }
            else
            {
                return "Theme Error: Component \"" + comname + "\" Not Found";
            }
        }

        private static string PhraseStatment(string text, Dictionary<string, string> postvars, WebPanelPhraser wp, string pageidf = "unknown")
        {
            string rettext = "";
            string[] lines = text.Split("\r\n");
            //If需要参数
            Dictionary<int, bool> canprint = new Dictionary<int, bool>();
            int inifcond = 0;
            //For循环必要参数
            bool inforcond = false;
            int forline = 0;
            int foridx = 0;
            string forvarname = "";
            bool noobjtofor = false;//For中没有元素
            List<object> forlist = null;
            int foritemid = -1;

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
                        if ((inifcond != 0 && !canprint[inifcond]) || (inforcond && noobjtofor)) continue; //在if中不允许输出
                        rettext += line.Substring(lastphraseidx);
                    }
                    else
                    {
                        string phrasecod = "";
                        if (line.Length <= phraseidx + 4)
                        {
                            phrasecod = "none";
                        }
                        else
                        {
                            phrasecod = line.Substring(phraseidx, 4);
                        }

                        if (((inifcond != 0 && !canprint[inifcond]) || (inforcond && noobjtofor)) && (phrasecod != "{end" && phrasecod != "{els" && phrasecod != "{bre"))
                        {//假如说if不允许就真滴不允许了,后面放心大胆写
                            lastphraseidx = line.IndexOf('}', phraseidx);
                            goto linephrase;
                        }

                        if (!(inifcond != 0 && !canprint[inifcond]))
                        {
                            rettext += line.Substring(lastphraseidx, phraseidx - lastphraseidx);
                        }
                        if (phrasecod == "{end")
                        {//if结束,允许print
                            inifcond--;
                            lastphraseidx = phraseidx + 7;
                            goto linephrase;
                        }
                        if (phrasecod == "{els")
                        {
                            if (inifcond == 0) throw new Exception("Unexpected endif without if statement start");
                            canprint[inifcond] = !canprint[inifcond];
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
                            inifcond++;
                            int iflidx = line.IndexOf("}", phraseidx);
                            string varname = line.Substring(phraseidx + 4, iflidx - 4 - phraseidx);
                            if (PhraseVarName.isBool(varname, wp, postvars))
                            {
                                canprint[inifcond] = true;
                            }
                            else
                            {
                                canprint[inifcond] = false;
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


                        //////////////////////  FOREACH 循环开始   ////////////////////////
                        if (phrasecod == "{for")
                        {
                            int forlidx = line.IndexOf("}", phraseidx) + 1;
                            forvarname = line.Substring(phraseidx + 9, forlidx - phraseidx - 10);
                            inforcond = true;
                            forline = lid;
                            foridx = forlidx;
                            forlist = PhraseVarName.ForList(forvarname, wp);
                            if (forlist == null || forlist.Count <= 0)
                            {
                                noobjtofor = true;
                            }
                            else
                            {
                                if (forvarname == "var.servers")
                                {
                                    wp.vars.for_server = (Server)forlist[++foritemid];
                                }
                            }

                            lastphraseidx = forlidx;
                            goto linephrase;
                        }

                        if (phrasecod == "{bre")
                        {//if结束,允许print
                            if (forlist == null || forlist.Count <= foritemid + 1)
                            {
                                //跳出For循环
                                forlist = null;
                                inforcond = false;
                                noobjtofor = false;
                                foritemid = -1;
                                lastphraseidx = phraseidx + 7;
                                goto linephrase;
                            }
                            else
                            {
                                if (forvarname == "var.servers")
                                {
                                    wp.vars.for_server = (Server)forlist[++foritemid];
                                }
                                inforcond = true;
                                lid = forline;
                                line = lines[lid];
                                lastphraseidx = foridx;
                                goto linephrase;
                            }

                        }

                        //////////////////////  INIT 内置变量加载   ////////////////////////

                        if (phrasecod == "{ini")
                        {
                            int inilidx = line.IndexOf("}", phraseidx) + 1;
                            string initname = line.Substring(phraseidx + 6, inilidx - phraseidx - 7);
                            if (initname == "servers")
                            {
                                if (wp.vars.user.type >= (int)UserType.customer)//可以查看全部服务器
                                {
                                    wp.vars.servers = ServerManager.servers.Values.ToList();
                                }
                                else
                                {
                                    wp.vars.servers = ServerManager.servers.Values.ToList().Where(s => s.owner == wp.vars.user.uid).ToList();

                                }
                            }
                            lastphraseidx = inilidx;
                            goto linephrase;
                        }


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
            /*
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
            }*/
            return component.ContainsKey(page);
        }

        public static bool CheckPagePath(string pagename)
        {
            /*
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
            }*/
            return page.ContainsKey(pagename);
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
                string varval = VarString(varname, wp, postvar);
                result = !(varval == "false" || varval == "0");
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
                    case "var.for.server.id":
                        return wp.vars.for_server.id.ToString();
                    case "var.for.server.running":
                        return wp.vars.for_server.running ? "true" : "false";
                    case "var.for.server.name":
                        return wp.vars.for_server.name;
                    case "var.for.server.owner":
                        return wp.vars.for_server.owner.ToString();
                    case "var.for.server.port":
                        return wp.vars.for_server.port.ToString();
                    case "var.for.server.maxplayer":
                        return wp.vars.for_server.maxplayer.ToString();
                    case "var.for.server.ram":
                        return wp.vars.for_server.ram.ToString();
                    case "var.for.server.expired":
                        return ((wp.vars.for_server.expiretime - DateTime.Now).Days < 0) ? "true" : "false";
                    case "var.for.server.expiretime":
                        return wp.vars.for_server.expiretime.ToString();
                    case "var.servers.count":
                        return wp.vars.servers.Count.ToString();
                    case "var.servers.running.count":
                        return wp.vars.servers.Where(s => s.running).ToList().Count.ToString();
                    case "var.servers.willexpire.count":
                        return wp.vars.servers.Where(s => (s.expiretime - DateTime.Now).Days <= 3 && (s.expiretime - DateTime.Now).Days >= 0).ToList().Count.ToString();
                    case "var.servers.expired.count":
                        return wp.vars.servers.Where(s => (s.expiretime - DateTime.Now).Days < 0).ToList().Count.ToString();
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


        public static List<object> ForList(string varname, WebPanelPhraser wp)
        {
            switch (varname)
            {
                case "var.servers":
                    return wp.vars.servers.ConvertAll(s => (object)s);
            }
            return null;
        }
    }
}
