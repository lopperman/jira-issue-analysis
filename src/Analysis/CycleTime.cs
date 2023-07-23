using System;
using System.Text;
using Atlassian.Jira;
using JTIS.Config;
using JTIS.Console;
using JTIS.Data;
using JTIS.Extensions;
using JTIS.Menu;
using Spectre.Console;

namespace JTIS.Analysis;

public class CycleTime
{

    private List<CycleTimeEvent> _ctEvents = new List<CycleTimeEvent>();
    private SliceDice? slice = null;
    private JiraStatus? startStatus = null;
    private JiraStatus? endStatus = null;
    private IssueType? issType = null;
    public CycleTime()
    {
    }

    public static string IssueTypeStringConverter(IssueType issType)
    {
        return $"name: {issType.Name} (desc: {issType.Description ?? "n/a"})";
    }

    public CycleTime(AnalysisType analysisType): this()
    {
        var projIssueTypes = CfgManager.RefData.ProjectIssuesTypes(CfgManager.config.defaultProject).Where(x=>x.IsSubTask==false).OrderBy(y=>y.Name).ToList();
        issType = MenuManager.SelectSingle<IssueType>("SELECT ISSUE TYPE FOR CYCLE-TIME ANALYSIS",projIssueTypes,clearConsole:true,useConverter:IssueTypeStringConverter);
        var statuses = CfgManager.config.DefaultStatuses;
        startStatus = MenuManager.SelectSingle<JiraStatus>("Select Status to use as starting point for Cycle-Time calculation",statuses);
        endStatus = MenuManager.SelectSingle<JiraStatus>("Select Status to use as ending point for Cycle-Time calculation",statuses.Where(x=>x.StatusName.StringsMatch(startStatus.StatusName)==false).ToList());
        int weeksOld = ConsoleUtil.GetInput<int>("Enter Number of historic weeks to search for 'End Status' updates",12);
        if (ConsoleUtil.Confirm($"Run Cycle-Time Analysis for issue type: '{issType.Name}' where issue moved to status '{endStatus.StatusName}' or later in the last {weeksOld} weeks, and calculate cylcle-time from '{startStatus.StatusName}' to '{endStatus.StatusName}'?",true))
        {
            slice = new SliceDice();
            slice.AddIssues(startStatus, endStatus, issType.Name, weeksOld);            
            if (slice.CycleTimeEvents.Count==0){
                ConsoleUtil.PressAnyKeyToContinue("NO ISSUES WERE FOUND THAT MET SEARCH CRITERIA");
                return;
            }

            _ctEvents = slice.CycleTimeEvents;
            Render();

        }
    }

    private void Render(bool showAll = true, int startIdx = 0)
    {

        ConsoleUtil.WriteBanner("CYCLE TIME ANALYSIS",Color.Blue);
        List<Double> busDays = new List<double>();
        _ctEvents.ForEach(ct =>
        {
            busDays.Add(ct.CycleTimeBusDays.TotalDays);
        });
        var stddev = busDays.StandardDeviation();
        var sdMax = busDays.Average() + stddev;
        var sdMin = busDays.Average() - stddev;
        var within2Count = _ctEvents.Where(x=>Math.Round(x.CycleTimeBusDays.TotalDays,2) <= sdMax && Math.Round(x.CycleTimeBusDays.TotalDays,2)>=sdMin).Count();
        var within2Perc = (double)within2Count/(double)_ctEvents.Count();

        AnsiConsole.MarkupLine($"[bold]{_ctEvents.Count()} issues found[/]");
        AnsiConsole.MarkupLine($"[italic](For '{issType.Name}' issues, calculate cycle-time from '{startStatus.StatusName}' to '{endStatus.StatusName}' where issue was moved to '{endStatus.StatusName}' or later since {slice.SearchDate})[/]");
        AnsiConsole.MarkupLine($"Within a normal distribution, 68.2% of all items fall within a standard deviation of the mean. For the current results, [bold maroon on cornsilk1]{within2Count} issues ({within2Perc:0.00%}) fall within this range ({sdMin:0.00} to {sdMax:0.00})[/].  A percentage that is higher or lower than 68.2% is likely an indication that the data can not support a high-confidence realistic forecast. ");

        var tbl = new Table();
        tbl.AddColumns("KEY","FROM/TO STATUS",$"FIRST ENTER '{startStatus.StatusName}'",$"LAST ENTER '{endStatus.StatusName}","AVG BUS DAYS", "CYCLE-TIME BUS DAYS", "BLOCKED BUS DAYS" );
        var avgBusDay = busDays.Average();

        var _ordered = _ctEvents.OrderBy(x=>x.cycleTimeEnd).ToList();

        for (int i = startIdx; i < _ordered.Count; i ++)
        {
            CycleTimeEvent item = _ordered[i];
            //RenderUtil.WriteIssueHeaderStyle1(item.Issue,i,_ctEvents.Count(),!showAll);
            tbl.AddRow(
                item.Issue.jIssue.Key,
                $"{item.ActualStatusStart.StatusName} / {item.ActualStatusEnd.StatusName}", 
                item.cycleTimeStart.CheckTimeZone().ToString(), 
                item.cycleTimeEnd.CheckTimeZone().ToString(), 
                $"{avgBusDay:0.00}", 
                $"{item.CycleTimeBusDays.TotalDays:0.00}", 
                $"{item.CycleTimeBlockedBus.TotalDays:0.00}"
                );
        }
        AnsiConsole.Write(tbl);
        //     if (showAll == false)
        //     {
        //         AnsiConsole.Write(new Rule(){Style=new Style(Color.DarkRed,Color.White)});                                                
        //         var waitLoop = true;
        //         string? resp = string.Empty;
        //         var currentTop = System.Console.GetCursorPosition().Top;
        //         while (waitLoop)
        //         {
        //             resp = ConsoleUtil.GetInput<string>("ENTER=View Next, P=View Previous, A=Show All, X=Return to Menu",allowEmpty:true);
        //             ConsoleUtil.ClearLinesBackTo(currentTop);

        //             if(resp.StringsMatch("X"))
        //                 {return;} 
        //             else if (resp.StringsMatch("P"))
        //             {
        //                 if (i <= 0){
        //                     Render(showAll,0);
        //                 } else {
        //                     Render(showAll, i-1);
        //                 }
        //                 return;
        //             }
        //             else if (resp.StringsMatch("A"))
        //             {
        //                 showAll = true;
        //                 Render(showAll);
        //                 return;
        //             }
        //             else 
        //             {
        //                 waitLoop = false;
        //                 break;
        //             }
        //         }       
        //     }
        // }

        ConsoleUtil.PressAnyKeyToContinue();


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


