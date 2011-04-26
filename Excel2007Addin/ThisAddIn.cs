using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;
using Microsoft.Office.Tools.Excel.Extensions;
using GA_Addin.UI;
using System.Windows.Forms;
using Excel2007Addin;


namespace GA_Excel2007
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Settings settings = Settings.Default;
            if (settings.FirstStartup)
            {
                settings.Dimensions = Analytics.Data.Validation.XmlLoader.Dimensions;
                settings.Metrics = Analytics.Data.Validation.XmlLoader.Metrics;
                settings.FirstStartup = false;
                settings.Save();
            }
            Updates.CheckForUpdates();
            
            Analytics.Settings.Instance.UseProxy = settings.UseProxy;
            Analytics.Settings.Instance.ProxyAddress = settings.ProxyAddress;
            Analytics.Settings.Instance.ProxyPassword = settings.ProxyPassword;
            Analytics.Settings.Instance.ProxyPort = settings.ProxyPort;
            Analytics.Settings.Instance.ProxyUsername = settings.ProxyUsername;
            Analytics.Settings.Instance.RequestTimeout = settings.RequestTimeout;
            Analytics.Settings.Instance.MetricsXml = settings.Metrics;
            Analytics.Settings.Instance.DimensionsXml = settings.Dimensions;
            
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {

        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
