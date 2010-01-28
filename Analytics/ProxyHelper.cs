using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Analytics {
    public class ProxyHelper {
        public static string _proxyAddress = string.Empty;
        public static int _proxyPort;
        public static bool _isProxyAuthSet = false;
        public static string _proxyUserName = string.Empty;
        public static string _proxyUserPassword = string.Empty;

        public static IWebProxy GetProxy() {
            if (_isProxyAuthSet) {
                IWebProxy proxy = new WebProxy(string.Format("{0}:{1}",_proxyAddress, _proxyPort));
                proxy.Credentials = new NetworkCredential(_proxyUserName, _proxyUserPassword);
                return proxy;
            }
            else
                return System.Net.WebRequest.DefaultWebProxy;
        }
    }
}
