using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Analytics.Data.Enums;

namespace Analytics.Data
{
    public class SortItem
    {
        #region Fields
        string _key;
        string _value;

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

        public string SimplifiedString
        {
            get { return ToSimplifiedString(); }
        }

        #endregion

        public SortItem(string key, string value)
        {
            _key = key;
            _value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        public string ToSimplifiedString()
        {
            return Key;
        }
    }
}
