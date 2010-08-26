using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WPFUIv2
{
    public partial class InvalidCombinations : Form
    {
        public InvalidCombinations()
        {
            InitializeComponent();
        }

        private void label1_Click_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited 
            // to true.
            linkLabel1.LinkVisited = true;
            //Call the Process.Start method to open the default browser 
            //with a URL:
            System.Diagnostics.Process.Start("http://code.google.com/intl/sv/apis/analytics/docs/gdata/gdataReferenceValidCombos.html");
        }

        private void OK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
