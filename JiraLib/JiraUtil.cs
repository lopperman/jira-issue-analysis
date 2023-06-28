﻿using System;
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
                System.Console.Beep();
                System.Console.Beep();
                System.Console.Error.WriteLine("Sorry, there seems to be a problem connecting to Jira with the arguments you provided. Error: {0}, {1}\r\n\r\n{2}", ex.Message, ex.Source, ex.StackTrace);
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


                    
            }
            catch(Exception ex)
            {
                ConsoleUtil.WriteError(string.Format("Error getting issues using search: {0}",jql),ex:ex);
            }
            
            return jIssueList;
            
        }        
    }
}
