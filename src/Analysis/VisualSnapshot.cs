using Atlassian.Jira;
using JTIS.Analysis;
using JTIS.Console;
using JTIS.Data;
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
        private List<jtisIssue> jtisIssues = new List<jtisIssue>();

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

        private void AddMissingKey(string key, ref SortedDictionary<string,double> dic)
        {
            if (dic.ContainsKey(key)==false)
            {
                dic.Add(key,0);
            }
        }
        private void BuildBlockedNonBlocked(bool clearScreen = true, bool showDetail = false)
        {
            if (jtisIssues.Count == 0){return;}

            SortedDictionary<string,double> chtCount = new SortedDictionary<string, double>();
            SortedDictionary<string,double> chtPerc = new SortedDictionary<string, double>();

            SortedDictionary<string,double> dict = new SortedDictionary<string, double>();
            foreach (var issue in jtisIssues)
            {
                var blocked = issue.jIssue.IsBlocked;
                string key1 = "";
                string key2 = "";

                key1 = "00-All IsBlocked";
                key2 = "00-All NotBlocked";
                AddMissingKey(key1, ref dict);
                AddMissingKey(key2, ref dict);
                string useKey = blocked ? key1 : key2;
                dict[useKey]+=1;

                key1 = $"01-Type {issue.issue.Type.Name} IsBlocked";
                key2 = $"01-Type {issue.issue.Type.Name} NotBlocked";
                AddMissingKey(key1, ref dict);
                AddMissingKey(key2, ref dict);
                useKey = blocked ? key1 : key2;
                dict[useKey]+=1;

                key1 = $"02-Status {issue.issue.Status.Name} IsBlocked";
                key2 = $"02-Status {issue.issue.Status.Name} NotBlocked";
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

            if (jtisIssues.Count == 0){return;}
            var statuses = jtisIssues.Select(x=>x.issue.Status.Name).ToList().Distinct();
            SortedDictionary<string,double> statusCounts = new SortedDictionary<string, double>();
            SortedDictionary<string,double> statusPercents = new SortedDictionary<string, double>();
            foreach (var tmpStatus in statuses)
            {   
                var pct = Math.Round((double)jtisIssues.Count(x=>x.issue.Status.Name.StringsMatch(tmpStatus)) / (double)jtisIssues.Count,2);
                statusCounts.Add(tmpStatus,jtisIssues.Count(x=>x.issue.Status.Name.StringsMatch(tmpStatus)));
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
                foreach (var issue in jtisIssues)
                {                    
                    tbl.AddRow(issue.issue.Key.Value,issue.issue.Type.Name,issue.issue.Status.Name,Markup.Escape(issue.issue.Summary));
                }
                AnsiConsole.Write(new Panel(tbl).Expand().Header("ISSUE STATUS BREAKDOWN - DETAIL DATA",Justify.Center));
            }
            
            if (showDetail == false &&  ConsoleUtil.Confirm($"[bold]Show Data?[/]",false,true))
            {
                BuildStatusBreakdown(clearScreen,true);
                return;
            }

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
        private void GetIssues()
        {
            var data = jtisIssueData.Create(JiraUtil.JiraRepo);
            jtisIssues.Clear();
            jtisIssues.AddRange(data.GetIssuesWithChangeLogs(searchJQL));
            ConsoleUtil.WritePerfData(data.Performance);
            if (SearchType==AnalysisType.atEpics)
            {
                PopulateEpicLinks();
            }

        }

        private void Summarize()
        {
            //https://graphiant.atlassian.net/rest/api/3/search?jql=project=WWT&fields=issueType,status,key,priority,flagged&expand=names
            //var issues = JiraUtil.JiraRepo.GetIssues
        }
    }
}