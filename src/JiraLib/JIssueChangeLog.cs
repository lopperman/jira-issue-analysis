using Atlassian.Jira;
using Newtonsoft.Json;

namespace JTIS
{
    public class JIssueChangeLog
    {
        private IssueChangeLog? _changeLog;
        private List<JIssueChangeLogItem>? _items ;
        public TimeSpan BlockedTimeSpan {get;set;}
        public TimeSpan BlockedBusinessTimeSpan {get;set;}

        [JsonIgnore]
        public JIssue? JIss {get;private set;}

        public JIssueChangeLog()
        {
            Id = string.Empty;
            Author = string.Empty;
            CreatedDate = DateTime.MinValue;
            BlockedTimeSpan = new TimeSpan();
        }

        public JIssueChangeLog(JIssue parentIssue, IssueChangeLog changeLog):this()
        {
            _changeLog = changeLog;
            JIss = parentIssue ;
            Initialize();
        }

        public string Id { get; set; }
        public string Author { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? EndDate {get;set;}
        

        public List<JIssueChangeLogItem> Items
        {
            get
            {
                if (_items == null) _items = new List<JIssueChangeLogItem>();
                return _items;
            }
            set
            {
                _items = value;
            }
        }

        private void Initialize()
        {
            Id = _changeLog.Id;
            Author = _changeLog.Author.Username;
            CreatedDate = _changeLog.CreatedDate;
            foreach (var i in _changeLog.Items)
            {
                Items.Add(new JIssueChangeLogItem(this, i));
            }

        }

        internal void CheckBlockers(List<Blocker> issueBlockers)
        {
            foreach (var clItem in Items)
            {
                if (clItem.ChangeLogType == ChangeLogTypeEnum.clStatus)
                {                
                    foreach (var b in issueBlockers)
                    {
                        DateTime? bStart = null;
                        DateTime? bEnd = null;
                        
                        if (b.EndDt.HasValue && b.StartDt <= clItem.EndDt && b.EndDt.Value >= clItem.StartDt)
                        {
                            if (b.StartDt > clItem.StartDt)
                            {
                                bStart = b.StartDt;
                            }
                            else 
                            {
                                bStart = clItem.StartDt;
                            }
                            if (b.EndDt.Value > clItem.EndDt)
                            {
                                bEnd = clItem.EndDt;
                            }
                            else 
                            {
                                bEnd = b.EndDt.Value;
                            }

                        }
                        if (bStart.HasValue && bEnd.HasValue)
                        {
                            BlockedTimeSpan = BlockedTimeSpan.Add(bEnd.Value - bStart.Value);

                            var ts = (bEnd.Value - bStart.Value);
                            var tStart = bStart.Value.Date;
                            do
                            {
                                if (bStart.Value.DayOfWeek == DayOfWeek.Saturday || bStart.Value.DayOfWeek == DayOfWeek.Sunday)
                                {
                                    ts = ts.Add(new TimeSpan(-24,0,0));
                                }
                                tStart = tStart.AddDays(1);
                            } while (tStart <= bEnd.Value.Date);
                            BlockedBusinessTimeSpan = BlockedBusinessTimeSpan.Add(ts);
                        }
                    }
                }
            }


        }

        [JsonIgnore]
        public IssueChangeLog JiraChangeLog
        {
            get
            {
                return _changeLog;
            }
            set
            {
                _changeLog = value;
                Initialize();

            }

        }

    }
}
