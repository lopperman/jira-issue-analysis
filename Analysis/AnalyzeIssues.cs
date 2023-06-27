using System.Data.Common;
using System.Net.NetworkInformation;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Text;
using Atlassian.Jira;
using Spectre.Console;

namespace JiraCon
{

    public enum AnalysisType
    {
        _atUnknown = -1, 
        atJQL = 1, 
        atIssues = 2, 
        atEpics = 3, 
        atIssueSummary = 4
    }

    public class AnalyzeIssues
    {
        private AnalysisType _type = AnalysisType._atUnknown;
        private string searchJQL = string.Empty;
        
        public List<JIssue> JIssues {get; private set;}
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
            JIssues = new List<JIssue>();
            JCalcs = new List<IssueCalcs>();
        }
        public AnalyzeIssues(AnalysisType analysisType): this()
        {
            _type = analysisType;
            string? data = string.Empty;
            if (_type == AnalysisType.atIssues)
            {
                searchJQL = ConsoleInput.IssueKeysToJQL();
            }
            else if (_type==AnalysisType.atJQL)
            {
                string? tJQL = ConsoleUtil.GetInput<string>("ENTER JQL STATEMENT TO SELECT ITEMS",allowEmpty:true);
                if (!string.IsNullOrWhiteSpace(tJQL))
                {
                    if (ConsoleUtil.Confirm($"Use the following JQL?{Environment.NewLine}{tJQL}",true))
                    {
                        searchJQL = tJQL;
                    }
                }
            }
            else if (_type == AnalysisType.atIssueSummary)
            {
                searchJQL = ConsoleInput.IssueKeysToJQL();
            }
            else if (_type == AnalysisType.atEpics)
            {
                ConsoleUtil.PressAnyKeyToContinue("NOT IMPLEMENTED");
                
            }
            // data = Console.ReadLine();
            // if (data == null || data.Length == 0)
            // {
            //     ConsoleUtil.WriteStdLine("'Y' TO SELECT SAVED JQL, OR PRESS 'ENTER'",StdLine.slResponse,false);
            //     if (Console.ReadKey(true).Key == ConsoleKey.Y)
            //     {
            //         data = SelectSavedJQL();
            //     }                
            // }
            // searchData = data;
        }

        public void ClassifyStates()
        {
            if (JIssues.Count == 0)
            {
                return;
            }
            foreach (JIssue iss in JIssues)
            {
                var issCalc = new IssueCalcs(iss);
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
                                var stCfg = JTISConfigHelper.config.StatusConfigs.FirstOrDefault(x=>x.StatusId == tmpID );
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
                // if (ic.Blockers.Count > 0)
                // {
                //     foreach (var b in ic.Blockers)
                //     {
                //         ConsoleUtil.WriteStdLine(string.Format("Added blocker for {0}: Start {1}, End {2}, Field {3}",b.IssueKey,b.StartDt,b.EndDt,b.BlockerFieldName),StdLine.slCode ,false);
                //     }
                // }
            }

        }

        public void WriteToConsole()
        {
            WriteIssueSummary();
        }

        public string WriteToCSV()
        {
            bool addedHeader = false ;
//            ConsoleUtil.WriteStdLine("PRESS 'Y' to Save to csv file",StdLine.slResponse,false);
            DateTime now = DateTime.Now;
            string fileName = string.Format("AnalysisOutput_{0:0000}{1}{2:00}_{3}.csv", now.Year, now.ToString("MMM"), now.Day, now.ToString("hhmmss"));
            string csvPath = Path.Combine(JTISConfigHelper.JTISRootPath,fileName);

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
                    // ConsoleUtil.WriteStdLine(string.Format("Key: {0}, FieldName: {7}, Start:{1}, End: {2}, FromId: {3}, FromValue:{4}, ToId: {5}, ToValue: {6}",cl.JIss.Key,cli.StartDt,cli.EndDt,cli.FromId,cli.FromValue,cli.ToId,cli.ToValue, cli.FieldName),StdLine.slCode,false);
                    if (cli.FieldName.ToLower()=="flagged"   )
                    {
                        if (cli.ToValue.ToLower()=="impediment")
                        {
                            var newBlocker = new Blocker(cl.JIss.Key,cli.StartDt, cli.ChangeLogType, cli.FieldName );
                            tmpStartingBlockers.Add(newBlocker);
                        }      
                        else if (cli.FromValue.ToLower()=="impediment")
                        {
                            var newBlocker = new Blocker(cl.JIss.Key,cli.StartDt,cli.ChangeLogType,cli.FieldName,cli.StartDt);
                            tmpEndingBlockers.Add(newBlocker);
                        }                                          
                    }
                    else if (cli.FieldName.ToLower()=="priority")
                    {
                        if (cli.ToValue.ToLower() == "blocked")                        
                        {
                            var newBlocker = new Blocker(cl.JIss.Key,cli.StartDt, ChangeLogTypeEnum.clBlockedField,cli.FieldName );
                            tmpStartingBlockers.Add(newBlocker);
                        }                        
                        if (cli.FromValue.ToLower() == "blocked")                        
                        {
                            var newBlocker = new Blocker(cl.JIss.Key,cli.StartDt, ChangeLogTypeEnum.clBlockedField,cli.FieldName );
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

            }            
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
            ret = JTISConfigHelper.GetSavedJQL(title);
            if (ret != null && ret.Length > 0)
            {
                ConsoleUtil.WriteStdLine("PRESS 'Y' TO USE THE FOLLOWING SAVED JQL/QUERY - ANY OTHER KEY TO CANCEL",StdLine.slResponse,false);
                ConsoleUtil.WriteStdLine(ret,StdLine.slCode,false);
                if (Console.ReadKey(true).Key == ConsoleKey.Y)
                {
                    return ret;
                }
            }
            return String.Empty;
        }

        public int GetData()
        {
            try 
            {
                List<Issue> issues = new List<Issue>();
                if (string.IsNullOrWhiteSpace(searchJQL))
                {
                    return 0;
                }
                ConsoleUtil.WriteStdLine("QUERYING JIRA ISSUES",StdLine.slInfo ,false);

                // string toJQL = string.Empty;
                // switch(_type)
                // {
                //     case AnalysisType.atIssues:
                //         toJQL = BuildJQLKeyInList(searchData);
                //         issues = JiraUtil.JiraRepo.GetIssues(toJQL);
                //         break;
                //     case AnalysisType.atIssueSummary:
                //         toJQL = BuildJQLKeyInListArr(searchDataArr);
                //         issues = JiraUtil.JiraRepo.GetIssues(toJQL);
                //         break;
                //     case AnalysisType.atEpics:
                //         toJQL = BuildJQLForEpicChildren(searchData);
                //         issues = JiraUtil.JiraRepo.GetIssues(toJQL);

                //         break;
                //     case AnalysisType.atJQL:
                //         issues = JiraUtil.JiraRepo.GetIssues(searchData);
                //         break;
                //     default:
                //         break;
                // }


                AnsiConsole.Status()
                    .Start($"Querying Jira", ctx=>
                    {
                        ctx.Status("[bold]Retrieving items ...[/]");
                        ctx.Spinner(Spinner.Known.Dots);
                        ctx.SpinnerStyle(new Style(AnsiConsole.Foreground,AnsiConsole.Background));
                        Thread.Sleep(1000);
                        issues = JiraUtil.JiraRepo.GetIssues(searchJQL);
                    });

                if (issues.Count > 0)
                {
                    ConsoleUtil.WriteStdLine(String.Format("Retrieved {0} issues ",issues.Count),StdLine.slCode ,false);
                    AnsiConsole.Progress()                        
                        .Columns(new ProgressColumn[]
                        {
                            new TaskDescriptionColumn(), 
                            new PercentageColumn(),
                            new ElapsedTimeColumn(), 
                            new SpinnerColumn(), 

                        })
                        .Start(ctx => 
                        {
                            var task1 = ctx.AddTask($"[blue] loading change logs for {issues.Count} issues [/]");
                            task1.MaxValue = issues.Count;
                            foreach (var issue in issues)
                            {
                                JIssue newIssue = new JIssue(issue);
                                newIssue.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(issue));
                                JIssues.Add(newIssue);
                                task1.Increment(1);
                            }
                        });
                }
            }
            catch(Exception ex)
            {
                ConsoleUtil.WriteError(string.Format("Error getting issues using search: {0}",searchJQL),ex:ex);
                GetDataFail = true;
            }
            return JIssues.Count;

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


            //     retJQL = string.Format("project={0} and parentEpic={1}",JTISConfigHelper.config.defaultProject,epicKey);
            // {
            //     retJQL = string.Format("parentEpic={0}",epicKey);
            // }
        }

        private string BuildJQLKeyInListArr(string[]? srchData)
        {
            if (srchData == null || srchData.Length == 0){return string.Empty;}
            string[] cards = srchData;
            StringBuilder sb = new StringBuilder();
            string defProj = JTISConfigHelper.config.defaultProject;
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
            string defProj = JTISConfigHelper.config.defaultProject;
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

        private void WriteIssueSummary()
        {
            foreach (var ic in JCalcs)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule(){Style=new Style(Color.Blue,Color.Cornsilk1), Justification=Justify.Center});
                AnsiConsole.Write(new Rule($"[bold]SUMMARY FOR: {ic.IssueObj.Key}  ({ic.IssueObj.StatusName})  [/]"){Style=new Style(Color.Blue,Color.Cornsilk1), Justification=Justify.Left});
                AnsiConsole.MarkupLineInterpolated($"\t[dim blue on white]{ic.IssueObj.Summary}[/]");

                AnsiConsole.MarkupLineInterpolated($"\t[{StdLine.slOutput.FontMkp()} on {StdLine.slOutput.BackMkp()}][dim]CURRENT STATUS:[/] {ic.IssueObj.StatusName}[/]");

                var scStart = ic.StateCalcs.FirstOrDefault(x=>x.ActivityType == StatusType.stStart);
                if (scStart != null)
                {
                    ic.FirstActiveStateCalc = scStart;
                    string tStartDt = scStart.StartDt.ToString("yyyy-MMM-dd HH:mm");
                    try 
                    {
                        AnsiConsole.MarkupLineInterpolated($"\t[{StdLine.slOutput.FontMkp()} on {StdLine.slOutput.BackMkp()}][dim]ACTIVE WORK STARTED:[/] {tStartDt}[/]");
                    }
                    catch 
                    {
                    }
                }
                else 
                {
                    AnsiConsole.MarkupLineInterpolated($"\t[{StdLine.slOutput.FontMkp()} on {StdLine.slOutput.BackMkp()}][dim]ACTIVE WORK HAS NOT STARTED:[/][/]");
                }

                var tbl = new Table();
                tbl.NoSafeBorder();
                tbl.Expand();
                tbl.RoundedBorder();                
                tbl.AddColumns(new TableColumn[]{
                    new TableColumn(new Text("Status",ConsoleUtil.StdStyle(StdLine.slOutputTitle)).Centered()),
                    new TableColumn(new Text("Category",ConsoleUtil.StdStyle(StdLine.slOutputTitle).Decoration(Decoration.Bold)).Centered()).Width(15),
                    new TableColumn(new Text($"Total{Environment.NewLine}Days",ConsoleUtil.StdStyle(StdLine.slOutputTitle)).Centered()),
                    new TableColumn(new Text($"Total{Environment.NewLine}BusDays",ConsoleUtil.StdStyle(StdLine.slOutputTitle).Decoration(Decoration.Bold)).Centered()),
                    new TableColumn(new Text($"Blocked{Environment.NewLine}Days",ConsoleUtil.StdStyle(StdLine.slError)).Centered()),
                    new TableColumn(new Text($"Started{Environment.NewLine}Count",ConsoleUtil.StdStyle(StdLine.slOutputTitle)).Centered()),
                    new TableColumn(new Text("FirstStart",ConsoleUtil.StdStyle(StdLine.slOutputTitle)).Centered()),
                    new TableColumn(new Text("LastExit",ConsoleUtil.StdStyle(StdLine.slOutputTitle)).Centered())
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


                foreach (var statSumm in ssList)
                {
                    var trackStyle = todoStyle;
                    if (statSumm.TrackType==StatusType.stActiveState || statSumm.TrackType==StatusType.stStart){trackStyle=inProgressStyle;}
                    if (statSumm.TrackType==StatusType.stEnd){trackStyle=doneStyle;}
                    if (statSumm.TrackType == StatusType.stActiveState || statSumm.TrackType == StatusType.stStart)
                    {
                        activeCalDays += statSumm.CalTime.TotalDays;
                        activeBusDays += statSumm.BusTime.TotalDays;
                        activeBlockDays += statSumm.BlockTime.TotalDays;
                    }
                    else 
                    {
                        passiveCalDays += statSumm.CalTime.TotalDays;
                        passiveBusDays += statSumm.BusTime.TotalDays;
                        passiveBlockDays += statSumm.BlockTime.TotalDays;
                    }
                    tbl.AddRow(new Text[]{
                        new Text($" {statSumm.Status} ",ConsoleUtil.StdStyle(StdLine.slInfo)).Centered(),
                        new Text($" {statSumm.TrackType} ",ConsoleUtil.StdStyle(StdLine.slInfo).Decoration(Decoration.Bold)).Centered(),
                        new Text($" {statSumm.CalTime.TotalDays:##0.00} ",ConsoleUtil.StdStyle(StdLine.slOutputTitle)).Centered(),
                        new Text($" {statSumm.BusTime.TotalDays:##0.00} ",ConsoleUtil.StdStyle(StdLine.slOutputTitle)).Centered(),
                        new Text($" {statSumm.BlockTime.TotalDays:##0.00} ",ConsoleUtil.StdStyle(StdLine.slError)).Centered(),
                        new Text($" {statSumm.EntryCount} ",ConsoleUtil.StdStyle(StdLine.slOutputTitle)).Centered(),
                        new Text($" {statSumm.FirstEntry} ",ConsoleUtil.StdStyle(StdLine.slOutputTitle)).Centered(),
                        new Text($" {statSumm.LastExit} ",ConsoleUtil.StdStyle(StdLine.slOutputTitle)).Centered()
                    });
                }

                tbl.AddEmptyRow();
                tbl.AddRow(new Text[]{
                    new Text(" TOTALS ",ConsoleUtil.StdStyle(StdLine.slInfo).Decoration(Decoration.Bold)).Centered(),
                    new Text(" ** ACTIVE ** ",ConsoleUtil.StdStyle(StdLine.slInfo).Decoration(Decoration.Bold)).Centered(),
                    new Text($" {activeCalDays:0.00} ",ConsoleUtil.StdStyle(StdLine.slInfo)).Centered(),
                    new Text($" {activeBusDays:0.00} ",ConsoleUtil.StdStyle(StdLine.slInfo)).Centered(),
                    new Text($" {activeBlockDays:0.00} ",ConsoleUtil.StdStyle(StdLine.slInfo)).Centered(),
                    new Text(" --- "),
                    new Text(" --- "),
                    new Text(" --- ")
                });
                tbl.AddRow(new Text[]{
                    new Text(" TOTALS ",ConsoleUtil.StdStyle(StdLine.slInfo).Decoration(Decoration.Bold)).Centered(),
                    new Text($" ** PASSIVE ** ",ConsoleUtil.StdStyle(StdLine.slInfo).Decoration(Decoration.Bold)).Centered(),
                    new Text($" {passiveCalDays:0.00} ",ConsoleUtil.StdStyle(StdLine.slInfo)).Centered(),
                    new Text($" {passiveBusDays:0.00} ",ConsoleUtil.StdStyle(StdLine.slInfo)).Centered(),
                    new Text($" {passiveBlockDays:0.00} ",ConsoleUtil.StdStyle(StdLine.slInfo)).Centered(),
                    new Text(" --- "),
                    new Text(" --- "),
                    new Text(" --- ")
                });
                tbl.AddEmptyRow();
                tbl.AddRow(new Text[]{
                    new Text(" GRAND TOTAL ",ConsoleUtil.StdStyle(StdLine.slInfo).Decoration(Decoration.Bold)).Centered(),
                    new Text($" ** ALL ** ",ConsoleUtil.StdStyle(StdLine.slInfo).Decoration(Decoration.Bold)).Centered(),
                    new Text($" {activeCalDays+passiveCalDays:0.00} ",ConsoleUtil.StdStyle(StdLine.slInfo)).Centered(),
                    new Text($" {activeBusDays+passiveBusDays:0.00} ",ConsoleUtil.StdStyle(StdLine.slInfo)).Centered(),
                    new Text($" {activeBlockDays+passiveBlockDays:0.00} ",ConsoleUtil.StdStyle(StdLine.slInfo)).Centered(),
                    new Text(" --- "),
                    new Text(" --- "),
                    new Text(" --- ")
                });

                AnsiConsole.Write(tbl);
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Rule($"[bold]BLOCKERS FOR: {ic.IssueObj.Key}[/]"){Style=new Style(Color.DarkRed,Color.White), Justification=Justify.Left});
                if (ic.Blockers.Count > 0)
                {
                    tbl = new Table();
                    tbl.AddColumns("Issue Key", "BlockStart", "BlockEnd");
                    foreach (var block in ic.Blockers)
                    {   
                        var tEndDt = string.Empty;                        
                        if (block.EndDt.HasValue)
                        {
                            tEndDt = block.EndDt.Value.ToString();
                        }
                        else 
                        {
                            tEndDt = "(blocked now)";
                        }
                        tbl.AddRow(new Text[]{
                            new Text($"{block.IssueKey}",ConsoleUtil.StdStyle(StdLine.slOutput).Decoration(Decoration.Bold)).Centered(),
                            new Text($"{block.StartDt}",ConsoleUtil.StdStyle(StdLine.slOutput).Decoration(Decoration.None)).Centered(),
                            new Text($"{tEndDt}",ConsoleUtil.StdStyle(StdLine.slOutput).Decoration(Decoration.None)).Centered()
                        });                        
                    }
                    AnsiConsole.Write(tbl);
                }

                // hdrLine.Style.Decoration(Decoration.Bold);
                // if (ic.Blockers.Count > 0)
                // {
                //     tbl = new Table()
                //     tbl.AddColumns("Issue Key","BlockStart","BlockEnd")
                //     foreach (var block in ic.Blockers)
                //     {
                //         block.
                //     }
                // }
                
                AnsiConsole.Write(new Rule(){Style=new Style(Color.DarkRed,Color.White)});


                ConsoleUtil.PressAnyKeyToContinue();

            }
        }


    }
}