using System;
using System.Collections.Generic;
using System.IO;
using Atlassian.Jira;

namespace JiraCon
{
    public class JiraData
    { 


        private string _sourceJQL = string.Empty;
        public string SourceJQL { get; }
        private List<Issue> _issues = new List<Issue>();
        private SortedList<string, List<IssueChangeLog>> _changeLog = new SortedList<string, List<IssueChangeLog>>();


        public JiraData(string jql)
        {
            _sourceJQL = jql;
        }

        
        public List<Issue> JiraIssues
        {
            get
            {
                return _issues;
            }
        }

        public SortedList<string, List<IssueChangeLog>> ChangeLogList
        {
            get
            {
                return _changeLog;
            }
        }

        public void AddIssueChangeLogs(string issueKey, List<IssueChangeLog> changeLogs)
        {
            if (_changeLog.ContainsKey(issueKey))
            {
                _changeLog[issueKey] = changeLogs;
            }
            else
            {
                _changeLog.Add(issueKey, changeLogs);
            }
        }

        public List<IssueChangeLog> GetIssueChangeLogs(string issueKey)
        {
            if (_changeLog.ContainsKey(issueKey))
            {
                return _changeLog[issueKey];
            }
            return null;
        }

    }
}
