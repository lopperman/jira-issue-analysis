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
        public string? FromId { get; set; }
        public string? FromValue { get; set; }
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
            try 
            {
                FieldName = _item.FieldName;
                FieldType = _item.FieldType;
                if (_item.FromId == null)
                {
                    FromId = "[empty]";
                }
                else 
                {
                    FromId = _item.FromId;
                }
                if (_item.FromValue == null)
                {
                    FromValue = "[empty]";
                }
                else 
                {
                    FromValue = _item.FromValue;
                }
                if (_item.ToId == null)
                {
                    ToId = "[empty]";
                }
                else 
                {
                    ToId = _item.ToId;
                }
                if (_item.ToValue == null)
                {
                    ToValue = "[empty]";
                }
                else 
                {
                    ToValue = _item.ToValue;
                }

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
            catch(Exception ex)
            {
                string errMsg = string.Format("Error occurred in JIssueChangeLogItem.Initialize). {0}: {1}",ex.Message,ex.ToString());
                errMsg = string.Format("{0}{1}Issue Field: {2}",errMsg,Environment.NewLine,_item.FieldName );
                ConsoleUtil.WriteError(errMsg);
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
