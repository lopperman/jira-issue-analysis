using System.Reflection.Emit;
using Atlassian.Jira;
using JTIS.Extensions;

namespace JTIS.Data;

    public enum StatusCategoryEnum
    {
        scToDo = 1, 
        scInProgress = 2,
        scDone = 3
    }

public class jtisTimeSpan
{
    private Guid BuildKey {get;set;} = Guid.Empty;
    public DateTime StartDt {get;set;} = DateTime.MinValue;
    public DateTime? EndDt {get;set;} = null;
    public string IssueStatus {get;set;} = string.Empty;
    public StatusCategoryEnum StatusCategory {get;set;}
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
        return null;

    }


}