using Atlassian.Jira;
using JTIS.Analysis;
using JTIS.Console;
using JTIS.Extensions;
using JTIS.ManagedObjects;
using JTIS.Menu;
using Spectre.Console;

namespace JTIS
{
    internal class VisualSnapshot
    {
        private string searchJQL;

        public VisualSnapshotType SnapshotType {get;private set;}
        public AnalysisType SearchType {get;private set;}
        private List<Issue> issues = new List<Issue>();
        private List<JIssue> jIssues = new List<JIssue>();

        private VisualSnapshot(VisualSnapshotType snapshotType, AnalysisType analysisType)
        {
            SnapshotType = snapshotType;
            SearchType = analysisType;
            searchJQL = string.Empty;
        }

        public static VisualSnapshot Create(VisualSnapshotType snapshotType, AnalysisType  analysisType)
        {
            var vs = new VisualSnapshot(snapshotType, analysisType);
            return vs;
        }

        public void BuildSearch()
        {
            if (SearchType == AnalysisType.atIssues)
            {
                searchJQL = ConsoleInput.GetJQLOrIssueKeys(true);
            }
            else if (SearchType == AnalysisType.atEpics)
            {
                searchJQL = ConsoleInput.GetJQLOrIssueKeys(true,findEpicLinks:true);
            }
            if (string.IsNullOrWhiteSpace(searchJQL))
            {
                return;
            }
            GetIssues();
            BuildStatusBreakdown();
            ConsoleUtil.PressAnyKeyToContinue($"NEXT: BLOCKED/NONBLOCKED SUMMARY");
            BuildBlockedNonBlocked(false,false);
            ConsoleUtil.PressAnyKeyToContinue($"NEXT: ???");

        }

        private bool IsBlocked(Issue item)
        {
            var blocked = false;
            if (item.Priority.Name.Contains("block",StringComparison.InvariantCultureIgnoreCase))
            {
                blocked = true;            
            }
            else 
            {
                var flagged = item.CustomFields.Where(x=>x.Name.ToLower()=="flagged").FirstOrDefault();
                if (flagged != null)
                {
                    if (flagged.Values.Any(x=>x.StringsMatch("impediment")))
                    {
                        blocked = true;
                    }
                }
            }
            return blocked;
        }

        private void AddMissingKey(string key, ref SortedDictionary<string,double> dic)
        {
            if (dic.ContainsKey(key)==false)
            {
                dic.Add(key,0);
            }
        }
        private void BuildBlockedNonBlocked(bool clearScreen = true, bool showDetail = false)
        {
            if (issues.Count == 0){return;}

            SortedDictionary<string,double> chtCount = new SortedDictionary<string, double>();
            SortedDictionary<string,double> chtPerc = new SortedDictionary<string, double>();

            SortedDictionary<string,double> dict = new SortedDictionary<string, double>();
            foreach (var issue in issues)
            {
                var blocked = IsBlocked(issue);
                string key1 = "";
                string key2 = "";

                key1 = "00-All IsBlocked";
                key2 = "00-All NotBlocked";
                AddMissingKey(key1, ref dict);
                AddMissingKey(key2, ref dict);
                string useKey = blocked ? key1 : key2;
                dict[useKey]+=1;

                key1 = $"01-Type {issue.Type.Name} IsBlocked";
                key2 = $"01-Type {issue.Type.Name} NotBlocked";
                AddMissingKey(key1, ref dict);
                AddMissingKey(key2, ref dict);
                useKey = blocked ? key1 : key2;
                dict[useKey]+=1;

                key1 = $"02-Status {issue.Status.Name} IsBlocked";
                key2 = $"02-Status {issue.Status.Name} NotBlocked";
                AddMissingKey(key1, ref dict);
                AddMissingKey(key2, ref dict);
                useKey = blocked ? key1 : key2;
                dict[useKey]+=1;

            }

            var barCht = new BarChart();
            barCht.ShowValues();
            string useStart = "00-";
            foreach (var kvp in dict)
            {
                Spectre.Console.Color clr = Spectre.Console.Color.Red;
                if (kvp.Key.Substring(0,3)==useStart)
                {
                    if (kvp.Key.Contains("NotBlocked",StringComparison.OrdinalIgnoreCase)){clr = Spectre.Console.Color.Green;}
                    barCht.AddItem(kvp.Key.Replace(useStart,""),kvp.Value,clr);
                }
            }
            AnsiConsole.Write(new Panel(barCht).Expand().Header("BLOCKED VS. NOT BLOCKED - ALL",Justify.Center));

            useStart = "01-";
            foreach (var kvp in dict)
            {
                Spectre.Console.Color clr = Spectre.Console.Color.Red;
                if (kvp.Key.Substring(0,3)==useStart)
                {
                    if (kvp.Key.Contains("NotBlocked",StringComparison.OrdinalIgnoreCase)){clr = Spectre.Console.Color.Green;}
                    barCht.AddItem(kvp.Key.Replace(useStart,""),kvp.Value,clr);
                }
            }
            AnsiConsole.Write(new Panel(barCht).Expand().Header("BLOCKED VS. NOT BLOCKED - ISSUE TYPE",Justify.Center));

            useStart = "02-";
            foreach (var kvp in dict)
            {
                Spectre.Console.Color clr = Spectre.Console.Color.Red;
                if (kvp.Key.Substring(0,3)==useStart)
                {
                    if (kvp.Key.Contains("NotBlocked",StringComparison.OrdinalIgnoreCase)){clr = Spectre.Console.Color.Green;}
                    barCht.AddItem(kvp.Key.Replace(useStart,""),kvp.Value,clr);
                }
            }
            AnsiConsole.Write(new Panel(barCht).Expand().Header("BLOCKED VS. NOT BLOCKED - ISSUE STATUS",Justify.Center));


            if (showDetail == false &&  ConsoleUtil.Confirm($"[bold]Show Data?[/]",false,true))
            {
                BuildBlockedNonBlocked(clearScreen,true);
                return;
            }
            ConsoleUtil.PressAnyKeyToContinue();

        }
        private void BuildStatusBreakdown(bool clearScreen = true, bool showDetail = false)
        {
            if (clearScreen)
            {
                ConsoleUtil.WriteAppTitle();
                AnsiConsole.Write(new Rule());
            }

            if (issues.Count == 0){return;}
            var statuses = issues.Select(x=>x.Status.Name).ToList().Distinct();
            SortedDictionary<string,double> statusCounts = new SortedDictionary<string, double>();
            SortedDictionary<string,double> statusPercents = new SortedDictionary<string, double>();
            foreach (var tmpStatus in statuses)
            {   
                var pct = Math.Round((double)issues.Count(x=>x.Status.StringsMatch(tmpStatus)) / (double)issues.Count,2);
                statusCounts.Add(tmpStatus,issues.Count(x=>x.Status.StringsMatch(tmpStatus)));
                statusPercents.Add(tmpStatus,Math.Round((pct*100),2));

            }
            var cht = new BreakdownChart();
            var chtPerc = new BreakdownChart();
            cht.FullSize().ShowTags();
            cht.ShowTagValues();
            chtPerc.FullSize().ShowPercentage().ShowTags();
            int clr = 1;
            foreach (var kvp in statusCounts)
            {
                cht.AddItem(kvp.Key,kvp.Value,Color.FromInt32(clr));
                clr += 1;
            }
            clr = 1;
            foreach (var kvp in statusPercents)
            {
                chtPerc.AddItem(kvp.Key,kvp.Value,clr);
                clr +=1;
            }
            
            AnsiConsole.Write(new Panel(cht).Expand().Header("ISSUE STATUS BREAKDOWN (COUNT)",Justify.Center));
            AnsiConsole.Write(new Panel(chtPerc).Expand().Header("ISSUE STATUS BREAKDOWN (PERCENT)",Justify.Center));

            if (showDetail)
            {
                var tbl = new Table();
                tbl.AddColumns("KEY","TYPE", "STATUS", "SUMMARY");
                tbl.Expand();                
                foreach (var issue in issues)
                {
                    tbl.AddRow(issue.Key.Value,issue.Type.Name,issue.Status.Name,Markup.Escape(issue.Summary));
                }
                AnsiConsole.Write(new Panel(tbl).Expand().Header("ISSUE STATUS BREAKDOWN - DETAIL DATA",Justify.Center));
            }
            
            if (showDetail == false &&  ConsoleUtil.Confirm($"[bold]Show Data?[/]",false,true))
            {
                BuildStatusBreakdown(clearScreen,true);
                return;
            }

        }

        private void GetIssues()
        {
            var p = new ManagedPipeline();
            p.Add("Populating issues",GetData);
            if (SearchType==AnalysisType.atEpics)
            {
                p.Add("Finding issues linked to Epics", PopulateEpicLinks);
            }
            p.Add("Populating change logs",PopulateChangeLogs);
            try 
            {
                p.ExecutePipeline();
            }
            catch(Exception errEx) 
            {
                p.CancelPipeline();
                p = null;
                ConsoleUtil.WriteError($"An error occurred processing JQL: {searchJQL} ({errEx.Message}) ");
                ConsoleUtil.PressAnyKeyToContinue("OPERATION CANCELLED");
                return;
            }
            // if (_jIssues.Count > 0)
            // // if (!PopulateIssues()) return;
            // // if (!PopulateChangeLogs()) return;
            // {
            //     if (ConsoleUtil.Confirm("Show results on screen? (To export only, enter 'n')",true))
            //     {
            //         Render();                
            //     }
            //     else 
            //     {
            //         doExport = true;
            //     }
            //     if (ConsoleUtil.Confirm("Export to csv file?",doExport))
            //     {
            //         Export();
            //     }
            // }            
        }

        private void GetData()
        {
            try 
            {
                issues.Clear();
                if (string.IsNullOrWhiteSpace(searchJQL))
                {
                    return;
                }
                issues = JiraUtil.JiraRepo.GetIssues(searchJQL);
            }
            catch(Exception ex)
            {
                ConsoleUtil.WriteError(string.Format("Error getting issues using search: {0}",searchJQL),ex:ex);
            }
        }

        private void PopulateEpicLinks()
        {
            if (issues.Any(x=>x.Type.StringsMatch("epic")))
            {
                IEnumerable<Issue> epics = issues.Where(x=>x.Type.StringsMatch("epic")).ToList();
                IEnumerable<Issue> epicIssues = JiraUtil.JiraRepo.GetEpicIssues(epics);
                var newList = issues.Concat(epicIssues)
                    .GroupBy(x=>x.Key.Value)
                    .Where(x=>x.Count() == 1)
                    .Select(x=>x.First())
                    .ToList();    
                issues = newList;
            }

        }

        private void PopulateChangeLogs()
        {
            if (issues.Count == 0)
            {
                return;
            }
            foreach (var tIss in issues )            
            {
                var jIss = new JIssue(tIss);
                jIss.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(tIss));
                jIssues.Add(jIss);
            }

        }
        private void Summarize()
        {
            //https://graphiant.atlassian.net/rest/api/3/search?jql=project=WWT&fields=issueType,status,key,priority,flagged&expand=names
            //var issues = JiraUtil.JiraRepo.GetIssues
        }
    }
}