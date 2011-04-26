using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Analytics
{

    public class HttpRequestFactory 
    {        
        private static HttpRequestFactory _instance;
        public static HttpRequestFactory Instance
        {
            get {
                return _instance == null ? (_instance = new HttpRequestFactory()) : _instance;
            }
        }

        private HttpRequestFactory() { }

        public System.Net.WebRequest CreateRequest(string uri)
        {
            System.Net.WebRequest request = System.Net.HttpWebRequest.Create(uri);
            request.Proxy = ProxyHelper.GetProxy();
            request.Timeout = (int)(Settings.Instance.RequestTimeout * 1000);
            return request;
        }
    }
}
