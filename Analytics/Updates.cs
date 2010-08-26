using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Analytics;
using System.IO;
using System.Windows.Forms;


namespace GA_Addin.UI
{
    /*@author Daniel Sandberg
     * This class compares the version number of the installation the user has with the version number 
     * of the latest upload of Excellent Analytics.
     */
    public class Updates
    {
        public bool CheckForUpdates()
        {
            string result = null;
            string url = "http://excellentanalytics.com/about/google-analytics-data-in-excel/download/";
            WebResponse response = null;
            StreamReader reader = null;
            string version = "";
            bool updateAvail = false;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                // handle error
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (response != null)
                    response.Close();
                if (result.Contains("Version:"))
                {
                    int position = result.IndexOf("Version:", 0);
                    version = result.Substring(position + 9, 9);
                    version = version.Trim();
                }

            }

            // The version number is only displayed in the GUI in the published application.
            // When debugging in a test environment the version number is not displayed.
            string currentVersion = "";
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                System.Deployment.Application.ApplicationDeployment ad =
                System.Deployment.Application.ApplicationDeployment.CurrentDeployment;
                currentVersion = ad.CurrentVersion.ToString();
            }

            if (version != currentVersion)
            {
                updateAvail = true;
            }
            else 
            {
                updateAvail = false;
            }
            
            return updateAvail;
        }
    }
}
