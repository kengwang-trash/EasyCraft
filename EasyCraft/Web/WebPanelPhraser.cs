using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasyCraft.Web
{
    class WebPanelPhraser
    {
        Dictionary<string, Cookie> cookiedic = new Dictionary<string, Cookie>();
        HttpListenerRequest request;
        HttpListenerResponse response;
        
        public void PhraseWeb(HttpListenerRequest req,HttpListenerResponse res)
        {
            request = req;
            response = res;
            foreach (Cookie cookie in request.Cookies)
            {
                cookiedic.Add(cookie.Name, cookie);
            }
            
        }
    }
}
