using System.Security.Cryptography.X509Certificates;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Spectre.Console;
using JTIS.Config;
using JTIS.Console;
using JTIS.Extensions;
using JTIS.Data;
using JTIS.Menu;

namespace JTIS.Analysis
{
    public class AnalyzeIssues
    {        
        private jtisFilterItems<string> _issueTypeFilter = new jtisFilterItems<string>();
        private List<jtisIssue> _filtered = new List<jtisIssue>();
        private DateTime? filterStatusChangeStart = null;
        private jtisIssueData? _jtisIssueData = null;
        FetchOptions fetchOptions = 
            FetchOptions.DefaultFetchOptions
                .CacheResults()
                .AllowCachedSelection()
                .AllowJQLSnippets()
                .IncludeChangeLogs()
                .AllowManualJQL();

        public bool GetDataFail {get;private set;}
        public AnalyzeIssues()
        {
        }

        public AnalyzeIssues(AnalysisType analysisType): this()
        {
            if (analysisType == AnalysisType.atEpics){fetchOptions.FetchEpicChildren=true;}
            _jtisIssueData = IssueFetcher.FetchIssues(fetchOptions);
            if (fetchOptions.Cancelled) {return;}


            if (_jtisIssueData != null && _jtisIssueData.jtisIssuesList.Count() > 0)
            {
                CheckIssueTypeFilter();       
                CheckStatusDateFilter(); 
                UpdateFilter();
                ConsoleUtil.StartAutoRecording();
                Render();
                ConsoleUtil.StopAutoRecording("AnalyzeIssues");
                WriteToCSV();
            }
        }
       private void CheckIssueTypeFilter()
        {
            _issueTypeFilter.Clear();
            foreach (var issType in _jtisIssueData.IssueTypesCount)
            {
                // int cnt = JCalcs.Count(x=>x.IssueObj.IssueType.StringsMatch(issType));
                _issueTypeFilter.AddFilterItem(issType.Key,$"Count: {issType.Value}");
                // _issueTypeFilter.AddFilterItem(issType,$"Count: {cnt}");
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

        private void UpdateFilter()
        {
            _filtered.Clear();
            _filtered = _jtisIssueData.jtisIssuesList.Where(x=>_issueTypeFilter.IsFiltered(x.jIssue.IssueType)).ToList();
            //  = JCalcs.Where(x=>_issueTypeFilter.IsFiltered(x.IssueObj.IssueType)).ToList();
            if (filterStatusChangeStart.HasValue)
            {
                _filtered = _filtered.Where(x=>x.StatusItems.Statuses.Any(
                    y=>y.LastEntryDate >= filterStatusChangeStart.Value || (y.LastExitDate.HasValue && y.LastExitDate.Value >= filterStatusChangeStart.Value))).ToList();
            }

        }

        private void CheckStatusDateFilter()
        {
            string filterStart = ConsoleUtil.GetInput<string>("Enter Status Change Start Date to filter issues changing status after 'Start Date', otherwise leave blank",allowEmpty:true);
            filterStatusChangeStart = null;
            
            if (filterStart != null)
            {
                DateTime tmpDt = DateTime.MinValue;
                if (DateTime.TryParse(filterStart, out tmpDt))
                {
                    filterStatusChangeStart = tmpDt;
                }
            }

        }

        private BreakdownChart? AveragesUnblockedBusDayChart(string issueType)
        {
            var tmpIssList = _jtisIssueData.jtisIssuesList.Where(x=>x.jIssue.IssueType.StringsMatch(issueType) && x.tsBusinessTime.TotalDays > 0).ToList();
            if (tmpIssList.Count() == 0 ) {return null;}
            BreakdownChart averagesChart = new BreakdownChart();

            SortedList<int,string> seqStat = new SortedList<int, string>();
            SortedDictionary<string,double> busDayValues = new SortedDictionary<string, double>();

            var uniqueStatuses = tmpIssList.SelectMany(x=>x.StatusItems.Statuses).Select(y=>y.IssueStatus).Distinct().ToList();
            foreach (var stat in uniqueStatuses)
            {
                JiraStatus? localStatus = CfgManager.config.StatusConfigs.SingleOrDefault(x=>x.DefaultInUse==true && x.StatusName.StringsMatch(stat));
                if (localStatus != null)
                {
                    seqStat.Add(localStatus.ProgressOrder, stat);
                }
                else 
                {
                    seqStat.Add(0, stat);
                }
                Double totBusDays = tmpIssList.SelectMany(x=>x.StatusItems.Statuses.Where(y=>y.IssueStatus.StringsMatch(stat))).Sum(z=>z.tsUnblockedBusinessTime.TotalDays).RoundTwo();
                totBusDays = totBusDays / (double)tmpIssList.SelectMany(x=>x.StatusItems.Statuses.Where(y=>y.IssueStatus.StringsMatch(stat))).Count();
                totBusDays = totBusDays.RoundTwo();
                busDayValues.Add(stat,totBusDays);
            }
            if (busDayValues.Count > 0)
            {
                int clr = 1;
                foreach (var stat in seqStat.Values)
                {
                    var localStat = CfgManager.config.StatusConfigs.SingleOrDefault(x=>x.DefaultInUse==true && x.StatusName.StringsMatch(stat));
                    if (localStat != null && localStat.ChartColor != null)
                    {
                        var chtColor = Style.Parse($"{localStat.ChartColor}").Foreground;
                        averagesChart.AddItem($"AVG: {stat}",busDayValues[stat], chtColor);
                    }
                    else 
                    {
                        averagesChart.AddItem($"AVG: {stat}",busDayValues[stat], clr);
                        clr +=1;
                    }
                }
                return averagesChart;
            }
            return null;            
        }

        private int IssueCountWithStatusChanges(string issueType)
        {
            try 
            {
                //_jtisIssueData.jtisIssuesList.Where(x=>x.StatusItems.Statuses.Count==0).Count();
                return _jtisIssueData.jtisIssuesList.Where(x=>x.jIssue.IssueType.StringsMatch(issueType) && x.tsBusinessTime.TotalDays > 0).Count();
            }
            catch (Exception ex)
            {
                if (JTIS.Info.IsDev)
                {
                    ConsoleUtil.WriteError("IssueCountWithStatusChanges",false,ex,true);
                }
                return 0;
            }
        }
        private BreakdownChart? AveragesBusDayChart(string issueType)
        {
            var tmpIssList = _jtisIssueData.jtisIssuesList.Where(x=>x.jIssue.IssueType.StringsMatch(issueType) && x.tsBusinessTime.TotalDays > 0).ToList();
            if (tmpIssList.Count() == 0 ) {return null;}
            BreakdownChart averagesChart = new BreakdownChart();

            SortedList<int,string> seqStat = new SortedList<int, string>();
            SortedDictionary<string,double> busDayValues = new SortedDictionary<string, double>();
            // SortedDictionary<string,double> busDayUnblockedValues = new SortedDictionary<string, double>();

            var uniqueStatuses = tmpIssList.SelectMany(x=>x.StatusItems.Statuses).Select(y=>y.IssueStatus).Distinct().ToList();
            foreach (var stat in uniqueStatuses)
            {
                JiraStatus? localStatus = CfgManager.config.StatusConfigs.SingleOrDefault(x=>x.DefaultInUse==true && x.StatusName.StringsMatch(stat));
                if (localStatus != null)
                {
                    seqStat.Add(localStatus.ProgressOrder, stat);
                }
                else 
                {
                    seqStat.Add(0, stat);
                }
                Double totBusDays = tmpIssList.SelectMany(x=>x.StatusItems.Statuses.Where(y=>y.IssueStatus.StringsMatch(stat))).Sum(z=>z.tsBusinessTime.TotalDays).RoundTwo();
                totBusDays = totBusDays / (double)tmpIssList.SelectMany(x=>x.StatusItems.Statuses.Where(y=>y.IssueStatus.StringsMatch(stat))).Count();
                totBusDays = totBusDays.RoundTwo();
                busDayValues.Add(stat,totBusDays);
                // Double totUnblockedBusDays = tmpIssList.SelectMany(x=>x.StatusItems.Statuses.Where(y=>y.IssueStatus.StringsMatch(stat))).Sum(z=>z.tsUnblockedBusinessTime.TotalDays).RoundTwo();
                // busDayUnblockedValues.Add(stat,totUnblockedBusDays);
            }
            if (busDayValues.Count > 0)
            {
                int clr = 1;
                foreach (var stat in seqStat.Values)
                {
                    var localStat = CfgManager.config.StatusConfigs.SingleOrDefault(x=>x.DefaultInUse==true && x.StatusName.StringsMatch(stat));
                    if (localStat != null && localStat.ChartColor != null)
                    {
                        var chtColor = Style.Parse($"{localStat.ChartColor}").Foreground;
                        averagesChart.AddItem($"AVG: {stat}",busDayValues[stat], chtColor);
                    }
                    else 
                    {
                        averagesChart.AddItem($"AVG: {stat}",busDayValues[stat], clr);
                        clr +=1;
                    }
                }
                return averagesChart;
            }
            return null;
            

        }

        private BreakdownChart? BuildStatusChart(jtisIssue iss)
        {
            if (iss.StatusItems.Statuses.Count()==0)
            {
                return null;
            }
            BreakdownChart? chart = new BreakdownChart();
            int clr = 1;
            var totDays = iss.StatusItems.tsBusinessTime.TotalDays;
            foreach (var tmpStatus in iss.StatusItems.Statuses)
            {
                var localStatus = CfgManager.config.StatusConfigs.SingleOrDefault(x=>x.StatusName.StringsMatch(tmpStatus.IssueStatus) && x.DefaultInUse==true);
                if (localStatus != null && localStatus.ChartColor != null)
                {
                    Color chtColor = Style.Parse(localStatus.ChartColor).Foreground;
                    chart.AddItem($"{tmpStatus.IssueStatus}",tmpStatus.tsBusinessTime.TotalDays.RoundTwo(),chtColor);
                }
                else 
                {
                    chart.AddItem(tmpStatus.IssueStatus,tmpStatus.tsBusinessTime.TotalDays.RoundTwo(),Color.FromInt32(clr));
                }
                clr += 1;
            }
            chart.FullSize().ShowTags();
            chart.ShowTagValues();            
            
            return chart;
        }
        private BreakdownChart? BuildBlockerChart(jtisIssue iss)
        {
            if (iss.StatusItems.tsBlockedBusinessTime.TotalDays ==0)
            {
                return null;
            }
            var bchart = new  BreakdownChart();
            bchart.AddItem($"Blocked (Active)",iss.StatusItems.tsBlockedActiveBusTime.TotalDays.RoundTwo(),Color.Red);
            bchart.AddItem($"Unblocked (Active)",iss.StatusItems.tsUnblockedActiveBusTime.TotalDays.RoundTwo(),Color.Green);
            bchart.AddItem($"Blocked (Passive)",iss.StatusItems.tsBlockedPassiveBusTime.TotalDays.RoundTwo(),Color.Yellow);
            bchart.AddItem($"Unblocked (Passive)",iss.StatusItems.tsUnblockedPassiveBusTime.TotalDays.RoundTwo(),Color.Grey);


            return bchart;
        }

        public void WriteToCSV()
        {
            if (ConsoleUtil.Confirm("Save data to csv file?",false)==false)
            {
                return;
            }

            ConsoleUtil.WriteBanner("CREATING CSV FILE");

            DateTime now = DateTime.Now;
            string fileName = string.Format("AnalysisOutput_{0:0000}{1}{2:00}_{3}.csv", now.Year, now.ToString("MMM"), now.Day, now.ToString("hhmmss"));
            string csvPath = Path.Combine(CfgManager.JTISRootPath,fileName);

            using (StreamWriter writer = new StreamWriter(csvPath))
            {                
                for (int i = 0; i < _jtisIssueData.jtisIssueCount; i ++)
                {
                    var iss = _jtisIssueData.jtisIssuesList[i];
                    if (i==0)
                    {
                        writer.WriteLine("IssueKey, IssueType, CurrentStatus, Summary, Status, StatusCategory, TotalCalDays, TotalBusDays, BlockedBusDays, UnblockedBusDays, EnteredCount, FirstEntry, LastEntry, LastExit");

                    }
                    foreach (var statItem in iss.StatusItems.Statuses)
                    {
                        string lastExitDt = statItem.LastExitDate.HasValue ? statItem.LastExitDate.Value.ToString() : string.Empty;
                        writer.WriteLine($"{iss.jIssue.Key}, {iss.jIssue.IssueType}, {iss.jIssue.StatusName}, {iss.jIssue.Summary.ClearCommas()}, {statItem.IssueStatus}, {statItem.StatusCategoryToString}, {statItem.tsCalendarTime.TotalDays.RoundTwo()}, {statItem.tsBusinessTime.TotalDays.RoundTwo()}, {statItem.tsBlockedBusinessTime.TotalDays.RoundTwo()}, {statItem.tsUnblockedBusinessTime.TotalDays.RoundTwo()}, {statItem.EnteredCount.ToString()}, {statItem.FirstEntryDate.ToString()}, {statItem.LastEntryDate.ToString()}, {lastExitDt}");
                    }
                }                
            }
            ConsoleUtil.WriteBanner($"CSV FILE SAVED TO: {csvPath}");
            ConsoleUtil.PressAnyKeyToContinue();
        }

        private bool FromIdNull(JIssueChangeLogItem item)
        {
            if (item.Base.FromId == null || item.Base.FromId.Length ==0)
            {
                return true;
            }
            return false;
        }
        private bool ToIdNull(JIssueChangeLogItem item)
        {
            if (item.Base.ToId == null || item.Base.ToId.Length ==0)
            {
                return true;
            }
            return false;
        }

        private void Render(bool writeAllAtOnce = false, int startIndex = 0)
        {
            bool writeAll = writeAllAtOnce;
            AnsiConsole.Clear();
            int totalCount = _filtered.Count;
            if (startIndex <0 || startIndex >= totalCount){startIndex=0;}
            for (int i = startIndex; i < totalCount; i ++)
            {
                jtisIssue jtisIss = _filtered[i];
                var charts = BuildStatusChart(jtisIss);
                var currentlyBlocked = jtisIss.jIssue.IsBlocked;
                string formattedStartDt = string.Empty;
                jtisStatus? firstActive = jtisIss.StatusItems.FirstActive;                
                if (firstActive == null)
                {
                    formattedStartDt = "[dim] ACTIVE WORK HAS NOT STARTED[/]";
                }
                else 
                {
                    formattedStartDt = string.Format("[dim] ACTIVE WORK STARTED:[/][bold] {0} [/]",firstActive.FirstEntryDate.CheckTimeZone());
                }
                if (writeAll == false)
                {
                    AnsiConsole.Clear();
                }
                // FIRST SUMMARY 'RULE' LINE
                if (JTISTimeZone.DefaultTimeZone==false)
                {
                    AnsiConsole.Write(new Rule(ConsoleUtil.TimeZoneAlert));
                }
                AnsiConsole.Write(new Rule($"[dim]({i+1:000} of {totalCount:#000} results)[/]"){Style=new Style(Color.Blue,Color.Cornsilk1), Justification=Justify.Center});
                if (currentlyBlocked)
                {
                    var blockedOnDesc = string.Empty;
                    if (jtisIss.Blockers.Blockers.Count() > 0)
                    {
                        blockedOnDesc = $" - BLOCKED ON {jtisIss.Blockers.Blockers.Max(x=>x.StartDt).ToString()}";
                    }
                    
                    AnsiConsole.Write(new Rule($"[bold](THIS ISSUE IS CURRENTLY BLOCKED{blockedOnDesc})[/]").NoBorder().LeftJustified().RuleStyle(new Style(Color.DarkRed_1,Color.Cornsilk1)));
                }
                AnsiConsole.Write(new Rule($"[dim]({jtisIss.jIssue.IssueType.ToUpper()}) [/][bold]{jtisIss.jIssue.Key}[/][dim], DESC:[/] {Markup.Escape(ConsoleUtil.Scrub(jtisIss.jIssue.Summary))}").NoBorder().LeftJustified().RuleStyle(new Style(Color.Blue,Color.Cornsilk1)));

                AnsiConsole.Write(new Rule($"[dim]Current Status:[/][bold] ({jtisIss.jIssue.StatusName.ToUpper()})[/]{formattedStartDt}").NoBorder().LeftJustified().RuleStyle(new Style(Color.Blue,Color.Cornsilk1)));

                // LAST SUMMARY 'RULE' LINE
                AnsiConsole.Write(new Rule(){Style=new Style(Color.Blue,Color.Cornsilk1), Justification=Justify.Center});                

                if (CfgManager.config.issueNotes.HasNote(jtisIss.jIssue.Key))
                {
                    AnsiConsole.MarkupLine($"[bold darkred_1 on cornsilk1]ISSUE NOTE: [/]{CfgManager.config.issueNotes.GetNote(jtisIss.jIssue.Key)}");
                }

                var tbl = new Table();
                tbl.NoSafeBorder();
                tbl.Expand();
                tbl.RoundedBorder();                
                tbl.AddColumns(new TableColumn[]{
                    new TableColumn(new Markup($"{Environment.NewLine}[bold]STATUS[/]").Centered()),
                    new TableColumn(new Markup($"{Environment.NewLine}[bold]CATEGORY[/]").Centered()).Width(15),
                    new TableColumn(new Markup($"[bold]TOTAL{Environment.NewLine}DAYS[/]").Centered()).PadRight(2),
                    new TableColumn(new Markup($"[bold]TOTAL{Environment.NewLine}[underline]BUS[/] DAYS[/]").Centered()).PadRight(2),
                    new TableColumn(new Markup($"[bold]BLOCKED{Environment.NewLine}[underline]BUS[/] DAYS[/]").Centered()).PadRight(2),
                    new TableColumn(new Markup($"[bold][underline]UN[/]BLOCKED{Environment.NewLine}[underline]BUS[/] DAYS[/]").Centered()).PadRight(2),
                    new TableColumn(new Markup($"[bold]# TIMES{Environment.NewLine}STARTED[/]").Centered()),
                    new TableColumn(new Markup($"[bold][underline]FIRST[/]{Environment.NewLine}ENTERED[/]").Centered()),
                    new TableColumn(new Markup($"[bold][underline]LAST[/]{Environment.NewLine}ENTERED[/]").Centered()),
                    new TableColumn(new Markup($"[bold][underline]LAST[/]{Environment.NewLine}EXITED[/]").Centered())
                });

                var todoStyle = new Style(Color.DarkRed,Color.Cornsilk1);
                var inProgressStyle = new Style(Color.Blue,Color.Cornsilk1);
                var doneStyle = new Style(Color.Green,Color.Cornsilk1);

                foreach (var issStatus in jtisIss.StatusItems.Statuses)
                {
                    var trackStyle = todoStyle;
                    if (jtisIss.StatusCategory==StatusType.stActiveState || jtisIss.StatusCategory==StatusType.stStart) {trackStyle=inProgressStyle;}
                    if (jtisIss.StatusCategory==StatusType.stEnd) {trackStyle=doneStyle;}

                    Markup? lastEntry = null;
                    Markup? firstEntry = null;
                    Markup? lastexit = null;
                    if (filterStatusChangeStart.HasValue)
                    {
                        if (issStatus.FirstEntryDate >= filterStatusChangeStart.Value)
                        {
                            firstEntry = new Markup($"[bold blue on cornsilk1]{issStatus.FirstEntryDate.CheckTimeZone().ToString("MM/dd/yy HH:mm")}[/]").Centered();
                        }
                        else 
                        {
                            firstEntry = new Markup($"{issStatus.FirstEntryDate.CheckTimeZone().ToString("MM/dd/yy HH:mm")}").Centered();

                        }
                        if (issStatus.LastEntryDate  >= filterStatusChangeStart.Value)
                        {
                            lastEntry = new Markup($"[bold blue on cornsilk1]{issStatus.LastEntryDate.CheckTimeZone().ToString("MM/dd/yy HH:mm")}[/]").Centered();
                        }
                        else 
                        {
                            lastEntry = new Markup($"{issStatus.LastEntryDate.CheckTimeZone().ToString("MM/dd/yy HH:mm")}").Centered();

                        }
                        if (issStatus.LastExitDate.HasValue && issStatus.LastExitDate.Value >= filterStatusChangeStart.Value)
                        {
                            lastexit= new Markup($"[bold blue on cornsilk1]{issStatus.LastExitDate.CheckTimeZoneNullable()}[/]").Centered();
                        }
                        else if (issStatus.LastExitDate.HasValue)
                        {
                            lastexit = new Markup($"{issStatus.LastExitDate.CheckTimeZoneNullable()}").Centered();
                        }
                        else 
                        {
                            lastexit = new Markup($"[dim]---[/]").Centered();
                        }
                    }
                    else 
                    {
                        firstEntry = new Markup($"{issStatus.FirstEntryDate.CheckTimeZone().ToString("MM/dd/yy HH:mm")}").Centered();
                        lastEntry = new Markup($"{issStatus.LastEntryDate.CheckTimeZone().ToString("MM/dd/yy HH:mm")}").Centered();
                        if (issStatus.LastExitDate.HasValue)
                        {
                            lastexit = new Markup($"{issStatus.LastExitDate.Value.CheckTimeZone().ToString("MM/dd/yy HH:mm")}").Centered();
                        }
                        else 
                        {
                            lastexit = new Markup($"[dim]---[/]").Centered();
                        }
                    }
                    tbl.AddRow(new Markup[]{
                        new Markup($" {issStatus.IssueStatus} ").Centered(),

                        issStatus.StatusCategory == StatusType.stActiveState || issStatus.StatusCategory == StatusType.stStart ? 
                            new Markup($"[bold]{issStatus.StatusCategory}[/]").Centered() :
                            new Markup($"[dim]{issStatus.StatusCategory}[/]").Centered(),

                        new Markup($"{issStatus.tsCalendarTime.TotalDays:##0.00}").RightJustified(), 
                        new Markup($"{issStatus.tsBusinessTime.TotalDays:##0.00}").RightJustified(), 

                        issStatus.tsBlockedBusinessTime.TotalDays > 0 ? 
                            new Markup($"[bold red1 on cornsilk1] {issStatus.tsBlockedBusinessTime.TotalDays:##0.00} [/]").RightJustified() :
                            new Markup($"[dim]{issStatus.tsBlockedBusinessTime.TotalDays:##0.00}[/]").RightJustified(),

                        issStatus.tsUnblockedBusinessTime.TotalDays > 0 ?
                        // stActiveUnblockedDays > 0 ? 
                            new Markup($"[bold green on cornsilk1] {issStatus.tsUnblockedBusinessTime.TotalDays:##0.00} [/]").RightJustified() :
                            new Markup($"[dim]{issStatus.tsUnblockedBusinessTime.TotalDays:##0.00}[/]").RightJustified(),

                        issStatus.EnteredCount > 1 ? 
                            new Markup($"[bold]{issStatus.EnteredCount}[/]").Centered() :
                            new Markup($"[dim]{issStatus.EnteredCount}[/]").Centered(),

                        firstEntry, 
                        lastEntry, 
                        lastexit
                    });
                }
                tbl.AddEmptyRow();
                tbl.AddRow(new Markup[]{
                    new Markup($"[italic]ACTIVE TOTALS:[/]").RightJustified(),
                    new Markup($"[italic]ACTIVE[/]").Centered(),
                    new Markup($"[italic]{jtisIss.StatusItems.tsActiveCalTime.TotalDays:##0.00}[/]").RightJustified(),
                    new Markup($"[italic]{jtisIss.StatusItems.tsActiveBusTime.TotalDays:##0.00}[/]").RightJustified(),
                    new Markup($"[italic red1 on cornsilk1] {jtisIss.StatusItems.tsBlockedActiveBusTime.TotalDays:##0.00} [/]").RightJustified(),
                    new Markup($"[italic green on cornsilk1] {jtisIss.StatusItems.tsUnblockedActiveBusTime.TotalDays:##0.00} [/]").RightJustified(),
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered()
                });
                tbl.AddRow(new Markup[]{
                    new Markup($"[italic]PASSIVE TOTALS:[/]").RightJustified(),
                    new Markup($"[italic]PASSIVE[/]").Centered(),

                    new Markup($"[italic]{jtisIss.StatusItems.tsPassiveCalTime.TotalDays:##0.00}[/]").RightJustified(),
                    new Markup($"[italic]{jtisIss.StatusItems.tsPassiveBusTime.TotalDays:##0.00}[/]").RightJustified(),
                    new Markup($"[italic red1 on cornsilk1] {jtisIss.StatusItems.tsBlockedPassiveBusTime.TotalDays:##0.00} [/]").RightJustified(),
                    new Markup($"[italic green on cornsilk1] {jtisIss.StatusItems.tsUnblockedPassiveBusTime.TotalDays:##0.00} [/]").RightJustified(),

                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered()
                });

                tbl.AddEmptyRow();

                tbl.AddRow(new Markup[]{
                    new Markup($"[bold underline]GRAND TOTAL:[/]").RightJustified(),
                    new Markup($"[bold underline]** ALL ** [/]").Centered(),
                    new Markup($"[bold underline]{jtisIss.StatusItems.tsCalendarTime.TotalDays:##0.00}[/]").RightJustified(),
                    new Markup($"[bold underline]{jtisIss.StatusItems.tsBusinessTime.TotalDays:##0.00}[/]").RightJustified(),
                    new Markup($"[bold underline red1 on cornsilk1] {jtisIss.StatusItems.tsBlockedBusinessTime.TotalDays:##0.00} [/]").RightJustified(),
                    new Markup($"[bold underline green on cornsilk1] {jtisIss.StatusItems.tsUnblockedBusinessTime.TotalDays:##0.00} [/]").RightJustified(),
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered()
                });
                AnsiConsole.Write(tbl);
                AnsiConsole.WriteLine();


                if (IssueCountWithStatusChanges(jtisIss.jIssue.IssueType)>1)
                {
                    ConsoleUtil.WriteBanner($"AVERAGE BUSINESS DAYS BY STATUS FOR {IssueCountWithStatusChanges(jtisIss.jIssue.IssueType)} QUERIED ITEMS (ISSUE TYPE: {jtisIss.jIssue.IssueType})",Color.Black, Color.Grey93);
                    var avgBusDaysChart = AveragesBusDayChart(jtisIss.jIssue.IssueType);
                    if (avgBusDaysChart != null)
                    {
                        AnsiConsole.Write(new Rule($"[bold]TOTAL BUSINESS DAYS - AVERAGES FOR QUERIED RESULTS ({jtisIss.jIssue.IssueType})[/]"));
                        AnsiConsole.Write(new Panel(avgBusDaysChart));
                    }
                    var avgUnblockedBusDaysChart = AveragesUnblockedBusDayChart(jtisIss.jIssue.IssueType);
                    if (avgUnblockedBusDaysChart != null)
                    {
                        AnsiConsole.Write(new Rule($"[bold]TOTAL ** UNBLOCKED **  BUSINESS DAYS  - AVERAGES FOR QUERIED RESULTS ({jtisIss.jIssue.IssueType})[/]"));
                        AnsiConsole.Write(new Panel(avgUnblockedBusDaysChart));
                    }
                }

                ConsoleUtil.WriteBanner($"TOTAL BUSINESS DAYS BY STATUS FOR ISSUE {jtisIss.jIssue.Key}", Color.Black, Color.Grey93);
                var statusChart = BuildStatusChart(jtisIss);
                if (statusChart != null) 
                {
                    AnsiConsole.Write(new Panel(statusChart));
                }
                var blockChart = BuildBlockerChart(jtisIss);
                if (blockChart != null)
                {
                    AnsiConsole.Write(new Panel(blockChart));
                }


                if (jtisIss.BlockerCount > 0)
                {
                    AnsiConsole.Write(new Rule($"[bold]BLOCKERS FOR: {jtisIss.jIssue.Key}[/]"){Style=new Style(Color.DarkRed,Color.White), 
                    Justification=Justify.Left});
                    tbl = new Table();
                    tbl.AddColumns("Issue Key", "Based On", "BlockStart", "BlockEnd");
                    
                    foreach (var block in jtisIss.Blockers.Blockers)
                    {   
                        var tEndDt = string.Empty;      
                        if (block.BlockerClosed)
                        {
                            tEndDt = block.EndDt.CheckTimeZone().ToString();
                        }
                        else if (jtisIss.jIssue.IsBlocked)
                        {
                            tEndDt = "* Currently Blocked *";
                        }
                        else 
                        {
                            tEndDt = block.EndDt.CheckTimeZone().ToString();
                        }
                        string blockEnd = tEndDt;
                        if (tEndDt.StringsMatch("blocked",StringCompareType.scContains))
                        {
                            tEndDt = $"[bold maroon on cornsilk1]{tEndDt}[/]";
                        }
                        tbl.AddRow(new Markup[]{
                            new Markup($"[bold]{jtisIss.jIssue.Key}[/]").Centered(),
                            new Markup($"{block.FieldName}").Centered(),
                            new Markup($"{block.StartDt.CheckTimeZone()}").Centered(),
                            new Markup($"{tEndDt}")
                        });                        
                    }
                    AnsiConsole.Write(tbl);
                }

                if (writeAll == false)
                {
                    AnsiConsole.Write(new Rule(){Style=new Style(Color.DarkRed,Color.White)});                                                
                    var waitLoop = true;
                    string? resp = string.Empty;
                    var currentTop = System.Console.GetCursorPosition().Top;
                    while (waitLoop)
                    {
                        resp = ConsoleUtil.GetInput<string>("ENTER=View Next, P=View Previous, N=Add Issue Note, A=Show All, X=Return to Menu",allowEmpty:true);

                        if (resp.StringsMatch("N")) 
                        {
                            IssueNotesUtil.AddEdit(jtisIss.jIssue.Key, false);
                            ConsoleUtil.ClearLinesBackTo(currentTop);
                        } 
                        else if(resp.StringsMatch("X"))
                            {return;} 
                        else if (resp.StringsMatch("P"))
                        {
                            Render(writeAllAtOnce, i-1);
                            return;
                        }
                        else if (resp.StringsMatch("A"))
                        {
                            writeAll = true;
                            Render(writeAll);
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
            ConsoleUtil.PressAnyKeyToContinue();
                
        }
        
    }

}