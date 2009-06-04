using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Analytics.Data.Enums;

namespace Analytics.Data
{
    public class FilterItem
    {
        #region Fields
        string _expression;
        string _key;
        string _value;

        SizeOperator _operator;
        LogicalOperator _logicalOperator;
        SizeKeyType _filterSizeKeyType;
        
        #endregion

        #region Properties
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public SizeOperator Operator
        {
            get { return _operator; }
            set { _operator = value; }
        }

        public LogicalOperator LOperator
        {
            get { return _logicalOperator; }
            set { _logicalOperator = value; }
        }

        public string Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }

        public SizeKeyType FItemType
        {
            get { return _filterSizeKeyType; }
            set { _filterSizeKeyType = value; }
        }

        public string SimplifiedString
        {
            get { return ToSimplifiedString(); }
        }

        #endregion

        public FilterItem(string key, string value, SizeOperator op, string expression, SizeKeyType filterSizeKeyType, LogicalOperator logicalOperator)
        {
            _operator = op;
            _key = key;
            _value = value;

            _expression = expression;
            _logicalOperator = logicalOperator;
            _filterSizeKeyType = filterSizeKeyType;
        }

        public override string ToString()
        {
            return LOperator == LogicalOperator.And ? "," : LOperator == LogicalOperator.Or ? ";" : string.Empty
            + Value + Operator.URIEncoded + Expression;
        }

        public string ToSimplifiedString()
        {
            return Key + " " + Operator.Description + " " + Expression;
        }
    }
}
