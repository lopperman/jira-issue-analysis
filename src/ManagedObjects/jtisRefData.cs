using Atlassian.Jira;
using JTIS.Config;

namespace JTIS.Data
{
    public class jtisRefData
    {
        private JiraRepo? repo = null;
        public Project? project {get;private set;}
        private List<CustomField> _customFieldsList = new List<CustomField>();
        private SortedDictionary<string,List<IssueType>> _projectIssueTypes = new SortedDictionary<string, List<IssueType>>();

        private SortedDictionary<string,List<CustomField>> _projectCustomFields = new SortedDictionary<string, List<CustomField>>();
        // public IReadOnlyList<Project> Projects {get{return _projectList;}}
        public IReadOnlyList<CustomField> CustomFields {get{return _customFieldsList;}}
        public IReadOnlyList<IssueType>? ProjectIssuesTypes(string prjKey)
        {
            if (_projectIssueTypes.ContainsKey(prjKey))
            {
                return _projectIssueTypes.Single(x=>x.Key==prjKey).Value;
            }
            else 
            {
                return null;
            }
        }
        public IReadOnlyList<CustomField>? ProjectCustomFields(string prjKey)
        {
            if (_projectCustomFields.ContainsKey(prjKey)){
                return _projectCustomFields.Single(x=>x.Key==prjKey).Value;
            } else {
                return null;
            }
        }            

        public static jtisRefData Create(JTISConfig cfg)
        {
            var instance = new jtisRefData();
            instance.repo = cfg.jiraRepo;
            instance.project = cfg.jira.Projects.GetProjectAsync(cfg.defaultProject).GetAwaiter().GetResult();            
            Task.WaitAll(instance.Initialize());
            Task.WaitAll(instance.InitializeProjects());
            return instance;
        }        
        private async Task Initialize()
            {
            List<Task> tasks = new List<Task>();
            // tasks.Add(GetProjectsAsync());
            tasks.Add(GetCustomFieldsAsync());
            await Task.WhenAll(tasks);

        }
        private async Task InitializeProjects()
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(GetProjectIssueTypesAsync());
            tasks.Add(GetProjectCustomFieldsAsync());
            await Task.WhenAll(tasks[0]);
            await Task.WhenAll(tasks[1]);

        }
        private async Task  GetCustomFieldsAsync()
        {
            Task<IEnumerable<CustomField>> fldsTask = repo.jira.Fields.GetCustomFieldsAsync();
            await Task<IEnumerable<CustomField>>.WhenAny(fldsTask);
            _customFieldsList.AddRange(fldsTask.Result);
        }
        private async Task  GetProjectIssueTypesAsync()
        {
            Task<IEnumerable<IssueType>> issTypesTask = project.GetIssueTypesAsync();
            var issType = await Task.WhenAny(issTypesTask);
            await issType;
            if (_projectIssueTypes.ContainsKey(project.Key)){
                _projectIssueTypes[project.Key].AddRange(issType.Result);
            } else {
                _projectIssueTypes.Add(project.Key,issType.Result.ToList());
            }
        }
        private async Task  GetProjectCustomFieldsAsync()
        {
                
            CustomFieldFetchOptions options = new CustomFieldFetchOptions();
            options.ProjectKeys.Add(project.Key);                
            Task<IEnumerable<CustomField>> cstmFieldsTask = repo.jira.Fields.GetCustomFieldsAsync(options);
            var cstmFields = Task.WhenAny(cstmFieldsTask);

            await cstmFields;
            if (_projectCustomFields.ContainsKey(project.Key))
            {
                _projectCustomFields[project.Key].AddRange(cstmFields.Result.Result);
            }
            else 
            {
                _projectCustomFields.Add(project.Key,cstmFields.Result.Result.ToList());
            }
        }

        // private async Task  GetCustomProjectFieldsAsync()
        // {
        //     CustomFieldFetchOptions options = new CustomFieldFetchOptions();
        //     options.
        //     foreach (var prj in _projectList)
        //     {
        //         Task
        //     }
        //     repo.jira.Fields.GetCustomFieldsForProjectAsync()
        //         // Task<IEnumerable<IssueType>> issTypesTask = prj.GetIssueTypesAsync();
        //         // await Task.WhenAll(issTypesTask);
        //         // _projectIssueTypes.Add(prj.Key,issTypesTask.Result.ToList());                
        // }


        // private async Task<IPagedQueryResult<Issue>> IssuesAsync(string jql)
        // {
        //     return await repo.jira.Issues.GetIssuesFromJqlAsync(jql,startAt:_nextStart);
        // }        

        // private SortedList<string,jtisIssue> _jtisIssues = new SortedList<string, jtisIssue>();
        // private jtisIssue AddjtisIssue(Issue issue)
        // {
        //     if (_jtisIssues.ContainsKey(issue.Key.Value)==false)
        //     {
        //         _jtisIssues.Add(issue.Key.Value,new jtisIssue(issue));
        //     }
        //     return _jtisIssues.Single(x=>x.Key==issue.Key.Value).Value;
        // }
        // private void AddjtisIssueChangeLogs(Issue issue, IEnumerable<IssueChangeLog> logs)
        // {
        //     AddjtisIssue(issue).AddChangeLogs(logs);
        // }

        // public List<jtisIssue> GetIssuesWithChangeLogs(string jql)
        // {            
        //     _totReturnCount = repo.GetJQLResultsCount(jql);
        //     sw = Stopwatch.StartNew();                        

        //     AnsiConsole.Progress()
        //         .AutoClear(true)
        //         .HideCompleted(false)
        //         .Columns(
        //             new ProgressColumn[]{
        //                 new TaskDescriptionColumn(), 
        //                 //new ProgressBarColumn(), 
        //                 new SpinnerColumn(), 
        //                 new ElapsedTimeColumn()
        //             })
        //         .Start(ctx=> {
                    
        //             while (_jtisIssues.Count() < _totReturnCount)
        //             {
        //                 if (_jtisIssues.Count()==0){
        //                     _nextStart = 0;
        //                 }
        //                 else{ 
        //                     _nextStart += 100;                        
        //                 }
        //                 var task = ctx.AddTask($"(startAt: {_nextStart}) async issues and change logs search");
        //                 Task.WaitAll(IssuesWithChangeLogs(jql));
        //             }

        //         });
        //     sw.Stop();
        //     var totIssues = _jtisIssues.Count();
        //     var totCL = _jtisIssues.Values.SelectMany(x=>x.ChangeLogs).Count();
        //     var totCLI = _jtisIssues.Values.SelectMany(s=>s.ChangeLogs.SelectMany(y=>y.Items)).Count();
        //     Performance.Add($"(Overall Total Time) Issues: {totIssues}, Change Logs: {totCL}, Change Log Items: {totCLI}",sw.Elapsed);
        //     return _jtisIssues.Values.ToList();            
        // }
        // private async Task IssuesWithChangeLogs(string jql)
        // {            
        //     var iss = await Task<IPagedQueryResult<Issue>>.WhenAny(IssuesAsync(jql));
        //     List<Task> clTasks = new List<Task>();
        //     foreach (var i in iss.Result)
        //     {  
        //         AddjtisIssue(i);
        //         Task clResult = GetChangeLogs(i);
        //         clTasks.Add(clResult);
        //     }

        //     while (clTasks.Any())
        //     {
        //         var clTask = await Task.WhenAny(clTasks);
        //         await clTask;
        //         clTasks.Remove(clTask);
        //     }
        // }

        // private async Task<IPagedQueryResult<Issue>> IssuesAsync(string jql)
        // {
        //     return await repo.jira.Issues.GetIssuesFromJqlAsync(jql,startAt:_nextStart);
        // }

        // private async Task GetChangeLogs(Issue issue)
        // {   
        //     // AnsiConsole.WriteLine($"GetChangeLogs first line for {issue.Key.Value}");         
        //     Task<IEnumerable<IssueChangeLog>>? response = repo.jira.Issues.GetChangeLogsAsync(issue.Key.Value);
        //     await response;
        //     if (response != null && response.Result != null && response.Result.Count() > 0)
        //     {
        //         // AnsiConsole.WriteLine($"GetChangeLogs for {issue.Key.Value}, adding {response.Result.Count()} change logs");
        //         AddjtisIssueChangeLogs(issue,response.Result);
        //     }
        // }        
    }
}