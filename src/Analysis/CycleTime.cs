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
        return $"[dim]name:[/] [bold]{issType.Name.ToUpper()}[/] [dim](desc: {issType.Description ?? "n/a"})[/]";
    }

    public CycleTime(AnalysisType analysisType): this()
    {
        var projIssueTypes = CfgManager.RefData.ProjectIssuesTypes(CfgManager.config.defaultProject).Where(x=>x.IsSubTask==false).OrderBy(y=>y.Name).ToList();
        issType = MenuManager.SelectSingle<IssueType>("SELECT ISSUE TYPE FOR CYCLE-TIME ANALYSIS",projIssueTypes,clearConsole:true,useConverter:IssueTypeStringConverter);
        AnsiConsole.MarkupLine($"{"\t"}[dim]Issue Type selected: [/][bold underline]{CycleTime.IssueTypeStringConverter(issType)}[/]");
        var statuses = CfgManager.config.LocalProjectDefaultStatuses;
        startStatus = MenuManager.SelectSingle<JiraStatus>("Select Status to use as starting point for Cycle-Time calculation",statuses,useConverter:JiraStatus.ToMarkup);
        AnsiConsole.MarkupLine($"{"\t"}[dim]Starting Status selected: [/][bold underline]{JiraStatus.ToMarkup(startStatus)}[/]");
        endStatus = MenuManager.SelectSingle<JiraStatus>("Select Status to use as ending point for Cycle-Time calculation",statuses.Where(x=>x.StatusName.StringsMatch(startStatus.StatusName)==false).ToList());
        AnsiConsole.MarkupLine($"{"\t"}[dim]Ending Status selected: [/][bold underline]{JiraStatus.ToMarkup(endStatus)}[/]");
        DateTime startDt = DateTime.Today.StartOfWeek().AddDays(-7*12);
        startDt = ConsoleUtil.GetInput<DateTime>($"Enter the oldest date to filter when issues were changed to '{endStatus.StatusName}' (Press ENTER for default - 12 weeks)",startDt);
        if (startDt.DayOfWeek != DayOfWeek.Monday)
        {
            startDt = startDt.StartOfWeek().Date;
        }
        if (startDt >= DateTime.Today.StartOfWeek())
        {
            ConsoleUtil.WriteError("START DATE MUST BE AT LEAST 1 WEEK IN THE PAST");
            return;
        }
        AnsiConsole.MarkupLine($"{"\t"}[dim]Start Date: [/][bold underline]{startDt.ToShortDateString()}[/]");
        DateTime endDt = DateTime.Today.StartOfWeek().AddDays(6);
        if (endDt.Date > DateTime.Today)
        {
            endDt = endDt.AddDays(-7);
        }
        endDt = ConsoleUtil.GetInput<DateTime>($"Enter the latest date to filter when issues were changed to '{endStatus.StatusName}' (Press ENTER for default - latest week)",endDt);
        if (endDt.DayOfWeek != DayOfWeek.Sunday)
        {
            endDt = endDt.StartOfWeek().AddDays(6);
        }
        if (endDt <= startDt)
        {
            ConsoleUtil.WriteError("END DATE MUST BE LATER THAN START DATE, AND MUST BE IN THE PAST");
            return;
        }
        AnsiConsole.MarkupLine($"{"\t"}[dim]End Date: [/][bold underline]{endDt.ToShortDateString()}[/]");

        // int weeksOld = ConsoleUtil.GetInput<int>($" Enter Number of weeks after '{startDt.ToShortDateString}' to search for 'End Status' updates. Enter '0' (zero) to include up to latest full week",12);
        if (ConsoleUtil.Confirm($"Run Cycle-Time Analysis for issue type: '{issType.Name}' where issue moved to status '{endStatus.StatusName}' or later between '{startDt.ToShortDateString()}' and '{endDt.ToShortDateString()}', and calculate cylcle-time from '{startStatus.StatusName}' to '{endStatus.StatusName}'?",true))
        {
            slice = new SliceDice();
            slice.AddIssues(startStatus, endStatus, issType.Name, startDt, endDt);            
            if (slice.CycleTimeEvents.Count==0){
                ConsoleUtil.PressAnyKeyToContinue("NO ISSUES WERE FOUND THAT MET SEARCH CRITERIA");
                return;
            }

            _ctEvents = slice.CycleTimeEvents;
            ConsoleUtil.StartAutoRecording();
            Render();
            ConsoleUtil.StopAutoRecording("CycleTime");

            if (ConsoleUtil.Confirm("View breakdown by WEEK?",true))
            {
                ConsoleUtil.StartAutoRecording();
                RenderWeekly();
                ConsoleUtil.StopAutoRecording("CycleTimeWeekly");

                ConsoleUtil.PressAnyKeyToContinue();
            }

        }
    }

    private void Render()
    {
        ConsoleUtil.WriteAppTitle();
        ConsoleUtil.WriteBanner("CYCLE TIME ANALYSIS - ALL RESULTS",Color.Blue);
        List<Double> busDays = new List<double>();
        _ctEvents.ForEach(ct =>
        {
            busDays.Add(ct.CycleTimeBusDays.TotalDays);
        });
        var stddev = busDays.StandardDeviation();
        var stddevBessel = busDays.StandardDeviation(true);
        var stdErr = busDays.AveragesStdErr(stddev);
        var stdErrBessel = busDays.AveragesStdErr(stddev,true);
        var sdMax = busDays.Average() + stddev;
        var sdMin = busDays.Average() - stddev;
        var sdMaxBessel = busDays.Average() + stddevBessel;
        var sdMinBessel = busDays.Average() - stddevBessel;
        var within2Count = _ctEvents.Where(x=>Math.Round(x.CycleTimeBusDays.TotalDays,2) <= sdMax && Math.Round(x.CycleTimeBusDays.TotalDays,2)>=sdMin).Count();
        var within2Perc = (double)within2Count/(double)_ctEvents.Count();
        var within2CountBessel = _ctEvents.Where(x=>Math.Round(x.CycleTimeBusDays.TotalDays,2) <= sdMaxBessel  && Math.Round(x.CycleTimeBusDays.TotalDays,2)>=sdMinBessel).Count();
        var within2PercBessel = (double)within2CountBessel/(double)_ctEvents.Count();

        AnsiConsole.MarkupLine($"[bold]{_ctEvents.Count()} issues found[/]");
        AnsiConsole.MarkupLine($"[italic](For '{issType.Name}' issues, calculate cycle-time from '{startStatus.StatusName}' to '{endStatus.StatusName}' where issue was moved to '{endStatus.StatusName}' (or a later status) between '{slice.SearchStartDt.ToShortDateString()}' and '{slice.SearchEndDt.ToShortDateString()}' )[/]");
        AnsiConsole.MarkupLine($"Within a normal distribution, 68.2% of all items fall within a standard deviation of the mean. For the current results{Environment.NewLine}{"\t"} [bold blue on cornsilk1]{within2CountBessel} issues ({within2PercBessel:0.00%}) within range ({sdMinBessel:0.00} to {sdMaxBessel:0.00}), [underline]StdDev={stddevBessel:0.00}[/] ** Bessel's Correction: TRUE[/]{Environment.NewLine}{"\t"} [bold blue on cornsilk1]{within2Count} issues ({within2Perc:0.00%}) within range ({sdMin:0.00} to {sdMax:0.00}), [underline]StdDev={stddev:0.00}[/] ** Bessel's Correction: FALSE[/]");
        AnsiConsole.MarkupLine($"[dim]Averages outside the first std deviation from null (center), or high values of the std deviation value (accompanied by greater than 68% within a std deviation from the mean) are indicative of a dataset that cannot contribute to a realistic accurate forecast[/]");

        var tbl = new Table();
        tbl.AddColumns(new TableColumn[]{
            new TableColumn(new Markup("[bold underline]KEY[/]")).Centered(), 
            new TableColumn(new Markup($"[bold underline]STATUS{Environment.NewLine}(FROM/TO)[/]")).Centered(), 
            new TableColumn(new Markup($"[bold underline]FIRST ENTERED{Environment.NewLine}('START' STATUS)[/]")).Centered(), 
            new TableColumn(new Markup($"[bold underline]LAST ENTERED{Environment.NewLine}('END' STATUS)[/]")).Centered(), 
            new TableColumn(new Markup($"[dim underline]AVG BUS DAYS{Environment.NewLine}({busDays.Count():0} items)[/]")).Centered(), 
            new TableColumn(new Markup($"[bold underline]CYCLE-TIME{Environment.NewLine}(BUS DAYS)[/]")).Centered(),
            new TableColumn(new Markup($"[bold underline]BLOCKED{Environment.NewLine}(BUS DAYS)[/]")).Centered().PadRight(2)
        });
        // tbl.AddColumns("KEY","FROM/TO STATUS",$"FIRST ENTER '{startStatus.StatusName}'",$"LAST ENTER '{endStatus.StatusName}","AVG BUS DAYS", "CYCLE-TIME BUS DAYS", "BLOCKED BUS DAYS" ).Centered();
        var avgBusDay = busDays.Average();

        var _ordered = _ctEvents.OrderBy(x=>x.cycleTimeEnd).ToList();

        for (int i = 0; i < _ordered.Count; i ++)
        {
            CycleTimeEvent item = _ordered[i];
            //RenderUtil.WriteIssueHeaderStyle1(item.Issue,i,_ctEvents.Count(),!showAll);
            double blockedDays = item.CycleTimeBlockedBus.TotalDays;
            Markup mkpBlocked = new Markup((blockedDays >= 0.01) ? ($"[bold red on cornsilk1]{blockedDays:0.00}[/]") : ($"[dim]0.00[/]"));

            Markup? cycleTimeVal = null;
            var ctBusDays = Math.Round(item.CycleTimeBusDays.TotalDays,2);
            if (ctBusDays >= sdMinBessel && ctBusDays <= sdMaxBessel)
            {
                cycleTimeVal = new Markup($"[bold blue on cornsilk1]  {ctBusDays:00.00}  [/]");            
            }
            else 
            {
                cycleTimeVal = new Markup($"[bold maroon on cornsilk1]  {ctBusDays:00.00}  [/]");
            }

            tbl.AddRow(new Markup[]{
                new Markup(item.Issue.jIssue.Key).Centered(),
                new Markup($"{item.ActualStatusStart.StatusName} / {item.ActualStatusEnd.StatusName}"), 
                new Markup(item.cycleTimeStart.CheckTimeZone().ToString("MM/dd/yy HH:mm")).Centered(), 
                new Markup(item.cycleTimeEnd.CheckTimeZone().ToString("MM/dd/yy HH:mm")).Centered(), 
                new Markup($"[dim]{avgBusDay:0.00}[/]").Centered(), 
                cycleTimeVal.RightJustified(), 
                mkpBlocked.RightJustified()
            }); 
               
        }
        AnsiConsole.Write(tbl);

        if (slice.IgnoredIssues.Count() > 0)
        {
            AnsiConsole.WriteLine();
            ConsoleUtil.WriteError("THE FOLLOWING ISSUES WERE EXCLUDED FROM CYCLE-TIME ANALYSIS");
            foreach (var desc in slice.IgnoredIssues)
            {
                AnsiConsole.MarkupLine($"[italic]{desc}[/]");
            }
            AnsiConsole.Write(new Rule());
        }


    }

    private void RenderWeekly()
    {

        ConsoleUtil.WriteAppTitle();
        ConsoleUtil.WriteBanner("CYCLE TIME ANALYSIS - WEEKLY BREAKDOWN",Color.Blue);
        DateTime workingWeek = slice.SearchStartDt.Date;
        while(true)
        {
            DateTime workingWeekEnd = workingWeek.AddDays(7).Date.AddSeconds(-1);
            if (_ctEvents.Any(x=>x.cycleTimeEnd >= workingWeek && x.cycleTimeEnd <= workingWeekEnd))
            {
                List<CycleTimeEvent> _events = _ctEvents.Where(x=>x.cycleTimeEnd >= workingWeek && x.cycleTimeEnd <= workingWeekEnd).ToList();
                List<Double> busDays = new List<double>();
                _events.ForEach(ct =>
                {
                    busDays.Add(ct.CycleTimeBusDays.TotalDays);
                });

                ConsoleUtil.WriteBanner($"{workingWeek.ToShortDateString()} through {workingWeekEnd.ToShortDateString()} -- Issue Type: {issType.Name} (Cycle Time: {startStatus.StatusName} to {endStatus.StatusName})",Color.Maroon);

                var stddev = busDays.StandardDeviation();
                var stddevBessel = busDays.StandardDeviation(true);
                var stdErr = busDays.AveragesStdErr(stddev);
                var stdErrBessel = busDays.AveragesStdErr(stddev,true);
                var sdMax = busDays.Average() + stddev;
                var sdMin = busDays.Average() - stddev;
                var sdMaxBessel = busDays.Average() + stddevBessel;
                var sdMinBessel = busDays.Average() - stddevBessel;
                var within2Count = _events.Where(x=>Math.Round(x.CycleTimeBusDays.TotalDays,2) <= sdMax && Math.Round(x.CycleTimeBusDays.TotalDays,2)>=sdMin).Count();
                var within2Perc = (double)within2Count/(double)_events.Count();
                var within2CountBessel = _events.Where(x=>Math.Round(x.CycleTimeBusDays.TotalDays,2) <= sdMaxBessel  && Math.Round(x.CycleTimeBusDays.TotalDays,2)>=sdMinBessel).Count();
                var within2PercBessel = (double)within2CountBessel/(double)_events.Count();

                AnsiConsole.MarkupLine($"[bold]{_events.Count()} issues found[/]");
                AnsiConsole.MarkupLine($"[italic](For '{issType.Name}' issues, calculate cycle-time from '{startStatus.StatusName}' to '{endStatus.StatusName}' where issue was moved to '{endStatus.StatusName}' (or a later status) between '{workingWeek.ToShortDateString()}' and '{workingWeekEnd.ToShortDateString()}' )[/]");
                AnsiConsole.MarkupLine($"Within a normal distribution, 68.2% of all items fall within a standard deviation of the mean. For the current results{Environment.NewLine}{"\t"} [bold blue on cornsilk1]{within2CountBessel} issues ({within2PercBessel:0.00%}) within range ({sdMinBessel:0.00} to {sdMaxBessel:0.00}), [underline]StdDev={stddevBessel:0.00}[/] ** Bessel's Correction: TRUE[/]");

                var tbl = new Table();
                tbl.AddColumns(new TableColumn[]{
                    new TableColumn(new Markup("[bold underline]KEY[/]")).Centered(), 
                    new TableColumn(new Markup($"[bold underline]STATUS{Environment.NewLine}(FROM/TO)[/]")).Centered(), 
                    new TableColumn(new Markup($"[bold underline]FIRST ENTERED{Environment.NewLine}('START' STATUS)[/]")).Centered(), 
                    new TableColumn(new Markup($"[bold underline]LAST ENTERED{Environment.NewLine}('END' STATUS)[/]")).Centered(), 
                    new TableColumn(new Markup($"[dim underline]AVG BUS DAYS{Environment.NewLine}({busDays.Count():0} items)[/]")).Centered(), 
                    new TableColumn(new Markup($"[bold underline]CYCLE-TIME{Environment.NewLine}(BUS DAYS)[/]")).Centered(),
                    new TableColumn(new Markup($"[bold underline]BLOCKED{Environment.NewLine}(BUS DAYS)[/]")).Centered().PadRight(2)
                });
                var avgBusDay = busDays.Average();
                var _ordered = _events.OrderBy(x=>x.cycleTimeEnd).ToList();

                for (int i = 0; i < _ordered.Count; i ++)
                {
                    CycleTimeEvent item = _ordered[i];
                    //RenderUtil.WriteIssueHeaderStyle1(item.Issue,i,_ctEvents.Count(),!showAll);
                    double blockedDays = item.CycleTimeBlockedBus.TotalDays;
                    Markup mkpBlocked = new Markup((blockedDays >= 0.01) ? ($"[bold red on cornsilk1]{blockedDays:0.00}[/]") : ($"[dim]0.00[/]"));

                    Markup? cycleTimeVal = null;
                    var ctBusDays = Math.Round(item.CycleTimeBusDays.TotalDays,2);
                    if (ctBusDays >= sdMinBessel && ctBusDays <= sdMaxBessel)
                    {
                        cycleTimeVal = new Markup($"[bold blue on cornsilk1]  {ctBusDays:00.00}  [/]");            
                    }
                    else 
                    {
                        cycleTimeVal = new Markup($"[bold maroon on cornsilk1]  {ctBusDays:00.00}  [/]");
                    }

                    tbl.AddRow(new Markup[]{
                        new Markup(item.Issue.jIssue.Key).Centered(),
                        new Markup($"{item.ActualStatusStart.StatusName} / {item.ActualStatusEnd.StatusName}"), 
                        new Markup(item.cycleTimeStart.CheckTimeZone().ToString("MM/dd/yy HH:mm")).Centered(), 
                        new Markup(item.cycleTimeEnd.CheckTimeZone().ToString("MM/dd/yy HH:mm")).Centered(), 
                        new Markup($"[dim]{avgBusDay:0.00}[/]").Centered(), 
                        cycleTimeVal.RightJustified(), 
                        mkpBlocked.RightJustified()
                    }); 
                    
                }

                AnsiConsole.Write(tbl);
                AnsiConsole.Write(new Rule());
                AnsiConsole.WriteLine();
            }

            workingWeek = workingWeek.AddDays(7);
            if (workingWeek > slice.SearchEndDt)
            {
                break;
            }
        }

        if (slice.IgnoredIssues.Count() > 0)
        {
            AnsiConsole.WriteLine();
            ConsoleUtil.WriteError("THE FOLLOWING ISSUES WERE EXCLUDED FROM CYCLE-TIME ANALYSIS");
            foreach (var desc in slice.IgnoredIssues)
            {
                AnsiConsole.MarkupLine($"[italic]{desc}[/]");
            }
            AnsiConsole.Write(new Rule());
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


