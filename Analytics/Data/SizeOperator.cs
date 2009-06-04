using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Analytics.Data
{
    public class SizeOperator
    {
        string _description;
        string _uriEncoded;

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string URIEncoded
        {
            get { return _uriEncoded; }
            set { _uriEncoded = value; }
        }

        public SizeOperator(string description, string uriEncoded)
        {
            _description = description;
            _uriEncoded = uriEncoded;
        }
    }
}
