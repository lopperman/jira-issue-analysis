using System.Diagnostics;
using Atlassian.Jira;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Spectre.Console;

namespace JTIS.Data
{
    

    public class jtisIssues 
    {
        private List<JIssue>? _jIssList = null;
        private List<string>  _jqlList = new List<string>();
        private List<Issue> _issues = new List<Issue>();
        private List<JIssue> _jIssues 
        {
            get 
            {
                if (_jIssList ==null){
                    _jIssList = new List<JIssue>();
                }
                return _jIssList;                
            }
        }

        private JiraRepo? repo = null;
        private Jira jira
        {
            get
            {
                return repo.jira;
            }
        }
        private SortedList<string,IEnumerable<IssueChangeLog>> _changeLogs = new SortedList<string, IEnumerable<IssueChangeLog>>();

        public static jtisIssues Create(JiraRepo jRepo)
        {
            var instance = new jtisIssues();
            instance.repo = jRepo;
            return instance;
        }

        // private async void AddChangeLogs(string issueKey, IEnumerable<IssueChangeLog> logs)
        // {
        //     await Task.Run()
        //      _changeLogs.Add(issueKey,logs);
        //     // await Task.WhenAll(logs.Select(async log => 
        //     // {
        //     //     await _changeLogs.Add(issueKey, logs);
                
        //     // }))
        // }

        private void test()
        {
            // var j = new JIssue();
            // j.AddChangeLogs
            //repo.GetIssueChangeLogs
        }

        internal IEnumerable<JIssue> GetIssues2(string jql)
        {
            var sw = Stopwatch.StartNew();
            var issues = GetIssuesAsync(jql).GetAwaiter().GetResult();
            List<Task> clTasks = new List<Task>();
            foreach (var iss in issues)
            {
                clTasks.Add(GetIssueChangeLogsAsync2(iss));
            }
            Task.WhenAll(clTasks);
            // while (clTasks.Any())
            // {
            //     Task finishedTask = await Task.WhenAny(clTasks);
            //     clTasks.Remove(finishedTask);
            //     await finishedTask;
            // }

            return _jIssues;
        }
        internal IEnumerable<JIssue> GetIssues(IEnumerable<string> jqlQueries, bool includeChangeLogs = true)
        {
            _issues.AddRange(GetIssuesAsync(jqlQueries.ToList()[0]).GetAwaiter().GetResult());


            Task.WhenAll(AnsiConsole.Progress()
                .AutoClear(true)
                .HideCompleted(true)                
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new SpinnerColumn(),
                    new ElapsedTimeColumn(),
                })                
                .StartAsync( async ctx => 
                {
                    await Task.WhenAny(_issues.Select(async item => {
                        var progTaskStg = new ProgressTaskSettings();
                        progTaskStg.AutoStart = false;
                        var pTask = ctx.AddTask($"cl for {item.Key.Value}",progTaskStg);
                        await Task.WhenAll(GetIssueChangeLogsAsync(item.Key.Value,pTask));
                        
                    }
                    
                    ));                    
                }));     

            foreach (var iss in _issues)
            {
                var jIss = new JIssue(iss);
                if (_changeLogs.Any(x=>x.Key==jIss.Key))
                {
                    var clvals = _changeLogs.Where(x=>x.Key==jIss.Key).ToList();
                    for (int i = 0; i < clvals.Count;i++)
                    {
                        jIss.AddChangeLogs(clvals[i].Value);
                    }
                }
                _jIssues.Add(jIss);
            }
            return _jIssues;
        }



        private async Task GetIssueChangeLogsAsync2(Issue Iss,  CancellationToken token = default(CancellationToken))
        {
            // List<IssueChangeLog> result = new List<IssueChangeLog>();
            var jIss = new JIssue(Iss);
            int incr = 0;
            int total = 0;
            do
            {
                var resourceUrl = $"rest/api/3/issue/{Iss.Key.Value}/changelog?maxResults=100&startAt={incr}";
                var serializerSettings = repo.jira.RestClient.Settings.JsonSerializerSettings;
                var webTask = jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token);
                var response = await  webTask;                
                JToken changeLogs = response["values"];
                JToken totalChangeLogs = response["total"];
                if (totalChangeLogs != null)
                {
                    total = JsonConvert.DeserializeObject<Int32>(totalChangeLogs.ToString(), serializerSettings);
                }
                if (changeLogs != null)
                {
                    var items = changeLogs.Select(cl => 
                        JsonConvert.DeserializeObject<IssueChangeLog>(cl.ToString(), serializerSettings));
                    incr += items.Count();
                    jIss.AddChangeLogs(items);
                }
            }
            while (incr < total);

            _jIssues.Add(jIss);
        }        

        private async Task GetIssueChangeLogsAsync(string issueKey, ProgressTask? progress = null,  CancellationToken token = default(CancellationToken))
        {
//            List<IssueChangeLog> result = new List<IssueChangeLog>();

            // int incr = 0;
            // int total = 0;

            if (progress != null)
            {
                progress.StartTask();
                progress.MaxValue=3;
                progress.Increment(1);
            }

            var rslt = await Task.WhenAll(jira.Issues.GetChangeLogsAsync(issueKey,token));
            progress.Increment(1);
            _changeLogs.Add(issueKey, (IEnumerable<IssueChangeLog>)rslt);
            progress.Increment(1);

            // do
            // {
            //     var resourceUrl = $"rest/api/3/issue/{issueKey}/changelog?maxResults=100&startAt={incr}";
            //     var serializerSettings = repo.jira.RestClient.Settings.JsonSerializerSettings;
            //     var response =  await jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token).ConfigureAwait(true);
                

            //     JToken changeLogs = response["values"];
            //     JToken totalChangeLogs = response["total"];
            //     if (totalChangeLogs != null)
            //     {
            //         total = JsonConvert.DeserializeObject<Int32>(totalChangeLogs.ToString(), serializerSettings);
            //         if (progress != null)
            //         {
            //             if (progress.IsStarted==false)
            //             {
            //                 progress.Description = $"Loading ({total}) Change Logs for {issueKey}";                        
            //                 progress.MaxValue = total;
            //                 progress.StartTask();
            //             }
            //             progress.Increment(changeLogs.Count());                    
            //             Thread.Sleep(1000);
            //         }   
            //     }

            //     if (changeLogs != null)
            //     {
            //         var items = changeLogs.Select(cl => 
            //             JsonConvert.DeserializeObject<IssueChangeLog>(cl.ToString(), serializerSettings));

            //         incr += items.Count();

            //         result.AddRange(items);
            //     }
            // }
            // while (incr < total);
            // _changeLogs.Add(issueKey,result);
            
            //progress.StopTask();
//            return result;
        }

        private async Task<IEnumerable<Issue>> GetIssuesAsync(string jql, ProgressTask? progress=null)
        {
            if (progress == null)
            {
                return await repo.jira.Issues.GetIssuesFromJqlAsync(jql);
            }            
            else 
            {
                progress.StartTask();
                var issueTask = repo.jira.Issues.GetIssuesFromJqlAsync(jql).GetAwaiter().GetResult();
                progress.StopTask();
                return issueTask;
            }
            // var resp = await(Task.WhenAll(issueTask.Result.ToList());

            // var resp = await Task<IEnumerable<Issue>>.WhenAll<IEnumerable<Issue>>(repo.jira.Issues.GetIssuesFromJqlAsync(jql));
            // progress.StopTask();
            // return resp.ToList<Issue>();
        }    

    }
}