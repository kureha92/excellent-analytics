using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Analytics {
    public class ProxyHelper {
        public static IWebProxy GetProxy() {
            if (Settings.Instance.UseProxy)
            {
                IWebProxy proxy = new WebProxy(Settings.Instance.ProxyAddress, (int)Settings.Instance.ProxyPort);
                proxy.Credentials = new NetworkCredential(Settings.Instance.ProxyUsername, Settings.Instance.ProxyPassword);
                return proxy;
            }
            else
                return System.Net.WebRequest.DefaultWebProxy;
        }
    }
}
