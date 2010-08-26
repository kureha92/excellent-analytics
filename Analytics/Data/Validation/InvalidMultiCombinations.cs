using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Analytics.Data.Validation;

namespace Analytics.Data.General
{
    /*author: Daniel Sandberg
     * THIS CLASS IS NOT IN USE. 
     * 
     * Google's validation chart is to unclear.
     * 
     * 
     * Description: In-adequate multi-combinations are invalidated based on dimension and metric input.
     */
    public class InvalidMultiCombinations
    {
        private List<XElement> metCategories;
        private List<XElement> dimCategories;
        bool multiCombo = false;
        List<string> prohibited = new List<string>();

        /*
         
         */
        public List<string> MultiCombo(List<string> input)
        {
            XmlLoader loader = new XmlLoader();
            loader.Loader();
            metCategories = loader.MetCategories;
            dimCategories = loader.DimCategories;
               
            List<string> metDimCombo = new List<string>();
            List<string> comboHelperOne = new List<string>();
            List<string> comboHelperTwo = new List<string>();

            /************************ If query contains ga:visitors *****************************/
            if (input.ToString().Contains("visitors"))
            {
                // Invalidates all metrics.
                foreach (XElement subMetElement in metCategories.Elements("Metric"))
                {
                    metDimCombo.Add(subMetElement.FirstAttribute.Value);
                }

                // Re-validates M1. Visitors
                foreach (XElement subMetElement in metCategories[0].Elements("Metric"))
                {
                    metDimCombo.Remove(subMetElement.FirstAttribute.Value);
                }

                foreach (string metric in TimeDimensions())
                {
                    metDimCombo.Remove(metric);
                }
            }

            if (comboHelperOne.Count > 0 && comboHelperTwo.Count < 1)
            {
                // Prohibits M2. Campaign
                foreach (XElement subMetElement in metCategories[1].Elements("Metric"))
                {
                    metDimCombo.Add(subMetElement.FirstAttribute.Value);
                }
                // Prohibits M7. Events
                foreach (XElement subMetElement in metCategories[6].Elements("Metric"))
                {
                    metDimCombo.Add(subMetElement.FirstAttribute.Value);
                }
                metDimCombo.Add("time on site");
                metDimCombo.Add("visits");
                metDimCombo.Add("visitors");

                // Prohibits D2. Campaign
                foreach (XElement subDimElement in dimCategories[1].Elements("Dimension"))
                {
                    metDimCombo.Add(subDimElement.FirstAttribute.Value);
                }

                // Prohibits D4. 
                foreach (XElement subDimElement in dimCategories[3].Elements("Dimension"))
                {
                    metDimCombo.Add(subDimElement.FirstAttribute.Value);
                }

                // Prohibits D7.
                foreach (XElement subDimElement in dimCategories[6].Elements("Dimension"))
                {
                    metDimCombo.Add(subDimElement.FirstAttribute.Value);
                }

            }

            comboHelperOne.Clear();
            comboHelperTwo.Clear();

            /************************** Any M2. Campaign AND/OR ga:adContent, ga:adSlot and ga:adSlotPosition *******************************************/

            // If the input string contains all these three values then a multi-combination is selected.
            multiCombo = comboMulti(input);

            // This block of code validates the metrics allowed.
            if (multiCombo)
            {
                // Invalidates all M7. Events.
                foreach (XElement subMetElement in metCategories[6].Elements("Metric"))
                {
                    metDimCombo.Add(subMetElement.FirstAttribute.Value);
                }

                metDimCombo.Add("visitors");

                // Prohibit all dimensions
                foreach (XElement subDimElement in dimCategories.Elements("Dimension"))
                {
                    metDimCombo.Add(subDimElement.FirstAttribute.Value);
                }

                // Remove the valid combination D2. Campaign from the prohibitation list.
                foreach (XElement subDimElement in dimCategories[1].Elements("Dimension"))
                {
                    metDimCombo.Remove(subDimElement.FirstAttribute.Value);
                }

                // Remove the valid time dimension combinations 
                foreach (string dimension in TimeDimensions())
                {
                    metDimCombo.Remove(dimension);
                }
            }

            // Clears the helper lists
            comboHelperOne.Clear();
            comboHelperTwo.Clear();
            multiCombo = false;

            /********************** Any D3. Content AND D2. Campaign, except ga:adContent, ga:adSlot and ga:adSlotPosition ****************************/
            foreach (string dimension in input)
            {
                foreach (XElement subDimElement in dimCategories[2].Elements("Dimension"))
                {
                    if (subDimElement.FirstAttribute.Value.Equals(dimension))
                        comboHelperOne.Add(subDimElement.FirstAttribute.Value);
                }

                foreach (XElement subDimElement in dimCategories[1].Elements("Dimension"))
                {
                    if (subDimElement.FirstAttribute.Value.Equals(dimension) && !dimension.Equals("ad content") && !dimension.Equals("ad slot") && !dimension.Equals("ad slotposition"))
                        comboHelperTwo.Add(subDimElement.FirstAttribute.Value);
                }
            }

            if (comboHelperOne.Count > 0 && comboHelperTwo.Count > 0)
            {
                foreach (XElement subDimElement in dimCategories[3].Elements("Dimension"))
                    metDimCombo.Add(subDimElement.FirstAttribute.Value);

                foreach (XElement subDimElement in dimCategories[6].Elements("Dimension"))
                    metDimCombo.Add(subDimElement.FirstAttribute.Value);

                metDimCombo.Add("ad content");
                metDimCombo.Add("ad slot");
                metDimCombo.Add("ad slot position");

                foreach (XElement subMetElement in metCategories[0].Elements("Metric"))
                    metDimCombo.Add(subMetElement.FirstAttribute.Value);

                metDimCombo.Remove("time on site");
                metDimCombo.Remove("visitors");
                metDimCombo.Remove("visits");

                foreach (XElement subMetElement in metCategories[2].Elements("Metric"))
                    metDimCombo.Add(subMetElement.FirstAttribute.Value);

                foreach (XElement subMetElement in metCategories[4].Elements("Metric"))
                    metDimCombo.Add(subMetElement.FirstAttribute.Value);
            }

            // Clears the helper lists
            comboHelperOne.Clear();
            comboHelperTwo.Clear();
            multiCombo = false;

            /**************************** Only D3. Content (any from the group) ********************************/
            foreach (string dimension in input)
            {
                foreach (XElement subDimElement in dimCategories[2].Elements("Dimension"))
                {
                    if (subDimElement.FirstAttribute.Value.Equals(dimension))
                        comboHelperOne.Add(subDimElement.FirstAttribute.Value);
                }
            }

                if (comboHelperOne.Count > 0 && comboHelperOne.Count == input.Count)
                {
                    foreach (XElement subDimElement in dimCategories[1].Elements("Dimension"))
                    {
                        metDimCombo.Add(subDimElement.FirstAttribute.Value);
                    }

                    foreach (XElement subDimElement in dimCategories[3].Elements("Dimension"))
                    {
                        metDimCombo.Add(subDimElement.FirstAttribute.Value);
                    }

                    foreach (XElement subDimElement in dimCategories[4].Elements("Dimension"))
                    {
                        metDimCombo.Add(subDimElement.FirstAttribute.Value);
                    }

                    foreach (XElement subDimElement in dimCategories[6].Elements("Dimension"))
                    {
                        metDimCombo.Add(subDimElement.FirstAttribute.Value);
                    }
                }

                // Clears the helper lists
                comboHelperOne.Clear();
                comboHelperTwo.Clear();
                multiCombo = false;


            /*************************** Any D7. Event AND/OR ga:adContent, ga:adSlot and ga:adSlotPosition *************************************/
            multiCombo = comboMulti(input);
            
            foreach (string dimension in input)
            {
                foreach (XElement subDimElement in dimCategories[6].Elements("Dimension"))
                {
                    if (subDimElement.FirstAttribute.Value.Equals(dimension))
                    {
                        comboHelperOne.Add(subDimElement.FirstAttribute.Value);
                    }
                }
            }

            if ((comboHelperOne.Count > 0 && multiCombo) || multiCombo)
            {
                // Invalidates prohibited dimensions.

                // D3.
                foreach (XElement subDimElement in dimCategories[2].Elements("Dimension"))
                {
                    metDimCombo.Add(subDimElement.FirstAttribute.Value);
                }

                // D4.
                foreach (XElement subDimElement in dimCategories[3].Elements("Dimension"))
                {
                    metDimCombo.Add(subDimElement.FirstAttribute.Value);
                }

                // D5.
                foreach (XElement subDimElement in dimCategories[4].Elements("Dimension"))
                {
                    metDimCombo.Add(subDimElement.FirstAttribute.Value);
                }

                // ga:pagePath and ga:pageTitle
                metDimCombo.Add("page path");
                metDimCombo.Add("page title");
            }

            // Clears the helper lists
            comboHelperOne.Clear();
            comboHelperTwo.Clear();
            multiCombo = false;

            // Prohibited dimensions and metrics are returned.
            return metDimCombo;
            
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

         private bool comboMulti(List<string> input)
         {
             List<string> comboList = new List<string>();

             foreach (string param in input)
             {
                 if (param.Equals("ad content"))
                 {
                     if (comboList.Contains(param))
                     {
                     }
                     else
                         comboList.Add(param);
                 }

                 if (param.Equals("ad slot"))
                 {
                     if (comboList.Contains(param))
                     {
                     }
                     else
                         comboList.Add(param);
                 }

                 if (param.Equals("ad slot position"))
                 {
                     if (comboList.Contains(param))
                     {
                     }
                     else
                         comboList.Add(param);
                 }
             }

             if (comboList.Count > 2)
                 multiCombo = true;

             return multiCombo;
         }
    }
 }
