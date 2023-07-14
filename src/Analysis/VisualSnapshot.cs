using JTIS.Analysis;
using JTIS.Console;
using JTIS.Data;
using JTIS.Extensions;
using JTIS.Menu;
using Spectre.Console;

namespace JTIS
{
    internal class VisualSnapshot
    {
        jtisFilterItems<string> _issueTypeFilter = new jtisFilterItems<string>();
        public VisualSnapshotType SnapshotType {get;private set;}
        public AnalysisType SearchType {get;private set;}
        private FetchOptions fetchOptions = FetchOptions.DefaultFetchOptions;

        private jtisIssueData? _jtisIssueData = null;

        private VisualSnapshot(VisualSnapshotType snapshotType, AnalysisType analysisType)
        {
            SnapshotType = snapshotType;
            SearchType = analysisType;
        }

        public static VisualSnapshot Create(VisualSnapshotType snapshotType, AnalysisType  analysisType)
        {
            var vs = new VisualSnapshot(snapshotType, analysisType);
            return vs;
        }

        private void CheckIssueTypeFilter()
        {

            _issueTypeFilter.Clear();
            foreach (var kvp in _jtisIssueData.IssueTypesCount)
            {
                _issueTypeFilter.AddFilterItem(kvp.Key,$"Count: {kvp.Value}");
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

        public void Build()
        {
            fetchOptions.IncludeChangeLogs(false);
            if (SearchType == AnalysisType.atEpics){fetchOptions.FetchEpicChildren=true;}
            _jtisIssueData = IssueFetcher.FetchIssues(fetchOptions);

            if (_jtisIssueData != null && _jtisIssueData.jtisIssueCount > 0)
            {
                CheckIssueTypeFilter();
                BuildStatusBreakdown();
                ConsoleUtil.PressAnyKeyToContinue($"NEXT: BLOCKED/NONBLOCKED SUMMARY");
                BuildBlockedNonBlocked(false,false);
            }
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
            if (_jtisIssueData.jtisIssueCount == 0){return;}

            SortedDictionary<string,double> chtCount = new SortedDictionary<string, double>();
            SortedDictionary<string,double> chtPerc = new SortedDictionary<string, double>();
            SortedDictionary<string,double> dict = new SortedDictionary<string, double>();

            var filtered = _jtisIssueData.jtisIssuesList.Where(x=>_issueTypeFilter.IsFiltered(x.issue.Type.Name)).ToList();

            foreach (var issue in filtered)
            {
                var blocked = issue.jIssue.IsBlocked;
                string key1 = "";
                string key2 = "";

                key1 = $"00-[bold]ALL[/] Is [bold]Blocked[/]";
                key2 = $"00-[bold]ALL[/] NotBlocked";
                AddMissingKey(key1, ref dict);
                AddMissingKey(key2, ref dict);
                string useKey = blocked ? key1 : key2;
                dict[useKey]+=1;

                key1 = $"01-[dim]Type[/] [bold]{issue.issue.Type.Name.ToUpper()}[/] Is [bold]Blocked[/]";
                key2 = $"01-[dim]Type[/] [bold]{issue.issue.Type.Name.ToUpper()}[/] NotBlocked";
                AddMissingKey(key1, ref dict);
                AddMissingKey(key2, ref dict);
                useKey = blocked ? key1 : key2;
                dict[useKey]+=1;

                key1 = $"02-[dim]Status[/] [bold]{issue.issue.Status.Name.ToUpper()}[/] Is [bold]Blocked[/]";
                key2 = $"02-[dim]Status[/] [bold]{issue.issue.Status.Name.ToUpper()}[/] NotBlocked";
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


            if (showDetail == false &&  ConsoleUtil.Confirm($"[bold]Show Data?[/]",false))
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

            // int totalCount = filteredItems.Count;
            if (_jtisIssueData.jtisIssueCount == 0){return;}
            var filtered = _jtisIssueData.jtisIssuesList.Where(x=>_issueTypeFilter.IsFiltered(x.issue.Type.Name)).ToList();


            var statuses = filtered.Select(x=>x.issue.Status.Name).ToList().Distinct();
            SortedDictionary<string,double> statusCounts = new SortedDictionary<string, double>();
            SortedDictionary<string,double> statusPercents = new SortedDictionary<string, double>();
            foreach (var tmpStatus in statuses)
            {   
                var pct = (double)filtered.Count(x=>x.issue.Status.Name.StringsMatch(tmpStatus)) / (double)filtered.Count;
                statusCounts.Add(tmpStatus,filtered.Count(x=>x.issue.Status.Name.StringsMatch(tmpStatus)));
                statusPercents.Add(tmpStatus,pct);

            }
            var cht = new BreakdownChart();
            var chtPerc = new BreakdownChart();
            cht.FullSize().ShowTags();
            cht.ShowTagValues();
            chtPerc.FullSize().ShowPercentage().ShowTags();
            chtPerc.UseValueFormatter(x=>$"{x:0.00%}");
            chtPerc.HideTags();
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
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("ISSUE STATUS BREAKDOWN (COUNT/PERC)").Centered());
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Panel(cht).Expand().Header("ISSUE STATUS BREAKDOWN (COUNT)",Justify.Center).NoBorder());
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Panel(chtPerc).Expand().Header("ISSUE STATUS BREAKDOWN (PERCENT)",Justify.Center).NoBorder());
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("~~~").Centered());

            if (showDetail)
            {
                var tbl = new Table();
                tbl.AddColumns("KEY","TYPE", "STATUS", "SUMMARY");
                tbl.Expand();                
                foreach (var issue in filtered)
                {                    
                    tbl.AddRow(issue.issue.Key.Value,issue.issue.Type.Name,issue.issue.Status.Name,Markup.Escape(issue.issue.Summary));
                }
                AnsiConsole.Write(new Panel(tbl).Expand().Header("ISSUE STATUS BREAKDOWN - DETAIL DATA",Justify.Center));
            }
            
            if (showDetail == false &&  ConsoleUtil.Confirm($"[bold]Show Data?[/]",false))
            {
                BuildStatusBreakdown(clearScreen,true);
                return;
            }

        }


        private void Summarize()
        {
            //https://graphiant.atlassian.net/rest/api/3/search?jql=project=WWT&fields=issueType,status,key,priority,flagged&expand=names
            //var issues = JiraUtil.JiraRepo.GetIssues
        }
    }
}