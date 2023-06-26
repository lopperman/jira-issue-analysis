using System;
using System.Linq;
using Atlassian.Jira;
using Spectre.Console;

namespace JiraCon
{
    public static class JiraUtil
    {
        private static JiraRestClientSettings? _settings ;
        private static JiraRepo? _jiraRepo;
        private static JTISConfig? _cfg;

        public static JiraRepo JiraRepo
        {
            get
            {
                CreateRestClient();
                if (_jiraRepo != null)
                {                     
                    return _jiraRepo;
                }
                else 
                {
                    throw new NullReferenceException("JiraUtil._jiraRepo is null");
                }
            }
        }

        public static bool CreateRestClient(JTISConfig cfg)
        {
            bool ret = false;
            try
            {
                if (_jiraRepo != null && _cfg != null && _cfg.userName == cfg.userName & _cfg.baseUrl == cfg.baseUrl && _cfg.apiToken == cfg.apiToken)
                {
                    ret = true;
                }
                else 
                {
                    _settings = new JiraRestClientSettings();
                    _settings.EnableUserPrivacyMode = true;
                    _cfg = cfg;

                    _jiraRepo = new JiraRepo(cfg.baseUrl , cfg.userName , cfg.apiToken );

                    if (_jiraRepo != null)
                    {
                        var test = _jiraRepo.GetJira().IssueTypes.GetIssueTypesAsync().Result.ToList();
                        if (test != null && test.Count > 0)
                        {
                            ret = true;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Beep();
                Console.Beep();
                Console.Error.WriteLine("Sorry, there seems to be a problem connecting to Jira with the arguments you provided. Error: {0}, {1}\r\n\r\n{2}", ex.Message, ex.Source, ex.StackTrace);
                return false;
            }


            return ret;            
        }

        public static bool CreateRestClient()
        {
            if (JTISConfigHelper.config != null)
            {
                return CreateRestClient(JTISConfigHelper.config);
            }
            else
            {
                return false;
            }
        }

        public static List<JIssue> GetJIssues(string jql, bool? expandChangeLog = true)
        {
            List<Issue> issues = new List<Issue>();
            List<JIssue> jIssueList = new List<JIssue>();
            try 
            {
                ConsoleUtil.WriteStdLine("QUERYING JIRA ISSUES",StdLine.slInfo ,false);

                AnsiConsole.Progress().Columns(new ProgressColumn[]
                    {
                        new TaskDescriptionColumn(), 
                        new PercentageColumn(),
                        new ElapsedTimeColumn(), 
                        new SpinnerColumn(), 

                    })
                    .Start(ctx => 
                    {
//                        var task1 = ctx.AddTask("[dim blue on white]Validate JQL Query[/]");
                        var task2 = ctx.AddTask("[dim blue on white]Load Jira Issues[/]");
                        var task3 = ctx.AddTask("[dim blue on white]Populate Change Logs[/]");        

                        // task1.MaxValue = 2;
                        // task1.Description = $"[blue on white] validating jql[/]";
                        // task1.Increment(1);
                        
                        task2.StartTask();
                        task2.MaxValue(3);
                        task2.Increment(1);
                        issues = JiraUtil.JiraRepo.GetIssues(jql);
                        task2.StopTask();
                        if (issues.Count > 0)
                        {
                            task3.MaxValue = issues.Count;
                            task3.Description = $"[dim blue on white]Populate Change Logs for {issues.Count} issues[/]"; 
                            task3.StartTask();
                            foreach (var issue in issues)
                            {
                                JIssue newIssue = new JIssue(issue);
                                newIssue.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(issue));
                                jIssueList.Add(newIssue);
                                task3.Increment(1);
                            }
                            task3.StopTask();
                        }
                    });
                    
            }
            catch(Exception ex)
            {
                ConsoleUtil.WriteError(string.Format("Error getting issues using search: {0}",jql),ex:ex);
            }
            
            return jIssueList;
            
        }        
    }
}
