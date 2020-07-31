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
