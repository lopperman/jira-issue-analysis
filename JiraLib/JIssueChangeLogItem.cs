using System;
using Atlassian.Jira;
using JTIS.Config;
using JTIS.Console;
using Newtonsoft.Json;

namespace JTIS
{
    public enum ChangeLogTypeEnum
    {
        clUnknown = 0, 
        clStatus = 1, 
        clBlockedFlag = 2, 
        clBlockedField = 3, 
        clOther = 4
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
            TrackType = StatusType.stUnknown;

        }

        public JIssueChangeLogItem(JIssueChangeLog jIssChangeLog, IssueChangeLogItem item): this()
        {
            _item = item;
            ChangeLog = jIssChangeLog;
            Initialize();
        }

        [JsonIgnore]
        public IssueChangeLogItem? Base
        {
            get
            {
                return _item;
            }
        }

        private DateTime? tempEndDt {get;set;}
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string? FromId { get; set; }
        public string? FromValue { get; set; }
        public string ToId { get; set; }
        public string ToValue { get; set; }
        public DateTime StartDt 
        {
            get
            {
                return ChangeLog.CreatedDate;
            }
        }
        public DateTime EndDt 
        {
            get
            {
                if (ChangeLog.EndDate != null)
                {
                    return ChangeLog.EndDate.Value;
                }
                else 
                {
                    if (tempEndDt == null)
                    {
                        tempEndDt = DateTime.Now;
                    }
                    return tempEndDt.Value;
                }
            }
        }
        
        public TimeSpan TotalCalendarTime
        {
            get
            {
                return EndDt.Subtract(StartDt);
            }
        }
        public TimeSpan TotalCalendarBlockedTime
        {
            get
            {
                return this.ChangeLog.BlockedTimeSpan;
            }
        }
        public TimeSpan TotalBusinessTime
        {
            get
            {
                var ts = TotalCalendarTime;
                var tStart = StartDt.Date;
                do
                {
                    if (tStart.DayOfWeek == DayOfWeek.Saturday || tStart.DayOfWeek == DayOfWeek.Sunday)
                    {
                        ts = ts.Add(new TimeSpan(-24,0,0));
                    }
                    tStart = tStart.AddDays(1);
                } while (tStart <= EndDt.Date);
                return ts;

            }
        }        
        public TimeSpan TotalBlockedBusinessTime
        {
            get
            {
                return ChangeLog.BlockedBusinessTimeSpan;
            }
        }

        public ChangeLogTypeEnum ChangeLogType {get;set;} 
        public StatusType TrackType {get;set;}
        
        [JsonIgnore]
        public JIssueChangeLog? ChangeLog {get;private set;}

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
                    FromId = "(empty)";
                }
                else 
                {
                    FromId = _item.FromId;
                }
                if (_item.FromValue == null)
                {
                    FromValue = "(empty)";
                }
                else 
                {
                    FromValue = _item.FromValue;
                }
                if (_item.ToId == null)
                {
                    ToId = "(empty)";
                }
                else 
                {
                    ToId = _item.ToId;
                }
                if (_item.ToValue == null)
                {
                    ToValue = "(empty)";
                }
                else 
                {
                    ToValue = _item.ToValue;
                }
                if (_item.FieldName.ToLower()=="status")
                {
                    ChangeLogType = ChangeLogTypeEnum.clStatus ;
                }
                else if (_item.FieldName.ToLower()=="flagged")
                {
                    // && (_item.FromValue.ToLower()=="impediment" || _item.ToValue.ToLower()=="impediment"))                    
                    if (_item.FromValue != null && _item.FromValue.ToLower()=="impediment")
                    {
                        ChangeLogType = ChangeLogTypeEnum.clBlockedFlag;
                    }
                    else if (_item.ToValue != null && _item.ToValue.ToLower()=="impediment")
                    {
                        ChangeLogType = ChangeLogTypeEnum.clBlockedFlag;
                    }                    
                    else
                    {
                        ChangeLogType = ChangeLogTypeEnum.clOther ;
                    }
                }
                else if (_item.FieldName.ToLower()=="priority")
                {
                    if (_item.FromValue != null && _item.FromValue.ToLower().StartsWith("block"))
                    {
                        ChangeLogType = ChangeLogTypeEnum.clBlockedField ;
                    }
                    else if (_item.ToValue != null && _item.ToValue.ToLower().StartsWith("block"))
                    {
                        ChangeLogType = ChangeLogTypeEnum.clBlockedField ;
                    }
                    else 
                    {
                        ChangeLogType = ChangeLogTypeEnum.clOther;
                    }
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
