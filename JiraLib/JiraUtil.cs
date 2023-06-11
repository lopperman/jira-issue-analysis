using System;
using System.Linq;
using Atlassian.Jira;

namespace JiraCon
{
    public static class JiraUtil
    {
        private static JiraRestClientSettings? _settings ;
        private static JiraRepo? _jiraRepo;

        public static JiraRepo JiraRepo
        {
            get
            {
                if (_jiraRepo == null && JTISConfigHelper.config != null && JTISConfigHelper.config.ValidConfig == true)
                {
                    CreateRestClient();
                }
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
                _settings = new JiraRestClientSettings();
                _settings.EnableUserPrivacyMode = true;

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
    }
}
