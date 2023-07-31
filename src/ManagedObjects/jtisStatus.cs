using System.Linq;
using JTIS.Analysis;
using JTIS.Config;
using JTIS.Extensions;

namespace JTIS.Data;

public class jtisStatus: ITimeSummary 
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

    public string StatusCategoryToString
    {
        get {
            return Enum.GetName(typeof(StatusType),StatusCategory);
        }
    }

    public bool IsActiveStatus {
        get{
            return StatusCategory==StatusType.stActiveState || StatusCategory==StatusType.stStart;
        }
    }
    public bool IsDoneStatus {
        get{
            return StatusCategory==StatusType.stEnd;
        }
    }
    public bool IsPassiveStatus {
        get{
            return StatusCategory == StatusType.stPassiveState;
        }
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

    public TimeSpan tsCalendarTime {
        get {
            return _CalendarTimeTotal;
        }
    }
    public TimeSpan tsBusinessTime {
        get {
            return _BusinessTimeTotal;
        }
    }

    public TimeSpan tsBlockedCalendarTime {
        get {
            return _blockedCalendarTime;
        }
    }

    public TimeSpan tsBlockedBusinessTime {
        get {
            return _blockedBusinessTime;
        }
    }

    public TimeSpan tsUnblockedCalendarTime {
        get {
            return _CalendarTimeTotal.Subtract(_blockedCalendarTime);
        }
    }

    public TimeSpan tsUnblockedBusinessTime {
        get {
            return _BusinessTimeTotal.Subtract(_blockedBusinessTime);
        }
    }

    public TimeSpan tsActiveCalTime {
        get{
            if (IsActiveStatus)
            {
                return tsCalendarTime;
            }
            else 
            {
                return new TimeSpan();
            }
        }
    }

    public TimeSpan tsPassiveCalTime {
        get{
            if (IsPassiveStatus)
            {
                return tsCalendarTime;
            }
            else 
            {
                return new TimeSpan();
            }

        }
    }

    public TimeSpan tsBlockedActiveCalTime {
        get{
            if (IsActiveStatus)
            {
                return tsBlockedCalendarTime;
            }
            else 
            {
                return new TimeSpan();
            }

        }
    }

    public TimeSpan tsUnblockedActiveCalTime {
        get{
            if (IsActiveStatus)
            {
                return tsUnblockedCalendarTime;
            }
            else 
            {
                return new TimeSpan();
            }
        }
    }

    public TimeSpan tsBlockedPassiveCalTime {
        get{
            if (IsPassiveStatus)
            {
                return tsBlockedCalendarTime;
            }
            else 
            {
                return new TimeSpan();
            }
        }
    }

    public TimeSpan tsUnblockedPassiveCalTime {
        get{
            if (IsPassiveStatus)
            {
                return tsUnblockedCalendarTime;
            }
            else 
            {
                return new TimeSpan();
            }
        }
    }

    public TimeSpan tsActiveBusTime {
        get{
            if (IsActiveStatus)
            {
                return tsBusinessTime;
            }
            else 
            {
                return new TimeSpan();
            }
        }
    }
    public TimeSpan tsPassiveBusTime {
        get{
            if (IsPassiveStatus)
            {
                return tsBusinessTime;
            }
            else 
            {
                return new TimeSpan();
            }
        }
    }
    public TimeSpan tsBlockedActiveBusTime {
        get{
            if (IsActiveStatus)
            {
                return tsBlockedBusinessTime;
            }
            else 
            {
                return new TimeSpan();
            }
        }
    }
    public TimeSpan tsUnblockedActiveBusTime {
        get{
            if (IsActiveStatus)
            {
                return tsUnblockedBusinessTime;
            }
            else 
            {
                return new TimeSpan();
            }
        }
    }

    public TimeSpan tsBlockedPassiveBusTime {
        get{
            if (IsPassiveStatus)
            {
                return tsBlockedBusinessTime;
            }
            else 
            {
                return new TimeSpan();
            }
        }
    }
    public TimeSpan tsUnblockedPassiveBusTime {
        get{
            if (IsPassiveStatus)
            {
                return tsUnblockedBusinessTime;
            }
            else 
            {
                return new TimeSpan();
            }
        }
    }

}
