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
            this.tab1 = this.Factory.CreateRibbonTab();
            this.GoogleAnalytics = this.Factory.CreateRibbonGroup();
            this.buttonAccount = this.Factory.CreateRibbonButton();
            this.separator1 = this.Factory.CreateRibbonSeparator();
            this.buttonQuery = this.Factory.CreateRibbonButton();
            this.buttonUpdate = this.Factory.CreateRibbonButton();
            this.separator2 = this.Factory.CreateRibbonSeparator();
            this.SettingsButton = this.Factory.CreateRibbonButton();
            this.separator3 = this.Factory.CreateRibbonSeparator();
            this.AboutButton = this.Factory.CreateRibbonButton();
            this.tab1.SuspendLayout();
            this.GoogleAnalytics.SuspendLayout();
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
            this.GoogleAnalytics.Items.Add(this.buttonUpdate);
            this.GoogleAnalytics.Items.Add(this.separator2);
            this.GoogleAnalytics.Items.Add(this.SettingsButton);
            this.GoogleAnalytics.Items.Add(this.separator3);
            this.GoogleAnalytics.Items.Add(this.AboutButton);
            this.GoogleAnalytics.Label = "Excellent Analytics";
            this.GoogleAnalytics.Name = "GoogleAnalytics";
            // 
            // buttonAccount
            // 
            this.buttonAccount.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.buttonAccount.Label = "Account";
            this.buttonAccount.Name = "buttonAccount";
            this.buttonAccount.ShowImage = true;
            this.buttonAccount.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonAccount_Click);
            // 
            // separator1
            // 
            this.separator1.Name = "separator1";
            // 
            // buttonQuery
            // 
            this.buttonQuery.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.buttonQuery.Label = "New Query";
            this.buttonQuery.Name = "buttonQuery";
            this.buttonQuery.ShowImage = true;
            this.buttonQuery.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonQuery_Click);
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.buttonUpdate.Enabled = false;
            this.buttonUpdate.Label = "Update  Query";
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.ShowImage = true;
            this.buttonUpdate.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonUpdate_Click);
            // 
            // separator2
            // 
            this.separator2.Name = "separator2";
            // 
            // SettingsButton
            // 
            this.SettingsButton.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.SettingsButton.Image = global::Excel2007Addin.Properties.Resources.settings__1_;
            this.SettingsButton.Label = "Settings";
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.ShowImage = true;
            this.SettingsButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.SettingsButton_Click);
            // 
            // separator3
            // 
            this.separator3.Name = "separator3";
            // 
            // AboutButton
            // 
            this.AboutButton.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.AboutButton.Label = "About";
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.ShowImage = true;
            this.AboutButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.AboutButton_Click);
            // 
            // GA_Ribbon
            // 
            this.Name = "GA_Ribbon";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.tab1);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.GA_Ribbon_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.GoogleAnalytics.ResumeLayout(false);
            this.GoogleAnalytics.PerformLayout();

        }

        

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup GoogleAnalytics;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonQuery;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonAccount;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonUpdate;
        internal Microsoft.Office.Tools.Ribbon.RibbonSeparator separator1;
        internal Microsoft.Office.Tools.Ribbon.RibbonSeparator separator2;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton SettingsButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonSeparator separator3;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton AboutButton;
    }

    partial class ThisRibbonCollection : Microsoft.Office.Tools.Ribbon.RibbonReadOnlyCollection
    {
        internal GA_Ribbon Ribbon1
        {
            get { return this.GetRibbon<GA_Ribbon>(); }
        }
    }
}
