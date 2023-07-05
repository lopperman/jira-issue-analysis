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
    public class AsyncChangeLogs
    {
        public JiraRepo? repo {get;set;}

        static AsyncChangeLogs()
        {
        }
        internal AsyncChangeLogs(JiraRepo repo)
        {
            this.repo = repo;
            
        }
        public static AsyncChangeLogs Create(JiraRepo repo)
        {
            return  new AsyncChangeLogs(repo); 
        }

        public async Task<IEnumerable<JIssue>> GetIssuesAsync(string jql)
        {
            List<JIssue> jissues = new List<JIssue>();

            //1ST AWAIT, BUT IT'S SUPER QUICK TO RUN            
            var issues = await GetInitialIssues(jql);

            //'SORTING FOR SANITY' -- IF THINGS ARE SET UP RIGHT, 
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

    }
}