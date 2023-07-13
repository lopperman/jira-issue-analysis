using System.Globalization;
using System.Diagnostics;
using JTIS.Console;
using Spectre.Console;
using JTIS.Extensions;
using Atlassian.Jira;

namespace JTIS.Data
{
    public static class IssueFetcher
    {
        private static  SortedList<string,jtisIssueData> _cachedData = new SortedList<string, jtisIssueData>();

        public static jtisIssueData? FetchIssues(FetchOptions? options)
        {
            if (options.CacheResults)
            {
                if (string.IsNullOrWhiteSpace(options.CacheResultsDesc))
                {
                    options.CacheResultsDesc=$"(cached on: {DateTime.Now})";
                }
            }
            var usedCache = false;

            // bool handled = false;
            jtisIssueData? response = null;
            // if (options==null){options = FetchOptions.DefaultFetchOptions;}
            // if (options.AllowCachedSelection && _cachedData.Any())
            // {
            //     handled = SelectCachedResults();
            // }
            // if (!handled && options.JQL != null)
            // {
                response = ProcessJQL(options);                                
            // }
            // if (!handled)
            // {
            //     handled = ProcessJQL(options);
            // }
            if (options.CacheResults && usedCache==false)
            {
                _cachedData.Add(options.CacheResultsDesc,response);
            }
            return response;
        }

        //TODO: IMPLEMENT
        private static jtisIssueData? ProcessJQL(FetchOptions options)
        {
            var jql = string.Empty;
            jql = ConsoleInput.GetJQLOrIssueKeys(true);
            if (jql.Length == 0)
            {
                return null;
            }
            var jtisData = jtisIssueData.Create(JiraUtil.JiraRepo);         
            jtisData.GetIssuesWithChangeLogs(jql,options.IncludeChangeLogs);
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


        private static void pdb(Issue iss)
        {
            
        }
        private static List<jtisIssue>? PopulateEpicLinks(List<jtisIssue> epics, FetchOptions options)
        {

            //FOR COMPANY MANAGED JIRA CLOUD, GET CHILDREN OF EPIC USING:
            //project=CSSK and "epic link" in (CSSK-85, ETC, ETC)



            if (epics.Count() > 0)
            {
                AnsiConsole.MarkupLine($"getting linked issues for [bold]{epics.Count} epics[/]");
                var epicKeys = epics.Select(x=>x.issue.Key.Value).ToArray();
                var jql = JQLBuilder.BuildJQLForFindEpicIssues(epicKeys);
                var jtisData = jtisIssueData.Create(JiraUtil.JiraRepo);         
                var children = jtisData.GetIssuesWithChangeLogs(jql,options.IncludeChangeLogs);
                if (children.Count > 0)
                {
                    ConsoleUtil.WritePerfData(jtisData.Performance);
                    return children;
                }
            }
            return null;
        }
    }


}