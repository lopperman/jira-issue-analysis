using System.Diagnostics;
using Atlassian.Jira;
using JTIS.Console;
using Spectre.Console;

namespace JTIS.Data
{
    public class jtisIssueData
    {
        Stopwatch? sw = null;
        private int _totReturnCount;
        private int _nextStart;
        private JiraRepo? repo = null;
        public SortedList<string,TimeSpan> Performance = new SortedList<string, TimeSpan>();

        public static jtisIssueData Create(JiraRepo jRepo)
        {
            var instance = new jtisIssueData();
            instance.repo = jRepo;            
            return instance;
        }        
        private SortedList<string,jtisIssue> _jtisIssues = new SortedList<string, jtisIssue>();
        private jtisIssue AddjtisIssue(Issue issue)
        {
            if (_jtisIssues.ContainsKey(issue.Key.Value)==false)
            {
                _jtisIssues.Add(issue.Key.Value,new jtisIssue(issue));
            }
            return _jtisIssues.Single(x=>x.Key==issue.Key.Value).Value;
        }
        private void AddjtisIssueChangeLogs(Issue issue, IEnumerable<IssueChangeLog> logs)
        {
            AddjtisIssue(issue).AddChangeLogs(logs);
        }

        public List<jtisIssue> GetIssuesWithChangeLogs(string jql)
        {            
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
                        Task.WaitAll(IssuesWithChangeLogs(jql));
                    }

                });
            sw.Stop();
            var totIssues = _jtisIssues.Count();
            var totCL = _jtisIssues.Values.SelectMany(x=>x.ChangeLogs).Count();
            var totCLI = _jtisIssues.Values.SelectMany(s=>s.ChangeLogs.SelectMany(y=>y.Items)).Count();
            Performance.Add($"(Overall Total Time) Issues: {totIssues}, Change Logs: {totCL}, Change Log Items: {totCLI}",sw.Elapsed);
            return _jtisIssues.Values.ToList();            
        }
        private async Task IssuesWithChangeLogs(string jql)
        {            
            var iss = await Task<IPagedQueryResult<Issue>>.WhenAny(IssuesAsync(jql));
            List<Task> clTasks = new List<Task>();
            foreach (var i in iss.Result)
            {  
                AddjtisIssue(i);
                Task clResult = GetChangeLogs(i);
                clTasks.Add(clResult);
            }

            while (clTasks.Any())
            {
                var clTask = await Task.WhenAny(clTasks);
                await clTask;
                clTasks.Remove(clTask);
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