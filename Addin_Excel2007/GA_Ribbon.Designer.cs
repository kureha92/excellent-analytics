namespace GA_Excel2007
{
    partial class GA_Ribbon
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GA_Ribbon));
            this.tab1 = new Microsoft.Office.Tools.Ribbon.RibbonTab();
            this.GoogleAnalytics = new Microsoft.Office.Tools.Ribbon.RibbonGroup();
            this.buttonAccount = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.separator1 = new Microsoft.Office.Tools.Ribbon.RibbonSeparator();
            this.buttonQuery = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.separator2 = new Microsoft.Office.Tools.Ribbon.RibbonSeparator();
            this.buttonUpdate = new Microsoft.Office.Tools.Ribbon.RibbonButton();
            this.tab1.SuspendLayout();
            this.GoogleAnalytics.SuspendLayout();
            this.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.Groups.Add(this.GoogleAnalytics);
            this.tab1.Label = "Excellent Analytics";
            this.tab1.Name = "tab1";
            // 
            // GoogleAnalytics
            // 
            this.GoogleAnalytics.Items.Add(this.buttonAccount);
            this.GoogleAnalytics.Items.Add(this.separator1);
            this.GoogleAnalytics.Items.Add(this.buttonQuery);
            this.GoogleAnalytics.Items.Add(this.separator2);
            this.GoogleAnalytics.Items.Add(this.buttonUpdate);
            this.GoogleAnalytics.Label = "Excellent Analytics";
            this.GoogleAnalytics.Name = "GoogleAnalytics";
            // 
            // buttonAccount
            // 
            this.buttonAccount.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.buttonAccount.Image = ((System.Drawing.Image)(resources.GetObject("buttonAccount.Image")));
            this.buttonAccount.Label = "Account";
            this.buttonAccount.Name = "buttonAccount";
            this.buttonAccount.ShowImage = true;
            this.buttonAccount.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.buttonAccount_Click);
            // 
            // separator1
            // 
            this.separator1.Name = "separator1";
            // 
            // buttonQuery
            // 
            this.buttonQuery.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.buttonQuery.Image = ((System.Drawing.Image)(resources.GetObject("buttonQuery.Image")));
            this.buttonQuery.Label = "New Query";
            this.buttonQuery.Name = "buttonQuery";
            this.buttonQuery.ShowImage = true;
            this.buttonQuery.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.buttonQuery_Click);
            // 
            // separator2
            // 
            this.separator2.Name = "separator2";
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.buttonUpdate.Enabled = false;
            this.buttonUpdate.Image = ((System.Drawing.Image)(resources.GetObject("buttonUpdate.Image")));
            this.buttonUpdate.Label = "Update  Query";
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.ShowImage = true;
            this.buttonUpdate.Click += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs>(this.buttonUpdate_Click);
            // 
            // GA_Ribbon
            // 
            this.Name = "GA_Ribbon";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.tab1);
            this.Load += new System.EventHandler<Microsoft.Office.Tools.Ribbon.RibbonUIEventArgs>(this.GA_Ribbon_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.GoogleAnalytics.ResumeLayout(false);
            this.GoogleAnalytics.PerformLayout();
            this.ResumeLayout(false);

        }

        

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup GoogleAnalytics;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonQuery;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonAccount;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonUpdate;
        internal Microsoft.Office.Tools.Ribbon.RibbonSeparator separator1;
        internal Microsoft.Office.Tools.Ribbon.RibbonSeparator separator2;
    }

    partial class ThisRibbonCollection : Microsoft.Office.Tools.Ribbon.RibbonReadOnlyCollection
    {
        internal GA_Ribbon Ribbon1
        {
            get { return this.GetRibbon<GA_Ribbon>(); }
        }
    }
}
