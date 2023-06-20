

namespace JiraCon
{
    public class Blocker
    {
        public DateTime StartDt {get;private set ;}
        public DateTime? EndDt {get; set;}
        public string IssueKey {get;private set;}

        public ChangeLogTypeEnum BlockedType {get;set;}
        //we need to track blocker field name in order to find when then blocker was removed
        //fieldname blocker removal is whenever that field has changed to a different value
        public string? BlockerFieldName {get;set;}
        public Blocker()
        {
            StartDt = DateTime.MinValue;
            EndDt = DateTime.MinValue;
            IssueKey = string.Empty;
        }
        public Blocker(string issKey, DateTime start, ChangeLogTypeEnum blockType, string? blockerField = null, DateTime? end = null): this()
        {
            IssueKey = issKey;
            StartDt = start;
            BlockedType = blockType;
            BlockerFieldName = blockerField;
            EndDt = end;
        }


    }
}