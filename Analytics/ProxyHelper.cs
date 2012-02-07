using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Analytics {
    public class ProxyHelper 
    {
        public static IWebProxy GetProxy() 
        {
            if (Settings.Instance.UseProxy)
            {
                IWebProxy proxy = new WebProxy(Settings.Instance.ProxyAddress, (int)Settings.Instance.ProxyPort);
                string password = DataProtectionHelper.UnProtect(Settings.Instance.ProxyPassword);
                proxy.Credentials = new NetworkCredential(Settings.Instance.ProxyUsername, password);
                return proxy;
            }
            else
                return System.Net.WebRequest.DefaultWebProxy;
        }
    }
}
