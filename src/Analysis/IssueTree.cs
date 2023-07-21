using Atlassian.Jira;
using JTIS.Console;
using JTIS.Data;
using JTIS.Extensions;
using JTIS.Menu;
using Spectre.Console;

namespace JTIS.Analysis;

public class IssueTree
{
    private jtisFilterItems<string> _issueTypeFilter = new jtisFilterItems<string>();
    private List<jtisIssue> _filteredIssues = new List<jtisIssue>();
    private FetchOptions fetchOptions = FetchOptions.DefaultFetchOptions.CacheResults().AllowCachedSelection().IncludeChangeLogs();
    private jtisIssueData? _jtisIssueData = null;
    public IssueTree(AnalysisType analysisType=AnalysisType.atIssues)
    {
        if (analysisType == AnalysisType.atEpics){fetchOptions.FetchEpicChildren=true;}
        _jtisIssueData = IssueFetcher.FetchIssues(fetchOptions);
        if (fetchOptions.Cancelled) {return;}

        if (_jtisIssueData != null && _jtisIssueData.jtisIssueCount > 0)
        {
            CheckIssueTypeFilter();
            BuildFilteredList();
            Render();
        }

    }    

    private void CheckIssueTypeFilter()
    {
        _issueTypeFilter.Clear();
        foreach (var kvp in _jtisIssueData.IssueTypesCount)
        {
            _issueTypeFilter.AddFilterItem(kvp.Key,$"Count: {kvp.Value}");
        }
        if (_issueTypeFilter.Count > 1)
        {
            if (ConsoleUtil.Confirm($"Filter which of the {_issueTypeFilter.Count} issue types get displayed?",true))
            {
                var response = MenuManager.MultiSelect<jtisFilterItem<string>>($"Choose items to include. [dim](To select all items, press ENTER[/])",_issueTypeFilter.Items.ToList());
                if (response != null && response.Count() > 0)
                {
                    _issueTypeFilter.Clear();
                    _issueTypeFilter.AddFilterItems(response); 
                }
            }
        }

    }

    private void BuildFilteredList()
    {
        _filteredIssues = _jtisIssueData.jtisIssuesList.Where(x=>_issueTypeFilter.IsFiltered(x.issue.Type.Name)).ToList();
        _filteredIssues = _filteredIssues.OrderBy(x=>x.jIssue.Key).ToList();
    }

    private void Render(bool writeAll = false, int startAt = 0)
    {
        ConsoleUtil.WriteAppTitle(underDev:true);
        var issues = _filteredIssues;
        var filteredCount = _filteredIssues.Count;
        if (startAt < 0 || startAt >= filteredCount) {startAt = 0;}

        for (int i = startAt; i < _filteredIssues.Count(); i ++)
        {
            List<Table> tables = new List<Table>();
            int maxChars = 0;
            if (writeAll==false)
            {
                ConsoleUtil.WriteAppTitle(underDev:true);
            }
            jtisIssue iss = _filteredIssues[i];
            AnsiConsole.Write(new Rule($"[dim]({i+1:000} of {filteredCount:#000} results)[/]"){Style=new Style(Color.Blue,Color.Cornsilk1), Justification=Justify.Center});

            var cLogs = iss.ChangeLogs;
            var root = new Tree(new Panel(new Markup($"[bold]({iss.jIssue.IssueType}) {iss.jIssue.Key}[/] - [dim]Status: [/][bold]{iss.jIssue.StatusName}[/]")));
            var workingDay = iss.jIssue.CreateDate.Value.CheckTimeZone().Date;
            TreeNode curNode = root.AddNode($"[bold blue on cornsilk1]On {workingDay.CheckTimeZone().ToShortDateString()} [/]");
            Table curNodeTable = new Table().AddColumns("Desc");
            tables.Add(curNodeTable);
            curNodeTable.HideHeaders();
            curNodeTable.AddRow(new Markup($"Issue Created in Jira"));

            for (int clIdx = 0; clIdx < cLogs.Count; clIdx ++)
            {
                var cLog = cLogs[clIdx];
                if (cLog.Items.Any(x=>x.FieldName.StringsMatch("status")))
                {
                    if (cLog.CreatedDate.CheckTimeZone().Date > workingDay)
                    {
                        curNode = curNode.AddNode(curNodeTable);
                        workingDay = cLog.CreatedDate.CheckTimeZone().Date;
                        curNode = root.AddNode($"[bold blue on cornsilk1] On {workingDay.CheckTimeZone().ToShortDateString()} [/]");
                        curNodeTable = new Table().AddColumns("Desc");                        
                        curNodeTable.HideHeaders();
                        tables.Add(curNodeTable);
                    }
                    foreach (var cli in cLog.Items)
                    {
                        if (cli.FieldName.StringsMatch("status"))
                        {
                            var statusText = $"changed to: [bold blue on cornsilk1] {cli.ToValue.ToUpper()} [/]";
                            var markupText = $"changed to: [bold blue on cornsilk1] {cli.ToValue.ToUpper()} [/][dim] from: {cli.FromValue ?? "(empty)"}, at {cLog.CreatedDate.CheckTimeZone().ToShortTimeString()}[/]";
                            if (Markup.Remove(markupText).Length > maxChars)
                            {
                                maxChars = Markup.Remove(markupText).Length;
                            }
                            curNodeTable.AddRow(new Markup($"{markupText}"));
                        }
                    }
                }
            }
            curNode.AddNode(curNodeTable);
            tables.ForEach(tbl => {
                tbl.Width = maxChars + 4;
            });
            AnsiConsole.Write(root);

            if (writeAll==false)
            {
                var resp = ConsoleUtil.GetInput<string>($"ENTER= View Next, P=Show Previous, 1-{filteredCount}=Go To Item, A=Show All, X=Return to Menu",allowEmpty:true);
                if (resp.StringsMatch("X")) 
                {
                    return;
                } 
                else if (resp.StringsMatch("A"))
                {
                    Render(true);
                    return;
                }
                else if (resp.StringsMatch("P"))
                {
                    Render(writeAll,i-1);
                    return;
                }
                else if (resp.Length > 0)
                {
                    int tempIdx = -1;
                    if (int.TryParse(resp, out tempIdx))
                    {
                        tempIdx = tempIdx - 1;
                        if (tempIdx >=0 && tempIdx < filteredCount)
                        {
                            Render(writeAll,tempIdx);
                            return;
                        }
                    }
                }

            }
        }

        if (writeAll==true)
        {
            ConsoleUtil.PressAnyKeyToContinue();
        }
    }
}