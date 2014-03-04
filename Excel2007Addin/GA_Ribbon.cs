using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.Office.Interop.Excel;
using Analytics.Authorization;
using Analytics.Data;
using UI;
using Excel = Microsoft.Office.Interop.Excel;
using GA_Addin.UI;
using WPFUIv2;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Xml;
using Analytics;

namespace GA_Excel2007
{
	public partial class GA_Ribbon : RibbonBase
	{
		#region fields
		private const string queryInfoIdentifier = "queryInfo[";

		UserAccount _user;
		Login _login;
		ExecutionProgress _executionProgressWindow;
		QueryBuilder _queryBuilderWindow;
		//WorkSheetUpdate _workSheetUpdateWindow;
		Report _currentReport;
		ReportManager _reportManager;
		string _cellValue = "";
		string _sFirstFoundAddress = "";
		static List<string> _addressQueries;
		List<Query> _listQueries;

        #endregion
        private byte[] randomBytes = { 4, 32, 62, 9, 145, 5 };

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


        /*
        public GA_Ribbon()
		{
			InitializeComponent();
			this.Load += new EventHandler<RibbonUIEventArgs>(GA_Ribbon_Load);
			//buttonUpdateWorkSheet.Enabled = true;
		}
         */
        public GA_Ribbon()
            : base(Globals.Factory.GetRibbonFactory())
		{
			InitializeComponent();
			//this.Load += new EventHandler<RibbonUIEventArgs>(GA_Ribbon_Load);
            this.Load += new RibbonUIEventHandler(GA_Ribbon_Load);
			//buttonUpdateWorkSheet.Enabled = true;
		}

		#region Events
		protected void GA_Ribbon_Load(object sender, RibbonUIEventArgs e)
		{
			GA_Excel2007.Globals.ThisAddIn.Application.SheetSelectionChange +=
			    new AppEvents_SheetSelectionChangeEventHandler(Application_SheetSelectionChange);
            GA_Excel2007.Globals.ThisAddIn.Application.SheetChange +=
                new AppEvents_SheetChangeEventHandler(Application_SheetSelectionChange);
		}
		void Application_SheetSelectionChange(object Sh, Range Target)
		{
			buttonUpdate.Enabled = IsActiveCellUpdatable;
		}
		private void buttonQuery_Click(object sender, RibbonControlEventArgs e)
		{
			LaunchQueryBuilder(new Query());
		}
		/// <summary>
		/// Adds all queries to  a list. Gives each query one profileId if more than one.
		/// Sends the manipulated list of queries to execute.
		/// </summary>
		/// <param name="query"></param>
		void queryComplete(Query query)
		{
			// Here I need to go thru all selected profile and create a query for each one of them.
			// 
			_listQueries = new List<Query>();
			List<Query> queries = new List<Query>();


			foreach (Item item in query.Ids)
			{
				_listQueries.Add(query);
			}


			ExecuteQuery(_listQueries, false);
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
			AccountManager accMan = new AccountManager();
			_user = accMan.GetAccountData(email, authToken);
			LaunchQueryBuilder(new Query());
            Excel2007Addin.Updates.CheckForUpdates();
		}

		private void buttonUpdate_Click(object sender, RibbonControlEventArgs e)
		{
            Query query = new Query(GetQueryExcelParamValueFromActiveCell("queryString"));
            query.TimePeriod = (Analytics.Data.Enums.TimePeriod)Enum.Parse(typeof(Analytics.Data.Enums.TimePeriod),
                                                                            GetQueryExcelParamValueFromActiveCell("timePeriod")[1]);
            LaunchQueryBuilder(query);
		}

		#endregion

		#region Methods
		private bool IsActiveCellUpdatable
		{
			get { return !string.IsNullOrEmpty(GetQueryExcelParamValueFromActiveCell("queryString")[1]); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="queries"></param>
		/// <param name="worksheet"></param>
		private void ExecuteQuery(List<Query> queries, bool worksheet)
		{
			if (queries.Count == 0)
				return;
			if (_user == null)
				return;

			_addressQueries = new List<string>();
			bool clearFormat = false;
			List<Report> reportList = new List<Report>();

			int profileCounter = 0;
            int cellOffset = 0;
			foreach (Query query in queries)
			{
				_reportManager = new ReportManager();
				_executionProgressWindow = new ExecutionProgress(_reportManager);
				_executionProgressWindow.Show();

				_currentReport = _reportManager.GetReport(query, _user.AuthToken, profileCounter);
                if (!_currentReport.ValidateResult())
                    continue;

				reportList.Add(_currentReport);

				if (profileCounter == 0 && _currentReport != null && _currentReport.ValidateResult())
				{
					if (ActiveCellHasQueryResult || worksheet)
					{

                        clearFormat = Excel2007Addin.Settings.Default.CellFormatting == (int)WPFUIv2.CellFormattingEnum.never ||
                                        (Excel2007Addin.Settings.Default.CellFormatting == (int)WPFUIv2.CellFormattingEnum.ask && 
                                        MessageBox.Show("Do you want to erase the format of your excel query columns?",
													  "Document format", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
						ClearPreviousQueryResult(clearFormat, worksheet, query);
					}
				}

				if (!worksheet)
				{
					// If a query is executed containing more than one profile the active cell cursor must be 
					// moved to prevent clearing the prior report in Excel.
					PresentResult(query, _currentReport, profileCounter, cellOffset);

                    cellOffset += _currentReport.Data.GetLength(1) + 1; // +1 for space

					if (query.Ids.Count > profileCounter)
						profileCounter++;
				}
			}

			int i = 0;
			if (_user != null && worksheet)
			{
				foreach (Query query in queries)
				{
					PresentResultUpdate(query, reportList[i], profileCounter);
					i++;
				}
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
				if (int.TryParse(GetQueryExcelParamValueFromSpecificCell("rows", "")[1], out rows) &&
					int.TryParse(GetQueryExcelParamValueFromSpecificCell("columns", "")[1], out columns))
				{
					Worksheet _sheet = (Worksheet)GA_Excel2007.Globals.ThisAddIn.Application.ActiveSheet;

					int activeColumn = query.Column;
					int activeRow = query.Row;

					Range queryInformationRange = _sheet.get_Range(_sheet.Cells[activeRow, activeColumn],
					_sheet.Cells[activeRow + 1, activeColumn + 2 - 1]);
					queryInformationRange.MergeCells = false;

					Range rangeToClear = _sheet.get_Range(_sheet.Cells[activeRow + 1, activeColumn],
																_sheet.Cells[activeRow + rows - 1, activeColumn + columns - 1]);


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
				if (int.TryParse(GetQueryExcelParamValueFromActiveCell("rows")[1], out rows) &&
					int.TryParse(GetQueryExcelParamValueFromActiveCell("columns")[1], out columns))
				{
					Microsoft.Office.Interop.Excel.Application currentApp = GA_Excel2007.Globals.ThisAddIn.Application;
					Worksheet activeSheet = currentApp.ActiveSheet as Worksheet;
					int activeColumn = currentApp.ActiveCell.Column;
					int activeRow = currentApp.ActiveCell.Row;

					Range rangeToClear = activeSheet.get_Range(activeSheet.Cells[activeRow + 1, activeColumn],
																activeSheet.Cells[activeRow + rows - 1, activeColumn + columns - 1]);
					if (clearFormat)
						rangeToClear.Clear();
					else
						rangeToClear.ClearContents();
				}
			}
		}

		private string[] GetQueryExcelParamValueFromActiveCell(string name)
		{
            if (ActiveCell.Value2 == null)
                return new string[2];
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

				string[] rowsParam = new string[2];
				rowsParam[1] = paramArray.Where(p => p.StartsWith(name)).First().ToString();
				rowsParam[1] = rowsParam[1].Substring(rowsParam[1].IndexOf('=') + 1).Trim();
				return rowsParam;
			}
			return new string[2];
		}

		private string[] GetQueryExcelParamValueFromSpecificCell(string name, string cellValue)
		{
			if (cellValue.Equals("") && !_cellValue.Equals(""))
				cellValue = _cellValue;

			int bracket = cellValue.IndexOf("[");
			string profile = cellValue.Substring(0, bracket);
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
			rowsParam = rowsParam.Substring(rowsParam.IndexOf('=') + 1).Trim();
			string[] rowsParamReturn = new string[2];
			rowsParamReturn[0] = profile;
			rowsParamReturn[1] = rowsParam;
			return rowsParamReturn;

		}

		private static int ActiveColumn(int profileCounter, int dataLength)
		{
			Microsoft.Office.Interop.Excel.Application currentApp = GA_Excel2007.Globals.ThisAddIn.Application;
			int activeColumn = currentApp.ActiveCell.Column;

			if (!currentApp.ActiveCell.Cells.Value2.Equals(string.Empty))
			{
				activeColumn = currentApp.ActiveCell.Column + profileCounter * 2;
				if (dataLength > 1)
					activeColumn += dataLength - 1;
			}

			return activeColumn;
		}

		private static void PresentResult(Query query, Report report, int profileCounter, int cellOffset)
		{
			// This method shall include a verification of empty columns. If the user has selected more than one profile
			// the second query shall be presented on the right of the first query result in Excel.

			Microsoft.Office.Interop.Excel.Application currentApp = GA_Excel2007.Globals.ThisAddIn.Application;
			Worksheet activeSheet = currentApp.ActiveSheet as Worksheet;

			if (currentApp.ActiveSheet != null)
			{
				int dataLength = 2;
				if (report.Data != null)
				{
					dataLength = report.Data.GetLength(1);
				}

				int activeRow = currentApp.ActiveCell.Row;
				int activeColumn = currentApp.ActiveCell.Column + cellOffset;
				/*if (currentApp.ActiveCell.Cells.Value2 != null)
				{
					activeColumn = ActiveColumn(profileCounter, dataLength);
				}*/


				object[] queryInformation = GetQueryInformation(query, report, profileCounter);

                int infoRows = queryInformation.GetLength(0);
               
				if (report.Data != null)
                {
					dataLength = report.Data.GetLength(1);
                    Range dataRange = currentApp.get_Range((object)activeSheet.Cells[activeRow + infoRows + 1, activeColumn],
                    (object) activeSheet.Cells[activeRow + infoRows + report.Data.GetLength(0), activeColumn + report.Data.GetLength(1) - 1]);
					dataRange.Value2 = report.Data;

					Range headerRange = currentApp.get_Range((object)activeSheet.Cells[activeRow + infoRows, activeColumn],
					(object)activeSheet.Cells[activeRow + infoRows, activeColumn + report.Headers.GetLength(1) - 1]);
					headerRange.Value2 = report.Headers;
					headerRange.Font.Bold = true;
				}

				Range queryInformationRange = currentApp.get_Range((object)activeSheet.Cells[activeRow, activeColumn],
				(object)activeSheet.Cells[activeRow, activeColumn + dataLength - 1]);
				queryInformationRange.Font.Italic = true;
				queryInformationRange.MergeCells = true;
				queryInformationRange.Borders.Weight = XlBorderWeight.xlThin;
                               
                queryInformationRange.Value2 = queryInformation;
                //int height = (int)queryInformationRange.Height; queryInformationRange.RowHeight = height;
			}
		}

		private static void PresentResultUpdate(Query query, Report report, int profileCounter)
		{
			Microsoft.Office.Interop.Excel.Application currentApp = GA_Excel2007.Globals.ThisAddIn.Application;
			Worksheet activeSheet = currentApp.ActiveSheet as Worksheet;

			if (currentApp.ActiveSheet != null)
			{
				int activeColumn = query.Column;
				int activeRow = query.Row;
				string addressLocal = "";
				string addressHelper = "";

				object[] queryInformation = GetQueryInformation(query, report, profileCounter);

				int infoRows = queryInformation.GetLength(0);
				int dataLength = dataLength = report.Data.GetLength(1);

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

                ((Style)queryInformationRange.Style).WrapText = false;
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

		private static object[] GetQueryInformation(Query query, Report report, int profileCounter)
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
                                queryInfoIdentifier, query.ToString(profileCounter), report.Hits, query.GetDimensionsAndMetricsCount(), timePeriod)};
			return queryInformation;
		}

		private void InitLogin()
		{
            Excel2007Addin.Settings settings = Excel2007Addin.Settings.Default;

			this._login = new Login(_user);
			_login.authSuccessful += new Login.AuthSuccessful(User_Successful_Login);
			_login.logOut += new Login.Logout(User_Logout);


            _login.Username = settings.Username;
            _login.Password = DataProtectionHelper.UnProtect(settings.Password);
            _login.RememberPassword = settings.RememberPassword;
            if (_login.ShowDialog().GetValueOrDefault(false))
            {
                settings.RememberPassword = _login.RememberPassword;
                settings.Username = _login.Username;
                if (_login.RememberPassword)
                    settings.Password = DataProtectionHelper.Protect(_login.Password);
                else
                    settings.Password = DataProtectionHelper.Protect("");
                settings.Save();
            }
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

        private void SettingsButton_Click(object sender, RibbonControlEventArgs e)
        {
            Excel2007Addin.Settings settings = Excel2007Addin.Settings.Default;
            SettingsDialog settingsdlg = new SettingsDialog();

            settingsdlg.UseProxy = settings.UseProxy;
            settingsdlg.ProxyAddress = settings.ProxyAddress;
            settingsdlg.ProxyPort = settings.ProxyPort;
            settingsdlg.ProxyUsername = settings.ProxyUsername;
            settingsdlg.ProxyPassword = DataProtectionHelper.UnProtect(settings.ProxyPassword);
            settingsdlg.RequestTimeout = settings.RequestTimeout;
            settingsdlg.CellFormatting = (CellFormattingEnum)settings.CellFormatting;

            if (settingsdlg.ShowDialog().GetValueOrDefault(false))
            {   
                // Update metrics xml?
                if (!string.IsNullOrEmpty(settingsdlg.MetricsFileName))
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDataDocument();
                    try
                    {
                        doc.Load(XmlReader.Create(settingsdlg.MetricsFileName,
                                                    new XmlReaderSettings()
                                                    {
                                                        Schemas = Analytics.Data.Validation.XmlValidator.LoadSchema("metrics.xsd"),
                                                        ValidationType = ValidationType.Schema
                                                    }));
                        settings.Metrics = doc;
                    }
                    catch (Exception)
                    {
                         MessageBox.Show("Error parsing metrics xml. No metrics updated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                      
                }
                // Update dimensions xml?
                if (!string.IsNullOrEmpty(settingsdlg.DimensionsFileName))
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDataDocument();
                    try
                    {
                        doc.Load(XmlReader.Create(settingsdlg.DimensionsFileName,
                                                    new XmlReaderSettings() { 
                                                            Schemas = Analytics.Data.Validation.XmlValidator.LoadSchema("dimensions.xsd"),
                                                            ValidationType = ValidationType.Schema }));
                        settings.Dimensions = doc;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error parsing dimension xml. No dimensions updated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                settings.UseProxy = settingsdlg.UseProxy;
                settings.ProxyAddress = settingsdlg.ProxyAddress;
                settings.ProxyUsername = settingsdlg.ProxyUsername;
                settings.ProxyPassword = DataProtectionHelper.Protect(settingsdlg.ProxyPassword);
                settings.ProxyPort = settingsdlg.ProxyPort;
                settings.RequestTimeout = settingsdlg.RequestTimeout;
                settings.CellFormatting = (int)settingsdlg.CellFormatting;
                settings.Save();

                Analytics.Settings.Instance.UseProxy = settings.UseProxy;
                Analytics.Settings.Instance.ProxyAddress = settings.ProxyAddress;
                Analytics.Settings.Instance.ProxyPassword = settings.ProxyPassword;
                Analytics.Settings.Instance.ProxyPort = settings.ProxyPort;
                Analytics.Settings.Instance.ProxyUsername = settings.ProxyUsername;

                Analytics.Settings.Instance.RequestTimeout = settings.RequestTimeout;
                Analytics.Settings.Instance.MetricsXml = settings.Metrics;
                Analytics.Settings.Instance.DimensionsXml = settings.Dimensions;
            }
        }

        private void AboutButton_Click(object sender, RibbonControlEventArgs e)
        {
            About dlg = new About();
            dlg.ShowDialog();
        }

        //private string Protect(string str)
        //{
        //    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
        //    return Convert.ToBase64String(ProtectedData.Protect(encoding.GetBytes(str), randomBytes, DataProtectionScope.CurrentUser));
        //}
        //private string UnProtect(string bytes)
        //{
        //    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
        //    try
        //    {
        //        return encoding.GetString(ProtectedData.Unprotect(Convert.FromBase64String(bytes), randomBytes, DataProtectionScope.CurrentUser));
        //    }
        //    catch (Exception e)
        //    {
        //        return bytes;
        //    }
        //}
        //private void buttonUpdateWorkSheet_Click(object sender, RibbonControlEventArgs e)
        //{
        //    Worksheet _sheet = (Worksheet)GA_Excel2007.Globals.ThisAddIn.Application.ActiveSheet;

        //    Range _range = _sheet.Cells.Find(queryInfoIdentifier, _sheet.Cells.get_End(XlDirection.xlDown), Excel.XlFindLookIn.xlValues, Excel.XlLookAt.xlPart, XlSearchOrder.xlByRows,
        //              Excel.XlSearchDirection.xlNext, false, false);

        //    if (_range != null)
        //    {
        //        Query query1 = new Query();
        //        _sFirstFoundAddress = _range.get_Address(true, true, Excel.XlReferenceStyle.xlA1);
        //        if (_sFirstFoundAddress.Contains("A"))
        //        {
        //            _range = _sheet.Cells.FindNext(_range);
        //            _sFirstFoundAddress = _range.get_Address(true, true, Excel.XlReferenceStyle.xlA1);
        //        }

        //        // If Find doesn't find anything, _range will be null
        //        foreach (Range cell in _range.Cells)
        //        {
        //            _cellValue = cell.Value.ToString();

        //            query1 = new Query(GetQueryExcelParamValueFromSpecificCell("queryString", _cellValue));
        //            query1.TimePeriod = (Analytics.Data.Enums.TimePeriod)Enum.Parse(typeof(Analytics.Data.Enums.TimePeriod),
        //                                                                                                    GetQueryExcelParamValueFromSpecificCell("timePeriod", "")[1]);

        //            query1.Column = cell.Column;
        //            query1.Row = cell.Row;
        //        }

        //        _listQueries = new List<Query>();
        //        _listQueries.Add(query1);
        //        string sAddress = "";

        //        while (!sAddress.Equals(_sFirstFoundAddress))
        //        {

        //            _range = _sheet.Cells.FindNext(_range);
        //            sAddress = _range.get_Address(
        //                true, true, Excel.XlReferenceStyle.xlA1);
        //            if (sAddress.Equals(_sFirstFoundAddress))
        //                break;

        //            foreach (Range cell in _range.Cells)
        //            {
        //                if (cell.Address == sAddress)
        //                {
        //                    _cellValue = cell.Value.ToString();

        //                    Query query = new Query(GetQueryExcelParamValueFromSpecificCell("queryString", _cellValue));
        //                    query.TimePeriod = (Analytics.Data.Enums.TimePeriod)Enum.Parse(typeof(Analytics.Data.Enums.TimePeriod),
        //                                                                                    GetQueryExcelParamValueFromSpecificCell("timePeriod", "")[1]);
        //                    query.Column = cell.Column;
        //                    query.Row = cell.Row;
        //                    _listQueries.Add(query);
        //                }
        //            }
        //        }
        //        //            LaunchWorkSheetUpdate();
        //        _workSheetUpdateWindow = new WorkSheetUpdate(_user, _listQueries);
        //        _workSheetUpdateWindow.queryComplete += new WorkSheetUpdate.QueryComplete(queryComplete);
        //        _workSheetUpdateWindow.Queries = _listQueries;
        //        _workSheetUpdateWindow.ShowDialog();
        //    }
        //}




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
