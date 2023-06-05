using System;
using Atlassian.Jira;
using Newtonsoft.Json;

namespace JiraCon
{
    public class JIssueChangeLogItem
    {
        private IssueChangeLogItem _item = null;

        public JIssueChangeLogItem()
        {
        }

        public JIssueChangeLogItem(IssueChangeLogItem item)
        {
            _item = item;
            Initialize();
        }

        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string FromId { get; set; }
        public string FromValue { get; set; }
        public string ToId { get; set; }
        public string ToValue { get; set; }

        public T GetToValue<T>()
        {
            return JHelper.GetValue<T>(FromValue);
        }

        public T GetFromValue<T>()
        {
            return JHelper.GetValue<T>(ToValue);
        }

        private void Initialize()
        {
            FieldName = _item.FieldName;
            FieldType = _item.FieldType;
            FromId = _item.FromId;
            FromValue = _item.FromValue;
            ToId = _item.ToId;
            ToValue = _item.ToValue;
        }

        [JsonIgnore]
        public IssueChangeLogItem IssueChangeLogItem
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
                Initialize();
            }
        }
    }
}
