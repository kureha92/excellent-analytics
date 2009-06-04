using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Linq;
using System.IO;
using System.Collections;

namespace Analytics.Data
{
    public class ReportManager
    {
        public delegate void ExecutionProgress(int progress, string progressMessage, string errorMsg);
        public event ExecutionProgress executionProgress;

        private void NotifySubscribers(int progress, string progressMessage, string errorMsg)
        {
            if (executionProgress != null)
            {
                executionProgress(progress, progressMessage, errorMsg);
            }
        }

        public Report GetReport(Query query, string authToken)
        {
            NotifySubscribers(10 , "Requesting report" , null);
            int dimensionsAndMetrics = query.GetDimensionsAndMetricsCount();
            string uri = query.ToString();
            
            WebRequest request = HttpWebRequest.Create(uri);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            UTF8Encoding encoding = new UTF8Encoding();
            request.Headers.Add("Authorization: GoogleLogin auth=" + authToken);
            request.ContentLength = 0;
            Report report = new Report();
            XDocument xDoc = null;
            HttpWebResponse response = null;
            
            try
            {
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                    {
                        NotifySubscribers(20, "Geting response" , null);
                        xDoc = XDocument.Load(new StreamReader(response.GetResponseStream()));
                        NotifySubscribers(30, "Request complete" , null);
                    }
                }
            }
            catch (Exception)
            {
                NotifySubscribers(50, "Request failed" , HttpStatusCode.BadRequest.ToString() );
            }
            if (xDoc != null)
            {
                NotifySubscribers(50, "Extract data" , null);
                report.Data = ExtractDataFromXml(xDoc, dimensionsAndMetrics);
                report.Query = query.ToString();
                report.SiteURI = query.Ids.Keys.First();
            }
            report.Headers = SetHeaders(query);
            return report;
        }

        private object[,] SetHeaders(Query query)
        {
            object[,] headers = new object[1, query.GetDimensionsAndMetricsCount()];
            int columnIndex = 0;
            foreach (string item in query.Dimensions.Keys)
            {
                headers[0, columnIndex] = item;
                columnIndex++;
            }
            foreach (string item in query.Metrics.Keys)
            {
                headers[0, columnIndex] = item;
                columnIndex++;
            }
            return headers;
        }

        private object[,] ExtractDataFromXml(XDocument xDoc, int dimensionsAndMetrics)
        {
            XNamespace dxp = "http://schemas.google.com/analytics/2009";
            XNamespace atom = "http://www.w3.org/2005/Atom";
            XName dimensionElementName = dxp + "dimension";
            XName metricElementName = dxp + "metric";
            XName entryElementName = atom + "entry";
            string value = "value";
            List<XElement> entryElements = xDoc.Root.Elements(entryElementName).ToList<XElement>();
            object[,] data = new object[entryElements.Count() > 0 ? entryElements.Count() : 1  , dimensionsAndMetrics];

            for (int rowIndex = 0; rowIndex < entryElements.Count(); rowIndex++)
            {
                int columnIndex = 0;
                foreach (XElement dimEle in entryElements[rowIndex].Elements(dimensionElementName))
                {
                    data[rowIndex, columnIndex] = dimEle.Attribute(value).Value;
                    columnIndex++;
                }
                foreach (XElement metEle in entryElements[rowIndex].Elements(metricElementName))
                {
                    data[rowIndex, columnIndex] = metEle.Attribute(value).Value;
                    columnIndex++;
                }
            }
            NotifySubscribers(100, "Report complete" , null);
            return data;
        }
    }
}
