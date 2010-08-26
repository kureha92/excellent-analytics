using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Deployment;
using System.Net;
using System.IO;


namespace WPFUIv2
{
    public partial class About : Form
    {

        public About()
        {
            InitializeComponent();
        }

        private void linkLabelAboutEA_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited 
            // to true.
            linkLabelAboutEA.LinkVisited = true;
            //Call the Process.Start method to open the default browser 
            //with a URL:
            System.Diagnostics.Process.Start("http://excellentanalytics.com/about/");
        }

        private void linkLabelFAQ_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited 
            // to true.
            linkLabelFAQ.LinkVisited = true;
            //Call the Process.Start method to open the default browser 
            //with a URL:
            System.Diagnostics.Process.Start("http://excellentanalytics.com/faq/");
        }

        private void linkLabelFeedback_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited 
            // to true.
            linkLabelFeedback.LinkVisited = true;
            //Call the Process.Start method to open the default browser 
            //with a URL:
            System.Diagnostics.Process.Start("http://excellentanalytics.com/about/#feedback");
            
        }

        private void linkLabelRelease_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited 
            // to true.
            linkLabelRelease.LinkVisited = true;
            //Call the Process.Start method to open the default browser 
            //with a URL:
            System.Diagnostics.Process.Start("http://feeds.feedburner.com/excellentanalytics/");
        }

        private void OK_Click(object sender, EventArgs e)
        {
            Close();
        }



    }
}
