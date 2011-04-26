using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Linq;
using System.IO;
using System.Collections;
using System.Windows.Forms;

namespace Analytics.Data
{
    public class ReportManager
    {
        public delegate void ExecutionProgress(int progress, string progressMessage, string errorMsg);
        public event ExecutionProgress executionProgress;
        int totalHitResult;
        int upperLimitBound;
        string authenticationToken;
        XDocument xDoc = null;
        HttpWebResponse response = null;
        WebRequest request;
        int rowPosition;
        object[,] data;
        
        private void NotifySubscribers(int progress, string progressMessage, string errorMsg)
        {
            if (executionProgress != null)
            {
                executionProgress(progress, progressMessage, errorMsg);
            }
        }

        public Report GetReport(Query query, string authToken, int profileCounter)
        {
            authenticationToken = authToken;
            NotifySubscribers(10 , "Requesting report" , null);
            Report report = new Report();
            int originalStartIndex = query.StartIndex;

            CreateRequest(query, profileCounter);
            if (!RequestData(request))
                return report;

            int dimensionsAndMetrics = query.GetDimensionsAndMetricsCount();

            if (xDoc != null)
            {
                NotifySubscribers(50, "Extract data", null);
                report.Data = ExtractDataFromXml(xDoc, dimensionsAndMetrics);
                report.Query = query.ToString();
                report.SiteURI = query.Ids[profileCounter].Value;
            }
            report.Headers = SetHeaders(query);

            // Checks if paging is neccessary.
            while (totalHitResult > upperLimitBound && upperLimitBound > 10000)
            {
                query.StartIndex = query.StartIndex + 10000;
                query.MaxResults = upperLimitBound = query.MaxResults + 10000;
                CreateRequest(query, profileCounter);
                if (!RequestData(request))
                    return report;
                NotifySubscribers(50, "Extract data", null);
                report.Data = ExtractDataFromXml(xDoc, dimensionsAndMetrics);
                if (upperLimitBound <= query.MaxResults)
                    break;
            }
            query.StartIndex = originalStartIndex;
            return report;
        }

        /*@author Daniel Sandberg
         * The method constructs the request.
         */
        private void CreateRequest(Query query, int profileCounter)
        {
            string uri = query.ToString(profileCounter);
            request = HttpRequestFactory.Instance.CreateRequest(uri);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            UTF8Encoding encoding = new UTF8Encoding();
            request.Headers.Add("Authorization: GoogleLogin auth=" + authenticationToken);
            request.Headers.Add("GData-Version: 2");
            request.ContentLength = 0;
        }

        /*@author Daniel Sandberg
         * This method executes the call to Google Analytics.
         * Google Analytics return an XML document, which is saved into the global parameter xDoc.
         */
        private bool RequestData(WebRequest request)
        {
            try
            {
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                    {
                        NotifySubscribers(20, "Geting response", null);
                        xDoc = XDocument.Load(new StreamReader(response.GetResponseStream()));
                        NotifySubscribers(30, "Request complete", null);
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    NotifySubscribers(50, "Request timed out", HttpStatusCode.RequestTimeout.ToString());
                    MessageBox.Show("Request timed out. Increase timeout in settings and try again.", "Request timed out",MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                NotifySubscribers(50, "Request failed", HttpStatusCode.BadRequest.ToString());
                MessageBox.Show("Request failed. Probably because of invalid input.", "Bad request");
                return false;
            }
            catch (Exception)
            {
                NotifySubscribers(50, "Request failed", HttpStatusCode.BadRequest.ToString());
                MessageBox.Show("Request failed. Probably because of invalid input.", "Bad request");
                return false;
            }
            return true;
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


        /*@ co-author Daniel Sandberg
         * Retrieves XML elements and saves them into an object array. 
         * Each object contains a text value and two integers. The integers specify which cell the certain object belongs to.
         */
        private object[,] ExtractDataFromXml(XDocument xDoc, int dimensionsAndMetrics)
        {
            XNamespace dxp = "http://schemas.google.com/analytics/2009";
            XNamespace atom = "http://www.w3.org/2005/Atom";
            XNamespace opensearch = "http://a9.com/-/spec/opensearch/1.1/";
            XName dimensionElementName = dxp + "dimension";
            XName metricElementName = dxp + "metric";
            XName segmentElementName = dxp + "segment";
            XName entryElementName = atom + "entry";
            XName totalResult = opensearch + "totalResults";
            XName paging = opensearch + "itemsPerPage";
            string value = "value";

            List<XElement> segmentElements = xDoc.Root.Elements(segmentElementName).ToList<XElement>();
            totalHitResult = Int32.Parse(xDoc.Root.Element(totalResult).Value);
            upperLimitBound = Int32.Parse(xDoc.Root.Element(paging).Value);
            
            List<XElement> entryElements = xDoc.Root.Elements(entryElementName).ToList<XElement>();
            if (rowPosition < 1)
            {
                data = new object[entryElements.Count() > 0 ? totalHitResult + 2 : 1  , dimensionsAndMetrics];
            }
            

            for (int rowIndex = 0; rowIndex < entryElements.Count(); rowIndex++)
            {
                int columnIndex = 0;
                foreach (XElement dimEle in entryElements[rowIndex].Elements(dimensionElementName))
                {
                    data[rowPosition, columnIndex] = dimEle.Attribute(value).Value;
//                    if (data.Length != rowPosition)
                        columnIndex++;
                }
                foreach (XElement metEle in entryElements[rowIndex].Elements(metricElementName))
                {
                    data[rowPosition, columnIndex] = metEle.Attribute(value).Value;
//                    if (data.Length != rowPosition)
                        columnIndex++;
                }
                rowPosition++;
            }
            NotifySubscribers(100, "Report complete" , null);

            return data;
        }
    }
}
