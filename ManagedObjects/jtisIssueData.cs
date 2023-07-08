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
    public class jtisIssueData
    {
        private JiraRepo? repo = null;
        private List<Issue> _issues = new List<Issue>();

        private List<JIssue> _jIssues = new List<JIssue>();
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

        public void Test()
        {
            // Task.WhenAll(Test1());
            var sw = Stopwatch.StartNew();
            Task.WaitAll(Test1());
            sw.Stop();
            AnsiConsole.WriteLine($"Time: {sw.Elapsed.TotalSeconds} seconds");

            ConsoleUtil.PressAnyKeyToContinue();
        }
        public async Task Test1()
        {            
            var jql = "project=WWT and status='backlog'";
            var iss = await Task<IPagedQueryResult<Issue>>.WhenAny(Test2(jql));
            int issueCount = 0;
            List<Task<IEnumerable<IssueChangeLog>>> clTasks = new List<Task<IEnumerable<IssueChangeLog>>>();
            foreach (var i in iss.Result)
            {
                issueCount += 1;
                AnsiConsole.WriteLine($"{issueCount} - Issue: {i.Key.Value}");
                _jIssues.Add(new JIssue(i));
                clTasks.Add(Test3(i));
//                var cl = await Task<IEnumerable<IssueChangeLog>>.WhenAny(Test3(i));
                // foreach (var ch in cl.Result)
                // {
                //     AnsiConsole.WriteLine($"{i.Key.Value} ChangeLog id: {ch.Id}, has {ch.Items.Count()} items");
                // }
            }
            int totChangeLogs = 0;
            while (clTasks.Any())
            {
                var clTask = await Task<IEnumerable<IssueChangeLog>>.WhenAny(clTasks);
                await clTask;
                totChangeLogs += clTask.Result.Count();
                AnsiConsole.WriteLine($"received cl task count: {clTask.Result.Count()}");
                clTasks.Remove(clTask);
            }
            if (clTasks.Count==0)
            {
                AnsiConsole.WriteLine($"Total Change Logs: {totChangeLogs}");
            }

        }

        public async Task CreateJIssue(Issue iss)
        {
            xx =>
            {
                JIssue j = new JIssue(iss);
                _jIssues.Add(j);
            }

        }
        public async Task<IPagedQueryResult<Issue>> Test2(string jql)
        {
            return await repo.jira.Issues.GetIssuesFromJqlAsync(jql);
        }
        public async Task<IEnumerable<IssueChangeLog>> Test3(Issue issue)
        {
            return await repo.jira.Issues.GetChangeLogsAsync(issue.Key.Value);
        }

        // public async Task GetIssues(string jql)
        // {
        //     jira.Issues.GetIssuesFromJqlAsync()
        //     repo.GetIssues
        // }

    }
}