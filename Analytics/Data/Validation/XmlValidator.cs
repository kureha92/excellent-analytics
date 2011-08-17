using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using System.Xml;
using System.Reflection;

namespace Analytics.Data.Validation
{
    public class XmlValidator
    {
        public static XmlSchemaSet LoadSchema(string schema)
        {
            XmlTextReader xtr = new XmlTextReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Analytics.Data.General." + schema));
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.Add(null, xtr);
            return xss;
        }
    }
}
