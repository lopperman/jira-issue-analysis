using System.ComponentModel.DataAnnotations;
using System.Text;
using JTIS.Config;
using JTIS.Console;
using JTIS.Extensions;
using Spectre.Console;

namespace JTIS.Data;

public class CycleTimeEvent
{
    public jtisIssue Issue {get; private set;}
    public DateTime cycleTimeStart {get;private set;}
    public DateTime cycleTimeEnd {get;private set;}
    public JiraStatus ActualStatusStart {get;private set;}
    public JiraStatus ActualStatusEnd {get;private set;}    
    public TimeSpan CycleTimeBusDays {
        get {
            return TimeSlots.BusinessTime(cycleTimeStart,cycleTimeEnd);
        }
    }
    public TimeSpan CycleTimeCalDays {
        get {
            return TimeSlots.CalendarTime(cycleTimeStart,cycleTimeEnd);
        }
    }
    public TimeSpan CycleTimeBlockedBus {
        get{
            return Issue.Blockers.BlockedTime(cycleTimeStart,cycleTimeEnd,false);
        }
    }
    public TimeSpan CycleTimeBlockedCal {
        get{
            return Issue.Blockers.BlockedTime(cycleTimeStart,cycleTimeEnd,true);
        }
    }

    public CycleTimeEvent(jtisIssue iss, DateTime start, DateTime end, JiraStatus actualStartStatus, JiraStatus actualEndStatus)
    {
        Issue = iss;
        cycleTimeStart = start;
        cycleTimeEnd = end;
        ActualStatusStart = actualStartStatus;
        ActualStatusEnd = actualEndStatus;
    }

}
public class SliceDice
{

    public JiraStatus? StartMin {get;set;} = null;
    public JiraStatus? EndMin {get;set;} = null;
    public string IssueType {get;set;} = string.Empty;
    private jtisIssueData? _data = null;
    private List<CycleTimeEvent> _ctEvents = new List<CycleTimeEvent>();
    public DateTime SearchDate {get;set;}
    public List<CycleTimeEvent> CycleTimeEvents {        
            get {
                return _ctEvents;
            }
        }


    SortedDictionary<DateTime,List<jtisIssue>> _issues = new SortedDictionary<DateTime, List<jtisIssue>>();

    public void AddIssues(JiraStatus startMin, JiraStatus endMin, string issueType, int weeksOld)
    {
        FetchOptions options = FetchOptions.DefaultFetchOptions;
        options.AllowManualJQL().IncludeChangeLogs().RequiredIssueStatusSequence();
        var updDt = DateTime.Today.StartOfWeek().AddDays(-(weeksOld*7));
        SearchDate = updDt;
        string jql = $"project={CfgManager.config.defaultProject} and issueType='{issueType}' and status in ({BuildGTEStatusInList(endMin)}) and updated >= '{updDt.ToString("yyyy-MM-dd")}'";        
        options.JQL=jql;
        ConsoleUtil.WriteBanner($"Running JQL Query: {jql}");
        _data = IssueFetcher.FetchIssues(options);
        if (_data != null && _data.jtisIssueCount > 0)
        {
            foreach (var iss in _data.jtisIssuesList)
            {
                var firstStat = iss.EnteredOnOrAfter(startMin,true);
                var lastStat = iss.EnteredOnOrAfter(endMin,false);
                var startJiraStatus = CfgManager.config.DefaultStatuses.Single(x=>x.StatusName.StringsMatch(firstStat.IssueStatus));
                var endJiraStatus = CfgManager.config.DefaultStatuses.Single(x=>x.StatusName.StringsMatch(lastStat.IssueStatus));
                if (Math.Round(TimeSlots.BusinessTime(firstStat.FirstEntryDate,lastStat.LastEntryDate).TotalDays,2) > 0)
                {    
                    _ctEvents.Add(new CycleTimeEvent(iss,firstStat.FirstEntryDate, lastStat.LastEntryDate,startJiraStatus, endJiraStatus));
                }
                // AnsiConsole.WriteLine($"{iss.jIssue.Key} CURRENT STATUS: {iss.jIssue.StatusName}{Environment.NewLine}{"\t"} 'FROM' {firstStat.IssueStatus} on {firstStat.LastEntryDate},  'TO' {lastStat.IssueStatus} on {lastStat.LastEntryDate}");
            }
        }
    }


    private string BuildGTEStatusInList(JiraStatus startStatus)
    {
        StringBuilder sb = new StringBuilder();
        var issStatuses = CfgManager.config.DefaultStatuses;
        foreach (var issSt in issStatuses.Where(x=>x.ProgressOrder >= startStatus.ProgressOrder).ToList())
        {
            if (sb.Length == 0) {
                sb = sb.Append($"'{issSt.StatusName}'");
            } else {
                sb = sb.Append($", '{issSt.StatusName}'");
            }
        }
        return sb.ToString();
    }

}