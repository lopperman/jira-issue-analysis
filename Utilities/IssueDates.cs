using System;


namespace JiraCon
{
    public enum DateTypeEnum
    {
        dtUnknown = 0, 
        dtStatus = 1, 
        dtBlocker = 2
    }

    public class IssueDates
    {
        public List<DateDet> DateList {get;set;}
        private DateTime NullEndDt {get;set;}
        public IssueDates(DateTime nullEndStaticForNow)
        {
            NullEndDt = nullEndStaticForNow;
            DateList = new List<DateDet>();
        }
        public IReadOnlyList<DateDet> AddDateDet(string issueKey, string projectKey, DateTypeEnum dateType, DateTime startDt, DateTime? endDt = null) 
        {
            var dd = new DateDet();
            dd.IssueKey = issueKey;
            dd.ProjectKey = projectKey;
            dd.DtType = dateType;
            dd.StartDt = startDt;

            DateList.Add(dd);
            return DateList;
        }



    }

        public class DateDet
        {
            public string IssueKey{get;set;}            
            public string ProjectKey {get;set;}
            public DateTypeEnum DtType {get;set;}
            public DateTime StartDt {get;set;}
            public DateTime EndDt {get;set;} 
            public bool FakeEndDt {get;set;}
            public bool FakeStartDt {get;set;}

            public DateDet()
            {
                IssueKey = string.Empty;
                ProjectKey = string.Empty;
                StartDt = DateTime.MinValue;
                EndDt = DateTime.MinValue;

            }

        }

}