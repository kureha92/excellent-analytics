using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using System.Xml;
using System.Drawing.Text;

using Analytics.Data;
using UI.Controls;
using Analytics.Authorization;
using System.Windows.Interop;
using Analytics.Data.Enums;


using UI;
using WPFUIv2;
using Analytics.Data.General;
using Analytics;
using System.Data;
using System.Collections;
using System.Collections.ObjectModel;


namespace UI
{
    /// <summary>
    /// Interaction logic for QueryBuilder.xaml
    /// </summary>
    public partial class QueryBuilder : Window 
    {
        #region Fields
        DoubleAnimation _animatePropertyHeight;
        DoubleAnimation _animatePropertyWidth;

        Query _query;
        UserAccount _currentUserAccount;

        public delegate void QueryComplete(Query query);
        public event QueryComplete queryComplete;

        public enum ListType { Dim, Met, Fil, Sort, Profile};

        private const int maxSupportedDimensions = 7;
        private const int maxSupportedMetrics = 10;

        private bool queryNotCompleted = false;
        private string descending = "Descending";
        private string ascending = "Ascending";
        bool inValidParameters = false;

        SizeKeyType activeSize; 
        #endregion

        #region Properties

        public UserAccount CurrentUser
        {
            get { return _currentUserAccount; }
            set { _currentUserAccount = value; }
        }

        private List<RadioButton> TimeSpanBoxesColl
        {
            get
            {
                return new RadioButton[] { todayCheckBox, yesterdayCheckBox, weekCheckBox, weekCheckBoxAnglosax, monthCheckBox, 
                    quarterCheckBox, yearCheckBox, periodNotSpecifiedCheckBox }.Where(p => p != null).ToList<RadioButton>();
            }
        }

        private DoubleAnimation AnimatePropertyHeight
        {
            get
            {
                return _animatePropertyHeight != null ? _animatePropertyHeight :
                new DoubleAnimation(0.0, 259.0, new Duration(TimeSpan.FromSeconds(0.2))) { DecelerationRatio = 0.2 };
            }
            set { _animatePropertyHeight = value; }
        }

        private DoubleAnimation AnimatePropertyWidth
        {
            get
            {
                return _animatePropertyWidth != null ? _animatePropertyWidth :
                new DoubleAnimation(0.0, 543.0, new Duration(TimeSpan.FromSeconds(0.2))) { DecelerationRatio = 0.9 };
            }
            set { _animatePropertyWidth = value; }
        } 
        #endregion

        public QueryBuilder(UserAccount userAccount, Query query)
        {
            double pixelHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            if (pixelHeight < 800)
            {

                SnapsToDevicePixels = true;

            }

            InitializeComponent();
            if (pixelHeight > 800)
            SizeToContent = System.Windows.SizeToContent.WidthAndHeight;

            this._query = query != null ? query : new Query();
            _currentUserAccount = userAccount;
            InitializeForm();
            SetTimePeriod(query);
        }


        #region Events

        private void addProfile_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxProfile.SelectedItem != null)
            {
                Item pItem = new Item(_query.ProfileId.First().Value, _query.ProfileId.First().Key);
                _query.Ids.Add(pItem);
                BindProfileListBox();
            }
        }

        private void BindProfileListBox()
        {
            MultipleProfiles multiProfiles = new MultipleProfiles();
            Binding profileBinding = new Binding();
            MultipleProfiles multipleProfiles = new MultipleProfiles();
            Item pItem;
            if (_query.Ids.Count == 0)
            {
                pItem = new Item(_query.ProfileId.First().Value, _query.ProfileId.First().Key);
                multipleProfiles.Add(pItem);
                _query.Ids = multipleProfiles;
            }

            foreach (Item item in _query.Ids)
            {
                multiProfiles.Add(item);
            }

            _query.Ids = multiProfiles;
            profileBinding.Source = multiProfiles;
            profileBox.SetBinding(ListBox.ItemsSourceProperty, profileBinding);
        }

        private void removeProfile_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxProfile.SelectedIndex != -1)
            {
                _query.Ids.RemoveAt(profileBox.SelectedIndex);
                BindProfileListBox();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        void Timespan_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton sendCheck = sender as RadioButton;
            foreach (RadioButton itBox in TimeSpanBoxesColl)
                itBox.IsChecked = itBox.Name == sendCheck.Name;

            setCalendarToDefault();

        }



        private void Expand(object sender, RoutedEventArgs e)
        {
            Button callButton = sender as Button;
            Expander targetExpander = callButton.Parent as Expander;
            bool isExpanded = targetExpander.IsExpanded;
            VisualStateManager.GoToState(callButton, isExpanded ? "Normal"  : "Pressed", true);
            targetExpander.IsExpanded = !isExpanded;
            if (!isExpanded)
                DataBindSortByDropDown();
        }

        private void DimensionsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            MetricsExpander.IsExpanded = false;
            FilterExpander.IsExpanded = false;
            SortExpander.IsExpanded = false;
            DimensionsView.Visibility = Visibility.Visible;
            DimensionsView.BeginAnimation(TreeView.WidthProperty, AnimatePropertyWidth);
            DimensionsView.BeginAnimation(TreeView.HeightProperty, AnimatePropertyHeight); 
        }

        private void DimensionsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            if (validationActivate.IsChecked == true)
                inValidParameters = ValidationHandler();
            _query.Dimensions.Clear();
            _query.Dimensions = GetCheckedItems(DimensionsView.tree.Items[0] as SizeViewModel);
            BindSizeList(ListType.Dim);
            DataBindSortByDropDown();
            BindSortListBox();
            BindSizeList(ListType.Sort);

            // Verifies that erased parameters are not used in a filter.
            filterCheck();
            textBoxExpression.Clear();
            BindFilterListBox();

            if (inValidParameters)
            {
                DimensionsExpander.IsExpanded = true;
            }
            else
            {
                // The maximum number of dimensions is seven.
                if (!NumberOfDimensions())
                {
                    MainNotify.Visibility = Visibility.Collapsed;
                    MainNotify.ErrorMessage = string.Empty;
                }
                else
                {
                    DimensionsExpander.IsExpanded = true;
                    ValidateForm();

                }                
            }
        }


        private void MetricsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            DimensionsExpander.IsExpanded = false;
            FilterExpander.IsExpanded = false;
            SortExpander.IsExpanded = false;
            MetricsView.Visibility = Visibility.Visible;
            MetricsView.BeginAnimation(TreeView.WidthProperty, AnimatePropertyWidth);
            MetricsView.BeginAnimation(TreeView.HeightProperty, AnimatePropertyHeight);
        }




        private void MetricsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            if (validationActivate.IsChecked == true)
                inValidParameters = ValidationHandler();
            if (inValidParameters)
            {
                MetricsExpander.IsExpanded = true;
            }
            _query.Metrics.Clear();
            // The maximum number of metrics is ten.
            if (!NumberOfMetrics())
            {
                _query.Metrics = GetCheckedItems(MetricsView.tree.Items[0] as SizeViewModel);
                BindSizeList(ListType.Met);
                DataBindSortByDropDown();
                BindSortListBox();
                BindSizeList(ListType.Sort);

                // Verifies that an erased parameter is not used in a filter.
                filterCheck();
                textBoxExpression.Clear();
                BindFilterListBox();

                MainNotify.Visibility = Visibility.Collapsed;
                MainNotify.ErrorMessage = string.Empty;
            }
            else
            {
                MetricsExpander.IsExpanded = true;
                ValidateForm();
            }
        }
         
        
        private void CancelBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void FilterExpander_Expanded(object sender, RoutedEventArgs e)
        {
            MetricsExpander.IsExpanded = false;
            DimensionsExpander.IsExpanded = false;
            SortExpander.IsExpanded = false;
            FilterCanvas.BeginAnimation(Canvas.WidthProperty, AnimatePropertyWidth);
            FilterCanvas.BeginAnimation(Canvas.HeightProperty, AnimatePropertyHeight);

            BindFilterListBox();

            Binding dimBinding = new Binding();
            dimBinding.Source = _query.Dimensions;
            Binding metBinding = new Binding();
            metBinding.Source = _query.Metrics;
            Binding segBinding = new Binding();
            comboBoxDimensions.SetBinding(ComboBox.ItemsSourceProperty, dimBinding);
            comboBoxMetrics.SetBinding(ComboBox.ItemsSourceProperty, metBinding);
        }

        private void FilterExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            BindSizeList(ListType.Fil);
        }

        private void comboBoxSegments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxSegments.SelectedItem != null)
            {
                this._query.Segments.Clear();
                this._query.Segments.Add((comboBoxSegments.SelectedItem as UserSegment).SegmentName, (comboBoxSegments.SelectedItem as UserSegment).SegmentId);

            }
        }

        private void cancelFilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterExpander.IsExpanded = false;
        }

        private void comboBoxMetrics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            activeSize = SizeKeyType.Metric;
            BindOperatorList(SizeKeyType.Metric);
        }

        private void comboBoxMetrics_DropDownOpened(object sender, EventArgs e)
        {
            comboBoxDimensions.SelectedIndex = -1;
            activeSize = SizeKeyType.Metric;
            BindOperatorList(SizeKeyType.Metric);
        }

        private void comboBoxDimensions_DropDownOpened(object sender, EventArgs e)
        {
            comboBoxMetrics.SelectedIndex = -1;
            activeSize = SizeKeyType.Dimension;
            BindOperatorList(SizeKeyType.Dimension);
        }

        private void comboBoxDimensions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            activeSize = SizeKeyType.Dimension;
            BindOperatorList(SizeKeyType.Dimension);
        }

        private void addFilter_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxOperator.SelectedIndex != -1 && !String.IsNullOrEmpty(textBoxExpression.Text))
                AddFilter(activeSize);
        }

        private void removeFilter_Click(object sender, RoutedEventArgs e)
        {
            if (filterBox.SelectedIndex != -1)
            {
                _query.Filter.RemoveAt(filterBox.SelectedIndex);
                BindFilterListBox();
            }
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                CompleteQuery();
                this.Close();
                SetSelectedDates();
                queryComplete(_query);
            }
        }



        private void SortDropDown_MouseEnter(object sender, RoutedEventArgs e)
        {
            _query.Metrics.Clear();
            _query.Metrics = GetCheckedItems(MetricsView.tree.Items[0] as SizeViewModel);

            _query.Dimensions.Clear();
            _query.Dimensions = GetCheckedItems(DimensionsView.tree.Items[0] as SizeViewModel);

            DataBindSortByDropDown();
        }

        private void ExecuteButton_MouseEnter(object sender, MouseEventArgs e)
        {
            SortDropDown_MouseEnter(sender, e);

            _query.Metrics.Clear();
            _query.Metrics = GetCheckedItems(MetricsView.tree.Items[0] as SizeViewModel);

            _query.Dimensions.Clear();
            _query.Dimensions = GetCheckedItems(DimensionsView.tree.Items[0] as SizeViewModel);

            if (ValidateForm())
            {
                MainNotify.Visibility = Visibility.Collapsed;
                MainNotify.ErrorMessage = string.Empty;
            }
         }

        private void ExecuteButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ExecuteButton.IsEnabled = true;
            MainNotify.Visibility = Visibility.Collapsed;
            MainNotify.ErrorMessage = string.Empty;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void logOPBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (filterBox.Items.Count > 0)
            {
                ComboBox cBox = sender as ComboBox;
                filterBox.SelectedItem = (cBox.Parent as StackPanel).DataContext;
                if (filterBox.SelectedItem != null)
                {
                    this._query.Filter[filterBox.SelectedIndex].LOperator =
                    cBox.SelectedIndex == 0 ? LogicalOperator.And : LogicalOperator.Or;
                }
            }
        }

        private void TimeSpanTab_MouseEnter(object sender, MouseEventArgs e)
        {
            if (startDateCalendar.SelectedDate.Value.Date != DateTime.Today || endDateCalendar.SelectedDate.Value.Date != DateTime.Today)
            {
                yesterdayCheckBox.IsChecked = false;
                weekCheckBox.IsChecked = false;
                weekCheckBoxAnglosax.IsChecked = false;
                monthCheckBox.IsChecked = false;
                quarterCheckBox.IsChecked = false;
                yearCheckBox.IsChecked = false;

            }        
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_query.Metrics.Count > 0)
            {
                SetCheckedItems(_query.Metrics, MetricsView.tree.Items[0] as SizeViewModel);
            }
            if (_query.Dimensions.Count > 0)
            {
                SetCheckedItems(_query.Dimensions, DimensionsView.tree.Items[0] as SizeViewModel);
            }
        }




        void DimensionsView_treeDatabound()
        {
            SetCheckedItems(_query.Dimensions, DimensionsView.tree.Items[0] as SizeViewModel);
        }

        private void validate_int(object sender, TextChangedEventArgs e)
        {
            int i;
            if (!int.TryParse((sender as TextBox).Text, out i))
                (sender as TextBox).Text = string.Empty;
        }

        private void listOrder_Click(object sender, RoutedEventArgs e)
        {
            if (listOrder.Content.Equals(ascending))
            {
                listOrder.Content = descending;
            }
            else
            {
                listOrder.Content = ascending;
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();

            about.Show();

        }

        private void ContentPresenter_MouseEnter(object sender, MouseEventArgs e)
        {
            ((ContentPresenter)sender).ToolTip = ((ContentPresenter)sender).Content.ToString();
        }

        private void addSort_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(sortBycomboBox.Text))
                AddSortByParam(activeSize);
        }


        private void removeSortBy_Click(object sender, RoutedEventArgs e)
        {
            if (sortingBox.SelectedIndex != -1)
            {
                _query.Sort.RemoveAt(sortingBox.SelectedIndex);
                BindSortListBox();
            }
        }

        private void comboBoxAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxAccount.SelectedItem != null)
            {
                this._query.AccountId.Clear();
                this._query.AccountId.Add((comboBoxAccount.SelectedItem as Entry).AccountName, (comboBoxAccount.SelectedItem as Entry).AccountId);
                DataBindSitesDropDown();
            }
        }

        private void comboBoxProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxProfile.SelectedItem != null)
            {
                this._query.ProfileId.Clear();
                this._query.ProfileId.Add((comboBoxProfile.SelectedItem as Entry).Title, (comboBoxProfile.SelectedItem as Entry).ProfileId);
                DataBindSitesDropDown();
            }
        }

        private void SortExpander_Expanded(object sender, RoutedEventArgs e)
        {
            MetricsExpander.IsExpanded = false;
            DimensionsExpander.IsExpanded = false;
            FilterExpander.IsExpanded = false;
            SortingCanvas.Visibility = Visibility.Visible;
            SortingCanvas.BeginAnimation(Canvas.WidthProperty, AnimatePropertyWidth);
            SortingCanvas.BeginAnimation(Canvas.HeightProperty, AnimatePropertyHeight);
            BindSortListBox();
        }

        private void SortExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            BindSizeList(ListType.Sort);
        }

        private void ProfileExpander_Expanded(object sender, RoutedEventArgs e)
        {
            MetricsExpander.IsExpanded = false;
            DimensionsExpander.IsExpanded = false;
            FilterExpander.IsExpanded = false;
            SortExpander.IsExpanded = false;
            ProfileCanvas.Visibility = Visibility.Visible;
            ProfileCanvas.BeginAnimation(Canvas.WidthProperty, AnimatePropertyWidth);
            ProfileCanvas.BeginAnimation(Canvas.HeightProperty, AnimatePropertyHeight);
            BindProfileListBox();
        }

        private void ProfileExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            BindSizeList(ListType.Profile);
        }

        private void validationActivate_Unchecked(object sender, RoutedEventArgs e)
        {
            SizeViewModel dimCategories = DimensionsView.tree.Items[0] as SizeViewModel;
            SizeViewModel metCategories = MetricsView.tree.Items[0] as SizeViewModel;
            EnableAllMetrics(metCategories);
            EnableAllDimenions(dimCategories);
        }

        private void DimensionsExpander_MouseMove(object sender, MouseEventArgs e)
        {
            _query.Dimensions.Clear();
            _query.Dimensions = GetCheckedItems(DimensionsView.tree.Items[0] as SizeViewModel);
            if (validationActivate.IsChecked == true)
                if (ValidationHandler())
                {
                    InvalidCombination invalidCombo = new InvalidCombination();
                    invalidCombo.Show();
                }
            if (_query.Dimensions.Count > 0)
            {
                SetCheckedItems(_query.Dimensions, DimensionsView.tree.Items[0] as SizeViewModel);
            }
        }

        private void MetricsExpander_MouseMove(object sender, MouseEventArgs e)
        {
            _query.Metrics.Clear();
            _query.Metrics = GetCheckedItems(MetricsView.tree.Items[0] as SizeViewModel);
            if (validationActivate.IsChecked == true)
                if (ValidationHandler())
                {
                    InvalidCombination invalidCombo = new InvalidCombination();
                    invalidCombo.Show();
                }
            if (_query.Metrics.Count > 0)
            {
                SetCheckedItems(_query.Metrics, MetricsView.tree.Items[0] as SizeViewModel);
            }
        }

        #endregion

        #region Methods

        private void InitializeForm()
        {
            BindSizeList(ListType.Dim);
            BindSizeList(ListType.Met);
            BindSizeList(ListType.Fil);
            BindSizeList(ListType.Sort);
            BindSizeList(ListType.Profile);

            if (_query.SortParams != null && _query.SortParams.Count > 0)
            {
                //Select the orderItem från query list.
                sortBycomboBox.SelectedValue = _query.SortParams.First().Value;
            }

            if (_query.GetMetricsAndDimensions.Count() > 0)
            {
                queryNotCompleted = true;
                DataBindSortByDropDown();
            }


            if (CurrentUser != null)
            {
                DataBindAccountsDropDown();
                DataBindSegmentsDropDown();
            }

            this._query.AccountId.OrderBy(x => x);

            // Setting the first account in the list
            if (this._query.AccountId.Count > 0)
            {
                string aID = this._query.AccountId.First().Value;
                if (_currentUserAccount.Entrys.Find(p => p.AccountId == aID) != null)
                    comboBoxAccount.SelectedValue = _currentUserAccount.Entrys.Find(p => p.AccountId == aID);
                else
                    Notify("You have no access to any accounts");
            }
            else if (_currentUserAccount != null)
                comboBoxAccount.SelectedIndex = _currentUserAccount.Entrys.Count > 0 ? 0 : -1;


            // The profiles are shown based on wich account is selected.
            if (this._query.ProfileId.Count > 0)
            {
                string pId = this._query.Ids.ToSimplifiedString();
                if (_currentUserAccount.Entrys.Find(p => p.ProfileId == pId) != null)
                {
                    foreach (Entry entry in _currentUserAccount.Entrys)
                    {
                        if (entry.ProfileId == pId) 
                        {
                            comboBoxAccount.SelectedValue = _currentUserAccount.Entrys.Find(p => p.AccountId == entry.AccountId); ;        
                        }
                    }
                    comboBoxProfile.SelectedValue = _currentUserAccount.Entrys.Find(p => p.ProfileId == pId);
                    
                }
                else
                    Notify("Your account lacks permission on the target profile");
            }
            else if (_currentUserAccount != null)
                comboBoxProfile.SelectedIndex = _currentUserAccount.Entrys.Count > 0 ? 0 : -1;

            if (CurrentUser != null)
            {
                DataBindSitesDropDown();
            }

            if (this._query.Segments.Count > 0)
            {
                string sId = this._query.Segments.First().Value;
                if (_currentUserAccount.Segments.Find(p => p.SegmentId == sId) != null)
                    comboBoxSegments.SelectedValue = _currentUserAccount.Segments.Find(p => p.SegmentId == sId);
            }
            else if (_currentUserAccount != null)
                comboBoxSegments.SelectedIndex = _currentUserAccount.Segments.Count > 0 ? 0 : -1;

            SetCalendars();

            activeSize = SizeKeyType.Dimension;

            startIndexTextBox.Text = _query.StartIndex.ToString();
            maxResultsTextBox.Text = _query.MaxResults.ToString();

            
        }


        private void DataBindSortByDropDown()
        {
            Binding sites = new Binding();
            sites.Source = _query.GetMetricsAndDimensions;
            sortBycomboBox.SetBinding(ComboBox.ItemsSourceProperty, sites);
        }

        private void Notify(string message)
        {
            MainNotify.Visibility = Visibility.Visible;
            MainNotify.ErrorMessage = message;
        }

        private void DataBindSitesDropDown()
        {
            Binding sites = new Binding();
            List<Entry> filtering = new List<Entry>();
                foreach (Entry item in CurrentUser.Entrys)
                {
                    if (item.AccountId == (comboBoxAccount.SelectedItem as Entry).AccountId)
                        {
                            filtering.Add(item);
                        }
                }
            sites.Source = filtering;
            comboBoxProfile.SetBinding(ComboBox.ItemsSourceProperty, sites);
        }

        private void DataBindAccountsDropDown()
        {
            Binding accounts = new Binding();
            List<Entry> duplicationCheck = new List<Entry>();
            foreach (Entry entry in CurrentUser.Entrys)
            {
                bool check = false;
                foreach (Entry duplic in duplicationCheck)
                {
                    if (duplic.AccountId == entry.AccountId)
                        check = true;
                }

                if (check.Equals(false))
                {
                    duplicationCheck.Add(entry);
                }
                
            }
            accounts.Source = duplicationCheck;
            comboBoxAccount.SetBinding(ComboBox.ItemsSourceProperty, accounts);
        }

        private void DataBindSegmentsDropDown()
        {
            Binding segments = new Binding();
            segments.Source = CurrentUser.Segments;
            comboBoxSegments.SetBinding(ComboBox.ItemsSourceProperty, segments);        
        }
        

        
        private void AddFilter(SizeKeyType size)
        {
            ComboBox sizeRefBox = size == SizeKeyType.Dimension ? comboBoxDimensions : comboBoxMetrics;
            if (sizeRefBox.SelectedItem != null)
            {
                KeyValuePair<string, string> item = (sizeRefBox.SelectedItem as Nullable<KeyValuePair<string, string>>).Value;
                KeyValuePair<string, string> selectedOperator = (comboBoxOperator.SelectedItem as Nullable<KeyValuePair<string, string>>).Value;
                FilterItem fItem = new FilterItem(item.Key, item.Value,
                         new SizeOperator(selectedOperator.Key, selectedOperator.Value), textBoxExpression.Text,
                         (SizeKeyType)size, _query.Filter.Count == 0 ?
                         LogicalOperator.None : LogicalOperator.And);
                _query.Filter.Add(fItem);
                BindFilterListBox();
            }
        }

        private void BindFilterListBox()
        {
            Filter f = new Filter();
            Binding filterBinding = new Binding();
            foreach (FilterItem item in _query.Filter)
                f.Add(item);
            if (f.Count == 1)
                f[0].LOperator = LogicalOperator.None;
            filterBinding.Source = f;
            filterBox.SetBinding(ListBox.ItemsSourceProperty, filterBinding);
            
        }

        // Bindes checked values and/parameters to the XAML.
        private void BindSizeList(ListType type)
        {
            Binding binding = new Binding();
            switch (type)
            {
                case ListType.Dim:
                    binding.Source = _query.Dimensions;
                    dimensionsSelected.SetBinding(ListBox.ItemsSourceProperty, binding);
                    break;
                case ListType.Met:
                    binding.Source = _query.Metrics;
                    metricsSelected.SetBinding(ListBox.ItemsSourceProperty, binding);
                    break;
                case ListType.Fil:
                    binding.Source = _query.Filter.ToSimplifiedList();
                    activeFilters.SetBinding(ListBox.ItemsSourceProperty, binding);
                    break;
                case ListType.Sort:
                    binding.Source = _query.Sort.ToSimplifiedList();
                    activeSortings.SetBinding(ListBox.ItemsSourceProperty, binding);
                    break;
                case ListType.Profile:
                    binding.Source = _query.Ids.ToSimplifiedList();
                    activeProfiles.SetBinding(ListBox.ItemsSourceProperty, binding);
                    break;
                default:
                    break;
            }
        }

        private void BindOperatorList(SizeKeyType activeSize)
        {
            Binding opBind = new Binding();
            opBind.Source = Query.GetOperatorCollection(activeSize);
            comboBoxOperator.SetBinding(ComboBox.ItemsSourceProperty, opBind);
        }

        private Dictionary<string, string> GetCheckedItems(SizeViewModel customTreeItems)
        {
            Dictionary<string, string> checkedSizes = new Dictionary<string, string>();
            foreach (SizeViewModel item in (customTreeItems).Children)
                foreach (SizeViewModel subItem in item.Children)
                {
                    if (subItem.IsChecked == true)
                    {
                        checkedSizes.Add(subItem.Name, subItem.Value);
                    }
                }

            return checkedSizes;
        }

        
        private void SetCalendars()
        {
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
            if (this._query != null && _query.StartDate.Year != 1 && _query.EndDate.Year != 1)
            {
                startDate = _query.StartDate;
                endDate = _query.EndDate;
            }
            startDateCalendar.SelectedDate = startDate;
            startDateCalendar.DisplayDate = startDate;
            endDateCalendar.SelectedDate = endDate;
            endDateCalendar.DisplayDate = endDate;

        }

        private void SetCheckedItems(Dictionary<string, string> sizeColl, SizeViewModel customTreeItems)
        {
            foreach (SizeViewModel category in customTreeItems.Children)
                foreach (SizeViewModel size in category.Children)
                    if (sizeColl.Keys.Contains(size.Name))
                        size.IsChecked = true;
        }

        // Validation rules. If the rules are not followed a message will appear in the GUI.
        // It is not possible to execute a question when the validation rules are not followed.
        private bool ValidateForm()
        {
            if (inValidParameters)
            {
                Notify("Your combination of metrics and dimensions is not valid. Valid combinations are described in Google's documentation: http://code.google.com/intl/sv/apis/analytics/docs/gdata/gdataReferenceValidCombos.html#explore2Pair");
                return false;
            }

            if (NumberOfDimensions())
            {
                Notify("You have exceeded the maximum limit of dimensions selected. Maximum is seven.");
                return false;
            }
            if (NumberOfMetrics())
            {
                Notify("You have exceeded the maximum limit of metrics selected. Maximum is ten.");
                return false;
            }
            if (comboBoxProfile.SelectedItem == null)
            {
                Notify("No profile is selected");
                return false;
            }
            if (startDateCalendar.SelectedDate > endDateCalendar.SelectedDate)
            {
                Notify("The start date can not be later than the end date");
                return false;
            }
            if (!(_query.Metrics.Count > 0))
            {
                Notify("Select atleast one metric");
                return false;
            }
            if (startDateCalendar.SelectedDate.Value.Date.Equals(DateTime.Today.Date) && !radioButtonChecked())
            {
                Notify("The start date can not be set to today");
                return false;
            }
            if (_query.Dimensions.Count > maxSupportedDimensions)
            {
                _query.Dimensions = _query.Dimensions.Take(maxSupportedDimensions).ToDictionary(k => k.Key, v => v.Value);
            }
            if (_query.Metrics.Count > maxSupportedMetrics)
            {
                _query.Metrics = _query.Metrics.Take(maxSupportedMetrics).ToDictionary(k => k.Key, v => v.Value);
            }
            return true;
        }

        // Checks if a radio button in the Time Period tab is selected.
        private bool radioButtonChecked()
        {
            bool radioButton = false;

            foreach (RadioButton radio in TimeSpanBoxesColl)
            {
                if (radio.IsChecked.Value)
                {
                    radioButton = true;
                    break;
                }
            }

            return radioButton;
        }


        private void AddSortByParam(SizeKeyType size)
        {
            if (sortBycomboBox.SelectedItem != null)
            {
                KeyValuePair<string, string> item = (sortBycomboBox.SelectedItem as Nullable<KeyValuePair<string, string>>).Value;
                string key = item.Key;
                if (listOrder.Content.Equals(descending) && !item.Key.Contains("-"))
                {
                    key = "-" + item.Key;
                }

                if (!(_query.SortParams.Keys.Contains(item.Key)))
                {
                    Item sItem = new Item(item.Value, key);
                    _query.Sort.Add(sItem);
                }
                BindSortListBox();
            }
        }

        private void BindSortListBox()
        {
            Sort s = new Sort();
            Binding sortBinding = new Binding();

            foreach (Item item in _query.Sort)
            {
                foreach (string value in _query.GetFriendlyMetricsAndDimensions)
                {
                    if (item.Value.Contains(value))
                    {
                        s.Add(item);
                        break;
                    }
                }
            }
            _query.Sort = s;
            sortBinding.Source = s;
            sortingBox.SetBinding(ListBox.ItemsSourceProperty, sortBinding);
        }

        private void RetractQueryStartDate(int days)
        {
            if (startDateCalendar != null)
            {
                startDateCalendar.SelectedDate = DateTime.Now.AddDays(days * -1);
                startDateCalendar.DisplayDate = DateTime.Now.AddDays(days * -1);
            }
        }

        // Verifies that filters based on unchecked dimensions or metrics are erased.
        private void filterCheck()
        {
            Filter validFilters = new Filter();

            foreach (string key in _query.Dimensions.Keys)
            {
                foreach (FilterItem filterItem in _query.Filter)
                {
                    if (filterItem.Key.Equals(key))
                    {
                        validFilters.Add(filterItem);
                    }
                }
            }

            foreach (string key in _query.Metrics.Keys)
            {
                foreach (FilterItem filterItem in _query.Filter)
                {
                    if (filterItem.Key.Equals(key))
                    {
                        validFilters.Add(filterItem);
                    }
                }
            }

            comboBoxOperator.SelectedIndex = -1;
            _query.Filter = validFilters;
        }

        // Google have got limits of dimensions and metrics a question can contain.
        private bool NumberOfDimensions()
        {
            bool exceeded = false;
            if (7 < (GetCheckedItems(DimensionsView.tree.Items[0] as SizeViewModel).Count))
            {
                exceeded = true;
            }
            return exceeded;
        }

        private bool NumberOfMetrics()
        {
            bool exceeded = false;
            if (10 < (GetCheckedItems(MetricsView.tree.Items[0] as SizeViewModel).Count))
            {
                exceeded = true;
            }
            return exceeded;
        }

        private void CompleteQuery()
        {

            if (startDateCalendar.SelectedDate.Value.Date.Equals(DateTime.Today))
            {
                _query.TimePeriod = (TimePeriod)Enum.Parse(typeof(TimePeriod), TimeSpanBoxesColl.Where(p => (bool)p.IsChecked).First().Tag.ToString());
                _query.SelectDates = false;
            }
            else
            {
                _query.EndDate = (DateTime)endDateCalendar.SelectedDate;
                _query.SelectDates = true;
            }

            if (_query.Ids.Count == 0)
            {
                Item pItem = new Item(_query.ProfileId.First().Value, _query.ProfileId.First().Key);
                MultipleProfiles multipleProfile = new MultipleProfiles();
                multipleProfile.Add(pItem);
                _query.Ids = multipleProfile;
            }

            // If the user have not entered a interval of hits to view, then set them static here before calling GA.
//            if (startIndexTextBox.Text.Equals(""))
//                startIndexTextBox.Text = "0";
            if (maxResultsTextBox.Text.Equals("0"))
                maxResultsTextBox.Text = "10000";

            _query.StartIndex = int.Parse(startIndexTextBox.Text);
            _query.MaxResults = int.Parse(maxResultsTextBox.Text);

            if (sortBycomboBox.SelectedIndex != -1)
            {
                queryNotCompleted = false;
                ListSortOrder();
            }


//            _query.Ids.Clear();
//            _query.Ids.Add((comboBoxProfile.SelectedItem as Entry).Title, (comboBoxProfile.SelectedItem as Entry).ProfileId);
        }


        /*@author Daniel Sandberg
         * This method verifies that the character for sorting descendent are correct.
         */
        private void ListSortOrder()
        {
            foreach (Item sort in _query.Sort)
            {
                if (!sort.Key.Contains("-") && sort.Value.Contains("-"))
                {
                    sort.Key = "-" + sort.Key;
                    sort.Value = sort.Value.Substring(1);
                    _query.SortParams.Add(sort.Key, sort.Value);
                }
                else
                {
                    _query.SortParams.Add(sort.Key, sort.Value);
                }

            }
        }

        public bool ValidationHandler()
        {
            SizeViewModel dimCategories =  DimensionsView.tree.Items[0] as SizeViewModel;
            SizeViewModel metCategories = MetricsView.tree.Items[0] as SizeViewModel;
            DimensionBasedValidation inValidMetrics = new DimensionBasedValidation();
            MetricBasedValidation inValidDimensions = new MetricBasedValidation();
            List<string> prohibitedParameter = new List<string>();
            List<string> prohibitedHelper = new List<string>();
            bool inValidCombo = false;

            // All previous invalidated metrics and dimensions are set to valid.
            EnableAllMetrics(metCategories);
            EnableAllDimenions(dimCategories);
            prohibitedParameter.Clear();
            prohibitedHelper.Clear();
            _query.Metrics.Clear();
            _query.Dimensions.Clear();


            /*
                This foreach-loop checks which dimensions that has been selected in the GUI. 
             *  Based on the selected dimensions check boxes are greyed out. This concerns both metrics and dimensions.
             */
            foreach (SizeViewModel dimensions in dimCategories.Children)
            {
                foreach (SizeViewModel dimension in dimensions.Children)
                {
                    if (dimension.IsChecked.Equals(true))
                    {
                        // This row of code gathers invalid dimensions and metrics based on selected dimensions in the GUI.
                        prohibitedHelper = inValidMetrics.prohibitedMetrics(dimension.Name);
                        foreach (string prohibitedMet in prohibitedHelper)
                        {
                            prohibitedParameter.Add(prohibitedMet);
                        }

                        prohibitedHelper.Clear();

                        // This row of code gathers invalid dimensions and metrics based on selected metrics in the GUI.
                        prohibitedHelper = inValidDimensions.prohibitedDimension(dimension.Name);
                        foreach (string prohibitedDim in prohibitedHelper)
                        {
                            prohibitedParameter.Add(prohibitedDim);
                        }
                    }
                }
            }

            /* This foreach-loop checks which metrics that has been selected in the GUI.
             * Based on the selected metrics check boxes are invalidated. This concerns both metric and dimension check boxes.
             */
            foreach (SizeViewModel metrics in metCategories.Children)
            {
                foreach (SizeViewModel metric in metrics.Children)
                {
                    if (metric.IsChecked == true)
                    {
                        prohibitedHelper = inValidMetrics.prohibitedMetrics(metric.Name);
                        foreach (string prohibitedMet in prohibitedHelper)
                        {
                            prohibitedParameter.Add(prohibitedMet);
                        }

                        prohibitedHelper = inValidDimensions.prohibitedDimension(metric.Name);
                        foreach (string prohibitedDim in prohibitedHelper)
                        {
                            prohibitedParameter.Add(prohibitedDim);
                        }

                    }
                }
            }

            /* It is time to check for invalid multi-combinations.
             */
//            InvalidMultiCombinations multiComboInvalids = new InvalidMultiCombinations();

            List<string> metDimList = new List<string>();
            //            metDimList = multiComboInvalids.MultiCombo(multiCombo);

            // Invalid dimensions and metrics returned from the multi-combination class are added to this class list of invalid values.
            foreach (string prohibitedParam in metDimList)
            {
                prohibitedParameter.Add(prohibitedParam);
            }

            // The dimension window will not close when an invalid combination is selected.
            bool dimensionInvalidationCheck = false;
            foreach (SizeViewModel dimensions in dimCategories.Children)
            {
                Dictionary<string, string> dimList = new Dictionary<string, string>();
                foreach (SizeViewModel dimension in dimensions.Children)
                {
                    foreach (string prohibited in prohibitedParameter)
                    {
                        if (dimension.Name.Equals(prohibited) && dimension.IsChecked == true)
                        {
                            inValidCombo = true;
                            dimension.IsChecked = false;
                            dimension.IsEnabled = true;
                            dimensionInvalidationCheck = true;

                        }
                        else if (dimension.Name.Equals(prohibited))
                        {
                            dimension.IsEnabled = false;
                        }
                    }

                    if (dimension.IsChecked == true)
                        dimList.Add(dimension.Name, dimension.Value);
                }
                if (dimensionInvalidationCheck)
                {
                    _query.Dimensions = dimList;
                    BindSizeList(ListType.Dim);                
                }
            }

            bool metricInvalidationCheck = false;
            foreach (SizeViewModel metrics in metCategories.Children)
            {
                Dictionary<string, string> metList = new Dictionary<string,string>();
                foreach (SizeViewModel metric in metrics.Children)
                {
                    foreach (string prohibited in prohibitedParameter)
                    {
                        if (metric.Name.Equals(prohibited) && metric.IsChecked == true)
                        {
                            inValidCombo = true;
                            metric.IsChecked = false;
                            metric.IsEnabled = true;
                            metricInvalidationCheck = true;
                        }
                        else if (metric.Name.Equals(prohibited))
                        {
                            metric.IsEnabled = false;
                        }
                    }
                    if (metric.IsChecked == true)
                        metList.Add(metric.Name, metric.Value);
                }
                if (metricInvalidationCheck)
                {
                    _query.Metrics = metList;
                    BindSizeList(ListType.Met);                
                }

            }

            return inValidCombo;
        }

        private void EnableAllMetrics(SizeViewModel metCategories)
        {
            foreach (SizeViewModel metrics in metCategories.Children)
            {
                foreach (SizeViewModel metric in metrics.Children)
                {
                    metric.IsEnabled = true;
                }
            }
        }

        private void EnableAllDimenions(SizeViewModel dimCategories)
        {
            foreach (SizeViewModel dimensions in dimCategories.Children)
            {
                foreach (SizeViewModel dimension in dimensions.Children)
                {
                    dimension.IsEnabled = true;
                }
            }
        }

        private void SetSelectedDates()
        {
            _query.StartDate = (DateTime)startDateCalendar.SelectedDate;
            _query.EndDate = (DateTime)endDateCalendar.SelectedDate;
        }

        private void SetTimePeriod(Query query)
        {
            if (!(query.Metrics.Values.Count.Equals(0)) && (_query.TimePeriod != TimePeriod.PeriodNotSpecified))
            {
                setCalendarToDefault();

                foreach (RadioButton itBox in TimeSpanBoxesColl)
                    itBox.IsChecked = query.TimePeriod.ToString() == itBox.Tag.ToString();

                timeSpanTab.IsSelected = true;
            }
        }

        private void setCalendarToDefault()
        {
            startDateCalendar.SelectedDate = DateTime.Now;
            startDateCalendar.DisplayDate = DateTime.Now;
            endDateCalendar.SelectedDate = DateTime.Now;
            endDateCalendar.DisplayDate = DateTime.Now;
        }

        #endregion



    }
}
