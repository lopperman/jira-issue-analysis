using System;
using System.Collections.Generic;
using Atlassian.Jira;
using JConsole.ConsoleHelpers.ConsoleTables;

namespace JiraCon 
{
    public class EpicAnalysis
    {
        public EpicAnalysis()
        {

        }

        public void Analyze()
        {
            while (Menu())
            {
            }
        }

        private static string BuildJQL_EpicChildren(string epicKey)
        {
            return string.Format("project=BAM AND parentEpic={0}", epicKey);
        }

        private static bool Menu()
        {
            BuildMenu();
            ConsoleUtil.Lines.WriteQueuedLines(true);

            //string jql2 = string.Format("project in (BAM,POS) AND parentEpic={0}", epicKey);

            //project in (require 1 or more))
            /*
            1.  specify 1 or more projects, space delimited
            2.  optional 
                 - ANY search terms found in Summary OR Description
                 - ALL search terms found in Summary OR Description
                 - 
            


            */
            
            var resp = Console.ReadKey(true);
            if (resp.Key == ConsoleKey.K)
            {
                ConsoleUtil.WriteLine("Enter Epic Key (e.g. BAM-1234)");
                var epicKey = Console.ReadLine();
                ConsoleUtil.WriteLine(string.Format("Create time metrics for Epic: {0}? (ENTER 'Y' TO CONTINUE OR PRESS ANOTHER KEY TO RETURN TO MENU", epicKey));
                var read = Console.ReadKey(true);
                if (read.Key != ConsoleKey.Y)
                {
                    return true;
                }

                var dic = MainClass.GetBusinessHours();
                int startHour = dic["start"];
                int endHour = dic["end"];

                MainClass.CreateWorkMetricsFile(BuildJQL_EpicChildren(epicKey), startHour, endHour,epicKey);

                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine("Press any key to continue.");
                Console.ReadKey(true);

            }
            else if (resp.Key == ConsoleKey.F)
            {
                string projects = string.Empty;
                bool ANDED = false;
                bool SUMMARY_ONLY = false;
                bool RETURN_ALL_RESULTS = false; ;
                string searchTerms = string.Empty;

                ConsoleUtil.WriteLine("You are required to enter 1 or more project keys.",ConsoleColor.White,ConsoleColor.DarkCyan,false);
                ConsoleUtil.WriteLine("optionally you can enter search terms to finds Epics by Summary (Title) or Description.", ConsoleColor.White, ConsoleColor.DarkCyan, false);
                ConsoleUtil.WriteLine("ENTER 1 OR MORE PROJECT KEYS, SEPARATED BY SPACES (e.g. BAM POS)", ConsoleColor.White, ConsoleColor.DarkCyan, false);
                projects = Console.ReadLine();
                ConsoleUtil.WriteLine("***** Epic Summary (Title) and Description Search *****", ConsoleColor.White, ConsoleColor.DarkCyan, false);
                ConsoleUtil.WriteLine("***** Choose a search option *****", ConsoleColor.White, ConsoleColor.DarkCyan, false);
                ConsoleUtil.WriteLine("1 - Return epics where Summary or Description contain ANY search terms", ConsoleColor.White, ConsoleColor.DarkCyan, false);
                ConsoleUtil.WriteLine("2 - Return epics where Summary or Description contain ALL search terms", ConsoleColor.White, ConsoleColor.DarkCyan, false);
                ConsoleUtil.WriteLine("3 - Return epics where Summary (Title) contains ANY search terms", ConsoleColor.White, ConsoleColor.DarkCyan, false);
                ConsoleUtil.WriteLine("4 - Return epics where Summary (Title) contains ALL search terms", ConsoleColor.White, ConsoleColor.DarkCyan, false);
                ConsoleUtil.WriteLine("5 - View list of all epics for selected project(s).", ConsoleColor.White, ConsoleColor.DarkCyan, false);

                var searchOption = Console.ReadKey(true);
                string searchStrategyDesc = string.Empty;

                if (searchOption.KeyChar.ToString() == "1")
                {
                    ConsoleUtil.WriteLine("Search Option 1 selected", ConsoleColor.White, ConsoleColor.DarkYellow, false);
                    searchStrategyDesc = "Enter 1 or more search terms, separated by spaces. Epics that contain ** ANY ** search terms (Summary or Desc fields) will be returned.";
                    ANDED = false;
                }
                else if (searchOption.KeyChar.ToString() == "2")
                {
                    ConsoleUtil.WriteLine("Search Option 2 selected", ConsoleColor.White, ConsoleColor.DarkYellow, false);
                    searchStrategyDesc = "Enter 1 or more search terms, separated by spaces. Epics that contain ** ALL ** search terms (Summary or Desc fields) will be returned.";
                    ANDED = true;
                }
                else if (searchOption.KeyChar.ToString() == "3")
                {
                    ConsoleUtil.WriteLine("Search Option 3 selected", ConsoleColor.White, ConsoleColor.DarkYellow, false);
                    searchStrategyDesc = "Enter 1 or more search terms, separated by spaces. Epics that contain ** ANY ** search terms (Summary field) will be returned.";
                    ANDED = false;
                    SUMMARY_ONLY = true;
                }
                else if (searchOption.KeyChar.ToString() == "4")
                {
                    ConsoleUtil.WriteLine("Search Option 4 selected", ConsoleColor.White, ConsoleColor.DarkYellow, false);
                    searchStrategyDesc = "Enter 1 or more search terms, separated by spaces. Epics that contain ** ALL ** search terms (Summary field) will be returned.";
                    ANDED = true;
                    SUMMARY_ONLY = true;
                }
                else if (searchOption.KeyChar.ToString() == "5")
                {
                    ConsoleUtil.WriteLine("Search Option 5 selected", ConsoleColor.White, ConsoleColor.DarkYellow, false);

                    RETURN_ALL_RESULTS = true;
                }

                

                if (projects == null || projects.Trim().Length == 0)
                {
                    ConsoleUtil.WriteLine("1 Or More Project Keys is required!  Doh!", ConsoleColor.Red,ConsoleColor.White,false);
                    ConsoleUtil.PressAnyKeyToContinue();
                    return true;
                }
                if (!RETURN_ALL_RESULTS)
                {
                    ConsoleUtil.WriteLine(searchStrategyDesc, ConsoleColor.White, ConsoleColor.DarkCyan, false);
                    searchTerms = Console.ReadLine();

                    if (searchTerms == null || searchTerms.Trim().Length == 0)
                    {
                        ConsoleUtil.WriteLine("You chose to search for epics based on search terms, but failed to enter any search terms.  Doh!",ConsoleColor.Red,ConsoleColor.Yellow,false);
                        ConsoleUtil.PressAnyKeyToContinue();
                        return true;
                    }
                }
                string epicSearchJQL = BuildJQL_EpicSearch(projects, searchTerms, ANDED, SUMMARY_ONLY, RETURN_ALL_RESULTS); ;
                ConsoleUtil.WriteLine(string.Format("AWESOME -- Please be patient while your results are fetched using the following *** JQL: {0}",epicSearchJQL),ConsoleColor.DarkBlue,ConsoleColor.Gray,false);

                var epics = JiraUtil.JiraRepo.GetIssues(epicSearchJQL);
                ConsoleUtil.WriteLine("***** ***** *****", ConsoleColor.White, ConsoleColor.Black, false);
                ConsoleUtil.WriteLine("***** ***** *****", ConsoleColor.White, ConsoleColor.Black, false);
                ConsoleUtil.WriteLine(string.Format("{0} epics were found matching your search criteria", epics.Count), ConsoleColor.White, ConsoleColor.Black, false);
                ConsoleUtil.WriteLine("***** ***** *****", ConsoleColor.White, ConsoleColor.Black, false);
                ConsoleUtil.WriteLine("***** ***** *****", ConsoleColor.White, ConsoleColor.Black, false);
                ConsoleUtil.PressAnyKeyToContinue();

                if (epics.Count == 0)
                {
                    return true;
                }
                ConsoleUtil.WriteLine("Please wait while the search results are evaluated", ConsoleColor.DarkBlue, ConsoleColor.Gray, false);
                SortedList<JIssue, List<JIssue>> epicsWithChildren = new SortedList<JIssue, List<JIssue>>();

                string[] projArray = projects.ToUpper().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string projSearchItems = projArray.Length == 1 ? projArray[0] : String.Join(",", projArray);

                int epicsEvaluated = 0;
                int totalEpicsFound = epics.Count;

                foreach (Issue epic in epics)
                {
                    epicsEvaluated += 1;
                    SortedDictionary<string, int> epicIssueBreakdown = new SortedDictionary<string, int>();
                    JIssue jepic = new JIssue(epic);
                    ConsoleUtil.WriteLine(string.Format("Analyzing Epic: {0} ({1}) - {2}",jepic.Key,jepic.StatusName,jepic.Summary), ConsoleColor.DarkBlue, ConsoleColor.Gray, false);

                    string epicIssuesJQL = string.Format("project in({0}) AND parentEpic={1}", projSearchItems, jepic.Key);

                    List<Issue> epicIssues = JiraUtil.JiraRepo.GetIssues(epicIssuesJQL);
                    List<JIssue> epicChildren = new List<JIssue>();
                    foreach (Issue epicIssue in epicIssues)
                    {
                        JIssue epicChild = new JIssue(epicIssue);
                        epicChildren.Add(epicChild);
                        if (epicIssueBreakdown.ContainsKey(epicChild.IssueType.ToLower()))
                        {
                            epicIssueBreakdown[epicChild.IssueType.ToLower()] = epicIssueBreakdown[epicChild.IssueType.ToLower()] + 1;
                        }
                        else
                        {
                            epicIssueBreakdown.Add(epicChild.IssueType.ToLower(), 1);
                        }
                    }
                    ConsoleTable table = new ConsoleTable(string.Format("Epic:{0} ({1}/{2} results)",jepic.Key,epicsEvaluated,totalEpicsFound) , "ChildType", "Qty");
                    foreach (var kvp in epicIssueBreakdown)
                    {
                        table.AddRow(" ***** ", kvp.Key, kvp.Value);
                    }
                    table.Write();

                    ConsoleUtil.WriteLine(string.Format("Isn't this cool? -- Do you want to generate the Work Time Analytics for this Epic ({0})? (Don't worry, we'll come back here where you left off if you decide to do that!)", jepic.Key), ConsoleColor.DarkBlue, ConsoleColor.Gray, false);
                    ConsoleUtil.WriteLine(string.Format("Press 'Y' to generate the Work Time Analytics file for epic {0}",jepic.Key), ConsoleColor.DarkBlue, ConsoleColor.Gray, false);
                    ConsoleUtil.WriteLine(string.Format("Press 'E' to stop reviewing epic search results", jepic.Key), ConsoleColor.DarkBlue, ConsoleColor.Gray, false);
                    ConsoleUtil.WriteLine(string.Format("Press any other key to continue reviewing epic search results.", jepic.Key), ConsoleColor.DarkBlue, ConsoleColor.Gray, false); var read = Console.ReadKey(true);
                    if (read.Key == ConsoleKey.Y)
                    {
                        MainClass.CreateWorkMetricsFile(epicIssuesJQL, 7, 18, jepic.Key);
                        ConsoleUtil.PressAnyKeyToContinue();
                    }
                    else if (read.Key == ConsoleKey.E)
                    {
                        return true;
                    }
                }
            }
            else if (resp.Key == ConsoleKey.M)
            {
                return false;
            }

            return true;
        }

        private static string BuildJQL_EpicSearch(string projects, string searchTerms, bool ANDED, bool SUMMARY_ONLY, bool RETURN_ALL_RESULTS)
        {
            string jql = string.Empty;
            string[] projArray = projects.ToUpper().Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
            string projSearchItems = projArray.Length == 1 ? projArray[0] : String.Join(",", projArray);
            string[] termArray = null;

            if (searchTerms != null && searchTerms.Length > 0)
            {
                termArray = searchTerms.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            jql = string.Format("project in({0}) AND issuetype=Epic", projSearchItems);

            if (!RETURN_ALL_RESULTS)
            {
                string expression = "";

                if (!ANDED)
                {
                    //ORED
                    if (SUMMARY_ONLY)
                    {
                        foreach (var term in termArray)
                        {
                            expression = (expression.Length == 0) ? string.Format("summary ~{0}", term) : string.Format(" {0} OR summary ~{1}", expression, term); 
                        }
                    }
                    else
                    {
                        foreach (var term in termArray)
                        {
                            expression = (expression.Length == 0) ? string.Format("summary ~{0} OR description ~{0}", term) : string.Format(" {0} OR summary ~{1} OR description ~{1}", expression, term);
                        }
                    }
                    jql = string.Format("{0} AND ({1})", jql, expression);
                }
                else
                {
                    //ANDED
                    if (SUMMARY_ONLY)
                    {
                        foreach (var term in termArray)
                        {
                            expression = (expression.Length == 0) ? string.Format("summary ~{0}", term) : string.Format(" {0} AND summary ~{1}", expression, term);
                        }
                        jql = string.Format("{0} AND {1}", jql, expression);
                    }
                    else
                    {
                        foreach (var term in termArray)
                        {
                            //project in(BAM) AND issuetype=Epic and ((summary ~curbside OR description ~curbside) AND (summary ~flexibility OR description ~flexibility))

                            expression = (expression.Length == 0) ? string.Format("(summary ~{0} OR description ~{0})", term) : string.Format(" {0} AND (summary ~{1} OR description ~{1})", expression, term);
                        }
                        jql = String.Format("{0} AND ({1})", jql, expression);

                    }

                }
            }

            return jql;

        }

        private static void BuildMenu()
        {
            var consoleLines = ConsoleUtil.Lines;
            consoleLines.AddConsoleLine(" --------------------- ", ConsoleColor.Black, ConsoleColor.White);
            consoleLines.AddConsoleLine("|  Epic Analysis Menu |");
            consoleLines.AddConsoleLine(" --------------------- ");
            consoleLines.AddConsoleLine("(K) Enter Epic Key");
            consoleLines.AddConsoleLine("(F) Find 1 or More Epics and Analyze Separately");
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("(M) Main Menu");
        }

    }
}
