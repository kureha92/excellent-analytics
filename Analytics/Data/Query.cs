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
        private Dictionary<string, string> _ids;
        private Dictionary<string, string> _sortParams;
        private Filter _filter;
        private enum DataType {Dimension , Metric , Unknown};

        private string _startDate;
        private string _endDate;

        private int _maxResults;
        private int _startIndex;

        private Dictionary<string, string> metricsDefinitions;
        private Dictionary<string, string> dimensionDefinitions;

        private Dictionary<string, string> metricOperators;
        private Dictionary<string, string> dimensionOperators;
        #endregion

        #region Properties
        public Dictionary<string, string> Dimensions
        {
            get 
            {
                if (_dimensions == null)
                {
                    _dimensions = new Dictionary<string, string>();
                }
                return _dimensions; 
            }
            set { _dimensions = value; }
        }

        public Dictionary<string, string> Metrics
        {
            get 
            {
                if (_metrics == null)
                {
                    _metrics = new Dictionary<string, string>();
                }
                return _metrics; 
            }
            set { _metrics = value; }
        }

        public Dictionary<string, string> Ids
        {
            get 
            {
                if (_ids == null)
                {
                    _ids = new Dictionary<string, string>();
                }
                return _ids; 
            }
            set { _ids = value; }
        }

        public Dictionary<string, string> SortParams
        {
            get 
            {
                if (_sortParams == null)
                {
                    _sortParams = new Dictionary<string, string>();
                }
                return _sortParams; 
            }
            set { _sortParams = value; }
        }

        public Filter Filter
        {
            get
            {
                if (_filter == null)
                {
                    _filter = new Filter();
                }
                return _filter;
            }
            set { _filter = value; }
        }

        public string StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        public string EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
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
        #endregion

        public Query()
        {
            _maxResults = 1000;
            _startIndex = 0;
        }

        /// <summary>
        /// Creates a query object from a valid analytics query string
        /// </summary>
        /// <param name="queryString"></param>
        public Query(string queryString) : this()
        {
            foreach (string queryParam in queryString.Split(new char[] { '?', '&' }))
            {
                if (!queryParam.Contains('='))
                    continue;

                int startIndex = queryParam.IndexOf('=');
                startIndex += startIndex < queryParam.Length ? 1 : 0;

                switch (queryParam.Substring(0, queryParam.IndexOf('=')))
                {
                    case "ids": Ids.Add(string.Empty ,queryParam.Substring(startIndex) );
                        break;
                    case "dimensions":
                        foreach (string dimension in queryParam.Substring(startIndex).Split(','))
                        {
                            Dimensions.Add(GetFriendlySizeName(dimension), dimension);
                        }
                        break;
                    case "metrics":
                        foreach (string metric in queryParam.Substring(startIndex).Split(','))
                        {
                            Metrics.Add(GetFriendlySizeName(metric), metric);
                        }
                        break;
                    case "filters":
                        List<char> separators = (from char c in queryParam.ToCharArray()
                                                where c.Equals(',') || c.Equals(';')
                                                select c).ToList<char>();

                        char dummy = '»';
                        separators.Insert(0, dummy);
                        string[] filters = queryParam.Substring(startIndex).Split(new char[] { ',', ';' });
                        for (int i = 0; i < filters.Count(); i++)
                        {
                            FilterItem fItem = GetFilterItem(filters[i] , separators[i]);
                            if (fItem != null)
                            {
                                Filter.Add(fItem);
                            }
                        }
                        break;
                    case "sort":
                        foreach (string sortParam in queryParam.Substring(startIndex).Split(','))
                        {
                            SortParams.Add(GetFriendlySizeName(sortParam), sortParam);
                        }
                        break;
                    case "start-date":
                        DateTime startDate;
                        this.StartDate = DateTime.TryParse(queryParam.Substring(startIndex), out startDate) ? startDate.ToShortDateString() : null;
                        break;
                    case "end-date":
                        DateTime endDate;
                        this.EndDate = DateTime.TryParse(queryParam.Substring(startIndex), out endDate) ? endDate.ToShortDateString() : null;
                        break;
                    case "start-index": int.TryParse(queryParam.Substring(startIndex), out _startIndex);
                        break;
                    case "max-results": int.TryParse(queryParam.Substring(startIndex), out _maxResults);
                        break;
                    default:
                        break;
                }
            }
        }

        public override string ToString()
        {
            string queryBaseUri = General.GA_RequestURIs.Default.ReportFeed;
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append(queryBaseUri);
            queryBuilder.Append(Ids.Count > 0 ? "?ids=" + string.Join(",", Ids.Values.ToArray()) : string.Empty);
            queryBuilder.Append(Dimensions.Count > 0 ? "&dimensions=" + string.Join(",", Dimensions.Values.ToArray()) : string.Empty);
            queryBuilder.Append(Metrics.Count > 0 ? "&metrics=" + string.Join(",", Metrics.Values.ToArray()): string.Empty);
            queryBuilder.Append(SortParams.Count > 0 ? "&sort=" + string.Join(",", SortParams.Values.ToArray()): string.Empty);
            queryBuilder.Append(Filter.ToString());
            queryBuilder.Append(!String.IsNullOrEmpty(StartDate) ? "&start-date=" + StartDate : string.Empty);
            queryBuilder.Append(!String.IsNullOrEmpty(EndDate) ? "&end-date=" + EndDate : string.Empty);
            queryBuilder.Append(StartIndex != 0 ? "&start-index=" + StartIndex : string.Empty);
            queryBuilder.Append(MaxResults != 1000 ? "&max-results=" + MaxResults : string.Empty);
            return queryBuilder.ToString();
        }

        public int GetDimensionsAndMetricsCount()
        {
            return Dimensions.Count + Metrics.Count;
        }

        public static Query FromString(string queryString)
        {
            return new Query(queryString);
        }

        private FilterItem GetFilterItem(string filter, char logicalOp)
        {
            SizeOperator paramOperator = null;
            foreach (KeyValuePair<string, string> item in MetricOperators)
            {
                if (filter.Contains(item.Value))
                {
                    paramOperator = new SizeOperator(item.Key, item.Value);
                    break;
                }
            }
            if (paramOperator == null)
            {
                foreach (KeyValuePair<string, string> item in DimensionOperators)
                {
                    if (filter.Contains(item.Value))
                    {
                        paramOperator = new SizeOperator(item.Key, item.Value);
                        break;
                    }
                }
            }
            if (paramOperator != null)
            {
                string[] filterParts = filter.Replace(paramOperator.URIEncoded, "|").Split('|');
                string size = filterParts[0];
                string expression = filterParts[1];
                LogicalOperator lOp;
                switch (logicalOp)
                {
                    case ',':   lOp = LogicalOperator.And; break;
                    case ';':   lOp = LogicalOperator.Or; break;
                    default:    lOp = LogicalOperator.None; break;
                }
                SizeKeyType sizeType;
                return (new FilterItem(GetFriendlySizeName(size, out sizeType), size, paramOperator, expression, sizeType, lOp));
            }
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
                KeyValuePair<string,string> op = metricOperators.First(p => p.Value == urlEncodedOperator);
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
            (feedObjectType == SizeKeyType.Dimension ? "Dimension" : "Metric") + "FilterOperators.xml")));
            foreach (XElement element in xDocument.Root.Elements("Operator"))
            {
                operators.Add(element.Attribute("description").Value, element.Attribute("urlEncoded").Value);
            }
            return operators;
        }

        private SizeKeyType GetFilterDataTypeFromSize(string urlEncoded)
        {
            if (DimensionDefinitions.Values.Contains(urlEncoded))
                return SizeKeyType.Dimension;
            if (MetricDefinitions.Values.Contains(urlEncoded))
                return SizeKeyType.Metric;
            throw new Exception("Invalid filter size param");
        }

        private Dictionary<string, string> GetSizeCollection(SizeKeyType feedObjectType)
        {
            Dictionary<string, string> sizes = new Dictionary<string, string>();
            XDocument xDocument = XDocument.Load(System.Xml.XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("Analytics.Data.General." +
            (feedObjectType == SizeKeyType.Dimension ? "Dimensions" : "Metrics") + ".xml")));
            foreach (XElement element in xDocument.Root.Elements("Category"))
            {
                foreach (XElement subElement in element.Elements(feedObjectType == SizeKeyType.Dimension ? "Dimension" : "Metric"))
                {
                    sizes.Add(subElement.Attribute("name").Value, subElement.Attribute("value").Value);
                }
            }
            return sizes;
        }

        public static XDocument GetSizeCollectionAsXML(SizeKeyType feedObjectType)
        {
            XDocument xDocument = XDocument.Load(System.Xml.XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("Analytics.Data.General." +
            (feedObjectType == SizeKeyType.Dimension ? "Dimensions" : "Metrics")  + ".xml")));
            //XDocument.Load(System.AppDomain.CurrentDomain.BaseDirectory + @"\Resources\" +
            //(feedObjectType == SizeKeyType.Dimension ? "Dimensions" : "Metrics") + ".xml");
            return xDocument;
        }
    }
}
