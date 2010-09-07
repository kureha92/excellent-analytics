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
using Analytics.Authorization;
using Analytics.Data;
using Analytics.Data.Enums;

namespace WPFUIv2
{
    /// <summary>
    /// Interaction logic for WorkSheetUpdate.xaml
    /// </summary>
    /// @author Daniel Sandberg
    /// 
    public partial class WorkSheetUpdate : Window
    {
        #region Fields
        Query _query;
        public delegate void QueryComplete(Query query, bool worksheet);
        public event QueryComplete queryComplete;
        List<Query> _listQueries;

        #endregion

        public List<Query> Queries
        {
            get 
            {
                return _listQueries;
            }
            set
            {

                _listQueries = value;
            }
        }

        public WorkSheetUpdate(UserAccount userAccount, List<Query> queries)
        {
            InitializeComponent();
            this._listQueries = queries;
            setCalendarToDefault();

        }

        #region Events

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

        void Timespan_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton sendCheck = sender as RadioButton;
            foreach (RadioButton itBox in TimeSpanBoxesColl)
                itBox.IsChecked = itBox.Name == sendCheck.Name;

            setCalendarToDefault();

        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                List<Query> queries = new List<Query>();
                Query queryHelper = new Query();
                foreach (Query query in _listQueries)
                {
                    queryHelper = CompleteQuery(query);
                    queryHelper = SetSelectedDates(queryHelper);
                    queries.Add(queryHelper);
                }
                _listQueries = queries;
                this.Close();
                queryComplete(_query, true);            
            }
        }

        #endregion


        #region Methods

 
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

        private List<RadioButton> TimeSpanBoxesColl
        {
            get
            {
                return new RadioButton[] { todayCheckBox, yesterdayCheckBox, weekCheckBox, weekCheckBoxAnglosax, monthCheckBox, 
                    quarterCheckBox, yearCheckBox, periodNotSpecifiedCheckBox }.Where(p => p != null).ToList<RadioButton>();
            }
        }

        private void setCalendarToDefault()
        {
            startDateCalendar.SelectedDate = DateTime.Now;
            startDateCalendar.DisplayDate = DateTime.Now;
            endDateCalendar.SelectedDate = DateTime.Now;
            endDateCalendar.DisplayDate = DateTime.Now;
        }

        private Query SetSelectedDates(Query query)
        {
            query.StartDate = (DateTime)startDateCalendar.SelectedDate;
            query.EndDate = (DateTime)endDateCalendar.SelectedDate;
            return query;
        }

        private Query CompleteQuery(Query query)
        {

            if (startDateCalendar.SelectedDate.Value.Date.Equals(DateTime.Today))
            {
                query.TimePeriod = (TimePeriod)Enum.Parse(typeof(TimePeriod), TimeSpanBoxesColl.Where(p => (bool)p.IsChecked).First().Tag.ToString());
                query.SelectDates = false;
            }
            else
            {
                query.EndDate = (DateTime)endDateCalendar.SelectedDate;
                query.SelectDates = true;
            }
            return query;
        }

        // Validation rules. If the rules are not followed a message will appear in the GUI.
        // It is not possible to execute a question when the validation rules are not followed.
        private bool ValidateForm()
        {
            if (startDateCalendar.SelectedDate > endDateCalendar.SelectedDate)
            {
                Notify("The start date can not be later than the end date");
                return false;
            }
            if (startDateCalendar.SelectedDate.Value.Date.Equals(DateTime.Today.Date) && !radioButtonChecked())
            {
                Notify("The start date can not be set to today");
                return false;
            }

            return true;
        }

        private void Notify(string message)
        {
            MainNotify.Visibility = Visibility.Visible;
            MainNotify.ErrorMessage = message;
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

        #endregion

        private void ExecuteButton_MouseEnter(object sender, MouseEventArgs e)
        {
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


    }
}
