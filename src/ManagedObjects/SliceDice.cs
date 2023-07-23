using System.Text;
using JTIS.Config;
using JTIS.Extensions;
using Spectre.Console;

namespace JTIS.Data;

public class SliceDice
{
    public JiraStatus? StartMin {get;set;} = null;
    public JiraStatus? EndMin {get;set;} = null;
    public string IssueType {get;set;} = string.Empty;
    private jtisIssueData? _data = null;

    SortedDictionary<DateTime,List<jtisIssue>> _issues = new SortedDictionary<DateTime, List<jtisIssue>>();

    public void AddIssues(JiraStatus startMin, JiraStatus endMin, string issueType, int weeksOld)
    {
        FetchOptions options = FetchOptions.DefaultFetchOptions;
        options.AllowManualJQL().IncludeChangeLogs().RequiredIssueStatusSequence();
        var updDt = DateTime.Today.StartOfWeek().AddDays(-(weeksOld*7));
        string jql = $"project={CfgManager.config.defaultProject} and issueType='{issueType}' and status in ({BuildGTEStatusInList(endMin)}) and updated >= '{updDt.ToString("yyyy-MM-dd")}'";        
        options.JQL=jql;
        _data = IssueFetcher.FetchIssues(options);
        if (_data != null && _data.jtisIssueCount > 0)
        {
            foreach (var iss in _data.jtisIssuesList)
            {
                var firstStat = iss.EnteredOnOrAfter(startMin);
                var lastStat = iss.EnteredOnOrAfter(endMin);

                AnsiConsole.WriteLine($"{iss.jIssue.Key} CURRENT STATUS: {iss.jIssue.StatusName}{Environment.NewLine}{"\t"} 'FROM' {firstStat.IssueStatus} on {firstStat.LastEntryDate},  'TO' {lastStat.IssueStatus} on {lastStat.LastEntryDate}");
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