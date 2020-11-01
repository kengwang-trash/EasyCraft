using EasyCraft.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace EasyCraft.Docker
{
    class Docker
    {
        public DockerConfig Config = new DockerConfig();
        public bool available = false;
        public Docker(string dockerurl = "http://localhost:2375")
        {
            Config.Uri = new Uri(dockerurl);
            Config.Url = Config.Uri.AbsoluteUri;
            available = CanReach();
            if (available)
            {
                FastConsole.PrintSuccess(string.Format( Language.t("成功连接到 Docker: {0}"),Config.Url));
            }
            else
            {
                FastConsole.PrintError(string.Format(Language.t("连接到 Docker: {0} 失败"), Config.Url));
            }
        }

        private string Request(string endpoint, RequestType requestType = RequestType.GET, Dictionary<string, string> postData = null)
        {
            HttpWebRequest request = HttpWebRequest.Create(Config.Url + endpoint) as HttpWebRequest;
            if (requestType == RequestType.GET)
            {
                request.Method = "GET";
            }
            else if (requestType == RequestType.POST)
            {
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                if (!(postData == null || postData.Count == 0))
                {
                    StringBuilder buffer = new StringBuilder();
                    int i = 0;
                    foreach (string key in postData.Keys)
                    {
                        if (i > 0)
                        {
                            buffer.AppendFormat("&{0}={1}", key, postData[key]);
                        }
                        else
                        {
                            buffer.AppendFormat("{0}={1}", key, postData[key]);
                            i++;
                        }
                    }
                    byte[] data = Encoding.ASCII.GetBytes(buffer.ToString());
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
            }
            else
            {
                //TODO HEAD PUT
                return "";
            }
            using (Stream s = request.GetResponse().GetResponseStream())
            {
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }
        public bool CanReach()
        {
            try
            {
                Structure.Info.Root responce = JsonConvert.DeserializeObject<Structure.Info.Root>(Request("/info"));
                Config.Architecture = responce.Architecture;
                Config.KernelVersion = responce.KernelVersion;
                Config.OSType = responce.OSType;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    enum RequestType
    {
        GET,
        POST
    }
}
