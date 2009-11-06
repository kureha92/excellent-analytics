using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Analytics.Data
{
    public class Report
    {
        string _query;
        string _UAId;
        string _siteURI;
        int _hits;
       
        object[,] _data;
        object[,] _headers;

        #region Properties
        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }

        public string UAId
        {
            get { return _UAId; }
            set { _UAId = value; }
        }

        public string SiteURI
        {
            get { return _siteURI; }
            set { _siteURI = value; }
        }

        public object[,] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public object[,] Headers
        {
            get { return _headers; }
            set { _headers = value; }
        }

        public int Hits
        {
            get
            {
                if (_data != null)
                {
                    _hits = _data.GetLength(0);
                }
                return _hits;
            }
            set { _hits = value; }
        } 
        #endregion

        public Report()
        {
           _hits = 0;
        }

        public Report(object[,] data) : this()
        {
            _data = data;
        }

        public Report(object[,] data , string query , string UAId , string siteURI) : this(data)
        {
            _query = query;
            _UAId = UAId;
            _siteURI = siteURI;
        }

        public bool ValidateResult()
        {
            return this.Data != null && this.Data.GetLength(0) > 0 && this.Data.GetLength(1) > 0;
        }
    }
}
