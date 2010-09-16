using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Analytics.Data.Enums
{
    public enum SizeKeyType { Dimension, Metric , Unknown, Segment, Profile};
    public enum SegmentType { Default, Custom};
    public enum LogicalOperator  { And , Or, None  };
    public enum TimePeriod { Today, Yesterday, Week, WeekAnglo, LastMonth, LastQuarter, LastYear, SelectDates, PeriodNotSpecified };
}
