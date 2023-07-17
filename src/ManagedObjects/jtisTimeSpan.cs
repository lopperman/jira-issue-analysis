using System.Reflection.Emit;
using Atlassian.Jira;
using JTIS.Extensions;

namespace JTIS.Data;

    public enum StatusCategoryEnum
    {
        scUnknown = 0,
        scToDo = 1, 
        scInProgress = 2,
        scDone = 3
    }

public class jtisTimeSpan
{
    private Guid BuildKey {get;set;} = Guid.Empty;
    public DateTime StartDt {get;set;} = DateTime.MinValue;
    public DateTime? EndDt {get;set;} = null;
    private DateTime UseEndDt = DateTime.Now;
    public string IssueStatus {get;set;} = string.Empty;
    public StatusCategoryEnum StatusCategory {get;set;} = StatusCategoryEnum.scUnknown;
    public TimeSpan TotalTime {get; private set;}
    public TimeSpan TotalBlockedTime {get; private set;}
    public TimeSpan TotalUnblockedTime {get;private set;}
    public TimeSpan TotalBusinessTime {get; private set;}
    public TimeSpan TotalBusinessBlockedTime {get; private set;}
    public TimeSpan TotalBusinessUnblockedTime {get;private set;}

    private jtisTimeSpan(Guid buildKey)
    {
        //prevent external creation
        BuildKey = buildKey;
    }

    //return null if changeLog is not for an issue status change
    public static jtisTimeSpan? Build(IssueChangeLog changeLog, jtisIssue issue)
    {
        if (changeLog.Items.Any(x=>x.FieldName.StringsMatch("status"))==false)
        {
            return null;
        }
        var guid = Guid.NewGuid();
        jtisTimeSpan result = new jtisTimeSpan(guid);
        if (result.Populate(changeLog, issue))
        {
            return result;
        }
        return null;
    }

    private DateTime? NextStatusChange(jtisIssue issue)
    {
        var laterList = issue.ChangeLogs.Where(x=>x.CreatedDate > StartDt && x.Items.Any(y=>y.FieldName.StringsMatch("status'"))).ToList();
        if (laterList.Count() > 0)
        {
            return laterList.Min(x=>x.CreatedDate);
        }
        return null;

    }
    private bool Populate(IssueChangeLog changeLog, jtisIssue issue)
    {
        StartDt = changeLog.CreatedDate;
        //end date -- if there are any changelogs where startdt > [StartDt] and FieldNmae = 'status'
        EndDt = NextStatusChange(issue);
        if (EndDt == null){UseEndDt = DateTime.Now;}
        else {UseEndDt = EndDt.Value;}
        IssueStatus = changeLog.Items.Single(x=>x.FieldName.StringsMatch("status")).ToValue ?? "ERROR";
        TotalTime = UseEndDt.Subtract(StartDt);
        TotalBusinessTime = BusinessDays(StartDt,UseEndDt);
        
        
        //TODO FINISH
        return false;
        
    }

    private TimeSpan BusinessDays(DateTime start, DateTime end)
    {
        var tStart = start;
        var ts = end.Subtract(start);
        do
        {
            if (tStart.DayOfWeek == DayOfWeek.Saturday || tStart.DayOfWeek == DayOfWeek.Sunday)
            {
                ts = ts.Add(new TimeSpan(-24,0,0));
            }
            tStart = tStart.AddDays(1);
        } while (tStart <= end);
        return ts;
    }        

}