using System.Linq;
using JTIS.Analysis;
using JTIS.Config;
using JTIS.Extensions;

namespace JTIS.Data;

public class jtisStatus 
{
    
    
    public string Key {get;private set;} = string.Empty;
    public string StatusId {get;set;}
    public string IssueType {get;private set;} = string.Empty;
    public string IssueStatus {get;private set;} = string.Empty;
    public int StatusSequence {get;set;}
    public StatusType StatusCategory {get;private set;} = StatusType.stUnknown;
    private SortedDictionary<DateTime,DateTime?> _startEnd = new SortedDictionary<DateTime, DateTime?>();
    private TimeSpan _blockedCalendarTime = new TimeSpan();
    private TimeSpan _CalendarTimeTotal = new TimeSpan();
    private TimeSpan _blockedBusinessTime = new TimeSpan();
    private TimeSpan _BusinessTimeTotal = new TimeSpan();

    public jtisStatus(string key, string issueType, string status, StatusType statusCategory, string statusId, int sequence)
    {
        Key=key;
        IssueType = issueType;
        IssueStatus = status;
        StatusCategory = statusCategory;
        StatusId=statusId;
        StatusSequence = sequence;
    }
    public void AddEntry(string status, DateTime startDt, DateTime? endDt, jtisIssue issue)
    {
        if (IssueStatus.StringsMatch(status)==false) 
        {
            throw new ArgumentException($"status: '{status}' must match existing IssueStatus: '{IssueStatus}'");
        }
        _startEnd.Add(startDt, endDt);
        var useEndDt = endDt.HasValue ? endDt.Value : DateTime.Now;
        if (StatusCategory == StatusType.stEnd)
        {
            useEndDt = startDt;
        }
        
        _blockedBusinessTime = _blockedBusinessTime.Add(issue.Blockers.BlockedTime(startDt,useEndDt,false));
        _blockedCalendarTime = _blockedCalendarTime.Add(issue.Blockers.BlockedTime(startDt,useEndDt,true));
        _CalendarTimeTotal = _CalendarTimeTotal.Add(TimeSlots.CalendarTime(startDt,useEndDt));
        _BusinessTimeTotal = _BusinessTimeTotal.Add(TimeSlots.BusinessTime(startDt,useEndDt));
    }

    public JiraStatus? LocalStatus 
    {
        get 
        {
            return CfgManager.config.StatusConfigs.SingleOrDefault(x=>x.StatusName.StringsMatch(IssueStatus) && x.DefaultInUse==true);
        }
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
            DateTime? lastExit = _exitDates.LastOrDefault();
            if (lastExit.HasValue && lastExit.Value.CompareTo(DateTime.MinValue)==0)
            {
                lastExit = null;
            }
            return lastExit;
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
