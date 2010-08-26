using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Analytics.Authorization
{
    public class UserAccount
    {
        private string _authToken;
        private string _eMail;
        private List<Entry> _entrys;
        private DateTime _tokenExpiration;
        private DateTime creationTime;
        private List<UserSegment> _segments;

        #region Properties
        public string EMail
        {
            get { return _eMail; }
            set { _eMail = value; }
        }

        public string AuthToken
        {
            get { return _authToken; }
            set { _authToken = value; }
        }

        public List<Entry> Entrys
        {
            get
            {
                if (_entrys == null)
                {
                    _entrys = new List<Entry>();
                }
                return _entrys;
            }
            set { _entrys = value; }
        }

        public List<UserSegment> Segments
        {
            get
            {
                if (_segments == null)
                {
                    _segments = new List<UserSegment>();
                }
                return _segments;
            }
            set { _segments = value; }
        }

        public DateTime TokenExpiration
        {
            get { return _tokenExpiration; }
        } 
        #endregion

        private UserAccount()
        {

        }

        public UserAccount(string authToken , string eMail)
        {
            _eMail = eMail;
            _authToken = authToken;
            creationTime = DateTime.Now;
            TimeSpan maxAge = new TimeSpan(2, 59, 0);
            _tokenExpiration = creationTime.Add(maxAge);
            System.Timers.Timer t = new System.Timers.Timer(60000.0);
            t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            t.Start();
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (e.SignalTime >= TokenExpiration)
            {
                AuthToken = null;
                (sender as System.Timers.Timer).Stop();
            }
        }
    }
}
