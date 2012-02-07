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
            string ver= System.Reflection.AssemblyName.GetAssemblyName(System.Reflection.Assembly.GetExecutingAssembly().Location).Version.ToString();
            VersionLabel.Text = "Version " + string.Join(".", ver.Split(new char[] { '.' }).Take(3).ToArray());


            DescriptionLabel.Text = "Excellent Analytics is an open source project. The people behind this tool have put a lot of hours of unpaid work into it. Therefore we hope you have patience with us. After all, you don't have to pay for this tool despite the fact that we have bills to pay. We hope it will save you many hours of work. Happy analyzing!" 
                                  + Environment.NewLine + ":)";

            Dictionary<string, string> labels = new Dictionary<string, string>();
            labels.Add("More about Excellent Analytics", "http://excellentanalytics.com/about/?utm_source=EA-addin&utm_medium=link&utm_campaign=EA-addin");
            labels.Add("Release history", "http://excellentanalytics.com/category/releases/?utm_source=EA-addin&utm_medium=link&utm_campaign=EA-addin");
            labels.Add("FAQ", "http://excellentanalytics.com/faq/?utm_source=EA-addin&utm_medium=link&utm_campaign=EA-addin");
            labels.Add("Support", "http://excellentanalytics.com/faq/?utm_source=EA-addin&utm_medium=link&utm_campaign=EA-addin");
            labels.Add("Suggest new features","http://excellentanalytics.uservoice.com/pages/30398-general?lang=en");
            labels.Add("Review","http://www.google.com/analytics/apps/about?start=0&app_id=3001");
            labels.Add("Community", "https://www.facebook.com/excellentanalytics");

            int y = 3;
            foreach (var pair in labels)
            {
                Label text = new Label() { Text = pair.Key + ":" };
                LinkLabel link = new LinkLabel() { Text = pair.Value };

                text.Left = 3;
                text.Top = y;
                text.AutoSize = true;
                
                link.Left = 3;
                link.Top = y + 15;
                link.AutoSize = true;

                text.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                link.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

                panel1.Controls.Add(text);
                panel1.Controls.Add(link);

                link.Click += new EventHandler(link_Click);

                y += 40;
            }
        }

        void link_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(((LinkLabel)sender).Text);
        }


        private void OK_Click(object sender, EventArgs e)
        {
            Close();
        }



    }
}
