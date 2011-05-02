using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Analytics;
using System.IO;
using System.Windows.Forms;


namespace Excel2007Addin
{
	internal class VersionInformation
	{
        private int major = 0;
        private int minor = 0;
        private int build = 0;
        private int rev = 0;
        public string Version
        {
            get {
                return string.Format("{0}.{1}.{2}.{3}", major, minor, build, rev);   
            }
            set
            {
                string[] parts = value.Split(new char[] { '.' });
                if (parts.Length > 0)
                    major = int.Parse(parts[0]);
                if (parts.Length > 1)
                    minor = int.Parse(parts[1]);
                if (parts.Length > 2)
                    build = int.Parse(parts[2]);
                if (parts.Length > 3)
                    rev = int.Parse(parts[3]);
            }
        }
		public string Url { get; set; }
        public override string ToString()
        {
            return Version;
        }
    }

    

	/*@author Daniel Sandberg
     * This class compares the version number of the installation the user has with the version number 
     * of the latest upload of Excellent Analytics.
     */
	public class Updates
	{
        public static void CheckForUpdates()
        {
            Updates checkUp = new Updates();
            if (Excel2007Addin.Settings.Default.RemingMeAgainIn < DateTime.Now && checkUp.NewVersionExists())
            {
                WPFUIv2.UpdateDialog upDialog = new WPFUIv2.UpdateDialog();
                if (upDialog.ShowDialog().GetValueOrDefault(false))
                {
                    System.Diagnostics.Process.Start(checkUp.UpdateUrl);
                }
                else
                {
                    Excel2007Addin.Settings.Default.RemingMeAgainIn = upDialog.GetRemindMeAgainDate();
                    Excel2007Addin.Settings.Default.Save();
                }
            }
        }

		private VersionInformation verinfo;
		public bool NewVersionExists()
		{
			string result = null;
            VersionInformation currentVersion = new VersionInformation() { Version = System.Reflection.AssemblyName.GetAssemblyName(System.Reflection.Assembly.GetExecutingAssembly().Location).Version.ToString() };
            string url = "http://version.excellentanalytics.com/?ver=" + currentVersion;
            
			WebResponse response = null;
			StreamReader reader = null;

			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
				request.Method = "GET";
				response = request.GetResponse();
				reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
				result = reader.ReadToEnd();
			}
			catch (Exception)
			{
				// handle error
				MessageBox.Show("Error fetching version information.", "Version check failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (reader != null)
                    reader.Close();
                if (response != null)
                    response.Close();
				return false;
			}

			if (reader != null)
				reader.Close();
			if (response != null)
				response.Close();

			try
			{
				System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
				verinfo = jss.Deserialize<VersionInformation>(result);
			}
			catch {
				MessageBox.Show("Error parsing version information.", "Version check failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
            
			// The version number is only displayed in the GUI in the published application.
			// When debugging in a test environment the version number is not displayed.
            //string currentVersion = "";
            //if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            //{
            //    System.Deployment.Application.ApplicationDeployment ad =
            //    System.Deployment.Application.ApplicationDeployment.CurrentDeployment;
            //    currentVersion = ad.CurrentVersion.ToString();
            //    currentVersion.Replace(".", "");
            //}                      

			return verinfo.Version != currentVersion.Version;			
		}

		public string UpdateUrl { get { if (verinfo == null) return ""; return verinfo.Url; } }
	}
}
