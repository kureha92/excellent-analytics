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

using Analytics.Data;
using UI.Controls;
using Analytics.Authorization;
using System.Windows.Interop;
using Analytics.Data.Enums;

using UI;

namespace UI
{
    /// <summary>
    /// Interaction logic for QueryBuilder.xaml
    /// </summary>
    public partial class QueryBuilder : Window
    {
        #region Fields
        DoubleAnimation expandPropertyHeight;
        DoubleAnimation expandPropertyWidth;
        
        Query query;
        UserAccount CurrentUser;

        public delegate void QueryComplete(Query query);
        public event QueryComplete queryComplete;

        public enum ListType { Dim, Met, Fil };
        bool hasInvokedDimSetCheck;
        bool hasInvokedMetSetCheck;

        private const int maxSupportedDimensions = 7;
        private const int maxSupportedMetrics = 10;

        SizeKeyType activeSize; 
        #endregion

        public QueryBuilder(UserAccount uAcc, Query query)
        {
            InitializeComponent();
            this.query = query != null ? query : new Query();
            CurrentUser = uAcc;
            InitializeForm();
        }

        public QueryBuilder(UserAccount uAcc, Query query , string errorMsg) : this(uAcc, query)
        {
            MainNotify.Visibility = Visibility.Visible;
            MainNotify.ErrorMessage = errorMsg;
        }

        #region Events

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        private void Expand(object sender, RoutedEventArgs e)
        {
            Button callButton = sender as Button;
            Expander targetExpander = callButton.Parent as Expander;
            bool isExpanded = targetExpander.IsExpanded;
            VisualStateManager.GoToState(callButton, isExpanded ? "Normal"  : "Pressed", true);
            targetExpander.IsExpanded = !isExpanded;
        }

        private void DimensionsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            MetricsExpander.IsExpanded = false;
            FilterExpander.IsExpanded = false;
            DimensionsView.Visibility = Visibility.Visible;
            DimensionsView.BeginAnimation(TreeView.WidthProperty, expandPropertyWidth);
            DimensionsView.BeginAnimation(TreeView.HeightProperty, expandPropertyHeight); 
        }

        private void DimensionsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            query.Dimensions.Clear();
            query.Dimensions = GetCheckedItems(DimensionsView.tree.Items[0] as SizeViewModel);
            BindSizeList(ListType.Dim);
        }

        private void MetricsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            DimensionsExpander.IsExpanded = false;
            FilterExpander.IsExpanded = false;

            MetricsView.Visibility = Visibility.Visible;
            MetricsView.BeginAnimation(TreeView.WidthProperty, expandPropertyWidth);
            MetricsView.BeginAnimation(TreeView.HeightProperty, expandPropertyHeight);
        }

        private void MetricsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            query.Metrics.Clear();
            query.Metrics = GetCheckedItems(MetricsView.tree.Items[0] as SizeViewModel);
            BindSizeList(ListType.Met);
        }

        private void CancelBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void FilterExpander_Expanded(object sender, RoutedEventArgs e)
        {
            MetricsExpander.IsExpanded = false;
            DimensionsExpander.IsExpanded = false;

            FilterCanvas.BeginAnimation(Grid.WidthProperty, expandPropertyWidth);
            FilterCanvas.BeginAnimation(Grid.HeightProperty, expandPropertyHeight);

            BindFilterListBox();

            Binding dimBinding = new Binding();
            dimBinding.Source = query.Dimensions;
            Binding metBinding = new Binding();
            metBinding.Source = query.Metrics;
            comboBoxDimensions.SetBinding(ComboBox.ItemsSourceProperty, dimBinding);
            comboBoxMetrics.SetBinding(ComboBox.ItemsSourceProperty, metBinding);
        }

        private void FilterExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            BindSizeList(ListType.Fil);
        }

        private void comboBoxSites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxSites.SelectedItem != null)
            {
                this.query.Ids.Clear();
                this.query.Ids.Add((comboBoxSites.SelectedItem as Entry).Title, (comboBoxSites.SelectedItem as Entry).ProfileId); 
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
            {
                AddFilter(activeSize);
            }
        }

        private void removeFilter_Click(object sender, RoutedEventArgs e)
        {
            if (filterBox.SelectedIndex != -1)
            {
                query.Filter.RemoveAt(filterBox.SelectedIndex);
                BindFilterListBox();
            }
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            string errorMsg;
            if (ValidateForm(out errorMsg))
            {
                this.query.StartDate = ToUnifiedCultureFormat((startDateCalendar.SelectedDate as Nullable<DateTime>).Value);
                this.query.EndDate = ToUnifiedCultureFormat((endDateCalendar.SelectedDate as Nullable<DateTime>).Value);

                this.query.Ids.Clear();
                this.query.Ids.Add((comboBoxSites.SelectedItem as Entry).Title, (comboBoxSites.SelectedItem as Entry).ProfileId);
                
                this.Close();
                queryComplete(this.query);
            }
        }

        private void ExecuteButton_MouseEnter(object sender, MouseEventArgs e)
        {
            query.Metrics.Clear();
            query.Metrics = GetCheckedItems(MetricsView.tree.Items[0] as SizeViewModel);

            query.Dimensions.Clear();
            query.Dimensions = GetCheckedItems(DimensionsView.tree.Items[0] as SizeViewModel);

            string errorMsg;
            if (ValidateForm(out errorMsg))
            {
                MainNotify.Visibility = Visibility.Collapsed;
                MainNotify.ErrorMessage = string.Empty;
            }
            else
            {
                MainNotify.Visibility = Visibility.Visible;
                MainNotify.ErrorMessage = errorMsg;
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
                    this.query.Filter[filterBox.SelectedIndex].LOperator =
                    cBox.SelectedIndex == 0 ? LogicalOperator.And : LogicalOperator.Or;
                }
            }
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!hasInvokedDimSetCheck && query.Metrics.Count > 0)
            {
                SetCheckedItems(query.Metrics, MetricsView.tree.Items[0] as SizeViewModel);
                hasInvokedDimSetCheck = true;
            }
            if (!hasInvokedMetSetCheck && query.Dimensions.Count > 0)
            {
                SetCheckedItems(query.Dimensions, DimensionsView.tree.Items[0] as SizeViewModel);
                hasInvokedMetSetCheck = true;
            }
        }

        #endregion

        #region Helpers

        private void AddFilter(SizeKeyType size)
        {
            ComboBox sizeRefBox = size == SizeKeyType.Dimension ? comboBoxDimensions : comboBoxMetrics;
            if (sizeRefBox.SelectedItem != null)
            {
                KeyValuePair<string, string> item = (sizeRefBox.SelectedItem as Nullable<KeyValuePair<string, string>>).Value;
                KeyValuePair<string, string> selectedOperator = (comboBoxOperator.SelectedItem as Nullable<KeyValuePair<string, string>>).Value;
                FilterItem fItem = new FilterItem(item.Key, item.Value,
                         new SizeOperator(selectedOperator.Key, selectedOperator.Value), textBoxExpression.Text,
                         (SizeKeyType)size, query.Filter.Count == 0 ?
                         LogicalOperator.None : LogicalOperator.And);
                query.Filter.Add(fItem);
                BindFilterListBox();
            }
        }

        private void BindFilterListBox()
        {
            Filter f = new Filter();
            Binding filterBinding = new Binding();
            foreach (FilterItem item in query.Filter)
            {
                f.Add(item);
            }
            if (f.Count == 1)
            {
                f[0].LOperator = LogicalOperator.None;
            }
            filterBinding.Source = f;
            filterBox.SetBinding(ListBox.ItemsSourceProperty, filterBinding);
        }

        private void BindSizeList(ListType type)
        {
            Binding binding = new Binding();
            switch (type)
            {
                case ListType.Dim:
                    binding.Source = query.Dimensions;
                    dimensionsSelected.SetBinding(ListBox.ItemsSourceProperty, binding);
                    break;
                case ListType.Met:
                    binding.Source = query.Metrics;
                    metricsSelected.SetBinding(ListBox.ItemsSourceProperty, binding);
                    break;
                case ListType.Fil:
                    binding.Source = query.Filter.ToSimplifiedList();
                    activeFilters.SetBinding(ListBox.ItemsSourceProperty, binding);
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
            {
                foreach (SizeViewModel subItem in item.Children)
                {
                    if (subItem.IsChecked == true)
                    {
                        checkedSizes.Add(subItem.Name, subItem.Value);
                    }
                }
            }
            return checkedSizes;
        }

        private void InitializeForm()
        {
            BindSizeList(ListType.Dim);
            BindSizeList(ListType.Met);
            BindSizeList(ListType.Fil);

            hasInvokedDimSetCheck = false;
            hasInvokedMetSetCheck = false;

            Binding sites = new Binding();
            sites.Source = CurrentUser.Entrys;
            comboBoxSites.SetBinding(ComboBox.ItemsSourceProperty, sites);

            if (this.query.Ids.Count > 0)
            {
                string pId = this.query.Ids.First().Value;
                if (CurrentUser.Entrys.Find( p => p.ProfileId == pId) != null )
	            {
                    comboBoxSites.SelectedValue = CurrentUser.Entrys.Find(p => p.ProfileId == pId);
	            }
                else
                {
                    // Notification: Current user has not permissions for the target profile of the original query.
                }
            }
            else
            {
                comboBoxSites.SelectedIndex = CurrentUser.Entrys.Count > 0 ? 0 : -1;
            }
            SetCalendars();
            activeSize = SizeKeyType.Dimension;

            double leftMargin = (double)DimensionsExpander.GetValue(Canvas.LeftProperty);

            expandPropertyHeight = new DoubleAnimation(0.0, 264.0, new Duration(TimeSpan.FromSeconds(0.2)));
            expandPropertyHeight.DecelerationRatio = 0.9;
            expandPropertyWidth = new DoubleAnimation(0.0, 547, new Duration(TimeSpan.FromSeconds(0.2)));
            expandPropertyWidth.DecelerationRatio = 0.9;
        }

        void DimensionsView_treeDatabound()
        {
            SetCheckedItems(query.Dimensions, DimensionsView.tree.Items[0] as SizeViewModel);
        }

        private void SetCalendars()
        {
            DateTime startDate = DateTime.Now.AddDays(-7);
            DateTime endDate = DateTime.Now;

            if (this.query != null && !String.IsNullOrEmpty(query.StartDate) && !String.IsNullOrEmpty(query.EndDate))
            {
                DateTime.TryParse(query.StartDate, out startDate);
                DateTime.TryParse(query.EndDate, out endDate);
            }
            startDateCalendar.SelectedDate = startDate;
            startDateCalendar.DisplayDate = startDate;
            endDateCalendar.SelectedDate = endDate;
            endDateCalendar.DisplayDate = endDate;
        }

        private void SetCheckedItems(Dictionary<string, string> sizeColl, SizeViewModel customTreeItems)
        {
            foreach (SizeViewModel category in customTreeItems.Children)
            {
                foreach (SizeViewModel size in category.Children)
                {
                    if (sizeColl.Keys.Contains(size.Name))
                    {
                        size.IsChecked = true;
                    }
                }
            }
        }

        private bool ValidateForm(out string errorMsg)
        {
            errorMsg = string.Empty;
            if (comboBoxSites.SelectedItem == null)
            {
                errorMsg = "No profile is selected";
                return false;
            }
            if (startDateCalendar.SelectedDate > endDateCalendar.SelectedDate)
            {
                errorMsg = "The start date can not be later than the end date";
                return false;
            }
            if (!(query.Dimensions.Count > 0))
            {
                errorMsg = "Select atleast one dimension";
                return false;
            }
            if (!(query.Metrics.Count > 0))
            {
                errorMsg = "Select atleast one metric";
                return false;
            }

            if (query.Dimensions.Count > maxSupportedDimensions)
            {
                query.Dimensions = query.Dimensions.Take(maxSupportedDimensions).ToDictionary(k => k.Key, v => v.Value);
            }
            if (query.Metrics.Count > maxSupportedMetrics)
            {
                query.Metrics = query.Metrics.Take(maxSupportedMetrics).ToDictionary(k => k.Key, v => v.Value);
            }
            return true;
        }

        public string ToUnifiedCultureFormat(DateTime date)
        {
            return date.Year + "-" + date.Month + "-" + date.Date;
        }
        #endregion
   }
}
