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
        private AnalysisType _type = AnalysisType._atUnknown;
        private string searchJQL = string.Empty;
        
        public List<jtisIssue> jtisIssues {get; private set;}
        public List<IssueCalcs> JCalcs {get; private set;}

        public bool GetDataFail {get;private set;}

        public bool HasSearchData
        {
            get
            {
                return (searchJQL != null && searchJQL.Length > 0);
            }
        }

        public AnalyzeIssues()
        {
            jtisIssues = new List<jtisIssue>();
            JCalcs = new List<IssueCalcs>();
        }
        public AnalyzeIssues(AnalysisType analysisType): this()
        {
            _type = analysisType;
            string? data = string.Empty;
            if (_type == AnalysisType.atIssues || _type == AnalysisType.atJQL)
            {
                searchJQL = ConsoleInput.GetJQLOrIssueKeys(true);
            }
            else if (_type == AnalysisType.atIssueSummary)
            {
                searchJQL = ConsoleInput.GetJQLOrIssueKeys(true);
            }
            else if (_type == AnalysisType.atEpics)
            {
                searchJQL = ConsoleInput.GetJQLOrIssueKeys(true,findEpicLinks:true);
            }
        }

        public void ClassifyStates()
        {
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
            foreach (var ic in JCalcs)
            {
                PopulateBlockers(ic);
                AddBlockerAdjustments(ic);
            }

        }

        private void CheckIssueTypeFilter()
        {
            //        private jtisFilterItems<string> _issueTypeFilter = new jtisFilterItems<string>();

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
            // var filteredItems = JCalcs.Where(x=>_issueTypeFilter.IsFiltered(x.IssueObj.IssueType)).ToList();
            // int totalCount = filteredItems.Count;

            // foreach (var ic in filteredItems)

        }

        public void WriteToConsole()
        {
            CheckIssueTypeFilter();        
            WriteIssueSummary();
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
                
                writer.WriteLine();
                writer.WriteLine("BLOCKERS");
                writer.WriteLine();
                writer.WriteLine(string.Format("{0},{1},{2},{3}","IssueKey","StartDt","EndDt","BlockerFieldName"));

                foreach (var jc in JCalcs)
                {
                    foreach (var b in jc.Blockers)
                    {
                        writer.WriteLine(string.Format("{0},{1},{2},{3}",b.IssueKey,b.StartDt,b.EndDt,b.BlockerFieldName));
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

        private void AddBlockerAdjustments(IssueCalcs ic)
        {
            if (ic.Blockers.Count > 0)
            {
                foreach (var cl in ic.IssueObj.ChangeLogs)
                {
                    if (ic.Blockers.Exists(x=>x.IssueKey==ic.IssueObj.Key))
                    {
                        var issueBlockers = ic.Blockers.Where(x=>x.IssueKey==ic.IssueObj.Key).ToList();
                        cl.CheckBlockers(issueBlockers);
                    }
                }
            }
        }

        private void PopulateBlockers(IssueCalcs ic)
        {
            //identify all change log that 'START' a blocker
            List<Blocker> tmpStartingBlockers = new List<Blocker>();
            List<Blocker> tmpEndingBlockers = new List<Blocker>();

            foreach (var cl in ic.IssueObj.ChangeLogs)
            {
                foreach (var cli in cl.Items)
                {
                    if (cli.FieldName.ToLower()=="flagged"   )
                    {
                        if (cli.ToValue.ToLower()=="impediment")
                        {
                            var newBlocker = new Blocker(cl.JIss.Key,cl.JIss.IsBlocked, cli.StartDt, cli.ChangeLogType, cli.FieldName );
                            tmpStartingBlockers.Add(newBlocker);
                        }      
                        else if (cli.FromValue.ToLower()=="impediment")
                        {
                            var newBlocker = new Blocker(cl.JIss.Key,cl.JIss.IsBlocked,cli.StartDt,cli.ChangeLogType,cli.FieldName,cli.StartDt);
                            tmpEndingBlockers.Add(newBlocker);
                        }                                          
                    }
                    else if (cli.FieldName.ToLower()=="priority")
                    {
                        if (cli.ToValue.ToLower() == "blocked")                        
                        {
                            var newBlocker = new Blocker(cl.JIss.Key,cl.JIss.IsBlocked,cli.StartDt, ChangeLogTypeEnum.clBlockedField,cli.FieldName );
                            tmpStartingBlockers.Add(newBlocker);
                        }                        
                        if (cli.FromValue.ToLower() == "blocked")                        
                        {
                            var newBlocker = new Blocker(cl.JIss.Key,cl.JIss.IsBlocked,cli.StartDt, ChangeLogTypeEnum.clBlockedField,cli.FieldName );
                            tmpEndingBlockers.Add(newBlocker);
                        }                                            
                    }
                    else if (cli.FieldName.StringsMatch("status"))
                    {
                        if (cli.ToValue.StringsMatch("blocked"))
                        {
                            var newBlocker = new Blocker(cl.JIss.Key,cl.JIss.IsBlocked,cli.StartDt, ChangeLogTypeEnum.clBlockedField,cli.FieldName );
                            tmpStartingBlockers.Add(newBlocker);
                        }
                        if (cli.FromValue.StringsMatch("blocked"))
                        {
                            var newBlocker = new Blocker(cl.JIss.Key,cl.JIss.IsBlocked,cli.StartDt, ChangeLogTypeEnum.clBlockedField,cli.FieldName );
                            tmpEndingBlockers.Add(newBlocker);
                        }
                    }
                }
            }
            if (tmpStartingBlockers.Count > 0)
            {
                tmpStartingBlockers = tmpStartingBlockers.OrderBy(x=>x.StartDt).ToList();
                
                foreach (var blocker in tmpStartingBlockers)
                {
                    if (tmpEndingBlockers.Count > 0)
                    {
                        foreach (var endBlocker in tmpEndingBlockers)
                        {
                            if (endBlocker.BlockerFieldName == blocker.BlockerFieldName && endBlocker.StartDt > blocker.StartDt)
                            {
                                if (blocker.EndDt == null)
                                {
                                    blocker.EndDt = endBlocker.StartDt;
                                }
                                else 
                                {
                                    if (endBlocker.StartDt < blocker.EndDt.Value)
                                    {
                                        blocker.EndDt = endBlocker.StartDt;
                                    }
                                }
                            }
                        }
                    }
                    //slap on 'now' if blocker does not have an end dt 
                    if (blocker.EndDt == null)
                    {
                        blocker.EndDt = DateTime.Now;
                    }                                           
                    ic.Blockers.Add(blocker);
                }
                if (ic.Blockers.Count > 1)
                {
                    RemoveBlockerOverlap(ic);
                }

            }            
        }

        private void RemoveBlockerOverlap(IssueCalcs ic)
        {
            var bList = ic.Blockers.OrderBy(x=>x.StartDt).ToList();
            foreach (var b1 in bList)
            {                   
                if (b1.Removed == false && bList.Any(x=>x.tmpID != b1.tmpID && x.StartDt <= b1.EndDt && x.Removed == false))
                {   
                    if (!b1.EndDt.HasValue) {b1.EndDt = DateTime.Now;}
                
                    var overlaps = bList.Where(x=>x.tmpID != b1.tmpID && x.StartDt <= b1.EndDt).ToList();
                    foreach (var o1 in overlaps)
                    {
                        if (!o1.EndDt.HasValue) {o1.EndDt = DateTime.Now;}
                        o1.Adjusted = true;
                        var newStartDt = b1.EndDt.Value.AddSeconds(1);
                        o1.AdjustmentNotes.Add($"Overlap - StartDt Changed from '{o1.StartDt}' to '{newStartDt}'");
                        o1.StartDt = newStartDt;
                        if (o1.EndDt.Value <= o1.StartDt)
                        {
                            o1.Removed = true;
                        }
                    }

                }
            }
            var goodList = bList.Where(x=>x.Removed == false).ToList();
            ic.Blockers = goodList;
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
        private string? SelectSavedJQL()
        {
            string title = string.Empty;
            string ret = string.Empty;
            switch(_type)
            {
                case AnalysisType.atIssues:
                    title = "SELECT SAVED LIST (SPACE-DELIMITED ISSUE KEYS) - ANALYSIS WILL RUN ON EACH ITEM IN THE LIST";
                    break;
                case AnalysisType.atEpics:
                    title = "SELECT SAVED LIST (SPACE-DELIMITED EPIC KEYS) - ANALYSIS WILL RUN ON ALL CHILDREN LINKED TO EPIC(S)";
                    break;
                case AnalysisType.atJQL:
                    title = "SELECT SAVED JQL QUERY (MUST BE VALID JQL) - ANALYSIS WILL RUN ON ALL EPIC 'CHILDREN'";
                    break;
                default:
                    title = string.Empty;
                    break;
            }
            if (title.Length == 0)
            {
                return string.Empty;;
            }
            ret = CfgManager.GetSavedJQL(title);
            if (ret != null && ret.Length > 0)
            {
                if (ConsoleUtil.Confirm("USE THE FOLLOWING SAVED JQL/QUERY?",true))
                {
                    return ret;
                }
            }
            return string.Empty;
        }

        private void PopulateEpicLinks()
        {
            List<jtisIssue> epics = jtisIssues.Where(x=>x.issue.Type.StringsMatch("epic")).ToList();
            if (epics.Count() > 0)
            {
                AnsiConsole.MarkupLine($"getting linked issues for [bold]{epics.Count} epics[/]");
                var epicKeys = epics.Select(x=>x.issue.Key.Value).ToArray();
                var jql = JQLBuilder.BuildJQLForFindEpicIssues(epicKeys);
                var jtisData = jtisIssueData.Create(JiraUtil.JiraRepo);         
                var children = jtisData.GetIssuesWithChangeLogs(jql);
                if (children.Count > 0)
                {
                    foreach (var child in children)
                    {
                        if (!jtisIssues.Exists(x=>x.issue.Key.Value == child.issue.Key.Value))
                        {
                            jtisIssues.Add(child);
                        }
                    }
                    ConsoleUtil.WritePerfData(jtisData.Performance);
                }
            }
        }
        public int GetData()
        {
            try 
            {
                var data = jtisIssueData.Create(JiraUtil.JiraRepo);
                jtisIssues.Clear();
                jtisIssues.AddRange(data.GetIssuesWithChangeLogs(searchJQL));
                ConsoleUtil.WritePerfData(data.Performance);
                if (_type == AnalysisType.atEpics)
                {
                    PopulateEpicLinks();
                }
            }
            catch (Exception excep)
            {
                ConsoleUtil.WriteError($"An error occurred getting data from Jira. Please double check the JQL syntax you are using ({searchJQL})");
                ConsoleUtil.WriteError($"Error Detail: {excep.Message}",pause:true);
            }

            return jtisIssues.Count();

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


            //     retJQL = string.Format("project={0} and parentEpic={1}",CfgManager.config.defaultProject,epicKey);
            // {
            //     retJQL = string.Format("parentEpic={0}",epicKey);
            // }
        }

        private string BuildJQLKeyInListArr(string[]? srchData)
        {
            if (srchData == null || srchData.Length == 0){return string.Empty;}
            string[] cards = srchData;
            StringBuilder sb = new StringBuilder();
            string defProj = CfgManager.config.defaultProject;
            sb.Append("key in (");
            int added = 0;
            for (int i = 0; i < cards.Length; i ++)
            {
                string tKey = cards[i];
                if (!tKey.Contains('-'))
                {
                    tKey = string.Format("{0}-{1}",defProj,tKey);
                }
                if (added == 0)
                {                        
                    sb.AppendFormat("'{0}'",tKey);
                    added +=1;
                }
                else 
                {
                    sb.AppendFormat(",'{0}'",tKey);
                    added +=1;
                }
            }
            sb.Append(")");

            return sb.ToString();            
        }
        private string BuildJQLKeyInList(string srchData)
        {
            string[] cards = srchData.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            string defProj = CfgManager.config.defaultProject;
            sb.Append("key in (");
            int added = 0;
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i ++)
                {
                    var tKey = cards[i];
                    if (!tKey.Contains("-"))
                    {
                        tKey = string.Format("{0}-{1}",defProj,tKey);
                    }
                    if (added == 0)
                    {                        
                        sb.AppendFormat("{0}",tKey);
                    }
                    else 
                    {
                        sb.AppendFormat(",{0}",tKey);
                    }
                }
                sb.Append(")");
            }
            return sb.ToString();
        }

        private void WriteIssueSummary(bool writeAllAtOnce = false)
        {
            int writeCount = 0;
            bool writeAll = writeAllAtOnce;
            AnsiConsole.Clear();

            var filteredItems = JCalcs.Where(x=>_issueTypeFilter.IsFiltered(x.IssueObj.IssueType)).ToList();
            int totalCount = filteredItems.Count;

            foreach (var ic in filteredItems)
            {

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
                    formattedStartDt = string.Format("[dim] ACTIVE WORK STARTED:[/][bold] {0} [/]",scStart.StartDt);
                }

                if (writeAll == false)
                {
                    AnsiConsole.Clear();
                }
                writeCount +=1;

                // FIRST SUMMARY 'RULE' LINE
                AnsiConsole.Write(new Rule($"[dim]({writeCount:000} of {totalCount:#000} results)[/]"){Style=new Style(Color.Blue,Color.Cornsilk1), Justification=Justify.Center});
                if (currentlyBlocked)
                {
                    AnsiConsole.Write(new Rule($"[bold](THIS ISSUE IS CURRENTLY BLOCKED)[/]").NoBorder().LeftJustified().RuleStyle(new Style(Color.DarkRed_1,Color.Cornsilk1)));
                }
                AnsiConsole.Write(new Rule($"[dim]({ic.IssueObj.IssueType.ToUpper()}) [/][bold]{ic.IssueObj.Key}[/][dim], DESC:[/] {Markup.Escape(ic.IssueObj.Summary)}").NoBorder().LeftJustified().RuleStyle(new Style(Color.Blue,Color.Cornsilk1)));

                AnsiConsole.Write(new Rule($"[dim]Current Status:[/][bold] ({ic.IssueObj.StatusName.ToUpper()})[/]{formattedStartDt}").NoBorder().LeftJustified().RuleStyle(new Style(Color.Blue,Color.Cornsilk1)));



                // AnsiConsole.Write(new Rule($"{formattedStartDt}").NoBorder().LeftJustified().RuleStyle(new Style(Color.Blue,Color.Cornsilk1)));

                // LAST SUMMARY 'RULE' LINE
                AnsiConsole.Write(new Rule(){Style=new Style(Color.Blue,Color.Cornsilk1), Justification=Justify.Center});                

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
                    new TableColumn(new Markup($"[bold][underline]LAST[/]{Environment.NewLine}EXITED DATE[/]").Centered())
                });
                
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
                        ss.BlockTime = ss.BlockTime.Add(sc.LogItem.TotalBlockedBusinessTime);
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

                        new Markup($"{statSumm.FirstEntry}").Centered(), 
                        new Markup($"{statSumm.LastExit}").Centered()
                    });

                    
                }
                    ic.SetCalendarDays(Math.Round(passiveCalDays + activeCalDays,2));
                    ic.SetBusinessDays(Math.Round(passiveBusDays + activeBusDays,2));
                    ic.SetBlockedActiveDays(Math.Round(totActiveBlockedDays,2));
                    ic.SetUnblockedActiveDays(Math.Round(totActiveUnblockedDays,2));

                        // activeCalDays += statSumm.CalTime.TotalDays;
                        // activeBusDays += statSumm.BusTime.TotalDays;
                        // activeBlockDays += statSumm.BlockTime.TotalDays;
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
                    new Markup($"[dim]---[/]").Centered()
                });
                AnsiConsole.Write(tbl);
                AnsiConsole.WriteLine();
                if (ic.Blockers.Count > 0)
                {
                    AnsiConsole.Write(new Rule($"[bold]BLOCKERS FOR: {ic.IssueObj.Key}[/]"){Style=new Style(Color.DarkRed,Color.White), 
                    Justification=Justify.Left});
                    tbl = new Table();
                    tbl.AddColumns("Issue Key", "BlockStart", "BlockEnd");
                    foreach (var block in ic.Blockers)
                    {   
                        var tEndDt = string.Empty;      
                        if (block.CurrentlyBlocked)                  
                        {
                            tEndDt = "* Currently Blocked *";
                        }
                        else if (block.EndDt.HasValue)
                        {
                            tEndDt = block.EndDt.Value.ToString();
                        }
                        tbl.AddRow(new Text[]{
                            new Text($"{block.IssueKey}",ConsoleUtil.StdStyle(StdLine.slOutput).Decoration(Decoration.Bold)).Centered(),
                            new Text($"{block.StartDt}",ConsoleUtil.StdStyle(StdLine.slOutput).Decoration(Decoration.None)).Centered(),
                            new Text($"{tEndDt}",ConsoleUtil.StdStyle(StdLine.slOutput).Decoration(Decoration.None)).Centered()
                        });                        
                    }
                    AnsiConsole.Write(tbl);
                }

                if (writeCount == totalCount)
                {
                    AnsiConsole.Write(new Rule(){Style=new Style(Color.DarkRed,Color.White)});
                    ConsoleUtil.PressAnyKeyToContinue($"Showing {writeCount} / {totalCount} results");
                }
                else 
                {
                    if (writeAll == false)
                    {
                        AnsiConsole.Write(new Rule(){Style=new Style(Color.DarkRed,Color.White)});
                        string? resp = ConsoleUtil.GetInput<string>("PRESS 'ENTER'=View Next, 'A'=Show All At Once, 'X'=Stop Showing Results",allowEmpty:true);
                        if (resp.StringsMatch("A"))
                        {
                            WriteIssueSummary(true);
                            return;
                        }
                        else if (resp.StringsMatch("X"))
                        {
                            return;
                        }
                    }
                }
                
            }
        }

    }
}