using EasyCraft.Core;
using EasyCraft.Docker.Structure;
using EasyCraft.Docker.Structure.Containers;
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
                FastConsole.PrintSuccess(string.Format(Language.t("成功连接到 Docker: {0}"), Config.Url));
            }
            else
            {
                FastConsole.PrintError(string.Format(Language.t("连接到 Docker: {0} 失败"), Config.Url));
            }
        }

        private string ResponsetToString(HttpWebResponse response)
        {
            using (Stream s = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        private HttpWebResponse Request(string endpoint, RequestType requestType = RequestType.GET, Dictionary<string, string> postData = null)
        {
            HttpWebRequest request = HttpWebRequest.Create(Config.Url + endpoint) as HttpWebRequest;
            StringBuilder buffer = new StringBuilder();

            if (!(postData == null || postData.Count == 0))
            {
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
            }

            if (requestType == RequestType.GET)
            {
                if (!(postData == null || postData.Count == 0))
                    request = HttpWebRequest.Create(Config.Url + endpoint + "?" + buffer.ToString()) as HttpWebRequest;
                request.Method = "GET";

            }
            else if (requestType == RequestType.POST)
            {
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                if (!(postData == null || postData.Count == 0))
                {
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
                return null;
            }
            return request.GetResponse() as HttpWebResponse;
        }

        /// <summary>
        /// 是否连接上 Docker
        /// </summary>
        /// <returns>true/false</returns>
        public bool CanReach()
        {
            try
            {
                Structure.Info.Root responce = JsonConvert.DeserializeObject<Structure.Info.Root>(ResponsetToString(Request("/info")));
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

        /// <summary>
        /// 列出 Docker 中的容器
        /// </summary>
        /// <param name="all">返回所有容器。 默认情况下，仅显示正在运行的容器。 [false]</param>
        /// <param name="limit">返回此数量的最近创建的容器，包括未运行的容器。</param>
        /// <param name="size">返回容器的大小作为字段SizeRw和SizeRootFs [false]</param>
        /// <param name="filters">过滤以对容器列表进行处理，编码为JSON（map [string] [] string）。 例如，{"status":["paused"]}将仅返回已暂停的容器。</param>
        /// <returns></returns>
        public List<Container> ListContainer(bool all = false, int limit = -1, bool size = false, string filters = "")
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param["all"] = all ? "true" : "false";
                if (limit != -1) param["limit"] = limit.ToString();
                if (size != false) param["size"] = size.ToString().ToLower();
                if (!string.IsNullOrEmpty(filters)) param["filters"] = filters;
                HttpWebResponse wr = Request("/containers/json", RequestType.GET, param);
                if (wr.StatusCode != HttpStatusCode.OK) return new List<Container>();
                return JsonConvert.DeserializeObject<List<Container>>(ResponsetToString(wr));

            }
            catch (Exception)
            {
                return new List<Container>();
            }
        }
    }

    enum RequestType
    {
        GET,
        POST
    }
}
