using System.Net.Cache;
using System.Text.RegularExpressions;
using Atlassian.Jira;
using JTIS.Config;
using JTIS.Console;
using Spectre.Console;

namespace JTIS
{

    public class AsyncChangeLogs
    {
        public JiraRepo? repo {get;private set;}

        private AsyncChangeLogs()
        {
        }
        internal AsyncChangeLogs(JiraRepo repo)
        {
            this.repo = repo;
        }

        public async Task<IEnumerable<JIssue>> GetIssuesAsync(string jql)
        {
            List<JIssue> jissues = new List<JIssue>();

            //1ST AWAIT, BUT IT'S SUPER QUICK TO RUN            
            var issues = await GetInitialIssues(jql);

            //SORTING FOR SANITY -- IF THINGS ARE SET UP RIGHT, 
            //RESULT SHOULD NEVER FINISH IN THE SAME ORDER THEY WERE ADDED
            //(THIS MAKES IT EASY TO CHECK)
            var sortedIssues = issues.OrderBy(x=>x.Key.Value).AsEnumerable();

            //'WhenAll' -- BECAUSE THIS EXITS OUT ONCE IT'S DONE
            await Task.WhenAll(sortedIssues.Select( async jItem => 
                {
                    //THIS IS THE ONLY AWAIT FOR CHANGE LOGS, NEEDED IN ORDER
                    //TO ADD RETURNED RESULTS TO CORRECT JIRA ISSUE
                    var issChangeLogs = await PopulateChangeLogAsync(jItem);
                    var newJI = new JIssue(jItem);
                    newJI.AddChangeLogs(issChangeLogs);
                    System.Console.WriteLine($"{StaticSeconds}: completed change logs for " + newJI.Key);
                }));                
            return jissues;
        }
        internal async Task<List<IssueChangeLog>> PopulateChangeLogAsync(Issue issue)
        {
            return await  repo.GetChangeLogsAsync(issue.Key.Value);
        }
        internal async Task<IEnumerable<Issue>> GetInitialIssues(string jql)
        {
            var issues = await this.repo.jira.Issues.GetIssuesFromJqlAsync(jql);
            return issues;            
        }        

        // public async void RunTests()
        // {
        //     string jql1 = "key in (WWT-310, WWT-302, WWT-297, WWT-296, WWT-295, WWT-294, WWT-293, WWT-292, WWT-291)";            
        //     string jql2 = "project=wwt and updated >= -30d and issueType=story";

        //     // var startSync = DateTime.Now;
        //     // var jIssuesSync = RunTestSync(jql1);
        //     // var endSync = DateTime.Now;
        //     var startAsync = DateTime.Now;
        //     var jIssuesAsync = await  RunTest(jql2);
            
            
        //     var endAsync = DateTime.Now;
            


        //     AnsiConsole.Write(new Rule());
        //     AnsiConsole.MarkupLine($"[bold] TEST 1 [/][dim]JQL: {jql1}[/]");
        //     // AnsiConsole.MarkupLine($"[bold]Total Issues: {jIssuesAsync.Count()}[/]");
        //     var totalChangeLogs=jIssuesAsync.Sum(x=>x.ChangeLogs.Count);
        //     AnsiConsole.MarkupLine($"[bold]Total Change Logs: {totalChangeLogs}[/]");
        //     var asyncTime1 = endAsync.Subtract(startAsync).TotalMilliseconds;
        //     // var syncTime1 = endSync.Subtract(startSync).TotalMilliseconds;
        //     AnsiConsole.MarkupLine($"[bold]Execute ASYNC: {asyncTime1/1000:0.00000} seconds[/]");
            // AnsiConsole.MarkupLine($"[bold]Execute SYNC: {syncTime1/1000:0.00000} seconds[/]");


            // startSync = DateTime.Now;
            // AnsiConsole.WriteLine($"{StaticSeconds}: Starting Sync 2");
            // jIssuesSync = RunTestSync(jql2);
            // AnsiConsole.WriteLine($"{StaticSeconds}: End Sync 2");
            // endSync = DateTime.Now;
            // AnsiConsole.WriteLine($"{StaticSeconds}: Starting Async 2");
            // startAsync = DateTime.Now;
            // AnsiConsole.WriteLine($"{StaticSeconds}: End Async 2");
            // jIssuesAsync = RunTest(jql2).GetAwaiter().GetResult();
            // endAsync = DateTime.Now;

            // AnsiConsole.Write(new Rule());
            // AnsiConsole.MarkupLine($"[bold] TEST 2 [/][dim]JQL: {jql2}[/]");
            // AnsiConsole.MarkupLine($"[bold]Total Issues: {jIssuesAsync.Count()}[/]");
            // totalChangeLogs=jIssuesAsync.Sum(x=>x.ChangeLogs.Count);
            // AnsiConsole.MarkupLine($"[bold]Total Change Logs: {totalChangeLogs}[/]");
            // asyncTime1 = endAsync.Subtract(startAsync).TotalMilliseconds;
            // syncTime1 = endSync.Subtract(startSync).TotalMilliseconds;
            // AnsiConsole.MarkupLine($"[bold]Execute ASYNC: {asyncTime1/1000:0.00000} seconds[/]");
            // AnsiConsole.MarkupLine($"[bold]Execute SYNC: {syncTime1/1000:0.00000} seconds[/]");
        // }

        private List<JIssue> RunTestSync(string jql1)
        {
            List<JIssue> response = new List<JIssue>();
            var issues = repo.GetIssues(jql1);
            foreach(var iss in issues)
            {
                var jIss = new JIssue(iss);
                jIss.AddChangeLogs(repo.GetIssueChangeLogs(jIss.Key));
            }
            return response;
        }

        public static AsyncChangeLogs Create(JiraRepo repo)
        {
            return  new AsyncChangeLogs(repo); 
        }





        // internal async Task<JIssue> PopulateChangeLogAsync(Issue issue)
        // {
        //     JIssue ji = new JIssue(issue);
        //     IEnumerable<IssueChangeLog> changeLogs = await repo.GetChangeLogsAsync(ji.Key);
        //     ji.AddChangeLogs(changeLogs);
        //     // await SleepPlease();
        //     //System.Console.WriteLine($"{StaticSeconds}: Populated change logs for " + issue.Key.Value);
        //     return ji;
        // }

        // internal async Task SleepPlease()
        // {
        //     Thread.Sleep(500);
        // }
        internal string StaticSeconds
        {
            get{
            var data = (DateTime.Now.TimeOfDay.TotalMilliseconds/1000).ToString("00.0000 seconds");
            return data;

            }
            // var ms = DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond;
            // return Math.Round(ms/1000,6).ToString();
        }
    }
}