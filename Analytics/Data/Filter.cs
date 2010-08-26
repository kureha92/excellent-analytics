using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Analytics.Data.Enums;

namespace Analytics.Data
{
    public class Filter : List<FilterItem>
    {
        public Filter()
        {
                          
        }

        public override string ToString()
        {
            StringBuilder filterBuilder = new StringBuilder();
            filterBuilder.Append(this.Count > 0 ? "&filters=" : string.Empty);
            foreach (FilterItem item in this)
            {
                filterBuilder.Append(item.ToString());
            }
            return filterBuilder.ToString();
        }

        public string ToSimplifiedString()
        {
            StringBuilder filterBuilder = new StringBuilder();
            foreach (FilterItem item in this)
            {
                switch (item.LOperator)
                {
                    case LogicalOperator.And: filterBuilder.Append(" And "); break;
                    case LogicalOperator.Or: filterBuilder.Append(" Or "); break;
                    case LogicalOperator.None: break;
                    default: break;
                }
                filterBuilder.Append(item.Key + " " + item.Operator.Description + " " + item.Expression);
            }
            return filterBuilder.ToString();
        }

        public List<string> ToSimplifiedList()
        {
            List<string> items = new List<string>();
            string logicOp = string.Empty;
            foreach (FilterItem item in this)
            {
                switch (item.LOperator)
                {
                    case LogicalOperator.And: logicOp = " And " + "\n"; break;
                    case LogicalOperator.Or: logicOp = " Or " + "\n"; break;
                    case LogicalOperator.None: break;
                    default: break;
                }
                items.Add(logicOp + item.Key);
            }
            return items;
        }

        public List<List<string>> ToElementList()
        {
            
            List<List<string>> items = new List<List<string>>();
            foreach (FilterItem item in this)
            {
                List<string> elements = new List<string>();
                elements.Add(item.Key);
                elements.Add(item.Operator.Description);
                elements.Add(item.Expression);
                elements.Add(item.LOperator.ToString());
                items.Add(elements);
            }
            return items;
        }
    }
}
