using System.Data;
using System.Runtime.InteropServices;
using JTIS.Extensions;

namespace JTIS.Data;


public class jtisBlockers 
{
    private string _key = string.Empty;
    private List<jtisBlocker> _blockers = new List<jtisBlocker>();

    public TimeSpan BlockedTime(DateTime startDt, DateTime endDt, bool includeWeekends = false)
    {
        if (_blockers.Count() == 0)
        {
            return new TimeSpan();
        }

        //remove any overlapping blocker time
        List<TimeSlot> timeIn = new List<TimeSlot>();

        var ts = new TimeSpan();
        var filteredBlockers = _blockers.Where(x=>x.EndDt > startDt && x.StartDt < endDt).ToList();
        if (filteredBlockers.Count()==0)
        {
            return new TimeSpan();
        }
        var adjustedBlockers = new List<jtisBlocker>();
        foreach (var b1 in filteredBlockers)
        {
            var newBlocker = new jtisBlocker();
            newBlocker.StartDt = b1.StartDt < startDt ? startDt : b1.StartDt;
            newBlocker.EndDt = b1.EndDt > endDt ? endDt : b1.EndDt;            
            adjustedBlockers.Add(newBlocker);
        }
        foreach (var block in adjustedBlockers)
        {
            timeIn.Add(new TimeSlot(block.StartDt,block.EndDt));
        }
        var timeOut = TimeSlots.SlicedTimeSlots(timeIn.ToArray());



        foreach (var slice in timeOut)
        {
            if (includeWeekends)
            {
                ts = ts.Add(TimeSlots.CalendarTime(slice.StartDate,slice.EndDate));
            }
            else 
            {
                ts = ts.Add(TimeSlots.BusinessTime(slice.StartDate,slice.EndDate));
            }
        }

        return ts;
    }

    public IReadOnlyList<jtisBlocker> Blockers
    {
        get{
            return _blockers;
        }
    }
    private jtisBlockers(string key)
    {
        _key = key;
    }
    public static jtisBlockers Create(jtisIssue issue)
    {
        var blocks = new jtisBlockers(issue.jIssue.Key);
        blocks.PopulateBlockers(issue);
        return blocks;
    }

    private void PopulateBlockers(jtisIssue iss)
    {
        //add all flagged -> impediment blockers, then
        //add all field blockers, adjusting for duplicate timespans

        var stillBlockedDate = DateTime.Now;

        CreateFlaggedBlockers(iss, stillBlockedDate);
        CreatePriorityFieldBlockers(iss,stillBlockedDate);
    }

    private void CreatePriorityFieldBlockers(jtisIssue iss, DateTime stillBlockedDate)
    {
        var impedimentStartList = iss.ChangeLogs.Where(x=>x.Items.Any(y=>y.FieldName.StringsMatch("priority") && y.ToValue.StringsMatch("block",StringCompareType.scContains))).ToList();
        var impedimentEndList = iss.ChangeLogs.Where(x=>x.Items.Any(y=>y.FieldName.StringsMatch("priority") && y.FromValue.StringsMatch("block",StringCompareType.scContains))).ToList();
        foreach (var chLog in impedimentStartList)
        {
            var blocker = new jtisBlocker();
            blocker.StartDt = chLog.CreatedDate;

            if (impedimentEndList.Any(x=>x.CreatedDate > blocker.StartDt))
            {
                blocker.EndDt = impedimentEndList.Where(x=>x.CreatedDate > blocker.StartDt).Min(y=>y.CreatedDate);
            }
            else 
            {
                blocker.EndDt = stillBlockedDate;
            }
            blocker.FieldName = "Priority";
            _blockers.Add(blocker);
        }    
    }

    private void CreateFlaggedBlockers(jtisIssue iss, DateTime stillBlockedDate)
    {
        var impedimentStartList = iss.ChangeLogs.Where(x=>x.Items.Any(y=>y.FieldName.StringsMatch("flagged") && y.ToValue.StringsMatch("impediment"))).ToList();
        var impedimentEndList = iss.ChangeLogs.Where(x=>x.Items.Any(y=>y.FieldName.StringsMatch("flagged") && y.FromValue.StringsMatch("impediment"))).ToList();
        foreach (var chLog in impedimentStartList)
        {
            var blocker = new jtisBlocker();
            blocker.StartDt = chLog.CreatedDate;
            if (impedimentEndList.Any(x=>x.CreatedDate > blocker.StartDt))
            {
                blocker.EndDt = impedimentEndList.Where(x=>x.CreatedDate > blocker.StartDt).Min(y=>y.CreatedDate);
            }
            else 
            {
                blocker.EndDt = stillBlockedDate;
            }
            blocker.FieldName = "Flagged";
            _blockers.Add(blocker);
        }
    }
}

public class jtisBlocker
{
    public Guid guid = Guid.NewGuid();
    public DateTime StartDt {get;set;} = DateTime.MinValue;
    public DateTime EndDt {get;set;} = DateTime.MinValue;
    
    //priority field contains 'block', or 
    //Flagged field contains 'impediment'
    public string FieldName {get;set;} = string.Empty;    
    public bool DuplicateAdjusted {get;set;}

}