using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.Office.Interop.Excel;
using System.Data.Common;
using System.Data;
using System.Reflection;
using System.Net;
using System.IO;
using Analytics.Authorization;
using Analytics.Data;
using GA_Excel2007;
using UI;
using System.Threading;
using System.Windows.Forms.Integration;
using System.Windows;
using Microsoft.Office.Tools;

namespace GA_Excel2007
{
    public partial class GA_Ribbon : OfficeRibbon
    {
        #region fields
        UserAccount user;
        Login login;
        ExecutionProgress exProg;
        QueryBuilder qb;

        #endregion

        public GA_Ribbon()
        {
            InitializeComponent();
            this.Load += new EventHandler<RibbonUIEventArgs>(GA_Ribbon_Load);
        }

        #region Events
        protected void GA_Ribbon_Load(object sender, RibbonUIEventArgs e)
        {
            GA_Excel2007.Globals.ThisAddIn.Application.SheetSelectionChange +=
            new AppEvents_SheetSelectionChangeEventHandler(Application_SheetSelectionChange);
        }

        void Application_SheetSelectionChange(object Sh, Range Target)
        {
            buttonUpdate.Enabled = false;
            buttonUpdate.Enabled = ActiveCellUpdatable();
        }

        private void buttonQuery_Click(object sender, RibbonControlEventArgs e)
        {
            LaunchQueryBuilder(new Query() , null);
        }

        void qb_queryComplete(Query query)
        {
            ExecuteQuery(query);
        }

        private void buttonAccount_Click(object sender, RibbonControlEventArgs e)
        {
            InitLogin();
        }

        void aRequest_logOut()
        {
            user = null;
        }

        void aRequest_authSuccessful(string authToken, string email)
        {
            AccountManager accMan = new AccountManager();
            user = accMan.GetAccountData(email, authToken);
            LaunchQueryBuilder(new Query() , null);
        }

        private void buttonUpdate_Click(object sender, RibbonControlEventArgs e)
        {
            LaunchQueryBuilder(new Query(GA_Excel2007.Globals.ThisAddIn.Application.ActiveCell.Value2.ToString().Split('\n')[1]) , null);
        }

        #endregion

        #region Helpers

        private bool ActiveCellUpdatable()
        {
            string activeValue = GA_Excel2007.Globals.ThisAddIn.Application.ActiveCell.Value2.ToString();
            return activeValue != null ? activeValue.Contains("\n") && activeValue.Split('\n')[1].StartsWith("https") : false;       
        }

        private void ExecuteQuery(Query query)
        {
            Microsoft.Office.Interop.Excel.Application currentApp = GA_Excel2007.Globals.ThisAddIn.Application;
            ReportManager repMan = new ReportManager();
            exProg = new ExecutionProgress(repMan);
            exProg.Show();

            Report report = repMan.GetReport(query, user.AuthToken);
            if (currentApp.ActiveSheet != null && report != null && report.Data != null 
                && report.Data.GetLength(0) > 0 && report.Data.GetLength(1) > 0)
            {
                Worksheet activeSheet = currentApp.ActiveSheet as Worksheet;


                int activeColumn = currentApp.ActiveCell.Column;
                int activeRow = currentApp.ActiveCell.Row;

                object[] queryInfoBox = new object[] { report.SiteURI + " [ " + query.StartDate + " -> " + query.EndDate + " ]" + "\n" + query.ToString() };
                int infoRows = queryInfoBox.GetLength(0);

                Range queryInfoRange = currentApp.get_Range(activeSheet.Cells[activeRow, activeColumn],
                activeSheet.Cells[activeRow, activeColumn + report.Data.GetLength(1) - 1]);
                queryInfoRange.Font.Italic = true;
                queryInfoRange.MergeCells = true;
                queryInfoRange.Borders.Weight = XlBorderWeight.xlThin;
                queryInfoRange.Value2 = queryInfoBox;

                Range headerRange = currentApp.get_Range(activeSheet.Cells[activeRow + infoRows, activeColumn],
                activeSheet.Cells[activeRow + infoRows, activeColumn + report.Headers.GetLength(1) - 1]);
                headerRange.Value2 = report.Headers;
                headerRange.Font.Bold = true;

                Range dataRange = currentApp.get_Range(activeSheet.Cells[activeRow + infoRows + 1, activeColumn],
                activeSheet.Cells[activeRow + infoRows + report.Data.GetLength(0), activeColumn + report.Data.GetLength(1) - 1]);
                dataRange.Value2 = report.Data;
            }
            else
            {
                if (report == null )
	            {
            		LaunchQueryBuilder(query , "Invalid query. The request was rejected");
	            }
                else if (report != null && report.Data != null && report.Data.GetLength(0) < 1)
	            {
                    LaunchQueryBuilder(query, "The request returned 0 hits");
	            }
            }
        }

        private void InitLogin()
        {
            this.login = new Login(user);
            login.authSuccessful += new Login.AuthSuccessful(aRequest_authSuccessful);
            login.logOut += new Login.Logout(aRequest_logOut);
            login.ShowDialog();
        }

        private void LaunchQueryBuilder(Query query , string Errormsg)
        {
            if (user != null && !String.IsNullOrEmpty(user.AuthToken))
            {
                qb = String.IsNullOrEmpty(Errormsg) ? new QueryBuilder(user, query) : new QueryBuilder(user, query , Errormsg);
                qb.queryComplete += new QueryBuilder.QueryComplete(qb_queryComplete);
                qb.ShowDialog();
            }
            else
            {
                InitLogin();
            }
        }

        //private void Chart()
        //{
        //    //Range chartRange = currentApp.get_Range(activeSheet.Cells[activeRow + 1, activeColumn], activeSheet.Cells[report.Data.GetLength(0) + activeRow, report.Data.GetLength(1)]);
        //    //ChartObjects chartObjs = (ChartObjects)activeSheet.ChartObjects(Type.Missing);
        //    //ChartObject chartObj = chartObjs.Add(300, 150, 400, 400);
        //    //Chart xlChart = chartObj.Chart;
        //    //xlChart.ChartWizard(chartRange, XlChartType.xl3DLine, Missing.Value,
        //    //XlRowCol.xlColumns, Missing.Value, Missing.Value, Missing.Value,
        //    //Missing.Value, Missing.Value, Missing.Value, Missing.Value);
        //}
        #endregion
    }
}
