using System.ComponentModel;
using System.Collections.Immutable;
using System.Text;
using System.Linq;
using Atlassian.Jira;
using JTIS.Config;
using JTIS.Console;
using Spectre.Console;
using JTIS.Extensions;

namespace JTIS
{
    public static class JiraUtil
    {
        private static JiraRestClientSettings? _settings ;
        private static JiraRepo? _jiraRepo;
        private static string _userName = string.Empty;
        private static string _apiToken = string.Empty;
        private static string _baseUrl = string.Empty;

        //return empty list if invalid connection, otherwise returns list of project keys
        public static List<string> ValidProjectKeys(string loginName, string loginAPIToken, string loginURL)
        {
            List<string> keys = new List<string>();
            try 
            {
                JiraRepo tempJiraRepo = new JiraRepo(loginURL, loginName, loginAPIToken);
                if (tempJiraRepo.jira == null) 
                {
                    return keys;
                }
                var projects = tempJiraRepo.jira.Projects.GetProjectsAsync().GetAwaiter().GetResult();
                foreach (var proj in projects)
                {
                    keys.Add(proj.Key);
                }
            }
            catch 
            {
                //INVALID CONNECTION
            }

            return keys;
        }

        public static void Reset()
        {
            _jiraRepo = null;

        }
        public static JiraRepo JiraRepo
        {
            get
            {
                if (_jiraRepo != null)
                {                     
                    return _jiraRepo;
                }
                else 
                {
                    ConsoleUtil.WriteError("JiraUtil.JiraRepo is not set",pause:true);
                    throw new NullReferenceException("JiraUtil._jiraRepo is null");
                }
            }
        }

        // public static bool CreateRestClient(JTISConfig cfg)
        // {
        //     return CreateRestClient(cfg.userName,cfg.apiToken,cfg.baseUrl);
        // }
        public static bool  CreateRestClient(string login, string apiToken, string baseUrl)
        {
            bool ret = false;
            try
            {
                if (_jiraRepo != null && login == _userName & apiToken == _apiToken && baseUrl == _baseUrl)
                {
                    ret = true;
                }
                else 
                {
                    _jiraRepo = null;
                    _settings = new JiraRestClientSettings();
                    _settings.EnableUserPrivacyMode = true;
                    _userName = login;
                    _apiToken = apiToken;
                    _baseUrl = baseUrl;

                    _jiraRepo = new JiraRepo(_baseUrl , _userName , _apiToken );

                    if (_jiraRepo != null)
                    {
                        return true;
                    }
                }

            }
            catch (Exception ex)
            {
                System.Console.Beep();
                System.Console.Beep();
                System.Console.Error.WriteLine("Sorry, there seems to be a problem connecting to Jira with the arguments you provided. Error: {0}, {1}\r\n\r\n{2}", ex.Message, ex.Source, ex.StackTrace);
                return false;
            }


            return ret;            
        }


        public static List<JIssue> GetJIssues(string jql, bool? expandChangeLog = true)
        {
            List<Issue> issues = new List<Issue>();
            List<JIssue> jIssueList = new List<JIssue>();
            try 
            {
                ConsoleUtil.WriteStdLine("QUERYING JIRA ISSUES",StdLine.slInfo ,false);


                    
            }
            catch(Exception ex)
            {
                ConsoleUtil.WriteError(string.Format("Error getting issues using search: {0}",jql),ex:ex);
            }
            
            return jIssueList;
            
        }

        internal static void DevScrub()
        {
            ConsoleUtil.WriteAppTitle();
            AnsiConsole.Write(new Rule("DEV - SCRUB TERMS FOR SCREENSHOTS"));
            AnsiConsole.MarkupLine($"[bold]{CfgManager.config.ScrubList().Count()} scrubbed items[/]");
            if (CfgManager.config.ScrubList().Count() > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendJoin(',',CfgManager.config.ScrubList().ToImmutableArray());
                AnsiConsole.Write(new Panel(sb.ToString()));
            }
            AnsiConsole.Write(new Rule());
            AnsiConsole.MarkupLine($"[bold]ADD TERM1|TERM2|TERM3|ETC TO ADD[/]");
            AnsiConsole.MarkupLine($"[bold]DEL TERM1|TERM2|TERM3|ETC TO REMOVE[/]");
            var scrubInput = ConsoleUtil.GetInput<string>("Scrub:",allowEmpty:true);
            bool addItems = false;
            if (scrubInput.StringsMatch("add",StringCompareType.scStartsWith))
            {
                addItems = true;
            }
            else if (scrubInput.StringsMatch("del",StringCompareType.scStartsWith))
            {
                addItems = false;
            }
            else 
            {
                ConsoleUtil.PressAnyKeyToContinue("OPERATION CANCELLED");
                return;
            }
            scrubInput = scrubInput.Replace("add ","",StringComparison.OrdinalIgnoreCase).Trim();
            scrubInput = scrubInput.Replace("del ","",StringComparison.OrdinalIgnoreCase).Trim();
            string[] itemArr = scrubInput.Split('|',StringSplitOptions.RemoveEmptyEntries);
            if (itemArr.Length > 0) 
            {
                if (addItems)
                {
                    CfgManager.config.AddScrubTerms(itemArr);
                    CfgManager.SaveConfigList();
                }
                else 
                {
                    CfgManager.config.DeleteScrubTerms(itemArr);
                    CfgManager.SaveConfigList();
                }
                DevScrub();
            }

        }
    }
}
