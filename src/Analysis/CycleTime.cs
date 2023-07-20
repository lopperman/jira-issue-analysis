using JTIS.Console;
using JTIS.Data;
using JTIS.Extensions;
using Spectre.Console;

namespace JTIS.Analysis;

public class CycleTime
{

    private jtisFilterItems<string> _issueTypeFilter = new jtisFilterItems<string>();
    private List<jtisIssue> _filtered = new List<jtisIssue>();
    private jtisIssueData? _jtisIssueData = null;
    FetchOptions fetchOptions = 
        FetchOptions.DefaultFetchOptions
            .CacheResults()
            .AllowCachedSelection()
            .AllowJQLSnippets()
            .IncludeChangeLogs()
            .AllowManualJQL();    
    public CycleTime()
    {
    }

    public CycleTime(AnalysisType analysisType): this()
    {
        if (analysisType == AnalysisType.atEpics){fetchOptions.FetchEpicChildren=true;}
        _jtisIssueData = IssueFetcher.FetchIssues(fetchOptions);

        if (_jtisIssueData != null && _jtisIssueData.jtisIssuesList.Count() > 0)
        {
            // CheckIssueTypeFilter();       
            // CheckStatusDateFilter(); 
            // UpdateFilter();
            Render();
        }
    }

    private void Render(bool showAll = false, int startIdx = 0)
    {
        var issues = _jtisIssueData.jtisIssuesList;
        // foreach (var tmpStatus in statuses)
        // {   
        //     var pct = (double)_filtered.Count(x=>x.issue.Status.Name.StringsMatch(tmpStatus)) / (double)_filtered.Count;
        //     statusCounts.Add(tmpStatus,_filtered.Count(x=>x.issue.Status.Name.StringsMatch(tmpStatus)));
        //     statusPercents.Add(tmpStatus,pct);

        // }
        for (int i = startIdx; i < issues.Count; i ++)
        {
            jtisIssue iss = issues[i];
            RenderUtil.WriteIssueHeaderStyle1(iss,i,issues.Count(),!showAll);

            SortedDictionary<string,double> statusCounts = new SortedDictionary<string, double>();
            var cht = new BreakdownChart();
            cht.FullSize().ShowTags();
            cht.ShowTagValues();
            
            int clr = 1;
            foreach (var status  in iss.StatusItems.Statuses)
            {
                cht.AddItem(status.IssueStatus,Math.Round(status.StatusBusinessTimeTotal.TotalDays,2),clr);
                clr += 1;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("  ISSUE STATUS - BUSINESS DAYS  ").Centered());
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Panel(cht).Expand().Header("  STATUS BUSINESS DAYS TOTAL  ",Justify.Center).NoBorder());
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("~~~").Centered());

            if (showAll == false)
            {
                AnsiConsole.Write(new Rule(){Style=new Style(Color.DarkRed,Color.White)});                                                
                var waitLoop = true;
                string? resp = string.Empty;
                var currentTop = System.Console.GetCursorPosition().Top;
                while (waitLoop)
                {
                    resp = ConsoleUtil.GetInput<string>("ENTER=View Next, P=View Previous, A=Show All, X=Return to Menu",allowEmpty:true);
                    ConsoleUtil.ClearLinesBackTo(currentTop);

                    if(resp.StringsMatch("X"))
                        {return;} 
                    else if (resp.StringsMatch("P"))
                    {
                        if (i <= 0){
                            Render(showAll,0);
                        } else {
                            Render(showAll, i-1);
                        }
                        return;
                    }
                    else if (resp.StringsMatch("A"))
                    {
                        showAll = true;
                        Render(showAll);
                        return;
                    }
                    else 
                    {
                        waitLoop = false;
                        break;
                    }
                }       
            }
        }
        if (showAll)
        {
            ConsoleUtil.PressAnyKeyToContinue();
        }


    }


}


