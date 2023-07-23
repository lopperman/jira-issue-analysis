using System.Text;
using JTIS.Config;
using JTIS.Console;
using JTIS.Data;
using JTIS.Extensions;
using JTIS.Menu;
using Spectre.Console;

namespace JTIS.Analysis;

public class CycleTime
{

    private jtisFilterItems<string> _issueTypeFilter = new jtisFilterItems<string>();
    private List<jtisIssue> _filtered = new List<jtisIssue>();
    private jtisIssueData? _jtisIssueData = null;
    private JiraStatus? toStatus = null;
    FetchOptions fetchOptions = 
        FetchOptions.DefaultFetchOptions
            .CacheResults()
            .AllowCachedSelection()
            .AllowJQLSnippets()
            .IncludeChangeLogs()
            .RequiredIssueStatusSequence()
            .AllowManualJQL();    
    public CycleTime()
    {
    }

    public CycleTime(AnalysisType analysisType): this()
    {
        
        ConsoleUtil.WriteBanner("Select 'End' Status for Cycle Time Calculation",Color.Blue);
        var p = new SelectionPrompt<JiraStatus>();
//        p.Title = "Select 'End' Status for Cycle Time Calculation";
        var issStatuses = CfgManager.config.DefaultStatuses;
        p.AddChoices(issStatuses.ToArray());
        p.PageSize=MenuManager.MenuPageSize;
        toStatus = AnsiConsole.Prompt(p);
        StringBuilder sb = new StringBuilder();
        foreach (var issSt in issStatuses.Where(x=>x.ProgressOrder >= toStatus.ProgressOrder).ToList())
        {
            if (sb.Length == 0) {
                sb = sb.Append($"'{issSt.StatusName}'");
            } else {
                sb = sb.Append($", '{issSt.StatusName}'");
            }
        }

        var jql = $"project={CfgManager.config.defaultProject} and status in ({sb.ToString()})";
        fetchOptions.JQL = jql;
        ConsoleUtil.WriteBanner($"Running JQL Query: {jql}");
        
        _jtisIssueData = IssueFetcher.FetchIssues(fetchOptions);
        if (fetchOptions.Cancelled) {return;}



        // if (analysisType == AnalysisType.atEpics){fetchOptions.FetchEpicChildren=true;}
        // _jtisIssueData = IssueFetcher.FetchIssues(fetchOptions);

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

    public static double GaussianCDF(double mean, double standardDev, double x)
        {
        double phi, result, z, denominator = 1,
            sum = 0;
        int i;
        z = (x - mean) / standardDev;
        phi = Math.Exp(-Math.Pow(z, 2) / 2) / Math.Sqrt(2 * Math.PI);
        for (i = 1; i <= 100; i += 2)
        {
            denominator *= i;
            sum += Math.Pow(z, i) / denominator;
        }
        result = 0.5 + phi * sum;
        return result;
        }

}


