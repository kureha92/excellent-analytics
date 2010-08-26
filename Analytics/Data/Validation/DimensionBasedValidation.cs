using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using Analytics.Data.Validation;

namespace Analytics.Data.General
{
    /*author: Daniel Sandberg
     * 
     * Description: This class invalidates metrics and dimensions based on selected dimension input.
     *              The class shall be used when closing the DimensionExpander method.
     *              DimenensionExpander shall not be closable when it contains an invalid combination of dimensions.
     */

    public class DimensionBasedValidation
    {
        List<string> prohibited = new List<string>();
        List<XElement> dimCategories;
        List<XElement> metCategories;
        private string dimItem;


        // Method called from QueryBuilder
        public List<string> prohibitedMetrics(string dimension)
        {
            XmlLoader loader = new XmlLoader();
            loader.Loader();
            dimItem = dimension;
            dimCategories = loader.DimCategories;
            metCategories = loader.MetCategories;


            // D1. Visitor
            foreach (XElement subDimElement in dimCategories[0].Elements("Dimension"))
            {
                if (subDimElement.FirstAttribute.Value.Equals(dimension) && TimeDimensions(dimension))
                {
                    // Disable ga:visitors and M2. Campaign
                    foreach (XElement subMetElement in metCategories[1].Elements("Metric"))
                    {
                        prohibited.Add(subMetElement.FirstAttribute.Value);
                    }
                    prohibited.Add("visitors");
                }                    
            }


            // D2. Campaign
            foreach (XElement subDimElement in dimCategories[1].Elements("Dimension"))
            {
                if (subDimElement.FirstAttribute.Value.Equals(dimension))
                {
                    prohibited.Add("visitors");
                    break;
                }
            }

            // D3. Content, only for ga:exitPagePath ga:landingPagePath ga:secondPagePath
            if (dimension.Contains("exit page path") || dimension.Contains("landing page path") || dimension.Contains("second page path"))
            {
                foreach (XElement subDimElement in dimCategories[2].Elements("Dimension"))
                {
                    if (subDimElement.FirstAttribute.Value.Equals(dimension))
                    {
                        prohibited.Add("time on site");
                        prohibited.Add("visitors");
                        prohibited.Add("visits");

                        // Disable M2. Campaign and M7. Events
                        foreach (XElement subMetElement in metCategories[1].Elements("Metric"))
                        {
                            prohibited.Add(subMetElement.FirstAttribute.Value);
                        }
                        foreach (XElement subMetElement in metCategories[6].Elements("Metric"))
                        {
                            prohibited.Add(subMetElement.FirstAttribute.Value);
                        }
                        
                        // Prohibits D2. Campaign
                        foreach (XElement subDimensionElement in dimCategories[1].Elements("Dimension"))
                        {
                            prohibited.Add(subDimensionElement.FirstAttribute.Value);
                        }

                        // Prohibits D4. Ecommerce
                        foreach (XElement subDimensionElement in dimCategories[3].Elements("Dimension"))
                        {
                            prohibited.Add(subDimensionElement.FirstAttribute.Value);
                        }

                        // Prohibits D5. Internal Search
                        foreach (XElement subDimensionElement in dimCategories[4].Elements("Dimension"))
                        {
                            prohibited.Add(subDimensionElement.FirstAttribute.Value);
                        }

                        // Prohibits D7. Events
                        foreach (XElement subDimensionElement in dimCategories[6].Elements("Dimension"))
                        {
                            prohibited.Add(subDimensionElement.FirstAttribute.Value);
                        }
                    }
                }
            }
            // D3. Content, excluding ga:exitPagePath ga:landingPagePath ga:secondPagePath
            else
            {
                foreach (XElement subDimElement in dimCategories[2].Elements("Dimension"))
                {
                    if (subDimElement.FirstAttribute.Value.Equals(dimension))
                    {
                        foreach (XElement subMetElement in metCategories[1].Elements("Metric"))
                        {
                            prohibited.Add(subMetElement.FirstAttribute.Value);
                        }
                        prohibited.Add("visitors");
                    }
                }
            }

            //D4. Ecommerce
            foreach (XElement subDimElement in dimCategories[3].Elements("Dimension"))
            {
                if (subDimElement.FirstAttribute.Value.Equals(dimension))
                {
                    // Disable ga:visitors
                    prohibited.Add("visitors");
                    //Disable M2. Campaign.
                    foreach (XElement subMetElement in metCategories[1].Elements("Metric"))
                    {
                        prohibited.Add(subMetElement.FirstAttribute.Value);
                    }
                }
            }
 
            //D5. Internal Search
            foreach (XElement subDimElement in dimCategories[4].Elements("Dimension"))
            {
                if (subDimElement.FirstAttribute.Value.Equals(dimension))
                {
                    // Disable ga:visitors
                    prohibited.Add("visitors");
                    //Disable M2. Campaign.
                    foreach (XElement subMetElement in metCategories[1].Elements("Metric"))
                    {
                        prohibited.Add(subMetElement.FirstAttribute.Value);
                    }
                }
            }

            //D6. Custom Variables
            foreach (XElement subDimElement in dimCategories[5].Elements("Dimension"))
            {
                if (subDimElement.FirstAttribute.Value.Equals(dimension))
                {
                    // Disable ga:visitors
                    prohibited.Add("visitors");
                    //Disable M2. Campaign.
                    foreach (XElement subMetElement in metCategories[1].Elements("Metric"))
                    {
                        prohibited.Add(subMetElement.FirstAttribute.Value);
                    }
                }
            }

            //D7. Events
            foreach (XElement subDimElement in dimCategories[6].Elements("Dimension"))
            {
                if (subDimElement.FirstAttribute.Value.Equals(dimension))
                {
                    // Invalidate ga:visitors
                    prohibited.Add("visitors");

                    // Invalidate M2. Campaign.
                    foreach (XElement subMetElement in metCategories[1].Elements("Metric"))
                    {
                        prohibited.Add(subMetElement.FirstAttribute.Value);
                    }
                    // Invalidate M3. 
                    foreach (XElement subMetElement in metCategories[2].Elements("Metric"))
                    {
                        prohibited.Add(subMetElement.FirstAttribute.Value);
                    }
                    // Invalidate M5. 
                    foreach (XElement subMetElement in metCategories[4].Elements("Metric"))
                    {
                        prohibited.Add(subMetElement.FirstAttribute.Value);
                    }
                    // Invalidate M6. 
                    foreach (XElement subMetElement in metCategories[5].Elements("Metric"))
                    {
                        prohibited.Add(subMetElement.FirstAttribute.Value);
                    }
                }
            }

            //D8. AdWords
            foreach (XElement subDimElement in dimCategories[7].Elements("Dimension"))
            {
                if (subDimElement.FirstAttribute.Value.Equals(dimension))
                {
                    // Disable ga:visitors
                    prohibited.Add("visitors");
                    //Disable M2. Campaign.
                    foreach (XElement subMetElement in metCategories[1].Elements("Metric"))
                    {
                        prohibited.Add(subMetElement.FirstAttribute.Value);
                    }
                }
            }

            return prohibited;
    }



        private static bool TimeDimensions(string dimension)
        {
            bool exist = true;

            switch (dimension)
            {
                case "month" :
                    exist = false;
                    break;
                case "day" :
                    exist = false;
                    break;
                case "days since last visit" :
                    exist = false;
                    break;
                case "date" :
                    exist = false;
                    break;
                case "week" :
                    exist = false;
                    break;
                case "year" :
                    exist = false;
                    break;
            }

            return exist;
        }
    }
}
