using System;
using System.Linq;
using Atlassian.Jira;
using JTIS.Config;
using JTIS.Console;
using Spectre.Console;

namespace JTIS
{
    public static class JiraUtil
    {
        private static JiraRestClientSettings? _settings ;
        private static JiraRepo? _jiraRepo;
        private static string _userName = string.Empty;
        private static string _apiToken = string.Empty;
        private static string _baseUrl = string.Empty;


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
        public static bool CreateRestClient(string login, string apiToken, string baseUrl)
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
    }
}
