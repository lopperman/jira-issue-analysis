using JTIS.Console;
using Spectre.Console;
using JTIS.Extensions;
using JTIS.Config;

namespace JTIS.Data
{
    public static class IssueFetcher
    {
        private static  List<jtisIssueData> _cachedData = new List<jtisIssueData>();

        public static void ClearCachedData()
        {
            if (ConsoleUtil.Confirm("Clear all cached issues search results?",true))
            {
                _cachedData.Clear();
            }
        }

        public static IReadOnlyList<jtisIssueData> CachedDataList
        {
            get 
            {
                return _cachedData;
            }
        }

        public static jtisIssueData? FetchIssues(FetchOptions options)
        {
            if (options.RequiredIssueStatusSequence && CfgManager.config.ValidIssueStatusSequence == false)
            {
                options.Cancelled = true;
                ConsoleUtil.WriteError("The feature requested requires all issue statuses to have a sequence order.  Please use the next screen to update issue status sequences.",pause:true);
                IssueStatesUtil.EditIssueSequence(true);
                return null;
            }

            options.JQL = GetJQL(options);
            if (options.JQL.Length==0)
            {
                return null;
            }

            if (options.AllowCachedSelection && _cachedData.Count > 0)
            {
                var cachedData = _cachedData.SingleOrDefault(
                    x=>x.fetchOptions.JQL.StringsMatch(options.JQL) && 
                    x.fetchOptions.FetchEpicChildren.Equals(options.FetchEpicChildren) && 
                    x.fetchOptions.IncludeChangeLogs.Equals(options.IncludeChangeLogs));
                
                if (cachedData != null)
                {                    
                    AnsiConsole.Write(new Rule($"[blue on cornsilk1]USING CACHED SEARCH RESULTS[/]").Border(BoxBorder.Heavy));
                    AnsiConsole.MarkupLine($"[bold]Using recent cached copy of the JQL search below[/].{Environment.NewLine}[dim]Cached search result are cleared when exiting app, or can be cleared in the Configuration Menu area[/]{Environment.NewLine}[italic]JQL: {cachedData.fetchOptions.JQL}[/]");
                    if (ConsoleUtil.Confirm("Continue and use cached results? (Answering no ('n') will replace cached results with new search)",true))
                    {
                        return cachedData;
                    }
                    else 
                    {
                        options.CacheResults(true);
                        options.CacheResultsDesc=$"(cached on: {DateTime.Now})";
                        _cachedData.Remove(cachedData);
                    }
                }
            }

            jtisIssueData? response = null;
            response = ProcessJQL(options);                                
            if (options.CacheResults)
            {
                _cachedData.Add(response);
            }
            if (options.IncludeChangeLogs)
            {
                foreach (var iss in response.jtisIssuesList)
                {
                    iss.BuildBlockers();
                }
            }
            return response;
        }

        private static string GetJQL(FetchOptions options)
        {
            var tmpJQL = string.Empty;
            if (options.JQL.Length==0)
            {
                tmpJQL=ConsoleInput.GetJQLOrIssueKeys(options.AllowJQLSnippets,options.FetchEpicChildren);
            }
            else 
            {
                tmpJQL=options.JQL;
            }
            return tmpJQL;
        }

        private static jtisIssueData? ProcessJQL(FetchOptions options)
        {
            var jql = string.Empty;
            if (options.JQL.Length ==0)
            {
                jql = ConsoleInput.GetJQLOrIssueKeys(true);
                options.JQL = jql;
            }
            else 
            {
                jql = options.JQL;
            }
            if (jql.Length == 0)
            {
                return null;
            }
            var jtisData = jtisIssueData.Create(JiraUtil.JiraRepo);         
            jtisData.GetIssuesWithChangeLogs(options);
            ConsoleUtil.WritePerfData(jtisData.Performance);
            if (options.FetchEpicChildren)
            {
                if (jtisData.EpicCount > 0)                
                {
                    var epics = jtisData.jtisIssuesList.Where(x=>x.jIssue.IssueType.StringsMatch("epic")).ToList();
                    var children = PopulateEpicLinks(epics, options);
                    if (children != null) 
                    {
                        jtisData.AddExternalJtisIssues(children);
                    }                    
                }
            }
            return jtisData;
        }

        private static List<jtisIssue>? PopulateEpicLinks(List<jtisIssue> epics, FetchOptions options)
        {
            var optionsClone = FetchOptions.Clone(options);
            //FOR COMPANY MANAGED JIRA CLOUD, GET CHILDREN OF EPIC USING:
            //  project=CSSK and "epic link" in (CSSK-85, ETC, ETC)
            //FOR TEAM MANAGER JIRA CLOUD, USE:
            //  project=CSSK AND parent in (EPIC KEY, EPIC KEY, ETC)



            if (epics.Count() > 0)
            {
                AnsiConsole.MarkupLine($"getting linked issues for [bold]{epics.Count} epics[/]");

                optionsClone.JQL = JQLBuilder.BuildJQLForFindEpicIssues(epics.Select(x=>x.issue.Key.Value).ToArray());

                var jtisData = jtisIssueData.Create(JiraUtil.JiraRepo);
                var children = jtisData.GetIssuesWithChangeLogs(optionsClone);
                if (children.Count > 0)
                {
                    ConsoleUtil.WritePerfData(jtisData.Performance);
                    return children;
                }
            }
            return null;
        }

        internal static void DisplayCachedResults()
        {
            foreach (var cacheItem in _cachedData)
            {
                var info = new Markup($"[bold]Cached Items: {cacheItem.jtisIssueCount}, Include Change Logs: {cacheItem.fetchOptions.IncludeChangeLogs}, Find Epic Children: {cacheItem.fetchOptions.FetchEpicChildren}[/]{Environment.NewLine}[dim]JQL: {cacheItem.fetchOptions.JQL}[/]");
                var p = new Panel(info);
                p.Header(cacheItem.fetchOptions.CacheResultsDesc);
                p.Border(BoxBorder.Rounded);
                AnsiConsole.Write(p);
            }
            ConsoleUtil.PressAnyKeyToContinue();
        }
    }


}