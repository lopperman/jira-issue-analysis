

using JTIS.Config;

namespace JTIS
{
    public class StateCalc
    {


        private string _fromValue = string.Empty;
        private string _fromId = string.Empty;
        private string _toValue = string.Empty;
        private string _toId = string.Empty;
        public JIssueChangeLogItem? LogItem {get;set;}
        

        public StateCalc()
        {
            CreatedDt = DateTime.MinValue;
        }
        public StateCalc(JIssueChangeLogItem cli, DateTime created):this()
        {
            LogItem = cli;
            CreatedDt = created;
            Populate();
        }

        private void Populate()
        {
            
            FromId = LogItem.FromId;
            FromValue = LogItem.FromValue;
            ToId = LogItem.ToId;
            ToValue = LogItem.ToValue;

        }

        public StatusType ActivityType
        {
            get
            {
                return LogItem.TrackType;                 
            }
        }

        public ChangeLogTypeEnum ChangeLogType
        {
            get
            {
                return LogItem.ChangeLogType;
            }
        }

        public DateTime CreatedDt {get;set;}
        public string FromValue
        {
            get
            {
                return _fromValue;
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    _fromValue = "(empty)";
                }
                else 
                {
                    _fromValue = value;
                }
            }
        }

        public DateTime StartDt
        {
            get
            {
                return CreatedDt;
            }
        }
        public DateTime? EndDt 
        {
            get
            {
                return this.LogItem.ChangeLog.EndDate;
            }
        }
        public string FromId
        {
            get
            {
                return _fromId;
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    _fromId = "(empty)";
                }
                else 
                {
                    _fromId = value;
                }
            }
        }
        public string ToValue
        {
            get
            {
                return _toValue;
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    _toValue = "(empty)";
                }
                else 
                {
                    _toValue = value;
                }
            }
        }
        public string ToId
        {
            get
            {
                return _toId;
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    _toId = "(empty)";
                }
                else 
                {
                    _toId = value;
                }
            }
        }
    }
}