

using JTIS.Config;

namespace JTIS
{
    public class StatusSummary
    {
        public string Key {get;set;}
        public string Status {get;set;}
        public DateTime? FirstEntry {get;set;}
        public DateTime? LastEntry {get;set;}
        public DateTime? FirstExit {get;set;}
        public DateTime? LastExit {get;set;}
        public int EntryCount {get;set;}
        public TimeSpan CalTime {get;set;}
        public TimeSpan BusTime {get;set;}
        public TimeSpan BlockTime {get;set;}
        //public DateTime StartDt {get;set ;}
        public StatusType TrackType {get;set;}         
        public SortedDictionary<string,double> StatusCategoryDays {get; private set;}
        public SortedDictionary<string,double> StatusDays {get; private set;}

        public StatusSummary()
        {
            
            Key = string.Empty;
            Status = string.Empty;
            StatusCategoryDays = new SortedDictionary<string, double>();
            StatusDays = new SortedDictionary<string, double>();
        }


    }
}