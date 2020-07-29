using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;

namespace EasyCraft.Web
{
    class ThemeController
    {
        static Dictionary<string, string> component = new Dictionary<string, string>();
        static Dictionary<string, string> themeConfig = new Dictionary<string, string>();
        static string themeName = "Default";
        HttpListenerRequest request;
        Dictionary<string, string> cookies = new Dictionary<string, string>();
        static Dictionary<string, Dictionary<string, string>> MultiSessions;
        Dictionary<string, string> session = new Dictionary<string, string>();

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
            foreach (Cookie cookie in request.Cookies)
            {
                if (!cookie.Expired)
                {
                    cookies.Add(cookie.Name, cookie.Value);
                }
            }
            if (cookies["SESSDATA"] == "")
            {

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

        public string LoadPage(string name)
        {
            return "";

        }

        public string PhraseComponent(string component)
        {
            return "";
        }
    }
}
