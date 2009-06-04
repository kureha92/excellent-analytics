using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Analytics.Authorization
{
    public class Entry
    {
        private string _profileId;
        private string _title;
        private string _lastUpdated;
        private string _accountName;
        private string _webPropertyId;
        private string _accountLink;
        private string _accountId;

        public string AccountId
        {
            get { return _accountId; }
            set { _accountId = value; }
        }


        public string ProfileId
        {
            get { return _profileId; }
            set { _profileId = value; }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string LastUpdated
        {
            get { return _lastUpdated; }
            set { _lastUpdated = value; }
        }

        public string AccountName
        {
            get { return _accountName; }
            set { _accountName = value; }
        }

        public string WebPropertyId
        {
            get { return _webPropertyId; }
            set { _webPropertyId = value; }
        }

        public string AccountLink
        {
            get { return _accountLink; }
            set { _accountLink = value; }
        }
    }
}
