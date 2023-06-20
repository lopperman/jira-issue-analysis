using System;
using System.Collections.Generic;
using Atlassian.Jira;
using Newtonsoft.Json;

namespace JiraCon
{
    public class JIssueChangeLog
    {
        private IssueChangeLog? _changeLog;
        private List<JIssueChangeLogItem>? _items ;

        [JsonIgnore]
        public JIssue? JIss {get;private set;}

        public JIssueChangeLog()
        {
            Id = string.Empty;
            Author = string.Empty;
            CreatedDate = DateTime.MinValue;
        }

        public JIssueChangeLog(JIssue parentIssue, IssueChangeLog changeLog):this()
        {
            _changeLog = changeLog;
            JIss = parentIssue ;
            Initialize();
        }

        public string Id { get; set; }
        public string Author { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? EndDate {get;set;}
        

        public List<JIssueChangeLogItem> Items
        {
            get
            {
                if (_items == null) _items = new List<JIssueChangeLogItem>();
                return _items;
            }
            set
            {
                _items = value;
            }
        }

        private void Initialize()
        {
            Id = _changeLog.Id;
            Author = _changeLog.Author.Username;
            CreatedDate = _changeLog.CreatedDate;
            foreach (var i in _changeLog.Items)
            {
                Items.Add(new JIssueChangeLogItem(this, i));
            }

        }

        [JsonIgnore]
        public IssueChangeLog JiraChangeLog
        {
            get
            {
                return _changeLog;
            }
            set
            {
                _changeLog = value;
                Initialize();

            }

        }

    }
}
