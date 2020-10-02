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
            themeConfig =
                Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    File.ReadAllText("panel/themes/" + themeName + "/config/config.json"));
        }

        public static void SaveThemeConfig()
        {
            File.WriteAllText("panel/themes/" + themeName + "/config/config.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(themeConfig));
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
                string pagetext =
                    File.ReadAllText("theme/" + ThemeController.themeName + "/component/" + comname + ".html");
                //string pagetext = component[comname];
                return PhraseStatment(pagetext, postvars, wp, "comp." + comname);
            }
            else
            {
                return "Theme Error: Component \"" + comname + "\" Not Found";
            }
        }

        private static string PhraseStatment(string text, Dictionary<string, string> postvars, WebPanelPhraser wp,
            string pageidf = "unknown")
        {
            string rettext = "";
            string[] lines = text.Split("\r\n");
            //If需要参数
            Dictionary<int, bool> canprint = new Dictionary<int, bool>();
            canprint[0] = true;
            bool silentplease = false; //是人为的else屏蔽
            int inifcond = 0;
            //For循环必要参数
            bool inforcond = false;
            int forline = 0;
            int foridx = 0;
            string forvarname = "";
            bool noobjtofor = false; //For中没有元素
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
                    {
                        //这行不需要编译
                        if ((inforcond && noobjtofor) || (inifcond != 0 && !canprint[inifcond])) continue;
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

                        if (((inifcond != 0 && !canprint[inifcond]) || (inforcond && noobjtofor)) &&
                            (phrasecod != "{end" && phrasecod != "{els" && phrasecod != "{bre"))
                        {
                            //假如说if不允许就真滴不允许了,后面放心大胆写
                            //假如里面有if则可能会误判else,则需要判断内部是否有if,有的话需要inifcond++=false
                            if (phrasecod == "{if:")
                            {
                                inifcond++;
                                canprint[inifcond] = false;
                                silentplease = true;
                            }

                            lastphraseidx = line.IndexOf('}', phraseidx);
                            goto linephrase;
                        }

                        if (!(inifcond != 0 && !canprint[inifcond]))
                        {
                            rettext += line.Substring(lastphraseidx, phraseidx - lastphraseidx);
                        }

                        if (phrasecod == "{end")
                        {
                            //if结束,允许print
                            inifcond--;
                            lastphraseidx = phraseidx + 7;
                            silentplease = false;
                            goto linephrase;
                        }

                        if (phrasecod == "{els")
                        {
                            if (inifcond == 0) throw new Exception("Unexpected endif without if statement start");
                            //使用父IF语句的输出状态
                            if (silentplease)
                                canprint[inifcond] = canprint[inifcond - 1];
                            else
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
                            {
                                //有空格 有参数
                                string varsstring = line.Substring(spacestart, kstart - spacestart).Trim();
                                string[] varitems = varsstring.Split(',');
                                foreach (string varitem in varitems)
                                {
                                    string[] kv = varitem.Split('=');
                                    if (kv.Length != 2) throw new Exception("Include parameter passing error");
                                    if (kv[1].StartsWith("\""))
                                    {
                                        //直接哦~
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
                                    wp.vars.for_server = (Server) forlist[++foritemid];
                                }

                                if (forvarname == "var.cores")
                                {
                                    wp.vars.for_core = (Core.Core) forlist[++foritemid];
                                }
                            }

                            lastphraseidx = forlidx;
                            goto linephrase;
                        }

                        if (phrasecod == "{bre")
                        {
                            //if结束,允许print
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
                                    wp.vars.for_server = (Server) forlist[++foritemid];
                                }

                                if (forvarname == "var.cores")
                                {
                                    wp.vars.for_core = (Core.Core) forlist[++foritemid];
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
                                if (wp.vars.user.CheckUserAbility((int)Permisson.SeeAllServer)) //可以查看全部服务器
                                {
                                    wp.vars.servers = ServerManager.servers.Values.ToList();
                                }
                                else
                                {
                                    wp.vars.servers = ServerManager.servers.Values.ToList()
                                        .Where(s => s.owner == wp.vars.user.uid).ToList();
                                }
                            }

                            if (initname == "server")
                            {
                                int sid = 0;
                                if (int.TryParse(wp.urllist[3], out sid))
                                {
                                    if (ServerManager.servers.ContainsKey(sid))
                                    {
                                        if (wp.vars.user.CheckUserAbility((int)Permisson.SeeServer) ||
                                            ServerManager.servers[sid].owner == wp.vars.user.uid)
                                        {
                                            wp.vars.server = ServerManager.servers[sid];
                                        }
                                        else
                                        {
                                            wp.Print404();
                                            return "";
                                        }
                                    }
                                    else
                                    {
                                        wp.Print404();
                                        return "";
                                    }
                                }
                                else
                                {
                                    wp.Print404();
                                    return "";
                                }
                            }

                            if (initname == "cores")
                            {
                                DirectoryInfo[] root = new DirectoryInfo("core/").GetDirectories();
                                wp.vars.cores.Clear();
                                foreach (DirectoryInfo di in root)
                                {
                                    wp.vars.cores.Add(new Core.Core(di.Name));
                                }
                            }

                            lastphraseidx = inilidx;
                            goto linephrase;
                        }
                        //////////////////////  INIT 内置变量结束   ////////////////////////


                        if (true)
                        {
                            //啥都不是
                            rettext += "{";
                            lastphraseidx = phraseidx + 1;
                            goto linephrase;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(
                        "Theme Phrase Error: at \"" + pageidf + "\" Line " + (lid + 1).ToString() + " Position: " +
                        (phraseidx + 1).ToString(), e);
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
                if (varname.Contains("="))
                {
                    string[] arr = varname.Split('=');
                    string comparestr = "";
                    string originstr = "";

                    bool isbigorsmall = false;
                    bool big = false;

                    if (arr[0].EndsWith('>'))
                    {
                        arr[0] = arr[0].TrimEnd('>');
                        isbigorsmall = true;
                        big = true;
                    }

                    if (arr[0].EndsWith('<'))
                    {
                        arr[0] = arr[0].TrimEnd('<');
                        isbigorsmall = true;
                        big = false;
                    }

                    if (arr[1].StartsWith("\""))
                    {
                        comparestr = arr[1].Trim('"');
                    }
                    else
                    {
                        comparestr = VarString(arr[1], wp, postvar);
                        if (isbigorsmall && comparestr == "null") comparestr = "0";
                    }


                    if (arr[0].StartsWith("\""))
                    {
                        originstr = arr[0].Trim('"');
                    }
                    else
                    {
                        originstr = VarString(arr[0], wp, postvar);
                        if (isbigorsmall && originstr == "null") originstr = "0";
                    }

                    if (!isbigorsmall)
                    {
                        bool res = originstr == comparestr;
                        return reverse ? !res : res;
                    }
                    else
                    {
                        int origint = 0, comint = 0;
                        int.TryParse(originstr, out origint);
                        int.TryParse(comparestr, out comint);
                        if (origint <= comint)
                        {
                            if (big)
                            {
                                return reverse ? true : false;
                            }
                            else
                            {
                                return reverse ? false : true;
                            }
                        }
                        else
                        {
                            if (!big)
                            {
                                return reverse ? true : false;
                            }
                            else
                            {
                                return reverse ? false : true;
                            }
                        }
                    }
                }
                else
                {
                    string varval = VarString(varname, wp, postvar);
                    result = !(varval == "false" || varval == "0");
                    return reverse ? !result : result;
                }
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
                    case "var.user.type":
                        return wp.vars.user.type.ToString();
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
                        return wp.vars.for_server.expiretime.ToString("yyyy-MM-dd");
                    case "var.for.server.core":
                        return wp.vars.for_server.core;

                    case "var.server.id":
                        return wp.vars.server.id.ToString();
                    case "var.server.running":
                        return wp.vars.server.running ? "true" : "false";
                    case "var.server.name":
                        return wp.vars.server.name;
                    case "var.server.owner":
                        return wp.vars.server.owner.ToString();
                    case "var.server.port":
                        return wp.vars.server.port.ToString();
                    case "var.server.maxplayer":
                        return wp.vars.server.maxplayer.ToString();
                    case "var.server.ram":
                        return wp.vars.server.ram.ToString();
                    case "var.server.expired":
                        return ((wp.vars.server.expiretime - DateTime.Now).Days < 0) ? "true" : "false";
                    case "var.server.expiretime":
                        return wp.vars.server.expiretime.ToString("yyyy-MM-dd");
                    case "var.server.core":
                        return wp.vars.server.core;
                    case "var.for.core.id":
                        return wp.vars.for_core.id;
                    case "var.for.core.name":
                        return wp.vars.for_core.name;
                    case "var.servers.count":
                        return wp.vars.servers.Count.ToString();
                    case "var.servers.running.count":
                        return wp.vars.servers.Where(s => s.running).ToList().Count.ToString();
                    case "var.servers.willexpire.count":
                        return wp.vars.servers
                            .Where(s => (s.expiretime - DateTime.Now).Days <= 3 &&
                                        (s.expiretime - DateTime.Now).Days >= 0).ToList().Count.ToString();
                    case "var.servers.expired.count":
                        return wp.vars.servers.Where(s => (s.expiretime - DateTime.Now).Days < 0).ToList().Count
                            .ToString();
                    case "var.easycraft.ftpaddr":
                        return Settings.remoteip;
                    case "var.easycraft.ftpport":
                        return Settings.ftpport.ToString();
                    case "var.easycraft.announcement":
                        return SettingsDatabase.annoucement;
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
                    return wp.vars.servers.ConvertAll(s => (object) s);
                case "var.cores":
                    return wp.vars.cores.ConvertAll(s => (object) s);
            }

            return null;
        }
    }
}