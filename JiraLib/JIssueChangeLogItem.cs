using System;
using Atlassian.Jira;
using Newtonsoft.Json;

namespace JiraCon
{
    public enum ChangeLogTypeEnum
    {
        clUnknown = 0, 
        clStatus = 1, 
        clBlockedFlag = 2, 
        clOther = 3
    }
    public class JIssueChangeLogItem
    {
        private IssueChangeLogItem? _item = null;

        public JIssueChangeLogItem()
        {
            FieldName = string.Empty;
            FieldType = string.Empty;
            FromId = string.Empty;
            ToId = string.Empty;
            ToValue = string.Empty;
            FromValue = string.Empty;
            ChangeLogType = ChangeLogTypeEnum.clUnknown;
        }

        public JIssueChangeLogItem(IssueChangeLogItem item): this()
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
        public ChangeLogTypeEnum ChangeLogType {get;set;} 

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
            if (_item.FieldName.ToLower()=="status")
            {
                ChangeLogType = ChangeLogTypeEnum.clStatus ;
            }
            else if (_item.FieldName.ToLower()=="flagged" && (_item.FromValue.ToLower()=="impediment" || _item.ToValue.ToLower()=="impediment"))
            {
                ChangeLogType = ChangeLogTypeEnum.clBlockedFlag;
            }
            else 
            {
                ChangeLogType = ChangeLogTypeEnum.clOther ;
            }
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
