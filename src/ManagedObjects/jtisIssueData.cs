using System.Collections.Generic;
using System.Diagnostics;
using Atlassian.Jira;
using JTIS.Extensions;
using Spectre.Console;

namespace JTIS.Data
{
    public class jtisIssueData
    {
        Stopwatch? sw = null;
        private int _totReturnCount;
        private int _nextStart;
        private JiraRepo? repo = null;

        public FetchOptions? fetchOptions = null;
        public SortedList<string,TimeSpan> Performance = new SortedList<string, TimeSpan>();
        private SortedList<string,jtisIssue> _jtisIssues = new SortedList<string, jtisIssue>();
        private int duplicateCount = 0;

        public static jtisIssueData Create(JiraRepo jRepo)
        {
            var instance = new jtisIssueData();
            instance.repo = jRepo;            
            return instance;
        }        

        public SortedDictionary<string,int> IssueTypesCount
        {
            get{
                SortedDictionary<string,int> response = new SortedDictionary<string,int>();
                foreach (var issType in jtisIssuesList.Select(x=>x.jIssue.IssueType).Distinct<string>())
                {
                    response.Add(issType, jtisIssuesList.Count(x=>x.jIssue.IssueType.StringsMatch(issType)));
                }
                return response;
            }

        }
        public int jtisIssueCount
        {
            get{
                return _jtisIssues.Count();
            }
        }
        public int EpicCount
        {
            get{
                return _jtisIssues.Where(x=>x.Value.jIssue.IssueType.StringsMatch("epic")).Count();
            }
        }



        public void  AddExternalJtisIssues(IEnumerable<jtisIssue> issues)
        {
            foreach (var jtisIss in issues)
            {
                if (_jtisIssues.ContainsKey(jtisIss.jIssue.Key)==false)
                {
                    _jtisIssues.Add(jtisIss.jIssue.Key,jtisIss);
                }
            }
        }

        public List<jtisIssue> jtisIssuesList 
        {
            get{
                return _jtisIssues.Values.ToList();
            }
        }
        private jtisIssue AddjtisIssue(Issue issue)
        {
            if (_jtisIssues.ContainsKey(issue.Key.Value)==false)
            {
                _jtisIssues.Add(issue.Key.Value,new jtisIssue(issue));
            }
            else 
            {
                duplicateCount +=1;
            }
            return _jtisIssues.Single(x=>x.Key==issue.Key.Value).Value;
        }
        private void AddjtisIssueChangeLogs(Issue issue, IEnumerable<IssueChangeLog> logs)
        {
            AddjtisIssue(issue).AddChangeLogs(logs);
        }

        public List<jtisIssue> GetIssuesWithChangeLogs(FetchOptions options)
        {            
            fetchOptions = options;
            var jql = options.JQL;


            _totReturnCount = repo.GetJQLResultsCount(jql);
            sw = Stopwatch.StartNew();                        

            AnsiConsole.Progress()
                .AutoClear(true)
                .HideCompleted(false)
                .Columns(
                    new ProgressColumn[]{
                        new TaskDescriptionColumn(), 
                        //new ProgressBarColumn(), 
                        new SpinnerColumn(), 
                        new ElapsedTimeColumn()
                    })
                .Start(ctx=> {
                    
                    while (_jtisIssues.Count() < _totReturnCount)
                    {
                        if (_jtisIssues.Count()==0){
                            _nextStart = 0;
                        }
                        else{ 
                            _nextStart += 100;                        
                        }
                        var task = ctx.AddTask($"(startAt: {_nextStart}) async issues and change logs search");
                        Task.WaitAll(IssuesChangeLogs(jql,options.IncludeChangeLogs));
                    }

                });
            sw.Stop();
            var totIssues = _jtisIssues.Count();
            var totCL = _jtisIssues.Values.SelectMany(x=>x.ChangeLogs).Count();
            var totCLI = _jtisIssues.Values.SelectMany(s=>s.ChangeLogs.SelectMany(y=>y.Items)).Count();
            Performance.Add($"(Overall Total Time) Issues: {totIssues}, Change Logs: {totCL}, Change Log Items: {totCLI}",sw.Elapsed);
            return _jtisIssues.Values.ToList();            
        }
        private async Task IssuesChangeLogs(string jql, bool includeChangeLogs = true)
        {            
            var iss = await Task<IPagedQueryResult<Issue>>.WhenAny(IssuesAsync(jql));
            List<Task> clTasks = new List<Task>();
            foreach (var i in iss.Result)
            {  
                AddjtisIssue(i);
                if (includeChangeLogs)
                {
                    Task clResult = GetChangeLogs(i);
                    clTasks.Add(clResult);
                }
            }

            if (includeChangeLogs)
            {
                while (clTasks.Any())
                {
                    var clTask = await Task.WhenAny(clTasks);
                    await clTask;
                    clTasks.Remove(clTask);
                }
            }
        }

        private async Task<IPagedQueryResult<Issue>> IssuesAsync(string jql)
        {
            return await repo.jira.Issues.GetIssuesFromJqlAsync(jql,startAt:_nextStart);
        }

        private async Task GetChangeLogs(Issue issue)
        {   
            // AnsiConsole.WriteLine($"GetChangeLogs first line for {issue.Key.Value}");         
            Task<IEnumerable<IssueChangeLog>>? response = repo.jira.Issues.GetChangeLogsAsync(issue.Key.Value);
            await response;
            if (response != null && response.Result != null && response.Result.Count() > 0)
            {
                // AnsiConsole.WriteLine($"GetChangeLogs for {issue.Key.Value}, adding {response.Result.Count()} change logs");
                AddjtisIssueChangeLogs(issue,response.Result);
            }
        }        
    }
}