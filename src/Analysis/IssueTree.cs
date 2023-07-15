using Atlassian.Jira;
using JTIS.Console;
using JTIS.Data;
using JTIS.Extensions;
using Spectre.Console;

namespace JTIS.Analysis;

public class IssueTree
{
    private FetchOptions fetchOptions = FetchOptions.DefaultFetchOptions.CacheResults().AllowCachedSelection().IncludeChangeLogs();
    private jtisIssueData? _jtisIssueData = null;
    public IssueTree(AnalysisType analysisType=AnalysisType.atIssues)
    {
        if (analysisType == AnalysisType.atEpics){fetchOptions.FetchEpicChildren=true;}
        _jtisIssueData = IssueFetcher.FetchIssues(fetchOptions);

        if (_jtisIssueData != null && _jtisIssueData.jtisIssueCount > 0)
        {
            RenderTrees();
        }

    }    

    private void RenderTrees(bool showAll = false, int startIdx = 0)
    {
        ConsoleUtil.WriteAppTitle();
        var issues = _jtisIssueData.jtisIssuesList.OrderBy(x=>x.jIssue.Key).ToList();

        for (int i = startIdx; i < issues.Count(); i ++)
        {
            if (showAll==false)
            {
                ConsoleUtil.WriteAppTitle();
            }
            jtisIssue iss = issues[i];
            List<IssueChangeLog> cLogs = new List<IssueChangeLog>();
            if (iss.ChangeLogs != null){cLogs = iss.ChangeLogs.OrderBy(x=>x.CreatedDate).ToList();}

            var root = new Tree(new Panel(new Markup($"[bold]({iss.jIssue.IssueType}) {iss.jIssue.Key}[/] - [dim]Status: [/][bold]{iss.jIssue.StatusName}[/]")));
            var workingDay = iss.jIssue.CreateDate.Value.CheckTimeZone().Date;
            TreeNode curNode = root.AddNode($"[bold blue on cornsilk1]On {workingDay.CheckTimeZone().ToShortDateString()} [/]");
            Table curNodeTable = new Table().AddColumns("Desc");
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
                        curNode = root.AddNode($"[bold blue on cornsilk1]On {workingDay.CheckTimeZone().ToShortDateString()} [/]");
                        curNodeTable = new Table().AddColumns("Desc");
                        curNodeTable.HideHeaders();
                    }
                    foreach (var cli in cLog.Items)
                    {
                        if (cli.FieldName.StringsMatch("status"))
                        {   
                            curNodeTable.AddRow(new Markup($"{cLog.CreatedDate.CheckTimeZone()} from: {Markup.Escape(cli.FromValue ?? "")}, to: [bold]{Markup.Escape(cli.ToValue ?? "")}[/]"));

                        }
                    }
                }
            }
           curNode.AddNode(curNodeTable);
            AnsiConsole.Write(root);


            if (showAll==false)
            {
                ConsoleUtil.PressAnyKeyToContinue();
            }
        }

        if (showAll==true)
        {
            ConsoleUtil.PressAnyKeyToContinue();
        }
    }
}