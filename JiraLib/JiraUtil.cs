using System;
using System.Linq;
using Atlassian.Jira;

namespace JiraCon
{
    public static class JiraUtil
    {
        private static JiraRestClientSettings _settings = null;
        private static JiraRepo _jiraRepo = null;

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
                    throw new NullReferenceException("JiraUtil._jiraRepo is null");
                }
            }
        }

        public static bool CreateRestClient(JiraConfiguration config)
        {
            bool ret = false;
            try
            {
                _settings = new JiraRestClientSettings();
                _settings.EnableUserPrivacyMode = true;

                _jiraRepo = new JiraRepo(config.jiraBaseUrl, config.jiraUserName, config.jiraAPIToken);

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
    }
}
