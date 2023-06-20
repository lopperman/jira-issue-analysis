using System.Net.NetworkInformation;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Text;
using Atlassian.Jira;

namespace JiraCon
{

    public enum AnalysisType
    {
        _atUnknown = -1, 
        atJQL = 1, 
        atIssues = 2, 
        atEpics = 3
    }

    public class AnalyzeIssues
    {
        private AnalysisType _type = AnalysisType._atUnknown;
        private string searchData = string.Empty;
        public List<JIssue> JIssues {get; private set;}
        public List<IssueCalcs> JCalcs {get; private set;}

        public bool HasSearchData
        {
            get
            {
                return (searchData != null && searchData.Length > 0);
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
                ConsoleUtil.WriteStdLine("ENTER 1 OR MORE ISSUE KEYS (E.G. WWT-100 WWT-101) SEPARATED BY SPACES, OR PRESS 'ENTER'",StdLine.slResponse,false);
            }
            else if (_type == AnalysisType.atJQL)
            {
                ConsoleUtil.WriteStdLine("ENTER JQL TO FILTER ISSUES, OR PRESS 'ENTER'",StdLine.slResponse,false);
            }
            else if (_type == AnalysisType.atEpics)
            {
                ConsoleUtil.WriteStdLine("ENTER 1 OR MORE EPICS SEPARATED BY SPACES, OR PRESS 'ENTER'",StdLine.slResponse,false);
            }
            data = Console.ReadLine();
            if (data == null || data.Length == 0)
            {
                ConsoleUtil.WriteStdLine("'Y' TO SELECT SAVED JQL, OR PRESS 'ENTER'",StdLine.slResponse,false);
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    data = SelectSavedJQL();
                }                
            }
            searchData = data;
        }

        public void ClassifyStates()
        {
            bool addedHeader = false ;
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
                if (ic.Blockers.Count > 0)
                {
                    foreach (var b in ic.Blockers)
                    {
                        ConsoleUtil.WriteStdLine(string.Format("Added blocker for {0}: Start {1}, End {2}, Field {3}",b.IssueKey,b.StartDt,b.EndDt,b.BlockerFieldName),StdLine.slCode ,false);
                    }
                    // ConsoleUtil.WriteStdLine("PRESS ANY KEY",StdLine.slResponse  ,false);
                    // Console.ReadKey(true);
                }
            }

            bool addConsoleHeader = true;
            foreach (IssueCalcs issCalcs in JCalcs)
            {
                foreach (var ln in issCalcs.StateCalcStringList(addConsoleHeader))
                {
                    ConsoleUtil.WriteStdLine(ln,StdLine.slOutput,false);
                }
                addConsoleHeader = false;
            }
            ConsoleUtil.WriteStdLine("PRESS 'Y' to Save to csv file",StdLine.slResponse,false);
            if (Console.ReadKey(true).Key == ConsoleKey.Y)
            {
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
                ConsoleUtil.WriteStdLine(string.Format("Saved to: {0}{1}{2}",csvPath,Environment.NewLine,"PRESS ANY KEY TO CONTINUE"),StdLine.slResponse,false);
                Console.ReadKey(true);
            }


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
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    return ret;
                }
            }
            return String.Empty;
        }

        public int GetData()
        {
            List<Issue> issues = new List<Issue>();
            ConsoleUtil.WriteStdLine("QUERYING JIRA ISSUES",StdLine.slOutputTitle ,false);
            string toJQL = string.Empty;
            switch(_type)
            {
                case AnalysisType.atIssues:
                    toJQL = BuildJQLKeyInList(searchData);
                    issues = JiraUtil.JiraRepo.GetIssues(toJQL);
                    break;
                case AnalysisType.atEpics:
                    toJQL = BuildJQLForEpicChildren(searchData);
                    issues = JiraUtil.JiraRepo.GetIssues(toJQL);

                    break;
                case AnalysisType.atJQL:
                    issues = JiraUtil.JiraRepo.GetIssues(searchData);
                    break;
                default:
                    break;
            }
            if (issues.Count > 0)
            {
                ConsoleUtil.WriteStdLine(String.Format("{0} issues found",issues.Count),StdLine.slOutputTitle ,false);
                foreach (var issue in issues)
                {
                    JIssue newIssue = new JIssue(issue);
                    ConsoleUtil.WriteStdLine(String.Format("Building Change Logs for {0} {1}",newIssue.Key,newIssue.Summary),StdLine.slOutputTitle ,false);
                    newIssue.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(issue));
                    JIssues.Add(newIssue);
                }
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

        private string BuildJQLKeyInList(string srchData)
        {
            string[] cards = srchData.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            sb.Append("key in (");
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
    }
}