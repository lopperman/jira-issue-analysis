using Atlassian.Jira;
using JTIS.Config;
using JTIS.Extensions;

namespace JTIS.Data;

public static class jtisWorkflow
{
    private static Project? _project;
    public static Project DefaultProject 
    {
        get{
            if (_project == null){
                _project = JiraUtil.JiraRepo.jira.Projects.GetProjectAsync(CfgManager.config.defaultProject).GetAwaiter().GetResult();
            }
            return _project;
        }
    }

    public static IEnumerable<IssueType> IssueTypes(Project prj)
    {
        return prj.GetIssueTypesAsync().GetAwaiter().GetResult();
    }
    public static void GetWorkflowScheme(string? projectKey = null)
    {
        var prj = DefaultProject;
        if (projectKey != null && projectKey.StringsMatch(CfgManager.config.defaultProject)==false)
        {
            prj = JiraUtil.JiraRepo.jira.Projects.GetProjectAsync(projectKey).GetAwaiter().GetResult();
        }
        // var xx = JiraUtil.JiraRepo.jira.Issues.ValidateQuery()
    }


}