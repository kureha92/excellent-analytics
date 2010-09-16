using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Analytics.Data.Enums;

namespace Analytics.Data
{
    public class Item
    {
        #region Fields
        string _key;
        string _value;
        int _profileCount;

        #endregion

        #region Properties

        public int ProfileCount
        {
            get { return _profileCount; }
            set { _profileCount = value; }
        }

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

        public Item(string key, string value)
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
