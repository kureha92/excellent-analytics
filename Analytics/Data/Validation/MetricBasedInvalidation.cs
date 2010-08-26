using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using Analytics.Data.Validation;

/*
 * author: Daniel Sandberg
 * 
 * Description: This class invalidates metrics and dimensions based on selected metrics input.
 */

namespace Analytics.Data.General
{
    public class MetricBasedValidation
    {
        private string metItem;
        private List<XElement> metCategories;
        private List<XElement> dimCategories;
        List<string> prohibited = new List<string>();

        public List<string> prohibitedDimension(string metric)
        {
            XmlLoader loader = new XmlLoader();
            loader.Loader();
            metItem = metric;
            metCategories = loader.MetCategories;
            dimCategories = loader.DimCategories;

            // M1. Visitor
            foreach (XElement subMetElement in metCategories[0].Elements("Metric"))
            {
                if (metric.Equals("time on site") || metric.Equals("visits"))
                {
                    prohibited.Add("exit page path");
                    prohibited.Add("landing page path");
                    prohibited.Add("second page path");
                }

                if (metric.Equals("visitors"))
                {
                    prohibited.Add("hour");

                    foreach (XElement subElement in dimCategories.Elements("Dimension"))
                    {
                        prohibited.Add(subElement.FirstAttribute.Value);
                    }

                    // Remove time dimensions, except "hour", from the invalidation list
                    foreach (string timeDimension in TimeDimensions())
                    {
                        prohibited.Remove(timeDimension);
                    }

                    foreach (XElement subMetricElement in metCategories.Elements("Metric"))
                    {
                        prohibited.Add(subMetricElement.FirstAttribute.Value);
                    }

                    // Remove M1. Visitor from the invalidation list.
                    foreach (XElement subVisitorElement in metCategories[0].Elements("Metric"))
                    {
                        prohibited.Remove(subVisitorElement.FirstAttribute.Value);
                    }

                }
            }
            /***************************************** M2. Campaign ******************************************/
            foreach (XElement subMetElement in metCategories[1].Elements("Metric"))
            {
                if (subMetElement.FirstAttribute.Value.Equals(metric))
                {
                    // Prohibited dimensions
                    foreach (XElement subDimElement in dimCategories.Elements("Dimension"))
                    {
                        prohibited.Add(subDimElement.FirstAttribute.Value);
                    }
                    // Remove valid dimensions from the prohibitation list.
                    foreach (XElement subDimElement in dimCategories[1].Elements("Dimension"))
                    {
                        prohibited.Remove(subDimElement.FirstAttribute.Value);
                    }
                    foreach (string dimension in TimeDimensions())
                    {
                        prohibited.Remove(dimension);
                    }
                    foreach (XElement subMetricElement in metCategories[6].Elements("Metric"))
                    {
                        prohibited.Add(subMetricElement.FirstAttribute.Value);
                    }
                    prohibited.Add("visitors");

                }
            }

            /*********************************** M3. Content  ***********************************************/
            // Prohibits D7. Events
            if (metric.Equals("unique page views"))
            {
                foreach (XElement subDimElement in dimCategories[6].Elements("Dimension"))
                {
                    prohibited.Add(subDimElement.FirstAttribute.Value);
                }
                prohibited.Add("visitors");
            }

            /*********************************************** M4. Ecommerce **********************************/
            // 
            bool checkedStatus = false;
            foreach (XElement subMetElement in metCategories[3].Elements("Metric"))
            {
                if (subMetElement.FirstAttribute.Value.Equals(metric))
                {
                    checkedStatus = true;
                }
            }
            if (checkedStatus)
            {
                prohibited.Add("visitors");
            }

            /******************************************** M5. Internal Search *******************************/
            foreach (XElement subMetElement in metCategories[4].Elements("Metric"))
            {
                if (subMetElement.FirstAttribute.Value.Equals(metric))
                {
                    foreach (XElement subDimElement in dimCategories[6].Elements("Dimension"))
                    {
                        prohibited.Add(subDimElement.FirstAttribute.Value);
                    }
                    prohibited.Add("visitors");
                }
            }

            /*************************************** M6. Goals **********************************************/
            foreach (XElement subMetElement in metCategories[5].Elements("Metric"))
            {
                if (subMetElement.FirstAttribute.Value.Equals(metric))
                {
                    foreach (XElement subDimElement in dimCategories[6].Elements("Dimension"))
                    {
                        prohibited.Add(subDimElement.FirstAttribute.Value);
                    }
                    prohibited.Add("visitors");
                }
            }

            /*************************************** M7. Events *****************************************/
            foreach (XElement subMetElement in metCategories[6].Elements("Metric"))
            {
                if (subMetElement.FirstAttribute.Value.Equals(metric))
                {
                    foreach (XElement subMetricElement in metCategories[1].Elements("Metric"))
                    {
                        prohibited.Add(subMetricElement.FirstAttribute.Value);
                    }

                    prohibited.Add("exit page path");
                    prohibited.Add("landing page path");
                    prohibited.Add("second page path");
                    prohibited.Add("visitors");
                }
            }


            return prohibited;
        }


        private static List<string> TimeDimensions()
        {
            List<string> timeDimensions = new List<string>();
            timeDimensions.Add("month");
            timeDimensions.Add("day");
            timeDimensions.Add("days since last visit");
            timeDimensions.Add("date");
            timeDimensions.Add("week");
            timeDimensions.Add("year");

            return timeDimensions;
        }
    }
}
