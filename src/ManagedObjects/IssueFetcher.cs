using System.Globalization;
using System.Diagnostics;
using System;
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
            var usedCache = false;
            if (options.CacheResults)
            {
                if (string.IsNullOrWhiteSpace(options.CacheResultsDesc))
                {
                    options.CacheResultsDesc=$"(cached on: {DateTime.Now})";
                }
            }
            if (options.AllowCachedSelection && _cachedData.Count > 0)
            {
                
            }

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


        private static void pdb(Issue iss)
        {
            
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
    }


}