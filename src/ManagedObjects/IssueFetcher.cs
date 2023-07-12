using System.Globalization;
using System.Diagnostics;
using JTIS.Console;
using Spectre.Console;
using JTIS.Extensions;

namespace JTIS.Data
{



    public class FetchOptions
    {
        public bool AllowJQLSnippets {get;set;}
        public bool AllowManualJQL {get;set;}
        public bool AllowCachedSelection {get;set;}
        public bool IncludeChangeLogs {get;set;}
        public bool CacheResults {get;set;}
        public bool FetchEpicChildren {get;set;}
        public string? CacheResultsDesc {get;set;}
        public string? JQL {get;set;}

        public FetchOptions()
        {
            //set defaults
            AllowJQLSnippets = true;
            AllowManualJQL = true;
            AllowCachedSelection = false;
            IncludeChangeLogs = true;
            CacheResults = false;       
            FetchEpicChildren = false;     

        }
        public static FetchOptions DefaultFetchOptions
        {
            get
            {
                var result = new FetchOptions();
                return result;
            }
        }

    }
    public static class IssueFetcher
    {
        private static  SortedList<string,jtisIssueData> _cachedData = new SortedList<string, jtisIssueData>();

        public static jtisIssueData? FetchIssues(FetchOptions? options)
        {
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

        private static List<jtisIssue>? PopulateEpicLinks(List<jtisIssue> epics, FetchOptions options)
        {
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