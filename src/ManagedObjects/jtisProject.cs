using Atlassian.Jira;

namespace JTIS.Data
{
    public class jtisProject
    {
        public Project project {get;private set;}
        public List<IssueType>? IssueTypes {get; private set;}

        public jtisProject(Project prj)
        {
            project = prj;
            // IssueTypes.AddRange(GetIssueTypesAsync().GetAwaiter().GetResult());
        }

        // private async void GetIssueTypesAsync()
        // {
        //     Task<IEnumerable<IssueType>> issTypeTask = project.GetIssueTypesAsync();
        //     var tasks = await Task.WaitAll(issTypeTask);
        // }

        // public jtisIssue AddChangeLogs(IEnumerable<IssueChangeLog>? changeLogs)
        // {
        //     if (changeLogs != null && changeLogs.Count() > 0)
        //     {
        //         ChangeLogs.AddRange(changeLogs);
        //     }
        //     return this;
        // }
    }
}