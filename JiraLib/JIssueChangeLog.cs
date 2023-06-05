using System;
using System.Collections.Generic;
using Atlassian.Jira;
using Newtonsoft.Json;

namespace JiraCon
{
    public class JIssueChangeLog
    {
        private IssueChangeLog _changeLog = null;
        private List<JIssueChangeLogItem> _items = null;

        public JIssueChangeLog()
        {
        }

        public JIssueChangeLog(IssueChangeLog changeLog)
        {
            _changeLog = changeLog;
            Initialize();
        }

        public string Id { get; set; }
        public string Author { get; set; }
        public DateTime CreatedDate { get; set; }

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
                Items.Add(new JIssueChangeLogItem(i));
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
