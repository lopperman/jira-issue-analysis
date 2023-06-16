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
                ConsoleUtil.WriteStdLine("ENTER 1 OR MORE ISSUE KEYS (E.G. WWT-100 WWT-101) SEPARATED BY SPACES",StdLine.slResponse,false);
                ConsoleUtil.WriteStdLine("(IF YOU WISH TO CANCEL OR SELECT A SAVED LIST OF ISSUES, PRESS 'ENTER')",StdLine.slResponse,false);
            }
            else if (_type == AnalysisType.atJQL)
            {
                ConsoleUtil.WriteStdLine("ENTER JQL TO FILTER ISSUES",StdLine.slResponse,false);
                ConsoleUtil.WriteStdLine("(IF YOU WISH TO CANCEL OR SELECT A SAVED LIST OF JQL, PRESS 'ENTER')",StdLine.slResponse,false);
            }
            data = Console.ReadLine();
            if (data == null || data.Length == 0)
            {
                ConsoleUtil.WriteStdLine("PRESS 'Y' TO SELECT A SAVED LIST OF JQL - ANY OTHER KEY TO CANCEL",StdLine.slResponse,false);
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
                JCalcs.Add(new IssueCalcs(iss));
            }

            foreach (var jc in JCalcs)
            {
                foreach (var ln in jc.StateCalcStringList())
                {
                    ConsoleUtil.WriteStdLine(ln,StdLine.slOutput,false);
                }
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
                }
                ConsoleUtil.WriteStdLine(string.Format("Saved to: {0}{1}{2}",csvPath,Environment.NewLine,"PRESS ANY KEY TO CONTINUE"),StdLine.slResponse,false);
                Console.ReadKey(true);

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
                    title = "SELECT SAVED JQL QUERY (MUST BE VALID JQL) - ANALYSIS WILL RUN ON ISSUES RETURNED FROM QUERY";
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
            switch(_type)
            {
                case AnalysisType.atIssues:
                    string toJQL = BuildJQLKeyInList(searchData);
                    issues = JiraUtil.JiraRepo.GetIssues(toJQL);
                    break;
                case AnalysisType.atEpics:
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