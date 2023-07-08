using Atlassian.Jira;
using Spectre.Console;

namespace JTIS
{
    /*
        (https://github.com/lopperman)

        AsyncChangeLogs 
        Find all issues based on [valid JQL query]
        Asynchronous retrieval of Change Logs for 
            all Issues returned in query
        Returns converted List<JIssue>
        (Jissue - custom strong object that parses
            Jira Issue Json and Jira ChangeLog Json
            into single structure)

        Usage Example:

        var jql = [get valid JQL statement];
        var jiraRepo = new JiraRep([string jiraUrl], [string jiraLogin], [string jiraAPIToken]);
        List<JIssue> jIssues = await AsyncChangeLogs.Create(jiraRepo).GetIssuesAsync(jql);

    */
    public class AsyncTesting
    {
        public JiraRepo? repo 
        {
            get
            {
                return JiraUtil.JiraRepo;
            }
        }
        static AsyncTesting()
        {
        }
        public static AsyncTesting Create()
        {
            return  new AsyncTesting(); 
        }

        internal List<Issue> _issues = new List<Issue>();
        internal List<JIssue> _jIssues = new List<JIssue>();
        internal List<IssueChangeLog> _changeLog = new List<IssueChangeLog>();

        bool progressConfigured = false;



        internal async Task<IEnumerable<IssueChangeLog>> getChangeLogs(IEnumerable<Issue> issues)
        {
            if (progressConfigured == false) 
            {
                progressConfigured = true;

            }

            List<Task<IEnumerable<IssueChangeLog>>> list = new List<Task<IEnumerable<IssueChangeLog>>>();

            foreach (var issue in issues)
            {
                list.Add(JiraUtil.JiraRepo.jira.Issues.GetChangeLogsAsync(issue.Key.Value));
            }
            while (list.Count > 0)
            {
                var clTask = await Task.WhenAny(list);
                _changeLog.AddRange(clTask.Result);
                await clTask;
                list.Remove(clTask);
            }
            return _changeLog;            
        }

        internal async Task<IEnumerable<Issue>> FindIssuesAsync()
        {
            var jql1 = "key in (WWT-310, WWT-302, WWT-297, WWT-296, WWT-295, WWT-294, WWT-293, WWT-292, WWT-291)";
            var jql2 = "Key in (WWT-291, WWT-292, WWT-294)";
            var jql3 = "project=WWT and status not in (backlog, done) and (priority = Blocked OR Flagged in (Impediment))";
            Task<IEnumerable<Issue>> task1 = GetIssuesAsync(jql1);            
            Task<IEnumerable<Issue>> task2 = GetIssuesAsync(jql2);
            Task<IEnumerable<Issue>> task3 = GetIssuesAsync(jql3);

            var tasks = new List<Task<IEnumerable<Issue>>>{task1, task2, task3};
            while (tasks.Count > 0)
            {
                Task<IEnumerable<Issue>> finishedTask = await Task.WhenAny(tasks);
                _issues.AddRange(finishedTask.Result);
                await finishedTask;
                tasks.Remove(finishedTask);
            }
            return _issues;
        }

        internal async Task<IEnumerable<Issue>> GetIssuesAsync(string jql)
        {
            return await repo.jira.Issues.GetIssuesFromJqlAsync(jql);
        }
    }
}