using System.Linq;
using Atlassian.Jira;
using JTIS.Analysis;
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

    public TimeSpan CalendarTimeTotal {
        get {
            return _CalendarTimeTotal;
        }
    }
    public TimeSpan BusinessTimeTotal {
        get {
            return _BusinessTimeTotal;
        }
    }

    public TimeSpan BlockedCalendarTime {
        get {
            return _blockedCalendarTime;
        }
    }

    public TimeSpan BlockedBusinessTime {
        get {
            return _blockedBusinessTime;
        }
    }

    public TimeSpan UnblockedCalendarTime {
        get {
            return _CalendarTimeTotal.Subtract(_blockedCalendarTime);
        }
    }

    public TimeSpan UnblockedBusinessTime {
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
            return _statuses;
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

        for (int i = 0 ; i < _statusChanges.Count ; i ++)
        {
            bool lastStatus = (i == _statusChanges.Count -1);
            var cli = ChangeLogItem(_statusChanges[i]);
            jtisStatus? item = _statuses.SingleOrDefault(x=>x.IssueStatus.StringsMatch(cli.ToValue));
            if (item == null) {
                item = new jtisStatus(jtisIss.jIssue.Key,jtisIss.jIssue.IssueType,cli.ToValue ?? "ERROR", jtisIss.StatusCategory);
                _statuses.Add(item);
            }
            item.AddEntry(cli.ToValue ?? "ERROR",_statusChanges[i].CreatedDate, lastStatus ? null : _statusChanges[i+1].CreatedDate, jtisIss);
        }

    }

    private IssueChangeLogItem ChangeLogItem(IssueChangeLog changeLog)
    {
        return changeLog.Items.Single(x=>x.FieldName.StringsMatch("status"));
    }
}