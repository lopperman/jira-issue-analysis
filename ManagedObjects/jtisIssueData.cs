using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Atlassian.Jira;
using JTIS.Console;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Spectre.Console;

namespace JTIS.Data
{    

    public class jtisIssue
    {
        public Issue issue {get;private set;}

        public List<IssueChangeLog> ChangeLogs {get; private set;}

        private JIssue? _jIssue = null;
        public JIssue jIssue 
        {
            get{
                if (_jIssue == null){
                    _jIssue = new JIssue(issue);
                }
                return _jIssue;
            }
        }

        public jtisIssue(Issue iss)
        {
            issue = iss;
            ChangeLogs = new List<IssueChangeLog>();
        }

        public jtisIssue AddChangeLogs(IEnumerable<IssueChangeLog>? changeLogs)
        {
            if (changeLogs != null && changeLogs.Count() > 0)
            {
                ChangeLogs.AddRange(changeLogs);
            }
            return this;
        }
    }

    public class jtisIssueData
    {
        
        private JiraRepo? repo = null;
        private List<Issue> _issues = new List<Issue>();

        private List<JIssue> _jIssues = new List<JIssue>();

        private SortedDictionary<string,List<IssueChangeLog>> _changeLogs = new SortedDictionary<string, List<IssueChangeLog>>();

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

        private void AddChangeLogs(string issueKey, IEnumerable<IssueChangeLog> logs)
        {
            if (_changeLogs.ContainsKey(issueKey)==false)
            {
                _changeLogs.Add(issueKey,new List<IssueChangeLog>());
            }
            _changeLogs.Single(x=>x.Key==issueKey).Value.AddRange(logs);
        }

        private Jira jira
        {
            get
            {
                return repo.jira;
            }
        }
        public static jtisIssueData Create(JiraRepo jRepo)
        {
            var instance = new jtisIssueData();
            instance.repo = jRepo;
            return instance;
        }        

        public List<jtisIssue> Test()
        {
            // Task.WhenAll(Test1());
            var sw = Stopwatch.StartNew();
            Task.WaitAll(Test1());
            sw.Stop();
            AnsiConsole.WriteLine($"Time: {sw.Elapsed.TotalSeconds} seconds");
            int totalCL = 0;
            int totalCLI = 0;
            foreach (var jti in _jtisIssues.Values)
            {
                var changeLogCount = jti.ChangeLogs.Count();
                var changeLogItemCount = jti.ChangeLogs.SelectMany(x=>x.Items).Count();
                totalCL += changeLogCount;
                totalCLI += changeLogItemCount;
                AnsiConsole.WriteLine($"jtisIssue: {jti.issue.Key.Value}, change logs: {changeLogCount}, change log items: {changeLogItemCount}");
            }

            ConsoleUtil.PressAnyKeyToContinue($"CLCount: {totalCL}, CLICount: {totalCLI}");
            return _jtisIssues.Values.ToList();
        }
        public async Task Test1()
        {            
            var jql = "project=WWT and status='backlog'";
            var iss = await Task<IPagedQueryResult<Issue>>.WhenAny(Test2(jql));
            int issueCount = 0;
            //int totChangeLogs = 0;
            // List<Task<IEnumerable<IssueChangeLog>>> clTasks = new List<Task<IEnumerable<IssueChangeLog>>>();
            List<Task> clTasks = new List<Task>();
            foreach (var i in iss.Result)
            {  
                AddjtisIssue(i);
                issueCount += 1;
                AnsiConsole.WriteLine($"{issueCount} - Issue: {i.Key.Value}");
                Task clResult = GetChangeLogs(i);
                // Task<IEnumerable<IssueChangeLog>> clResult = Test3(i);
                clTasks.Add(clResult);
                // var clresult = await clResult;
                // AddChangeLogs(i.Key.Value,clresult);
                // totChangeLogs += clresult.Count();


//                clTasks.Add(Test3(i));
//                var cl = await Task<IEnumerable<IssueChangeLog>>.WhenAny(Test3(i));
                // foreach (var ch in cl.Result)
                // {
                //     AnsiConsole.WriteLine($"{i.Key.Value} ChangeLog id: {ch.Id}, has {ch.Items.Count()} items");
                // }
            }
            while (clTasks.Any())
            {
                var clTask = await Task.WhenAny(clTasks);
                await clTask;
                clTasks.Remove(clTask);
                // var clTask = await Task<IEnumerable<IssueChangeLog>>.WhenAny(clTasks);
//??                await clTask;
                // totChangeLogs += clTask.Result.Count();
                // AnsiConsole.WriteLine($"received cl task count: {clTask.Result.Count()}");
                // clTasks.Remove(clTask);
            }
            if (clTasks.Count==0)
            {
                AnsiConsole.WriteLine($"Total jtisIssues: {_jtisIssues.Count()}");
            }
            // AnsiConsole.WriteLine($"Total Change Logs: {totChangeLogs}");


        }


        public async Task<IPagedQueryResult<Issue>> Test2(string jql)
        {
            return await repo.jira.Issues.GetIssuesFromJqlAsync(jql);
        }
        public async Task<IEnumerable<IssueChangeLog>>? Test3(Issue issue)
        {   
            AnsiConsole.WriteLine($"Test3 first line for {issue.Key.Value}");         
            Task<IEnumerable<IssueChangeLog>> response = repo.jira.Issues.GetChangeLogsAsync(issue.Key.Value);
            await response;
            AnsiConsole.WriteLine($"Test3 for {issue.Key.Value}, returning {response.Result.Count()} change logs");
            return response.Result;
            // return await repo.jira.Issues.GetChangeLogsAsync(issue.Key.Value);
        }

        public async Task<IEnumerable<IssueChangeLog>>? Test4(Issue issue)
        {   
            AnsiConsole.WriteLine($"Test3 first line for {issue.Key.Value}");         
            Task<IEnumerable<IssueChangeLog>> response = repo.jira.Issues.GetChangeLogsAsync(issue.Key.Value);
            await response;
            AnsiConsole.WriteLine($"Test3 for {issue.Key.Value}, returning {response.Result.Count()} change logs");
            return response.Result;
            // return await repo.jira.Issues.GetChangeLogsAsync(issue.Key.Value);
        }

        public async Task GetChangeLogs(Issue issue)
        {   
            AnsiConsole.WriteLine($"GetChangeLogs first line for {issue.Key.Value}");         
            Task<IEnumerable<IssueChangeLog>>? response = repo.jira.Issues.GetChangeLogsAsync(issue.Key.Value);
            await response;
            if (response != null && response.Result != null && response.Result.Count() > 0)
            {
                AnsiConsole.WriteLine($"GetChangeLogs for {issue.Key.Value}, adding {response.Result.Count()} change logs");
                AddjtisIssueChangeLogs(issue,response.Result);
            }
            // return response.Result;
            // return await repo.jira.Issues.GetChangeLogsAsync(issue.Key.Value);
        }



        // public async Task GetIssues(string jql)
        // {
        //     jira.Issues.GetIssuesFromJqlAsync()
        //     repo.GetIssues
        // }

    }
}