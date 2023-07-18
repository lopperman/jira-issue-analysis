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

        private DateTime? filterStatusChangeStart = null;
        // private DateTime? filterStatusChangeEnd = null;
        private jtisIssueData? _jtisIssueData = null;
        public List<IssueCalcs> JCalcs {get; private set;}
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
            JCalcs = new List<IssueCalcs>();
        }

        public AnalyzeIssues(AnalysisType analysisType): this()
        {
            if (analysisType == AnalysisType.atEpics){fetchOptions.FetchEpicChildren=true;}
            _jtisIssueData = IssueFetcher.FetchIssues(fetchOptions);

            //TODO:  FIX THIS STUPID JISSUE THING
            foreach (var jtisIss in _jtisIssueData.jtisIssuesList)
            {
                if (jtisIss.ChangeLogs.Count() > 0)
                {
                    jtisIss.jIssue.AddChangeLogs(jtisIss.ChangeLogs,true);
                }
            }
            
            if (_jtisIssueData != null && _jtisIssueData.jtisIssuesList.Count() > 0)
            {
                ClassifyStates();
                WriteToConsole();
            }
        }

        public void ClassifyStates()
        {
            var jtisIssues = _jtisIssueData.jtisIssuesList;
            if (jtisIssues.Count == 0)
            {
                return;
            }
            foreach (var iss in jtisIssues)
            {
                var issCalc = new IssueCalcs(iss.jIssue);
                JCalcs.Add(issCalc);
                JIssueChangeLogItem? firstActiveCLI = null;
                
                foreach (StateCalc sc in issCalc.StateCalcs)
                {
                    if (sc.LogItem.ChangeLogType == ChangeLogTypeEnum.clBlockedField || sc.LogItem.ChangeLogType == ChangeLogTypeEnum.clBlockedFlag )
                    {
                        sc.LogItem.TrackType = StatusType.stPassiveState ;
                    }
                    else if (sc.LogItem.ChangeLogType == ChangeLogTypeEnum.clStatus)
                    {
                        //if change is TO and Active State, then check
                        if (sc.LogItem.ToId != null)
                        {
                            if (Int32.TryParse(sc.LogItem.ToId , out int tmpID))
                            {
                                var stCfg = CfgManager.config.StatusConfigs.FirstOrDefault(x=>x.StatusId == tmpID );
                                if (stCfg != null)
                                {
                                    if (stCfg.Type == StatusType.stPassiveState )
                                    {
                                        sc.LogItem.TrackType = StatusType.stPassiveState ;
                                    }
                                    else if (stCfg.Type == StatusType.stEnd)
                                    {
                                        sc.LogItem.TrackType = StatusType.stEnd ;

                                    }
                                    else if (stCfg.Type == StatusType.stActiveState )
                                    {   
                                        sc.LogItem.TrackType = StatusType.stActiveState;
                                        if (firstActiveCLI == null)
                                        {
                                            firstActiveCLI = sc.LogItem;
                                        }
                                        else 
                                        {
                                            if (sc.LogItem.ChangeLog.CreatedDate < firstActiveCLI.ChangeLog.CreatedDate )
                                            {
                                                firstActiveCLI = sc.LogItem;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (firstActiveCLI != null) 
                {
                    firstActiveCLI.TrackType = StatusType.stStart;
                }
                CalculateEndDates();
            }
        }

        private void CheckIssueTypeFilter()
        {
            _issueTypeFilter.Clear();
            foreach (var issType in JCalcs.Select(x=>x.IssueObj.IssueType).Distinct())
            {
                int cnt = JCalcs.Count(x=>x.IssueObj.IssueType.StringsMatch(issType));
                _issueTypeFilter.AddFilterItem(issType,$"Count: {cnt}");
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

        public void WriteToConsole()
        {
            CheckIssueTypeFilter();       
            CheckStatusDateFilter(); 
            WriteIssueSummary();
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

        public string WriteToCSV()
        {
            bool addedHeader = false ;
//            ConsoleUtil.WriteStdLine("PRESS 'Y' to Save to csv file",StdLine.slResponse,false);
            DateTime now = DateTime.Now;
            string fileName = string.Format("AnalysisOutput_{0:0000}{1}{2:00}_{3}.csv", now.Year, now.ToString("MMM"), now.Day, now.ToString("hhmmss"));
            string csvPath = Path.Combine(CfgManager.JTISRootPath,fileName);

            using (StreamWriter writer = new StreamWriter(csvPath))
            {                
                foreach (var jc in JCalcs)
                {
                    if (addedHeader == false)
                    {
                        addedHeader = true;
                        foreach (var ln in jc.StateCalcStringList(true))
                        {
                            writer.WriteLine(ln);
                        }
                    }
                    else 
                    {
                        foreach (var ln in jc.StateCalcStringList())
                        {
                            writer.WriteLine(ln);
                        }
                    }
                }
                
            }
            return csvPath;   
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

        private void CalculateEndDates()
        {
            foreach (IssueCalcs issCalcs in JCalcs)
            {
                var allStateCalcs = issCalcs.StateCalcs;
                foreach (var sc1 in allStateCalcs)
                {
                    var srcChangeLogId = sc1.LogItem.ChangeLog.Id;
                    //status, blockedfield, blockedflag
                    var srcChangeLogType = sc1.LogItem.ChangeLogType ;
                    // if (srcChangeLogType == ChangeLogTypeEnum.clStatus || srcChangeLogType == ChangeLogTypeEnum.clBlockedField || srcChangeLogType == ChangeLogTypeEnum.clBlockedFlag)
                    if (srcChangeLogType == ChangeLogTypeEnum.clStatus )
                    {
                        foreach (var sc2 in allStateCalcs)
                        {
                            var tarChangeLogId = sc2.LogItem.ChangeLog.Id;
                            //status, blockedfield, blockedflag
                            var tarChangeLogType = sc2.LogItem.ChangeLogType ;
                            if (tarChangeLogId != srcChangeLogId)
                            {
                                if (sc2.LogItem.FieldName == sc1.LogItem.FieldName && tarChangeLogType == ChangeLogTypeEnum.clStatus )
                                {
                                    if (sc2.LogItem.ChangeLog.CreatedDate > sc1.LogItem.ChangeLog.CreatedDate)
                                    {
                                        if (sc1.LogItem.ChangeLog.EndDate == null)
                                        {
                                            sc1.LogItem.ChangeLog.EndDate = sc2.LogItem.ChangeLog.CreatedDate;
                                        }
                                        else 
                                        {
                                            if (sc2.LogItem.ChangeLog.CreatedDate < sc1.LogItem.ChangeLog.EndDate)
                                            {
                                                sc1.LogItem.ChangeLog.EndDate = sc2.LogItem.ChangeLog.CreatedDate;
                                            }
                                        }
                                    }    
                                }

                            }
                        }
                    }
                }
            }
        }

        private string BuildJQLForEpicChildren(string srchData)
        {
            string[] cards = srchData.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            sb.Append("parentEpic in (");
            int added = 0;
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i ++)
                {
                    if (added == 0)
                    {
                        sb.AppendFormat("{0}",cards[i]);
                    }
                    else 
                    {
                        sb.AppendFormat(",{0}",cards[i]);
                    }
                }
                sb.Append(")");
            }
            return sb.ToString();

        }


        private void WriteIssueSummary(bool writeAllAtOnce = false, int startIndex = 0)
        {
            bool writeAll = writeAllAtOnce;
            AnsiConsole.Clear();

            var filteredItems = JCalcs.Where(x=>_issueTypeFilter.IsFiltered(x.IssueObj.IssueType)).ToList();
            if (filterStatusChangeStart.HasValue)
            {
                var filteredDates = new List<IssueCalcs>();
                foreach (var iCalc in filteredItems)
                {
                    if (iCalc.StateCalcs.Any(x=>x.StartDt >= filterStatusChangeStart.Value) || (iCalc.StateCalcs.Any(y=>y.EndDt.HasValue && y.EndDt.Value >= filterStatusChangeStart.Value)))
                    {
                        filteredDates.Add(iCalc);
                    }
                }
                filteredItems = filteredDates;
            }
            int totalCount = filteredItems.Count;

            if (startIndex <0 || startIndex >= totalCount){startIndex=0;}

            for (int i = startIndex; i < totalCount; i ++)
            // foreach (var ic in filteredItems)
            {
                var ic = filteredItems[i];
                jtisIssue jtisIss = _jtisIssueData.jtisIssuesList.Single(x=>x.jIssue.Key.StringsMatch(ic.IssueObj.Key));

                ic.ResetTotalDaysFields();
                var currentlyBlocked = ic.IssueObj.IsBlocked;

                StateCalc? scStart = ic.StateCalcs.FirstOrDefault(x=>x.ActivityType == StatusType.stStart);
                string formattedStartDt = string.Empty;
                if (scStart == null)
                {
                    formattedStartDt = "[dim] ACTIVE WORK HAS NOT STARTED[/]";
                }
                else 
                {
                    ic.FirstActiveStateCalc = scStart;
                    formattedStartDt = string.Format("[dim] ACTIVE WORK STARTED:[/][bold] {0} [/]",scStart.StartDt.CheckTimeZone());
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
                AnsiConsole.Write(new Rule($"[dim]({ic.IssueObj.IssueType.ToUpper()}) [/][bold]{ic.IssueObj.Key}[/][dim], DESC:[/] {Markup.Escape(ConsoleUtil.Scrub(ic.IssueObj.Summary))}").NoBorder().LeftJustified().RuleStyle(new Style(Color.Blue,Color.Cornsilk1)));

                AnsiConsole.Write(new Rule($"[dim]Current Status:[/][bold] ({ic.IssueObj.StatusName.ToUpper()})[/]{formattedStartDt}").NoBorder().LeftJustified().RuleStyle(new Style(Color.Blue,Color.Cornsilk1)));

                // LAST SUMMARY 'RULE' LINE
                AnsiConsole.Write(new Rule(){Style=new Style(Color.Blue,Color.Cornsilk1), Justification=Justify.Center});                

                if (CfgManager.config.issueNotes.HasNote(ic.IssueObj.Key))
                {
                    AnsiConsole.MarkupLine($"[bold darkred_1 on cornsilk1]ISSUE NOTE: [/]{CfgManager.config.issueNotes.GetNote(ic.IssueObj.Key)}");
                }

                var tbl = new Table();
                tbl.NoSafeBorder();
                tbl.Expand();
                tbl.RoundedBorder();                
                tbl.AddColumns(new TableColumn[]{
                    new TableColumn(new Markup($"{Environment.NewLine}[bold]STATUS[/]").Centered()),
                    new TableColumn(new Markup($"{Environment.NewLine}[bold]CATEGORY[/]").Centered()).Width(15),
                    new TableColumn(new Markup($"[bold]TOTAL{Environment.NewLine}DAYS[/]").Centered()),
                    new TableColumn(new Markup($"[bold]TOTAL{Environment.NewLine}[underline]BUS[/] DAYS[/]").Centered()),
                    new TableColumn(new Markup($"[bold]BLOCKED{Environment.NewLine}[underline]ACTIVE[/] DAYS[/]").Centered()),
                    new TableColumn(new Markup($"[bold][underline]UN[/]BLOCKED{Environment.NewLine}[underline]ACTIVE[/] DAYS[/]").Centered()),
                    new TableColumn(new Markup($"[bold]# TIMES{Environment.NewLine}STARTED[/]").Centered()),
                    new TableColumn(new Markup($"[bold][underline]FIRST[/]{Environment.NewLine}ENTERED DATE[/]").Centered()),
                    new TableColumn(new Markup($"[bold][underline]LAST[/]{Environment.NewLine}ENTERED DATE[/]").Centered()),
                    new TableColumn(new Markup($"[bold][underline]LAST[/]{Environment.NewLine}EXITED DATE[/]").Centered())
                });
                

                SortedDictionary<string,double> newBlockedCalDays = new SortedDictionary<string, double>();
                SortedDictionary<string,double> newBlockedBusDays = new SortedDictionary<string, double>();

                //status, first entered, last entered, last exit, entered count, active/passive/etc, caltime, bustime
                List<StatusSummary> ssList = new List<StatusSummary>();
                foreach (StateCalc sc in ic.StateCalcs)
                {                    
                    StatusSummary? ss = null;
                    if (sc.ChangeLogType == ChangeLogTypeEnum.clStatus && sc.ToValue != null && sc.ToValue.Length > 0)
                    {
                        if (ssList.Exists(x=>x.Status == sc.ToValue))
                        {
                            ss = ssList.First(x=>x.Status == sc.ToValue);
                        }
                        else 
                        {
                            ss = new StatusSummary();
                            ss.Status = sc.ToValue;
                            ss.Key = sc.LogItem.ChangeLog.JIss.Key;
                            ss.FirstEntry = sc.CreatedDt;
                            ss.LastEntry = sc.CreatedDt;
                            ss.TrackType = sc.LogItem.TrackType;
                            if (sc.EndDt.HasValue){ss.LastEntry=sc.EndDt.Value;}
                            ssList.Add(ss);
                        }
                        if (sc.StartDt < ss.FirstEntry.Value){ss.FirstEntry = sc.StartDt;}
                        if (sc.EndDt.HasValue)
                        {
                            if (ss.LastExit.HasValue == false){ss.LastExit=sc.EndDt;}
                            else if (ss.LastExit.Value < sc.EndDt.Value){ss.LastExit = sc.EndDt.Value;}
                        }
                        ss.EntryCount +=1;
                        ss.CalTime = ss.CalTime.Add(sc.LogItem.TotalCalendarTime);
                        ss.BusTime = ss.BusTime.Add(sc.LogItem.TotalBusinessTime);


                        if (jtisIss.BlockerCount > 0)
                        {
                            if (ss.TrackType == StatusType.stActiveState || ss.TrackType == StatusType.stStart)
                            {
                                var tStart = sc.StartDt;
                                var tEnd = sc.EndDt.HasValue ? sc.EndDt.Value : DateTime.Now;
                                ss.BlockTime = ss.BlockTime.Add(jtisIss.Blockers.BlockedTime(tStart,tEnd,false));
                            }
                        }
                    }
                }
                var todoStyle = new Style(Color.DarkRed,Color.LightYellow3);
                var inProgressStyle = new Style(Color.Blue,Color.LightYellow3);
                var doneStyle = new Style(Color.Green,Color.LightYellow3);

                double activeCalDays = 0;
                double activeBusDays = 0;
                double activeBlockDays = 0;
                double passiveCalDays = 0;
                double passiveBusDays = 0;
                double passiveBlockDays = 0;

                double stActiveBlockedDays = 0;
                double stActiveUnblockedDays = 0;
                double totActiveBlockedDays = 0;
                double totActiveUnblockedDays = 0;


                foreach (var statSumm in ssList)
                {
                    stActiveBlockedDays = 0;
                    stActiveUnblockedDays = 0;

                    var trackStyle = todoStyle;
                    if (statSumm.TrackType==StatusType.stActiveState || statSumm.TrackType==StatusType.stStart){trackStyle=inProgressStyle;}
                    if (statSumm.TrackType==StatusType.stEnd){trackStyle=doneStyle;}
                    if (statSumm.TrackType == StatusType.stActiveState || statSumm.TrackType == StatusType.stStart)
                    {
                        activeCalDays += statSumm.CalTime.TotalDays;
                        activeBusDays += statSumm.BusTime.TotalDays;
                        activeBlockDays += statSumm.BlockTime.TotalDays;
                        stActiveBlockedDays = statSumm.BlockTime.TotalDays;
                        stActiveUnblockedDays = statSumm.BusTime.TotalDays - stActiveBlockedDays;
                        totActiveBlockedDays += stActiveBlockedDays;
                        totActiveUnblockedDays += stActiveUnblockedDays;
                    }
                    else 
                    {
                        passiveCalDays += statSumm.CalTime.TotalDays;
                        passiveBusDays += statSumm.BusTime.TotalDays;
                        passiveBlockDays += statSumm.BlockTime.TotalDays;
                    }

                    Markup? lastEntry = null;
                    Markup? firstEntry = null;
                    Markup? lastexit = null;
                    if (filterStatusChangeStart.HasValue)
                    {
                        if (statSumm.FirstEntry.HasValue && statSumm.FirstEntry.Value >= filterStatusChangeStart.Value)
                        {
                            firstEntry = new Markup($"[bold blue on cornsilk1]{statSumm.FirstEntry.CheckTimeZoneNullable()}[/]").Centered();
                        }
                        else 
                        {
                            firstEntry = new Markup($"{statSumm.FirstEntry.CheckTimeZoneNullable()}").Centered();

                        }
                        if (statSumm.LastEntry.HasValue && statSumm.LastEntry.Value >= filterStatusChangeStart.Value)
                        {
                            lastEntry = new Markup($"[bold blue on cornsilk1]{statSumm.LastEntry.CheckTimeZoneNullable()}[/]").Centered();
                        }
                        else 
                        {
                            lastEntry = new Markup($"{statSumm.LastEntry.CheckTimeZoneNullable()}").Centered();

                        }
                        if (statSumm.LastExit.HasValue && statSumm.LastExit.Value >= filterStatusChangeStart.Value)
                        {
                            lastexit= new Markup($"[bold blue on cornsilk1]{statSumm.LastExit.CheckTimeZoneNullable()}[/]").Centered();
                        }
                        else 
                        {
                            lastexit = new Markup($"{statSumm.LastExit.CheckTimeZoneNullable()}").Centered();

                        }
                    }
                    else 
                    {
                        firstEntry = new Markup($"{statSumm.FirstEntry.CheckTimeZoneNullable()}").Centered();
                        lastEntry = new Markup($"{statSumm.LastEntry.CheckTimeZoneNullable()}").Centered();
                        lastexit = new Markup($"{statSumm.LastExit.CheckTimeZoneNullable()}").Centered();
                    }

                    tbl.AddRow(new Markup[]{
                        new Markup($" {statSumm.Status} ").Centered(),

                        statSumm.TrackType == StatusType.stActiveState || statSumm.TrackType == StatusType.stStart ? 
                            new Markup($"[bold]{statSumm.TrackType}[/]").Centered() :
                            new Markup($"[dim]{statSumm.TrackType}[/]").Centered(),

                        new Markup($"{statSumm.CalTime.TotalDays:##0.00}").Centered(), 
                        new Markup($"{statSumm.BusTime.TotalDays:##0.00}").Centered(), 

                        stActiveBlockedDays > 0 ? 
                            new Markup($"[bold red1 on lightcyan1] {stActiveBlockedDays:##0.00} [/]").Centered() :
                            new Markup($"[dim]{stActiveBlockedDays:##0.00}[/]").Centered(),

                        stActiveUnblockedDays > 0 ? 
                            new Markup($"[bold green on lightcyan1] {stActiveUnblockedDays:##0.00} [/]").Centered() :
                            new Markup($"[dim]{stActiveUnblockedDays:##0.00}[/]").Centered(),

                        statSumm.EntryCount > 1 ? 
                            new Markup($"[bold]{statSumm.EntryCount}[/]").Centered() :
                            new Markup($"[dim]{statSumm.EntryCount}[/]").Centered(),

                        firstEntry, 
                        lastEntry, 
                        lastexit
                    });

                    
                }
                    ic.SetCalendarDays(Math.Round(passiveCalDays + activeCalDays,2));
                    ic.SetBusinessDays(Math.Round(passiveBusDays + activeBusDays,2));
                    ic.SetBlockedActiveDays(Math.Round(totActiveBlockedDays,2));
                    ic.SetUnblockedActiveDays(Math.Round(totActiveUnblockedDays,2));

                tbl.AddEmptyRow();
                tbl.AddRow(new Markup[]{
                    new Markup($"ACTIVE TOTALS:").RightJustified(),
                    new Markup($"[bold]ACTIVE[/]").Centered(),
                    new Markup($"[bold]{activeCalDays:0.00}[/]").Centered(),
                    new Markup($"[bold]{activeBusDays:0.00}[/]").Centered(),
                    new Markup($"[bold red1 on lightcyan1] {activeBlockDays:0.00} [/]").Centered(),
                    new Markup($"[bold green on lightcyan1] {activeBusDays-activeBlockDays:0.00} [/]").Centered(),
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered()
                });
                tbl.AddRow(new Markup[]{
                    new Markup($"PASSIVE TOTALS:").RightJustified(),
                    new Markup($"[bold]PASSIVE[/]").Centered(),
                    new Markup($"[bold]{passiveCalDays:0.00}[/]").Centered(),
                    new Markup($"[bold]{passiveBusDays:0.00}[/]").Centered(),
                    new Markup($"{passiveBlockDays:0.00}").Centered(),
                    new Markup($"[dim]n/a[/]").Centered(),
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered()
                });

                tbl.AddEmptyRow();

                tbl.AddRow(new Markup[]{
                    new Markup($"[bold]GRAND TOTAL:[/]").RightJustified(),
                    new Markup($"[bold]** ALL ** [/]").Centered(),
                    new Markup($"[bold]{ic.CalendarDays:0.00}[/]").Centered(),
                    new Markup($"[bold]{ic.BusinessDays:0.00}[/]").Centered(),
                    new Markup($"[bold red1 on cornsilk1] {ic.BlockedActiveDays:0.00} [/]").Centered(),
                    new Markup($"[bold green on cornsilk1] {ic.UnblockedActiveDays:0.00} [/]").Centered(),
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered(), 
                    new Markup($"[dim]---[/]").Centered()
                });
                AnsiConsole.Write(tbl);
                AnsiConsole.WriteLine();
                if (jtisIss.BlockerCount > 0)
                {
                    AnsiConsole.Write(new Rule($"[bold]BLOCKERS FOR: {ic.IssueObj.Key}[/]"){Style=new Style(Color.DarkRed,Color.White), 
                    Justification=Justify.Left});
                    tbl = new Table();
                    tbl.AddColumns("Issue Key", "Based On", "BlockStart", "BlockEnd");
                    
                    foreach (var block in jtisIss.Blockers.Blockers)
                    {   
                        var tEndDt = string.Empty;      
                        if (jtisIss.jIssue.IsBlocked)
                        {
                            tEndDt = "* Currently Blocked *";
                        }
                        else 
                        {
                            tEndDt = block.EndDt.CheckTimeZone().ToString();
                        }
                        tbl.AddRow(new Text[]{
                            new Text($"{jtisIss.jIssue.Key}",ConsoleUtil.StdStyle(StdLine.slOutput).Decoration(Decoration.Bold)).Centered(),
                            new Text($"{block.FieldName}",ConsoleUtil.StdStyle(StdLine.slOutput).Decoration(Decoration.None)).Centered(),
                            new Text($"{block.StartDt.CheckTimeZone()}",ConsoleUtil.StdStyle(StdLine.slOutput).Decoration(Decoration.None)).Centered(),
                            new Text($"{tEndDt}",ConsoleUtil.StdStyle(StdLine.slOutput).Decoration(Decoration.None)).Centered()
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
                            IssueNotesUtil.AddEdit(ic.IssueObj.Key, false);
                            ConsoleUtil.ClearLinesBackTo(currentTop);
                        } 
                        else if(resp.StringsMatch("X"))
                            {return;} 
                        else if (resp.StringsMatch("P"))
                        {
                            WriteIssueSummary(writeAllAtOnce, i-1);
                            return;
                        }
                        else if (resp.StringsMatch("A"))
                        {
                            writeAll = true;
                            WriteIssueSummary(writeAll);
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