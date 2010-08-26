using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Analytics.Data.Enums;

namespace Analytics.Data
{
    public class Sort : List<SortItem>
    {
        public Sort()
        {
                          
        }

        public override string ToString()
        {
            StringBuilder sortBuilder = new StringBuilder();
            sortBuilder.Append(this.Count > 0 ? "&sort=" : string.Empty);
            int i = 0;
            foreach (SortItem item in this)
            {
                if (i > 0)
                {
                    sortBuilder.Append("," + item.Key.ToString());
                }
                else 
                {
                    sortBuilder.Append(item.Key.ToString());
                }
                i++;
            }
            return sortBuilder.ToString();
        }

        public string ToSimplifiedString()
        {
            StringBuilder sortBuilder = new StringBuilder();
            foreach (SortItem item in this)
            {
                sortBuilder.Append(item.Key);
            }
            return sortBuilder.ToString();
        }

        public List<string> ToSimplifiedList()
        {
            List<string> items = new List<string>();
            foreach (SortItem item in this)
            {
                items.Add(item.Value);
            }
            return items;
        }
    }
}
