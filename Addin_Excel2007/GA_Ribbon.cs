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
        private const string queryInfoIdentifier = "queryInfo[";

        UserAccount _user;
        Login _login;
        ExecutionProgress _executionProgressWindow;
        QueryBuilder _queryBuilderWindow;
        Report _currentReport;
        ReportManager _reportManager;
        #endregion

        public Range ActiveCell
        {
            get
            {
                return GA_Excel2007.Globals.ThisAddIn.Application.ActiveCell;
            }
        }

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
            LaunchQueryBuilder(new Query());
        }

        void queryComplete(Query query)
        {
            ExecuteQuery(query);
        }

        private void buttonAccount_Click(object sender, RibbonControlEventArgs e)
        {
            InitLogin();
        }

        void User_Logout()
        {
            _user = null;
        }

        void User_Successful_Login(string authToken, string email)
        {
            AccountManager accMan = new AccountManager();
            _user = accMan.GetAccountData(email, authToken);
            LaunchQueryBuilder(new Query());
        }

        private void buttonUpdate_Click(object sender, RibbonControlEventArgs e)
        {
            Query query = new Query(GetQueryExcelParamValueFromActiveCell("queryString"));
            query.TimePeriod = (Analytics.Data.Enums.TimePeriod)Enum.Parse(typeof(Analytics.Data.Enums.TimePeriod), 
                                                                            GetQueryExcelParamValueFromActiveCell("timePeriod"));
            LaunchQueryBuilder(query);
        }

        #endregion

        #region Methods

        private bool ActiveCellUpdatable()
        {
            return !string.IsNullOrEmpty(GetQueryExcelParamValueFromActiveCell("queryString"));
        }

        private void ExecuteQuery(Query query)
        {
            _reportManager = new ReportManager();
            _executionProgressWindow = new ExecutionProgress(_reportManager);
            _executionProgressWindow.Show();
            _currentReport = _reportManager.GetReport(query, _user.AuthToken);

            if (_currentReport != null && _currentReport.ValidateResult())
            {
                if (ActiveCellHasQueryResult)
                    ClearPreviousQueryResult();
                PresentResult(query, _currentReport);
            }
        }

        private bool ActiveCellHasQueryResult
        {
            get
            { 
                return GA_Excel2007.Globals.ThisAddIn.Application.ActiveCell != null && 
                GA_Excel2007.Globals.ThisAddIn.Application.ActiveCell.Value2 != null &&
                GA_Excel2007.Globals.ThisAddIn.Application.ActiveCell.Value2.ToString().Contains(queryInfoIdentifier); 
            }
        }

        private void ClearPreviousQueryResult()
        {
            int rows;
            int columns;
            if (int.TryParse(GetQueryExcelParamValueFromActiveCell("rows"), out rows) &&
                int.TryParse(GetQueryExcelParamValueFromActiveCell("columns"), out columns))
            {
                Range activeCell = GA_Excel2007.Globals.ThisAddIn.Application.ActiveCell;
                Microsoft.Office.Interop.Excel.Application currentApp = GA_Excel2007.Globals.ThisAddIn.Application;
                Worksheet activeSheet = currentApp.ActiveSheet as Worksheet;

                Range rangeToClear = currentApp.get_Range(activeSheet.Cells[activeCell.Row + 1, activeCell.Column],
                                                          activeSheet.Cells[activeCell.Row + rows, columns]);
                rangeToClear.Clear();
            } 
        }

        private string GetQueryExcelParamValueFromActiveCell(string name)
        {
            string activeValue = ActiveCell.Value2.ToString();
            if (activeValue.Contains(queryInfoIdentifier))
            {
                string paramString = activeValue.Substring(activeValue.IndexOf(queryInfoIdentifier));
                paramString = paramString.Replace(queryInfoIdentifier, string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
                string[] paramArray = paramString.Split(';');
                string rowsParam = paramArray.Where(p => p.StartsWith(name)).First().ToString();
                return rowsParam.Substring(rowsParam.IndexOf('=') + 1).Trim();
            }
            return string.Empty;
        }

        private static void PresentResult(Query query, Report report)
        {
            Microsoft.Office.Interop.Excel.Application currentApp = GA_Excel2007.Globals.ThisAddIn.Application;
            Worksheet activeSheet = currentApp.ActiveSheet as Worksheet;

            if (currentApp.ActiveSheet != null)
            {
                int activeColumn = currentApp.ActiveCell.Column;
                int activeRow = currentApp.ActiveCell.Row;

                object[] queryInformation = GetQueryInformation(query, report);

                Range queryInformationRange = currentApp.get_Range(activeSheet.Cells[activeRow, activeColumn],
                activeSheet.Cells[activeRow, activeColumn + report.Data.GetLength(1) - 1]);
                queryInformationRange.Font.Italic = true;
                queryInformationRange.MergeCells = true;
                queryInformationRange.Borders.Weight = XlBorderWeight.xlThin;
                queryInformationRange.Value2 = queryInformation;

                int infoRows = queryInformation.GetLength(0);

                Range headerRange = currentApp.get_Range(activeSheet.Cells[activeRow + infoRows, activeColumn],
                activeSheet.Cells[activeRow + infoRows, activeColumn + report.Headers.GetLength(1) - 1]);
                headerRange.Value2 = report.Headers;
                headerRange.Font.Bold = true;

                Range dataRange = currentApp.get_Range(activeSheet.Cells[activeRow + infoRows + 1, activeColumn],
                activeSheet.Cells[activeRow + infoRows + report.Data.GetLength(0), activeColumn + report.Data.GetLength(1) - 1]);
                dataRange.Value2 = report.Data; 
            }
        }

        private static object[] GetQueryInformation(Query query, Report report)
        {
            object[] queryInformation = new object[] { report.SiteURI + " [ " + query.StartDate.ToShortDateString() + " -> " + query.EndDate.ToShortDateString() + " ]\n"
                + string.Format( "{0}queryString={1};rows={2};columns={3};timePeriod={4}]",
                                queryInfoIdentifier, query.ToString(), report.Hits, query.GetDimensionsAndMetricsCount() , query.TimePeriod.ToString()) };
            return queryInformation;
        }

        private void InitLogin()
        {
            this._login = new Login(_user);
            _login.authSuccessful += new Login.AuthSuccessful(User_Successful_Login);
            _login.logOut += new Login.Logout(User_Logout);
            _login.ShowDialog();
        }

        private void LaunchQueryBuilder(Query query)
        {
            if (_user != null && !string.IsNullOrEmpty(_user.AuthToken))
            {
                _queryBuilderWindow = new QueryBuilder(_user, query);
                _queryBuilderWindow.queryComplete += new QueryBuilder.QueryComplete(queryComplete);
                _queryBuilderWindow.ShowDialog();
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
