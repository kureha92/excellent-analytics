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
using Excel = Microsoft.Office.Interop.Excel;
using GA_Addin.UI;
using System.Windows.Forms;
using WPFUIv2;


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
        WorkSheetUpdate _workSheetUpdateWindow;
        Report _currentReport;
        ReportManager _reportManager;
        string _cellValue = "";
        string _sFirstFoundAddress = "";
        static List<string> _addressQueries;
        List<Query> _listQueries;

        #endregion

        public Range ActiveCell
        {
            get
            {
                return GA_Excel2007.Globals.ThisAddIn.Application.ActiveCell;
            }
        }

        public static List<string> AddressQueries
        {
            get
            {
                return _addressQueries;
            }

            set
            {
                _addressQueries = value;
            }
        }

        public string FirstFoundAddress
        {
            get
            {
                return _sFirstFoundAddress;
            }
            set
            {
                _sFirstFoundAddress = value;
            }
        }

        public GA_Ribbon()
        {
            InitializeComponent();
            this.Load += new EventHandler<RibbonUIEventArgs>(GA_Ribbon_Load);
            buttonUpdateWorkSheet.Enabled = true;
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
            List<Query> queries = new List<Query>();
            queries.Add(query);
            ExecuteQuery(queries, false);
        }

        void queryComplete(Query query, bool worksheet)
        {
            ExecuteQuery(_listQueries, worksheet);
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

            Updates checkUp = new Updates();
            if (checkUp.CheckForUpdates())
            {
                switch (System.Windows.Forms.MessageBox.Show("A new version of Excellent Analytics is available. Do you want to download it?",
                    "Update available !",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Question))
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        System.Diagnostics.Process.Start("www.excellentanalytics.com");
                        break;

                    case System.Windows.Forms.DialogResult.No:
                        break;
                }
            }

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

        private void ExecuteQuery(List<Query> queries, bool worksheet)
        {
            _addressQueries = new List<string>();
            bool erasedConfirm = false;
            bool clearFormat = false;
            List<Report> reportList = new List<Report>();

            foreach (Query query in queries)
            {
                _reportManager = new ReportManager();
                _executionProgressWindow = new ExecutionProgress(_reportManager);
                _executionProgressWindow.Show();
                _currentReport = _reportManager.GetReport(query, _user.AuthToken);
                reportList.Add(_currentReport);

                if (_currentReport != null && _currentReport.ValidateResult())
                {
                    if (!erasedConfirm)
                        if (ActiveCellHasQueryResult || worksheet)
                        {
                            switch (System.Windows.Forms.MessageBox.Show("Do you want to erase the format of your excel query columns?",
                                                "Document format",
                                                System.Windows.Forms.MessageBoxButtons.YesNo,
                                                System.Windows.Forms.MessageBoxIcon.Question))
                            {
                                case System.Windows.Forms.DialogResult.Yes:
                                    clearFormat = true;
                                    break;

                                case System.Windows.Forms.DialogResult.No:
                                    break;
                            }
                            ClearPreviousQueryResult(clearFormat, worksheet, query);
                        }

                    if (erasedConfirm)
                        ClearPreviousQueryResult(clearFormat, worksheet, query);

                    erasedConfirm = true;
                }
            }

            int i = 0;
            foreach (Query query in queries)
            {
                if (worksheet)
                {
                    PresentResultUpdate(query, reportList[i]);
                }
                else
                    PresentResult(query, _currentReport);
                i++;
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

        private void ClearPreviousQueryResult(bool clearFormat, bool worksheet, Query query)
        {
            int rows;
            int columns;

            if (worksheet)
            {
                if (int.TryParse(GetQueryExcelParamValueFromSpecificCell("rows", ""), out rows) &&
                    int.TryParse(GetQueryExcelParamValueFromSpecificCell("columns", ""), out columns))
                {
                    Worksheet _sheet = (Worksheet)GA_Excel2007.Globals.ThisAddIn.Application.ActiveSheet;

                    int activeColumn = query.Column;
                    int activeRow = query.Row;

                    Range queryInformationRange = _sheet.get_Range(_sheet.Cells[activeRow, activeColumn],
                    _sheet.Cells[activeRow + 1, activeColumn + 2 - 1]);
                    queryInformationRange.MergeCells = false;

                    Range rangeToClear = _sheet.get_Range(_sheet.Cells[activeRow + 1, activeColumn],
                                                                _sheet.Cells[activeRow + rows + 1, activeColumn + columns - 1]);
                                       

                    if (clearFormat)
                    {
                        queryInformationRange.Clear();
                        rangeToClear.Clear();
                    }
                    else
                        rangeToClear.ClearContents();                    
                 }
             }
                else
                {
                    if (int.TryParse(GetQueryExcelParamValueFromActiveCell("rows"), out rows) &&
                        int.TryParse(GetQueryExcelParamValueFromActiveCell("columns"), out columns))
                    {
                        Microsoft.Office.Interop.Excel.Application currentApp = GA_Excel2007.Globals.ThisAddIn.Application;
                        Worksheet activeSheet = currentApp.ActiveSheet as Worksheet;
                        int activeColumn = currentApp.ActiveCell.Column;
                        int activeRow = currentApp.ActiveCell.Row;


                        Range rangeToClear = currentApp.get_Range(activeSheet.Cells[activeRow + 1, activeColumn + 1],
                                                                    activeSheet.Cells[activeRow + rows + 1, activeColumn + columns - 1]);
                        if (clearFormat)
                        {
                            rangeToClear.Clear();
                        }
                        else
                            rangeToClear.ClearContents();
                    }
                }
        }

        private string GetQueryExcelParamValueFromActiveCell(string name)
        {
            string activeValue = ActiveCell.Value2.ToString();
            if (activeValue.Contains(queryInfoIdentifier))
            {
                string paramString = activeValue.Substring(activeValue.IndexOf(queryInfoIdentifier));
                int lastBracket = paramString.Length;
                paramString = paramString.Replace(queryInfoIdentifier, string.Empty);
                paramString = paramString.Substring(0, paramString.Length - 1);

                string[] paramArray = paramString.Split(';');
                int arrayLength = paramArray.Length;
                if (arrayLength > 4)
                {
                    int i = arrayLength - 4;
                    for (int j = 0; j < i; j++)
                    {
                        paramArray[0] = paramArray[0] + ";" + paramArray[j + 1];
                    }
                }

                string rowsParam = paramArray.Where(p => p.StartsWith(name)).First().ToString();
                return rowsParam.Substring(rowsParam.IndexOf('=') + 1).Trim();
            }
            return string.Empty;
        }

        private string GetQueryExcelParamValueFromSpecificCell(string name, string cellValue)
        {
            if (cellValue.Equals("") && !_cellValue.Equals(""))
                cellValue = _cellValue;

            string paramString = cellValue.Substring(cellValue.IndexOf(queryInfoIdentifier));
            int lastBracket = paramString.Length;
            paramString = paramString.Replace(queryInfoIdentifier, string.Empty);
            paramString = paramString.Substring(0, paramString.Length - 1);

            string[] paramArray = paramString.Split(';');
            int arrayLength = paramArray.Length;
            if (arrayLength > 4)
            {
                int i = arrayLength - 4;
                for (int j = 0; j < i; j++)
                {
                    paramArray[0] = paramArray[0] + ";" + paramArray[j + 1];
                }
            }

            string rowsParam = paramArray.Where(p => p.StartsWith(name)).First().ToString();
            return rowsParam.Substring(rowsParam.IndexOf('=') + 1).Trim();

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

                int infoRows = queryInformation.GetLength(0);
                int dataLength = 2;

                if (report.Data != null)
                {
                    dataLength = report.Data.GetLength(1);
                    Range dataRange = currentApp.get_Range(activeSheet.Cells[activeRow + infoRows + 1, activeColumn],
                    activeSheet.Cells[activeRow + infoRows + report.Data.GetLength(0), activeColumn + report.Data.GetLength(1) - 1]);
                    dataRange.Value2 = report.Data;

                    Range headerRange = currentApp.get_Range(activeSheet.Cells[activeRow + infoRows, activeColumn],
                    activeSheet.Cells[activeRow + infoRows, activeColumn + report.Headers.GetLength(1) - 1]);
                    headerRange.Value2 = report.Headers;
                    headerRange.Font.Bold = true;
                }

                Range queryInformationRange = currentApp.get_Range(activeSheet.Cells[activeRow, activeColumn],
                activeSheet.Cells[activeRow, activeColumn + dataLength - 1]);
                queryInformationRange.Font.Italic = true;
                queryInformationRange.MergeCells = true;
                queryInformationRange.Borders.Weight = XlBorderWeight.xlThin;
                queryInformationRange.Value2 = queryInformation;

            }
        }

        private static void PresentResultUpdate(Query query, Report report)
        {
            Microsoft.Office.Interop.Excel.Application currentApp = GA_Excel2007.Globals.ThisAddIn.Application;
            Worksheet activeSheet = currentApp.ActiveSheet as Worksheet;

            if (currentApp.ActiveSheet != null)
            {
                int activeColumn = query.Column;
                int activeRow = query.Row;
                string addressLocal = "";
                string addressHelper = "";

                object[] queryInformation = GetQueryInformation(query, report);

                int infoRows = queryInformation.GetLength(0);
                int dataLength = 2;

                Range queryInformationRange = currentApp.get_Range(activeSheet.Cells[activeRow, activeColumn],
                activeSheet.Cells[activeRow, activeColumn + dataLength - 1]);

                //
                addressLocal = queryInformationRange.get_Address().Replace("$", "");
                string[] adLocal = addressLocal.Split(':');

                // Looks after other reports in the same column. 
                foreach (string address in _addressQueries)
                {
                    addressHelper = address.Replace("$", "");
                    // If there exist a report above this report then check where that report's data range ends.
                    if (addressHelper.First().Equals(adLocal[0].First()))
                    {

                        addressHelper = addressHelper.Substring(1, addressHelper.Length - 1);
                        int addressHelperInt = Int32.Parse(addressHelper);
                        int addressLocalInt = Int32.Parse(adLocal[0].Substring(1, adLocal[0].Length - 1));

                        if (addressHelperInt > addressLocalInt)
                        {
                            activeRow = addressHelperInt + 2;
                            queryInformationRange = currentApp.get_Range(activeSheet.Cells[activeRow, activeColumn],
                            activeSheet.Cells[activeRow, activeColumn + dataLength - 1]);
                        }
                    }
                }

                queryInformationRange.Font.Italic = true;
                queryInformationRange.MergeCells = true;
                queryInformationRange.Borders.Weight = XlBorderWeight.xlThin;
                queryInformationRange.Value2 = queryInformation;


                if (report.Data != null)
                {
                    dataLength = report.Data.GetLength(1);
                    Range dataRange = currentApp.get_Range(activeSheet.Cells[activeRow + infoRows + 1, activeColumn],
                    activeSheet.Cells[activeRow + infoRows + report.Data.GetLength(0), activeColumn + report.Data.GetLength(1) - 1]);
                    dataRange.Value2 = report.Data;
                    // Saves the last cell in this particular data range.
                    string endDataRow = "";
                    endDataRow = adLocal[0].First() + (activeRow + infoRows + report.Data.GetLength(0)).ToString();
                    _addressQueries.Add(endDataRow);
                    Range headerRange = currentApp.get_Range(activeSheet.Cells[activeRow + infoRows, activeColumn],
                    activeSheet.Cells[activeRow + infoRows, activeColumn + report.Headers.GetLength(1) - 1]);
                    headerRange.Value2 = report.Headers;
                    headerRange.Font.Bold = true;
                }

            }
        }

        private void test(string address)
        { }

        private static object[] GetQueryInformation(Query query, Report report)
        {
            string timePeriod = "";
            if (!query.SelectDates)
            {
                timePeriod += query.TimePeriod.ToString();
            }
            else
            {
                timePeriod = "PeriodNotSpecified";
            }


            object[] queryInformation = new object[] { report.SiteURI + " [ " + query.StartDate.ToShortDateString() + " -> " + query.EndDate.ToShortDateString() + " ]\n"
                + string.Format( "{0}queryString={1};rows={2};columns={3};timePeriod={4}]",
                                queryInfoIdentifier, query.ToString(), report.Hits, query.GetDimensionsAndMetricsCount(), timePeriod)};
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

        private void buttonUpdateWorkSheet_Click(object sender, RibbonControlEventArgs e)
        {
            Worksheet _sheet = (Worksheet)GA_Excel2007.Globals.ThisAddIn.Application.ActiveSheet;
//            Range allCells = _sheet.Cells;

            Range _range = _sheet.Cells.Find(queryInfoIdentifier, _sheet.Cells.get_End(XlDirection.xlDown), Excel.XlFindLookIn.xlValues, Excel.XlLookAt.xlPart, XlSearchOrder.xlByRows,
                      Excel.XlSearchDirection.xlNext, false, false);

            if (_range != null)
            {
                Query query1 = new Query();
                _sFirstFoundAddress = _range.get_Address(true, true, Excel.XlReferenceStyle.xlA1);
                if (_sFirstFoundAddress.Contains("A"))
                {
                    _range = _sheet.Cells.FindNext(_range);
                    _sFirstFoundAddress = _range.get_Address(true, true, Excel.XlReferenceStyle.xlA1);
                }

               // If Find doesn't find anything, _range will be null
                foreach (Range cell in _range.Cells)
                {
                    _cellValue = cell.Value.ToString();

                    query1 = new Query(GetQueryExcelParamValueFromSpecificCell("queryString", _cellValue));
                    query1.TimePeriod = (Analytics.Data.Enums.TimePeriod)Enum.Parse(typeof(Analytics.Data.Enums.TimePeriod),
                                                                                                            GetQueryExcelParamValueFromSpecificCell("timePeriod", ""));

                    query1.Column = cell.Column;
                    query1.Row = cell.Row;
                }

                _listQueries = new List<Query>();
                _listQueries.Add(query1);
                //Continue finding subsequent items using FindNext
//                _range = _sheet.Cells.FindNext(_range);
                string sAddress = "";

                while (!sAddress.Equals(_sFirstFoundAddress))
                {

                    _range = _sheet.Cells.FindNext(_range);
                    sAddress = _range.get_Address(
                        true, true, Excel.XlReferenceStyle.xlA1);
                    if (sAddress.Equals(_sFirstFoundAddress))
                        break;

                    foreach (Range cell in _range.Cells)
                    {
                        if (cell.Address == sAddress)
                        {
                            _cellValue = cell.Value.ToString();

                            Query query = new Query(GetQueryExcelParamValueFromSpecificCell("queryString", _cellValue));
                            query.TimePeriod = (Analytics.Data.Enums.TimePeriod)Enum.Parse(typeof(Analytics.Data.Enums.TimePeriod),
                                                                                            GetQueryExcelParamValueFromSpecificCell("timePeriod", ""));
                            query.Column = cell.Column;
                            query.Row = cell.Row;
                            _listQueries.Add(query);
                        }
                    }
                }
                //            LaunchWorkSheetUpdate();
                _workSheetUpdateWindow = new WorkSheetUpdate(_user, _listQueries);
                _workSheetUpdateWindow.queryComplete += new WorkSheetUpdate.QueryComplete(queryComplete);
                _workSheetUpdateWindow.Queries = _listQueries;
                _workSheetUpdateWindow.ShowDialog();
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
