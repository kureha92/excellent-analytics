using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using Analytics.Data;
using Analytics.Data.General;

namespace Analytics.Authorization
{
    public class AccountManager
    {
        public delegate void AuthProgress( int progress , string progressMessage);
        public event AuthProgress authProgress;

        private void NotifySubscribers(int progress , string progressMessage)
        {
            if (authProgress != null)
            {
                this.authProgress(progress, progressMessage);
            }        
        }


        public UserAccount GetAccountData(string eMail , string authToken)
        {
            UserAccount uAcc = new UserAccount(authToken , eMail);
            //UTF8Encoding encoding = new UTF8Encoding();
            //WebRequest request = HttpRequestFactory.Instance.CreateRequest(Data.General.GA_RequestURIs.Default.AccountFeed);
            //request.Method = "GET";
            //request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentLength = 0;
            //request.Headers.Add("Authorization: GoogleLogin auth=" + uAcc.AuthToken);
            //request.Timeout = 20000;

            //HttpWebResponse response = null;

            //XDocument xDoc = null;
            //try
            //{
            //    using (response = (HttpWebResponse)request.GetResponse())
            //    {
            //        if (response != null && response.StatusCode == HttpStatusCode.OK)
            //        {
            //            xDoc = XDocument.Load(new StreamReader(response.GetResponseStream()));
            //        }
            //    }
            //}
            //catch (WebException webEx)
            //{
            //    throw webEx;
            //}
            //finally 
            //{
            //    if (xDoc != null)
            //    {
            //        uAcc.Entrys = ExtractDataFromXml(xDoc);
            //        //uAcc.Segments = ExtractSegmentDataFromXml(xDoc);
            //    }
            //    else
            //    {
            //        NotifySubscribers(0 , "Connection failure" );
            //    }
            //}
            XDocument accountData = GetData(Data.General.GA_RequestURIs.Default.AccountFeed, authToken);
            List<Entry> accounts = ExtractAccountDataFromXml(accountData);
            XDocument profileData = GetData(Data.General.GA_RequestURIs.Default.PorfileFeed, authToken);
            List<Entry> profiles = ExtractProfileDataFromXml(profileData, accounts);
            uAcc.Entrys = profiles;

            XDocument segmentData = GetData(Data.General.GA_RequestURIs.Default.SegmentFeed, authToken);
            uAcc.Segments = ExtractSegmentDataFromXml(segmentData);
            return uAcc;
        }
        public XDocument GetData(string url, string authToken)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            WebRequest request = HttpRequestFactory.Instance.CreateRequest(url);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = 0;
            request.Headers.Add("Authorization: GoogleLogin auth=" + authToken);
            request.Timeout = 20000;

            HttpWebResponse response = null;

            XDocument xDoc = null;
            try
            {
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                    {
                        xDoc = XDocument.Load(new StreamReader(response.GetResponseStream()));
                    }
                }
            }
            catch (WebException webEx)
            {
                throw webEx;
            }
            return xDoc;
        }
        private List<UserSegment> ExtractSegmentDataFromXml(XDocument xDoc)
        {
            List<UserSegment> segments = new List<UserSegment>();
            List<UserSegment> customSegments = new List<UserSegment>();

            XNamespace nsDxp = "http://schemas.google.com/analytics/2009";
            XNamespace nsAtom = "http://www.w3.org/2005/Atom";
            XName entryElementName = nsAtom + "entry";
            XName segmentElementName = nsDxp + "segment";

            IEnumerable<XElement> segmentElements = xDoc.Root.Elements(entryElementName);

            foreach (XElement temp in segmentElements)
            {
                XElement segmentElement = temp.Element(segmentElementName);
                string segmentId = segmentElement.Attribute("id").Value;
                string segmentName = segmentElement.Attribute("name").Value;
                int iSegmentId;
                if (int.TryParse(segmentId.Substring(segmentId.IndexOf(':') + 2), out iSegmentId) && iSegmentId > -1)
                    customSegments.Add(new UserSegment() { SegmentName=segmentName, SegmentId=segmentId} );
                else
                    segments.Add(new UserSegment() { SegmentName=segmentName, SegmentId=segmentId} );
            }
            customSegments.Sort(delegate(UserSegment a, UserSegment b) { return a.SegmentName.CompareTo(b.SegmentName); });
            if (customSegments.Count > 0)
                segments.Add(new UserSegment() { SegmentName = "____________________________", SegmentId= "" });
            segments.AddRange(customSegments);
            return segments;



            //XNamespace dxp = "http://schemas.google.com/analytics/2009";

            //XName segmentElementName = dxp + "segment";

            //IEnumerable<XElement> segmentElements = xDoc.Root.Elements(segmentElementName);
            //UserSegment noSegment = new UserSegment();
            //noSegment.SegmentName = "Default (use if uncertain)";
            //noSegment.SegmentId = "";
            //segments.Add(noSegment);

            //foreach (XElement segmentElement in segmentElements)
            //{
            //    UserSegment segment = new UserSegment();
            //    segment.SegmentId = segmentElement.FirstAttribute.Value;
            //    segment.SegmentName = segmentElement.FirstAttribute.NextAttribute.Value;                

            //    if (!segmentElement.FirstAttribute.Value.Contains("-"))
            //    {
            //        customerSegments.Add(segment);
            //    }
            //    else 
            //    {
            //        segments.Add(segment);
            //    }
            //}

            //UserSegment defCustSeparator = new UserSegment();
            //defCustSeparator.SegmentName = "____________________________";
            //defCustSeparator.SegmentId = "";
            //segments.Add(defCustSeparator);
            //List<UserSegment> allSegments = new List<UserSegment>();
            //foreach (UserSegment segment in customerSegments)
            //{
            //    segments.Add(segment);
            //}

            //return segments;
        }
        private List<Entry> ExtractProfileDataFromXml(XDocument xDoc, List<Entry> accounts)
        {
            XNamespace nsDxp = "http://schemas.google.com/analytics/2009";
            XNamespace nsAtom = "http://www.w3.org/2005/Atom";
            XName entryElementName = nsAtom + "entry";
            XName updatedElement = nsAtom + "updated";
            XName propertyElement = nsDxp + "property";

            List<Entry> profiles = new List<Entry>();
            IEnumerable<XElement> entryElements = xDoc.Root.Elements(entryElementName);
            foreach (XElement entryElement in entryElements)
            {
                IEnumerable<XElement> properties = entryElement.Elements(propertyElement);

                string title = properties.SingleOrDefault(p => p.Attribute("name").Value.Equals("ga:profileName")).Attribute("value").Value;
                string tableId = properties.SingleOrDefault(p => p.Attribute("name").Value.Equals("dxp:tableId")).Attribute("value").Value;
                string updated = entryElement.Element(updatedElement).Value;
                string accountId = properties.SingleOrDefault(p => p.Attribute("name").Value.Equals("ga:accountId")).Attribute("value").Value;

                Entry account = accounts.Single(a => a.AccountId == accountId);
                Entry profile = new Entry()
                {
                    AccountId=accountId,
                    AccountName=account.AccountName,
                    ProfileId=tableId,
                    Title=title,
                    LastUpdated=updated
                };
                profiles.Add(profile);
                
            }
            return profiles;
        }
        private List<Entry> ExtractAccountDataFromXml(XDocument xDoc)
        {

            XNamespace nsDxp = "http://schemas.google.com/analytics/2009";
            XNamespace nsAtom = "http://www.w3.org/2005/Atom";
            List<Entry> entrys = new List<Entry>();

            XName entryElementName = nsAtom + "entry";
            XName propertyElement = nsDxp + "property";

            IEnumerable<XElement> entryElements = xDoc.Root.Elements(entryElementName);

            foreach (XElement entryElement in entryElements)
            {
                IEnumerable<XElement> properties = entryElement.Elements(propertyElement);

                string accountName = properties.Where(p => p.Attribute("name").Value.Equals("ga:accountName")).First().Attribute("value").Value;
                string accountId = properties.Where(p => p.Attribute("name").Value.Equals("ga:accountId")).First().Attribute("value").Value;

                entrys.Add(new Entry() {
                        AccountName = accountName,
                        AccountId=accountId
                });
            }


            //XNamespace dxp = "http://schemas.google.com/analytics/2009";
            //XNamespace atom = "http://www.w3.org/2005/Atom";

            //string webPropertyId = "ga:webPropertyId";
            //string profileID = "ga:profileId";
            //string accountName = "ga:accountName";
            //string accountId = "ga:accountId";
            //string name = "name";
            //string value = "value";
            
            //XName title = atom + "title";
            //XName link = atom + "link";
            //XName updated = atom + "updated";
            
            //XName entryElementName = atom + "entry";
            //XName propertyElementName = dxp + "property";
            
            //IEnumerable<XElement> entryElements = xDoc.Root.Elements(entryElementName);
            //foreach (XElement entryEmelent in entryElements)
            //{
            //    Entry entry = new Entry();
            //    entry.Title = entryEmelent.Element(title).Value;
            //    entry.AccountLink = entryEmelent.Element(link).Value;
            //    entry.LastUpdated = entryEmelent.Element(updated).Value;

            //    //foreach (XElement propEle in entryEmelent.Elements(propertyElementName))
            //    //{
            //    //    if (propEle.Attribute(name).Value == profileID)
            //    //    {
            //    //        entry.ProfileId = "ga:" + propEle.Attribute(value).Value;
            //    //    }
            //    //    if (propEle.Attribute(name).Value == webPropertyId)
            //    //    {
            //    //        entry.WebPropertyId = propEle.Attribute(value).Value;
            //    //    }
            //    //    if (propEle.Attribute(name).Value == accountId)
            //    //    {
            //    //        entry.AccountId = propEle.Attribute(value).Value;
            //    //    }
            //    //    if (propEle.Attribute(name).Value == accountName)
            //    //    {
            //    //        entry.AccountName = propEle.Attribute(value).Value;
            //    //    }
            //    //}
            //    entrys.Add(entry);
            //}
            return entrys.OrderBy(p => p.Title).ToList<Entry>();
        }


        public string Authenticate(string email, string password , out HttpStatusCode responseCode)
        {
            string uri = "https://www.google.com/accounts/ClientLogin";
            WebRequest request = HttpRequestFactory.Instance.CreateRequest(uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            UTF8Encoding encoding = new UTF8Encoding();
            string service = "analytics";
            string source = "Excellent Analytics " + System.Reflection.AssemblyName.GetAssemblyName(System.Reflection.Assembly.GetExecutingAssembly().Location).Version.ToString();            
            string requestContent = "accountType=GOOGLE&Email=" + System.Web.HttpUtility.UrlEncode(email) + "&Passwd=" + System.Web.HttpUtility.UrlEncode(password) + "&service=" + service + "&source=" + source;
            request.ContentLength = encoding.GetByteCount(requestContent);


            NotifySubscribers(10, "begin auth");

            HttpWebResponse response = null;
            HttpStatusCode errorCode = HttpStatusCode.Forbidden;
            try
            {
                using (Stream reqStm = request.GetRequestStream())
                {
                    reqStm.Write(encoding.GetBytes(requestContent), 0,
                                 encoding.GetByteCount(requestContent));
                    NotifySubscribers(20, "send request");
                }
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                    {
                        NotifySubscribers(60, "get response");
                        StreamReader responseReader = new StreamReader(response.GetResponseStream());
                        string responseContent = responseReader.ReadToEnd();
                        string[] ids = responseContent.Split('\n');
                        string authLine = (string)ids.First(id => id.StartsWith("Auth="));
                        string authToken = authLine.Substring(authLine.LastIndexOf('=') + 1);
                        responseCode = response.StatusCode;
                        NotifySubscribers(100, "auth successful");
                        return authToken;
                    }
                }
            }
            catch (WebException ex)
            {
                if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.ProxyAuthenticationRequired) 
                {
                    errorCode = HttpStatusCode.ProxyAuthenticationRequired;    
                }
                if (ex.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    errorCode = HttpStatusCode.NotFound;
                }
                NotifySubscribers(60, "request failed: " + ex.Status.ToString());
            }
            responseCode = response != null ? response.StatusCode : errorCode;
            return null;
        }
    }
}
