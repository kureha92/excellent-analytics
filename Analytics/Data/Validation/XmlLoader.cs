using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;

namespace Analytics.Data.Validation
{
    /* @author Daniel Sandberg
     * 
     * This class loads the dimension and metric XML's. These XML's contains all google analytics filtering values.
     * The class is called upon when there is a need to manipulate these values when calling Google Analytics or validating combinations
     * in the Excellent Analytics GUI.
     */
    public class XmlLoader
    {
        
        private List<XElement> dimCategories;
        private List<XElement> metCategories;

        
        public void Loader()
        {
            dimCategories = new List<XElement>();
            metCategories = new List<XElement>();

            // Loads all metrics properties into a list.
            XDocument xDocumentMetric =
                XDocument.Load(
                    System.Xml.XmlReader.Create(
                        Assembly.GetExecutingAssembly().GetManifestResourceStream("Analytics.Data.General." +
                                                                                  "Metrics.xml")));

            foreach (XElement element in xDocumentMetric.Root.Elements("Category"))
            {
                metCategories.Add(element);
            }

            // Loads all dimension properties into a list.
            XDocument xDocumentDimension =
                XDocument.Load(
                    System.Xml.XmlReader.Create(
                        Assembly.GetExecutingAssembly().GetManifestResourceStream("Analytics.Data.General." +
                                                                                  "Dimensions.xml")));
            foreach (XElement element in xDocumentDimension.Root.Elements("Category"))
            {
                dimCategories.Add(element);
            }
        }

        public List<XElement> MetCategories
        {
            get { return metCategories; }
            set { metCategories = value; }
        }

        public List<XElement> DimCategories
        {
            get { return dimCategories; }
            set { dimCategories = value; }
        }
    }
}
