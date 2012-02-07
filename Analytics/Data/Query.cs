using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Analytics.Data.Enums;
using System.Reflection;



namespace Analytics.Data
{
    public class Query
    {
        #region Fields
        private Dictionary<string, string> _dimensions;
        private Dictionary<string, string> _metrics;
        private Dictionary<string, string> _segments;
        private Dictionary<string, string> _accountId;
        private Dictionary<string, string> _profileId;
        private Dictionary<string, string> _sortParams;
        private Dictionary<string, string> _listSortOrder;
        private Dictionary<string, string> metricsDefinitions;
        private Dictionary<string, string> dimensionDefinitions;
        private Dictionary<string, string> filtersDefinitions;
        private Dictionary<string, string> metricOperators;
        private Dictionary<string, string> dimensionOperators;
        private Dictionary<string, string> segmentOperators;
        private Filter _filter;
        private Sort _sort;
        private ProfileCollection _ids;
        private enum DataType {Dimension , Metric , Unknown};

        private DateTime _startDate;
        private DateTime _endDate;

        private bool _selectDates = false;

        private DateTime _radioButton;

        private TimePeriodHelper _timeHelper;

        private TimePeriod _timePeriod;

        private string _queryInfoIdentifier;

        private int _maxResults;
        private int _startIndex;
        private int _row;
        private int _column;



        #endregion

        #region Properties

        public string QueryInfoIdentifier
        {
            get { return _queryInfoIdentifier; }
            set { _queryInfoIdentifier = value; }
        }

        public int Column
        {
            get { return _column; }
            set { _column = value; }
        }        
        public int Row
        {
            get { return _row; }
            set { _row = value; }
        }
        public Dictionary<string, string> Dimensions
        {
            get 
            {
                if (_dimensions == null)
                    _dimensions = new Dictionary<string, string>();
                return _dimensions; 
            }
            set { _dimensions = value; }
        }
        public Dictionary<string, string> Metrics
        {
            get 
            {
                if (_metrics == null)
                    _metrics = new Dictionary<string, string>();
                return _metrics; 
            }
            set { _metrics = value; }
        }        
        public Dictionary<string, string> ListSortOrder
        {
            get
            {
                if (_listSortOrder == null)
                    _listSortOrder = new Dictionary<string, string>();
                return _listSortOrder;
            }
            set { _listSortOrder = value; }
        }
        public Dictionary<string, string> AccountId
        {
            get
            {
                if (_accountId == null)
                    _accountId = new Dictionary<string, string>();
                return _accountId;
            }
            set { _accountId = value; }
        }
        public Dictionary<string, string> ProfileId
        {
            get
            {
                if (_profileId == null)
                    _profileId = new Dictionary<string, string>();
                return _profileId;
            }
            set { _profileId = value; }
        }
        public Dictionary<string, string> Segments
        {
            get
            {
                if(_segments == null)
                    _segments = new Dictionary<string, string>();
                return _segments;
            }
            set { _segments = value; }
        }
        public Dictionary<string, string> SortParams
        {
            get 
            {
                if (_sortParams == null)
                    _sortParams = new Dictionary<string, string>();
                return _sortParams; 
            }
            set { _sortParams = value; }
        }
        public Filter Filter
        {
            get
            {
                if (_filter == null)
                    _filter = new Filter();
                return _filter;
            }
            set { _filter = value; }
        }
        public Sort Sort
        {
            get
            {
                if (_sort == null)
                    _sort = new Sort();
                return _sort;
            }
            set { _sort = value; }
        }

        public ProfileCollection Ids
        {
            get
            {
                if (_ids == null)
                    _ids = new ProfileCollection();
                return _ids;
            }
            set { _ids = value; }
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
        public bool SelectDates
        {
            get { return _selectDates; }
            set { _selectDates = value; }
        }        
        public DateTime RadioButton
        {
            get { return _radioButton; }
            set { _radioButton = value; }
        }
        public TimePeriod TimePeriod
        {
            get { return _timePeriod; }
            set { _timePeriod = value; }
        }
        public int MaxResults
        {
            get { return _maxResults; }
            set { _maxResults = value; }
        }
        public int StartIndex
        {
            get { return _startIndex; }
            set { _startIndex = value; }
        }
        #endregion
        #region Private properties
        private Dictionary<string, string> MetricDefinitions
        {
            get
            {
                if (metricsDefinitions == null)
                    metricsDefinitions = GetSizeCollection(SizeKeyType.Metric);
                return metricsDefinitions;
            }
            set { metricsDefinitions = value; }
        }

        private Dictionary<string, string> DimensionDefinitions
        {
            get
            {
                if (dimensionDefinitions == null)
                    dimensionDefinitions = GetSizeCollection(SizeKeyType.Dimension);
                return dimensionDefinitions;
            }
            set { dimensionDefinitions = value; }
        }

        private Dictionary<string, string> DimensionOperators
        {
            get
            {
                if (dimensionOperators == null)
                    dimensionOperators = GetOperatorCollection(SizeKeyType.Dimension);
                return dimensionOperators;
            }
            set { dimensionOperators = value; }
        }

        private Dictionary<string, string> MetricOperators
        {
            get
            {
                if (metricOperators == null)
                    metricOperators = GetOperatorCollection(SizeKeyType.Metric);
                return metricOperators;
            }
            set { metricOperators = value; }
        }

        private Dictionary<string, string> SegmentOperators
        {
            get
            {
                if (segmentOperators == null)
                    segmentOperators = GetOperatorCollection(SizeKeyType.Segment);
                return segmentOperators;
            }
            set { segmentOperators = value; }
        }


        #endregion

        public Query()
        {

        }

        /// <summary>
        /// Creates a query object from a valid analytics query string
        /// </summary>
        /// <param name="queryString"></param>
        public Query(string[] queryString) : this()
        {
            CreateFromQueryString(queryString);
        }

        #region Methods

        public IEnumerable<KeyValuePair<string, string>>GetMetricsAndDimensions
        {
            get{
                return Metrics.Concat(Dimensions);
            }
        }

        public List<string> GetFriendlyMetricsAndDimensions 
        {
            get{
                return (from p in GetMetricsAndDimensions
                        select GetFriendlySizeName(p.Value)).ToList<string>();
            }
        }


        private void CreateFromQueryString(string[] queryString)
        {
            /*
            string profile = "";
            foreach (string test in queryString.Split(new char[] { '[' }))
            {
                profile = test;
                break;
            }*/
//            queryString = queryString.Substring(profile.Length, queryString.Length - profile.Length);
            foreach (string queryParam in queryString[1].Split(new char[] { '?', '&' }).Where(s => s.Contains('=')))
            {
                string ids = "";
                if (queryParam.Contains("ids="))
                {
                    ids = queryString[0];
                }
                AddQueryParamToQuery(queryParam, ids);
            }

        }

        // Builds the query that will be sent to Gooogle Analytics.
        private void AddQueryParamToQuery(string queryParam, string profile)
        {
            int startIndex = queryParam.IndexOf('=');
            startIndex += startIndex < queryParam.Length ? 1 : 0;

            switch (queryParam.Substring(0, queryParam.IndexOf('=')))
            {
                case "ids": AddIds(queryParam, startIndex, profile); break;
                case "dimensions": AddDimensions(queryParam, startIndex); break;
                case "metrics": AddMetrics(queryParam, startIndex); break;
                case "segment": AddSegments(queryParam, startIndex); break;
                case "filters": AddFilters(queryParam, startIndex); break;
                case "sort": AddSortParams(queryParam, startIndex); break;
                case "start-date": AddStartDate(queryParam, startIndex); break;
                case "end-date": AddEndDate(queryParam, startIndex); break;
                case "start-index": AddStartIndex(queryParam, startIndex); break;
                case "max-results": AddMaxResults(queryParam, startIndex); break;
                default: break;
            }
        }

        private void AddMaxResults(string queryParam, int startIndex)
        {
            int.TryParse(queryParam.Substring(startIndex), out _maxResults);
        }

        private void AddStartIndex(string queryParam, int startIndex)
        {
            int.TryParse(queryParam.Substring(startIndex), out _startIndex);
        }

        private void AddEndDate(string queryParam, int startIndex)
        {
            DateTime endDate;
            if (DateTime.TryParse(queryParam.Substring(startIndex), out endDate))
                EndDate = endDate;
        }

        private void AddStartDate(string queryParam, int startIndex)
        {
            DateTime startDate;
            if (DateTime.TryParse(queryParam.Substring(startIndex), out startDate))
                StartDate = startDate;
        }

        private void AddSortParams(string sortQueryParam, int startIndex)
        {
            List<char> separators = SeparatorsFromFilterQueryParam(sortQueryParam);
            char placeFiller = '»';
            separators.Insert(0, placeFiller);
            
            string[] sortings = sortQueryParam.Substring(startIndex).Split(new char[] { ',', ';' });

            for (int i = 0; i < sortings.Count(); i++)
            {
                Item sItem;

                if (sortings[i].Contains("-"))
                {
                    sItem = new Item(sortings[i], "-" + sortings[i].Substring(4));
                }
                else 
                {
                    sItem = new Item(sortings[i], sortings[i].Substring(3));
                }

                Sort.Add(sItem);
                SortParams.Add(sItem.Key, sItem.Value);
            }
        }

        private void AddIds(string queryParam, int startIndex, string profile)
        {
            string sortings = queryParam.Substring(startIndex);
            Item pItem = new Item(sortings.Substring(3), profile);
            Ids.Add(pItem);
        }


        private void AddSegments(string queryParam, int startIndex)
        {
            Segments.Add(string.Empty, queryParam.Substring(startIndex));
        }

        private void AddFilters(string filterQueryParam, int startIndex)
        {
            List<char> separators = SeparatorsFromFilterQueryParam(filterQueryParam);
            char placeFiller = '»';
            separators.Insert(0, placeFiller);
            string[] filters = filterQueryParam.Substring(startIndex).Split(new char[] { ',', ';' });
            for (int i = 0; i < filters.Count(); i++)
            {
                FilterItem fItem = GetFilterItem(filters[i], separators[i]);
                if (fItem != null)
                    Filter.Add(fItem);
            }
        }

        private List<char> SeparatorsFromFilterQueryParam(string queryParam)
        {
            List<char> separators = (from char c in queryParam.ToCharArray()
                                     where c.Equals(',') || c.Equals(';')
                                     select c).ToList<char>();

            return separators;
        }

        private void AddMetrics(string queryParam, int startIndex)
        {
            foreach (string metric in queryParam.Substring(startIndex).Split(','))
                Metrics.Add(GetFriendlySizeName(metric), metric);
        }
        
        private void AddDimensions(string queryParam, int startIndex)
        {
            foreach (string dimension in queryParam.Substring(startIndex).Split(','))
                Dimensions.Add(GetFriendlySizeName(dimension), dimension);
        }

        
        // Transforms query to a string.
        public string ToString(int profileCounter)
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append(General.GA_RequestURIs.Default.ReportFeed);
//            (Ids.Count > 1 ?  Ids.ToList().First().ToString() :
            queryBuilder.Append(Ids.Count > 0 ? Ids.ToString(profileCounter) : string.Empty);
            queryBuilder.Append(Dimensions.Count > 0 ? "&dimensions=" + string.Join(",", Dimensions.Values.ToArray()) : string.Empty);
            queryBuilder.Append(Metrics.Count > 0 ? "&metrics=" + string.Join(",", Metrics.Values.ToArray()) : string.Empty);
            queryBuilder.Append(Segments.Count > 0 ? "&segment=" + string.Join(",", Segments.Values.ToArray()) : string.Empty);
            queryBuilder.Append(Filter.ToString());
            queryBuilder.Append(Sort.ToString());
            if (_selectDates.Equals(false))
            {
                queryBuilder.Append(GetQueryTimeSpan());
                TimeRange();
            }
            else
            { 
                string paramContainer = "&start-date={0}&end-date={1}";
                queryBuilder.Append(string.Format(paramContainer, ToUnifiedCultureFormat(StartDate), ToUnifiedCultureFormat(EndDate)));
            }
            queryBuilder.Append(StartIndex > 0 ? "&start-index=" + StartIndex : string.Empty);
            queryBuilder.Append( "&max-results=" + MaxResults);
            return queryBuilder.ToString();
        }

        private void TimeRange()
        {
            StartDate = _timeHelper.StartDate;
            EndDate = _timeHelper.EndDate;
        }

        // If a time span is selected in the Time period tab then the switch/case statement below will handle that value.
        // Each case calls the TimePeriodHelper class to set the start and end dates.
        private string GetQueryTimeSpan()
        {
            string paramContainer = "&start-date={0}&end-date={1}";
            _timeHelper = new TimePeriodHelper();
            bool startQuarterDate = true;

            switch (TimePeriod)
            {
                case TimePeriod.Today:
                    return string.Format(paramContainer, ToUnifiedCultureFormat(_timeHelper.startToday()), ToUnifiedCultureFormat(_timeHelper.endToday()));
                case TimePeriod.Yesterday:
                    return string.Format(paramContainer, ToUnifiedCultureFormat(_timeHelper.startYesterDay()), ToUnifiedCultureFormat(_timeHelper.endYesterDay()));
                case TimePeriod.Week:
                    return string.Format(paramContainer, ToUnifiedCultureFormat(_timeHelper.startWeekDay()), ToUnifiedCultureFormat(_timeHelper.endWeekDay()));
                case TimePeriod.WeekAnglo:
                    return string.Format(paramContainer, ToUnifiedCultureFormat(_timeHelper.startWeekDayAnglo()), ToUnifiedCultureFormat(_timeHelper.endWeekDayAnglo()));
                case TimePeriod.LastMonth:
                    return string.Format(paramContainer, ToUnifiedCultureFormat(_timeHelper.monthStart()), ToUnifiedCultureFormat(_timeHelper.monthEnd()));
                case TimePeriod.LastQuarter:
                    return string.Format(paramContainer, ToUnifiedCultureFormat(_timeHelper.LastQuarter(startQuarterDate)), ToUnifiedCultureFormat(_timeHelper.LastQuarter(startQuarterDate = false)));
                case TimePeriod.LastYear:
                    return string.Format(paramContainer, ToUnifiedCultureFormat(_timeHelper.startLastYear()), ToUnifiedCultureFormat(_timeHelper.endLastYear()));
                    case TimePeriod.ThisYear:
                    return string.Format(paramContainer, ToUnifiedCultureFormat(_timeHelper.startThisYear()), ToUnifiedCultureFormat(_timeHelper.endThisYear()));
                case TimePeriod.PeriodNotSpecified:
                    return string.Format(paramContainer, StartDate, EndDate);
                default:
                    throw new Exception("Date interval missing or incomplete");
            }
        }

        private string ToUnifiedCultureFormat(DateTime date)
        {
            return date.Year + "-" +
            (date.Month < 10 ? ("0" + date.Month) : date.Month.ToString())
            + "-" + (date.Day < 10 ? ("0" + date.Day) : date.Day.ToString());
        }

        public int GetDimensionsAndMetricsCount()
        {
            return Dimensions.Count + Metrics.Count;
        }

        
        public static Query FromString(string[] queryString)
        {
            return new Query(queryString);
        }

        private FilterItem GetFilterItem(string filter, char logicalOp)
        {
            SizeOperator paramOperator = GetParamOperator(filter);
            if (paramOperator != null)
            {
                ///string[] filterParts = filter.Replace(paramOperator.URIEncoded, "|").Split('|');
                string size =  filter.Substring(0,filter.IndexOf(paramOperator.URIEncoded)); //filterParts[0];
                string expression = filter.Substring(filter.IndexOf(paramOperator.URIEncoded) + paramOperator.URIEncoded.Length); //filterParts[1];
                LogicalOperator lOp;
                switch (logicalOp)
                {
                    case ';': lOp = LogicalOperator.And; break;
                    case ',': lOp = LogicalOperator.Or; break;
                    default: lOp = LogicalOperator.None; break;
                }
                SizeKeyType sizeType;
                return (new FilterItem(GetFriendlySizeName(size, out sizeType), size, paramOperator, expression, sizeType, lOp));
            }
            return null;
        }

        private SizeOperator GetSegmentParamOperator (string segment)            
        {
            foreach (KeyValuePair<string, string> item in SegmentOperators)
                if (segment.Contains(item.Value))
                    return new SizeOperator(item.Key, item.Value);
            return null;
        }

        private SizeOperator GetParamOperator(string filter)
        {
            foreach (KeyValuePair<string, string> item in MetricOperators)
                if (filter.Contains(item.Value))
                    return new SizeOperator(item.Key, item.Value);
            foreach (KeyValuePair<string, string> item in DimensionOperators)
                if (filter.Contains(item.Value))
                        return new SizeOperator(item.Key, item.Value);
            return null;
        }

        private string GetFriendlySizeName(string urlEncoded, out SizeKeyType outType)
        {
            outType = SizeKeyType.Unknown;
            string friendlyName = urlEncoded;

            if (DimensionDefinitions.Values.Contains(urlEncoded))
            {
                friendlyName = DimensionDefinitions.First(p => p.Value == urlEncoded).Key;
                outType = SizeKeyType.Dimension;
            }
            else if (MetricDefinitions.Values.Contains(urlEncoded))
            {
                friendlyName = MetricDefinitions.First(p => p.Value == urlEncoded).Key;
                outType = SizeKeyType.Metric;
            }

            return friendlyName;
        }

        private string GetFriendlySizeName(string urlEncoded)
        {
            return DimensionDefinitions.Values.Contains(urlEncoded) ? DimensionDefinitions.First(p => p.Value == urlEncoded).Key :
            MetricDefinitions.Values.Contains(urlEncoded) ? MetricDefinitions.First(p => p.Value == urlEncoded).Key : urlEncoded;
        }

        private SizeOperator GetFilterOperator(string urlEncodedOperator)
        {
            if (metricOperators.Values.Contains(urlEncodedOperator))
            {
                KeyValuePair<string, string> op = metricOperators.First(p => p.Value == urlEncodedOperator);
                return new SizeOperator(op.Key, op.Value);
            }
            if (dimensionOperators.Values.Contains(urlEncodedOperator))
            {
                KeyValuePair<string, string> op = dimensionOperators.First(p => p.Value == urlEncodedOperator);
                return new SizeOperator(op.Key, op.Value);
            }
            return null;
        }

        public static Dictionary<string, string> GetOperatorCollection(SizeKeyType feedObjectType)
        {
            Dictionary<string, string> operators = new Dictionary<string, string>();
            XDocument xDocument = XDocument.Load(System.Xml.XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("Analytics.Data.General." +
            FeedSizeKeyType(feedObjectType))));
            foreach (XElement element in xDocument.Root.Elements("Operator"))
                operators.Add(element.Attribute("description").Value, element.Attribute("urlEncoded").Value);
            return operators;
        }

        private static string FeedSizeKeyType (SizeKeyType feedObjectType)
        {
            if (SizeKeyType.Dimension == feedObjectType)
                return "DimensionFilterOperators.xml";
            else if (SizeKeyType.Metric == feedObjectType)
                return "MetricFilterOperators.xml";
            else
                return "SegmentOperators.xml";
        }

        private SizeKeyType GetFilterDataTypeFromSize(string urlEncoded)
        {
            if (DimensionDefinitions.Values.Contains(urlEncoded))
                return SizeKeyType.Dimension;
            if (MetricDefinitions.Values.Contains(urlEncoded))
                return SizeKeyType.Metric;
            throw new Exception("Invalid filter size param");
        }

        public static Dictionary<string, string> GetSizeCollection(SizeKeyType feedObjectType)
        {
            Dictionary<string, string> sizes = new Dictionary<string, string>();
            XDocument xDocument = GetSizeCollectionAsXML(feedObjectType);
            foreach (XElement element in xDocument.Root.Elements("Category"))
                foreach (XElement subElement in element.Elements(feedObjectType == SizeKeyType.Dimension ? "Dimension" : "Metric"))
                    sizes.Add(subElement.Attribute("name").Value, subElement.Attribute("value").Value);
            return sizes;
        }

        public static XDocument GetSizeCollectionAsXML(SizeKeyType feedObjectType)
        {
            return XDocument.Parse((feedObjectType == SizeKeyType.Dimension ? Settings.Instance.DimensionsXml : Settings.Instance.MetricsXml).OuterXml);
        } 

        #endregion

    }
}
