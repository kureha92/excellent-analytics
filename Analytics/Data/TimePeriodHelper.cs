using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Analytics.Data
{
    class TimePeriodHelper
    {
        public DateTime _startDate;
        public DateTime _endDate;


        public DateTime startToday()
        {
            DateTime today = DateTime.Today;
            StartDate = today;
            return today;
        }

        public DateTime endToday()
        {
            DateTime today = DateTime.Today;
            EndDate = today;
            return today;
        }

        public DateTime startYesterDay()
        {
            DateTime yesterDay = DateTime.Today.AddDays(-1);
            StartDate = yesterDay;
            return yesterDay;
        }

        public DateTime endYesterDay()
        {
            DateTime yesterDay = DateTime.Today.AddDays(-1);
            EndDate = yesterDay;
            return yesterDay;
        }

        public DateTime startWeekDay()
        {
            DateTime startWeek = DateTime.Now;
            while (startWeek.DayOfWeek != DayOfWeek.Monday) startWeek = startWeek.AddDays(-1);
            startWeek = startWeek.AddDays(-7);
            StartDate = startWeek;
            return startWeek;
        }

        public DateTime endWeekDay()
        {
            DateTime endWeek = DateTime.Now;
            while (endWeek.DayOfWeek != DayOfWeek.Sunday) endWeek = endWeek.AddDays(-1);
            EndDate = endWeek;
            return endWeek;
        }

        public DateTime startWeekDayAnglo()
        {
            DateTime startWeekAnglo = DateTime.Now;
            while (startWeekAnglo.DayOfWeek != DayOfWeek.Sunday) startWeekAnglo = startWeekAnglo.AddDays(-1);
            startWeekAnglo = startWeekAnglo.AddDays(-7);
            StartDate = startWeekAnglo;
            return startWeekAnglo;
        }

        public DateTime endWeekDayAnglo()
        {
            DateTime endWeekAnglo = DateTime.Now;
            while (endWeekAnglo.DayOfWeek != DayOfWeek.Saturday) endWeekAnglo = endWeekAnglo.AddDays(-1);
            EndDate = endWeekAnglo;
            return endWeekAnglo;
        }

        public DateTime monthStart()
        {
            DateTime dateTime = DateTime.Now;
            DateTime firstDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1).AddMonths(-1);
            StartDate = firstDayOfTheMonth;
            return firstDayOfTheMonth;
        }

        public DateTime monthEnd()
        {
            DateTime dateTime = DateTime.Now;
            DateTime lastDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1).AddDays(-1);
            EndDate = lastDayOfTheMonth;
            return lastDayOfTheMonth;
        }


        public DateTime LastQuarter(bool quarter)
        {
            string strMonth = DateTime.Now.ToString("MMMM");
            DateTime dateTime = DateTime.Now;
            DateTime dateOfQuarter = new DateTime();

            DateTime q1 = new DateTime(dateTime.Year, 1, 1);
            DateTime q2 = new DateTime(dateTime.Year, 4, 1);
            DateTime q3 = new DateTime(dateTime.Year, 7, 1);
            DateTime q4 = (new DateTime(dateTime.Year, 10, 1)).AddYears(-1);

            if(!quarter)
            {
                q1 = new DateTime(dateTime.Year, 3, 31);
                q2 = new DateTime(dateTime.Year, 6, 30);
                q3 = new DateTime(dateTime.Year, 9, 30);
                q4 = (new DateTime(dateTime.Year, 12, 31)).AddYears(-1);
            }

            if (strMonth.Equals("januari") || strMonth.Equals("februari") || strMonth.Equals("mars"))
            {
                dateOfQuarter = q4;

            }
            else if (strMonth.Equals("april") || strMonth.Equals("maj") || strMonth.Equals("juni"))
            {
                dateOfQuarter = q1;
            }
            else if (strMonth.Equals("juli") || strMonth.Equals("augusti") || strMonth.Equals("september"))
            {
                dateOfQuarter = q2;
            }
            else if (strMonth.Equals("oktober") || strMonth.Equals("november") || strMonth.Equals("december"))
            {
                dateOfQuarter = q3;
            }

            if (quarter)
            {
                StartDate = dateOfQuarter;
            }
            else
            {
                EndDate = dateOfQuarter;
            }
            
            return dateOfQuarter;
        }

        public DateTime startLastYear()
        {
            DateTime dateTime = DateTime.Now;
            DateTime lastDayOfPreviousYear = (new DateTime(dateTime.Year, 1, 1)).AddYears(-1);
            StartDate = lastDayOfPreviousYear;
            return lastDayOfPreviousYear;
        }

        public DateTime endLastYear()
        {
            DateTime dateTime = DateTime.Now;
            DateTime firstDayOfPreviousYear = (new DateTime(dateTime.Year, 1, 1)).AddDays(-1);
            EndDate = firstDayOfPreviousYear;
            return firstDayOfPreviousYear;
        }

        public DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }
    }
}
