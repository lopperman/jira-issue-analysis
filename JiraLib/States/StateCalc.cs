

namespace  JiraCon
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
                    _fromValue = "[EMPTY]";
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
        public DateTime? EndDt {get;set;}
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
                    _fromId = "[EMPTY]";
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
                    _toValue = "[EMPTY]";
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
                    _toId = "[EMPTY]";
                }
                else 
                {
                    _toId = value;
                }
            }
        }
    }
}