using System.Linq;
using Atlassian.Jira;
using JTIS.Analysis;
using JTIS.Config;
using JTIS.Extensions;

namespace JTIS.Data;

public class jtisStatus 
{
    
    
    public string Key {get;private set;} = string.Empty;
    public string IssueType {get;private set;} = string.Empty;
    public string IssueStatus {get;private set;} = string.Empty;
    public StatusType StatusCategory {get;private set;} = StatusType.stUnknown;
    private SortedDictionary<DateTime,DateTime?> _startEnd = new SortedDictionary<DateTime, DateTime?>();
    private TimeSpan _blockedCalendarTime = new TimeSpan();
    private TimeSpan _CalendarTimeTotal = new TimeSpan();
    private TimeSpan _blockedBusinessTime = new TimeSpan();
    private TimeSpan _BusinessTimeTotal = new TimeSpan();

    public jtisStatus(string key, string issueType, string status, StatusType statusCategory)
    {
        Key=key;
        IssueType = issueType;
        IssueStatus = status;
        StatusCategory = statusCategory;
    }
    public void AddEntry(string status, DateTime startDt, DateTime? endDt, jtisIssue issue)
    {
        if (IssueStatus.StringsMatch(status)==false) 
        {
            throw new ArgumentException($"status: '{status}' must match existing IssueStatus: '{IssueStatus}'");
        }
        _startEnd.Add(startDt, endDt);
        var useEndDt = endDt.HasValue ? endDt.Value : DateTime.Now;
        _blockedBusinessTime = _blockedBusinessTime.Add(issue.Blockers.BlockedTime(startDt,useEndDt,false));
        _blockedCalendarTime = _blockedCalendarTime.Add(issue.Blockers.BlockedTime(startDt,useEndDt,true));
        _CalendarTimeTotal = _CalendarTimeTotal.Add(TimeSlots.CalendarTime(startDt,useEndDt));
        _BusinessTimeTotal = _BusinessTimeTotal.Add(TimeSlots.BusinessTime(startDt,useEndDt));
    }
    private List<DateTime> _exitDates {
        get {
            var _dates = new List<DateTime>();
            if (_startEnd.Values.Count(x=>x!=null)>0)
            {
                _startEnd.Values.Where(x=>x!=null).ToList().ForEach(dt => {
                    _dates.Add(dt.Value);
                });
            }
            return _dates.Order().ToList();
        }
    }
    public int EnteredCount { get {
        return _startEnd.Count();
    }}
    public DateTime FirstEntryDate {get {
        return _startEnd.Min(x=>x.Key);
    }}
    public DateTime LastEntryDate {get {
        return _startEnd.Max(x=>x.Key);
    }}
    public DateTime? FirstExitDate {get {
        
        return _exitDates.FirstOrDefault();
    }}
    public DateTime? LastExitDate { 
        get {
            return _exitDates.LastOrDefault();
        }
    }

    public TimeSpan StatusCalendarTimeTotal {
        get {
            return _CalendarTimeTotal;
        }
    }
    public TimeSpan StatusBusinessTimeTotal {
        get {
            return _BusinessTimeTotal;
        }
    }

    public TimeSpan StatusBlockedCalendarTime {
        get {
            return _blockedCalendarTime;
        }
    }

    public TimeSpan StatusBlockedBusinessTime {
        get {
            return _blockedBusinessTime;
        }
    }

    public TimeSpan StatusUnblockedCalendarTime {
        get {
            return _CalendarTimeTotal.Subtract(_blockedCalendarTime);
        }
    }

    public TimeSpan StatusUnblockedBusinessTime {
        get {
            return _BusinessTimeTotal.Subtract(_blockedBusinessTime);
        }
    }


}

public class jtisStatuses
{
    private List<jtisStatus> _statuses = new List<jtisStatus>();

    public IReadOnlyList<jtisStatus> Statuses
    {
        get{
            return _statuses.OrderBy(x=>x.FirstEntryDate).ToList();
        }
    }

    public jtisStatus? FirstActive 
    {
        get {
            return _statuses.SingleOrDefault(x=>x.StatusCategory==StatusType.stStart);
        }
    }
    public static jtisStatuses Create(jtisIssue issue)
    {
        jtisStatuses newObj = new jtisStatuses();

        newObj.Populate(issue);

        return newObj;
    }

    private void Populate(jtisIssue jtisIss)
    {
        if (jtisIss.ChangeLogs.Count() == 0) {return;}

        List<IssueChangeLog> _statusChanges = jtisIss.ChangeLogs.Where(x=>x.Items.Any(y=>y.FieldName.StringsMatch("status"))).OrderBy(z=>z.CreatedDate).ToList();
        if (_statusChanges.Count() == 0) {return;}

        bool foundFirstActive = false;
        for (int i = 0 ; i < _statusChanges.Count ; i ++)
        {
            bool lastStatus = (i == _statusChanges.Count -1);
            var cli = ChangeLogItem(_statusChanges[i]);
            jtisStatus? item = _statuses.SingleOrDefault(x=>x.IssueStatus.StringsMatch(cli.ToValue));
            var statCat = StatusCategory(cli.ToId);
            // var statCat = jtisIss.StatusCategory;
            if (statCat == StatusType.stActiveState && foundFirstActive == false)
            {
                foundFirstActive = true;
                statCat = StatusType.stStart;
            }
            if (item == null) {
                item = new jtisStatus(jtisIss.jIssue.Key,jtisIss.jIssue.IssueType,cli.ToValue ?? "ERROR", statCat);
                _statuses.Add(item);
            }
            item.AddEntry(cli.ToValue ?? "ERROR",_statusChanges[i].CreatedDate, lastStatus ? null : _statusChanges[i+1].CreatedDate, jtisIss);
        }

    }

    private StatusType StatusCategory(string toStatusId)
    {
        var jStatus = CfgManager.config.StatusConfigs.Single(x=>x.StatusId.ToString().StringsMatch(toStatusId));
        switch (jStatus.CategoryName.ToLower())
        {
            case "done":
                return StatusType.stEnd;;
            case "in progress":
                return StatusType.stActiveState;
            case "to do":
                return StatusType.stPassiveState;       
            case "ignore":
                return StatusType.stIgnoreState;                 
            default:
                return StatusType.stUnknown;;
        }
    }


    public TimeSpan IssueCalendarTimeTotal {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                ts.Add(s.StatusCalendarTimeTotal);
            });
            return ts;
        }
    }
    public TimeSpan IssueBusinessTimeTotal {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                ts.Add(s.StatusBusinessTimeTotal);
            });
            return ts;
        }
    }

    public TimeSpan IssueBlockedCalendarTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                ts.Add(s.StatusBlockedCalendarTime);
            });
            return ts;
        }
    }

    public TimeSpan IssueBlockedBusinessTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                ts.Add(s.StatusBlockedBusinessTime);
            });
            return ts;
        }
    }

    public TimeSpan IssueUnblockedCalendarTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                ts.Add(s.StatusUnblockedCalendarTime);
            });
            return ts;
        }
    }

    public TimeSpan IssueUnblockedBusinessTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                ts.Add(s.StatusUnblockedBusinessTime);
            });
            return ts;
        }
    }

    public TimeSpan IssueBlockedActiveBusTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stActiveState || s.StatusCategory==StatusType.stStart)
                {
                    ts.Add(s.StatusBlockedBusinessTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan IssueUnblockedActiveBusTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stActiveState || s.StatusCategory==StatusType.stStart)
                {
                    ts.Add(s.StatusUnblockedBusinessTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan IssueBlockedActiveCalTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stActiveState || s.StatusCategory==StatusType.stStart)
                {
                    ts.Add(s.StatusBlockedCalendarTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan IssueUnblockedActiveCalTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stActiveState || s.StatusCategory==StatusType.stStart)
                {
                    ts.Add(s.StatusUnblockedCalendarTime);
                }
            });
            return ts;
        }
    }

    public TimeSpan IssueBlockedPassiveBusTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stPassiveState)
                {
                    ts.Add(s.StatusBlockedBusinessTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan IssueUnblockedPassiveBusTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stPassiveState)
                {
                    ts.Add(s.StatusUnblockedBusinessTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan IssueBlockedPassiveCalTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stPassiveState)
                {
                    ts.Add(s.StatusBlockedCalendarTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan IssueUnblockedPassiveCalTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stPassiveState)
                {
                    ts.Add(s.StatusUnblockedCalendarTime);
                }
            });
            return ts;
        }
    }

    public TimeSpan IssueTotalActiveCalTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stActiveState || s.StatusCategory==StatusType.stStart)
                {
                    ts.Add(s.StatusCalendarTimeTotal);
                }
            });
            return ts;
        }
    }
    public TimeSpan IssueTotalActiveBusTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stActiveState || s.StatusCategory==StatusType.stStart)
                {
                    ts.Add(s.StatusBusinessTimeTotal);
                }
            });
            return ts;
        }
    }
    public TimeSpan IssueTotalPassiveCalTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stPassiveState)
                {
                    ts.Add(s.StatusCalendarTimeTotal);
                }
            });
            return ts;
        }
    }
    public TimeSpan IssueTotalPassiveBusTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stPassiveState)
                {
                    ts.Add(s.StatusBusinessTimeTotal);
                }
            });
            return ts;
        }
    }

    private IssueChangeLogItem ChangeLogItem(IssueChangeLog changeLog)
    {
        return changeLog.Items.Single(x=>x.FieldName.StringsMatch("status"));
    }
}