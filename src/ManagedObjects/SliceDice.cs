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
    public List<string> IgnoredIssues {get;private set;} = new List<string>();
    public JiraStatus? StartMin {get;set;} = null;
    public JiraStatus? EndMin {get;set;} = null;
    public string IssueType {get;set;} = string.Empty;
    private jtisIssueData? _data = null;
    private List<CycleTimeEvent> _ctEvents = new List<CycleTimeEvent>();
    public DateTime SearchStartDt {get;set;}
    public DateTime SearchEndDt {get;set;}
    public List<CycleTimeEvent> CycleTimeEvents {        
            get {
                return _ctEvents;
            }
        }


    SortedDictionary<DateTime,List<jtisIssue>> _issues = new SortedDictionary<DateTime, List<jtisIssue>>();

    public bool IgnoreIfMissingStartState
    {
        get {
            bool cfgValue = true;
            if (CfgManager.config.cfgOptions.items.Exists(x=>x.configOption==CfgEnum.cfgCTIngoreIfMissingStart))
            {
                cfgValue = CfgManager.config.cfgOptions.items.Single(x=>x.configOption==CfgEnum.cfgCTIngoreIfMissingStart).Enabled;
            }
            return cfgValue;
        }
    }
    public void AddIssues(JiraStatus startMin, JiraStatus endMin, string issueType, DateTime startDt, DateTime endDt)
    {
        SearchStartDt = startDt;
        SearchEndDt = endDt;
        FetchOptions options = FetchOptions.DefaultFetchOptions;
        options.AllowManualJQL().IncludeChangeLogs().RequiredIssueStatusSequence();
        string jql = $"project={CfgManager.config.defaultProject} and issueType='{issueType}' and status in ({BuildGTEStatusInList(endMin)}) and updated >= '{startDt.ToString("yyyy-MM-dd")}' and updated <= '{endDt.ToString("yyyy-MM-dd")}'";        
        options.JQL=jql;
        ConsoleUtil.WriteBanner($"Running JQL Query: {jql}");
        _data = IssueFetcher.FetchIssues(options);
        if (_data != null && _data.jtisIssueCount > 0)
        {
            bool ignoreIfStartStateMissing = IgnoreIfMissingStartState;
            foreach (var iss in _data.jtisIssuesList)
            {
                bool canAdd = true;
                var firstStat = iss.EnteredOnOrAfter(startMin);
                var summary = Markup.Escape(iss.jIssue.Summary);
                var lastStat = iss.EnteredOnOrAfter(endMin);
                var line2 = $"{Environment.NewLine}  [dim]Status: {iss.jIssue.StatusName}, Summary: {summary}[/]";
                if (firstStat == null || lastStat == null)
                {
                    if (firstStat == null){
                        IgnoredIssues.Add($"{iss.jIssue.Key} was ignored - start status could not be determined.{line2}");
                    }
                    if (lastStat == null){
                    }
                        IgnoredIssues.Add($"{iss.jIssue.Key} was ignored - end status could not be determined.{line2}");
                    canAdd = false;
                }
                if (canAdd && ignoreIfStartStateMissing)
                {
                    if (firstStat.IssueStatus.StringsMatch(lastStat.IssueStatus))
                    {
                        IgnoredIssues.Add($"{iss.jIssue.Key} was ignored - start and end statuses are same status{line2}");
                        canAdd = false;
                    }
                }
                if (canAdd)
                {
                    var startJiraStatus = CfgManager.config.LocalProjectDefaultStatuses.Single(x=>x.StatusName.StringsMatch(firstStat.IssueStatus));
                    var endJiraStatus = CfgManager.config.LocalProjectDefaultStatuses.Single(x=>x.StatusName.StringsMatch(lastStat.IssueStatus));
                    if (Math.Round(TimeSlots.BusinessTime(firstStat.FirstEntryDate,lastStat.LastEntryDate).TotalDays,2) > 0)
                    {    
                        _ctEvents.Add(new CycleTimeEvent(iss,firstStat.FirstEntryDate, lastStat.LastEntryDate,startJiraStatus, endJiraStatus));
                    }
                    else 
                    {
                        IgnoredIssues.Add($"{iss.jIssue.Key} was ignored - time from start to end state was '0' Days{line2}");
                    }
                }

            }
        }
    }
    private string BuildGTEStatusInList(JiraStatus startStatus)
    {
        StringBuilder sb = new StringBuilder();
        var issStatuses = CfgManager.config.LocalProjectDefaultStatuses;
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

    public BarChart? EnteredStateChart(string issueType)
    {
        var tmpIssList = _data.jtisIssuesList.Where(x=>x.jIssue.IssueType.StringsMatch(issueType));

        if (tmpIssList.Count() == 0 ) {return null;}
        BarChart chart = new BarChart();

        SortedList<int,string> seqStat = new SortedList<int, string>();

        var uniqueStatuses = tmpIssList.SelectMany(x=>x.StatusItems.Statuses).Select(y=>y.IssueStatus).Distinct().ToList();
        int clr = 0;
        foreach (var stat in uniqueStatuses)
        {
            clr += 1;
            JiraStatus? localStatus = CfgManager.config.StatusConfigs.SingleOrDefault(x=>x.DefaultInUse==true && x.StatusName.StringsMatch(stat));
            Color? tmpColor;
            if (localStatus != null)
            {
                seqStat.Add(localStatus.ProgressOrder, stat);
                if (localStatus.ChartColor != null)
                {
                    tmpColor = Style.Parse(localStatus.ChartColor).Foreground;
                }
                else 
                {
                    tmpColor = Color.FromInt32(clr);
                }
            }
            else 
            {
                seqStat.Add(0, stat);
                tmpColor = Color.FromInt32(clr);
            }
            var tStatuses = tmpIssList.SelectMany(x=>x.StatusItems.Statuses.Where(y=>y.IssueStatus.StringsMatch(stat))).ToList();                
            DateTime maxDt = tStatuses.Max(x=>x.LastEntryDate).StartOfWeek();
            DateTime minDt = tStatuses.Min(x=>x.LastEntryDate).StartOfWeek();
            DateTime checkDt = minDt.Date;
            while (checkDt <= maxDt.Date)
            {
                DateTime endWeek = checkDt.AddDays(7).AddSeconds(-1);
                var weekIssues = tmpIssList.SelectMany(x=>x.StatusItems.Statuses.Where(y=>y.IssueStatus.StringsMatch(stat) && y.LastEntryDate >= checkDt && y.LastEntryDate <= endWeek)).Count();
                if (weekIssues > 0)
                {
                    chart.AddItem($"[dim]Entered[/] [bold]{stat}[/] [dim]week of[/] [bold]{checkDt.Date.ToString("MM/dd/yy")}[/]",weekIssues,tmpColor);
                }
                checkDt = checkDt.AddDays(7);
            }
        }
        return chart;
    }



}