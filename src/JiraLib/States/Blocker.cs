using System;


namespace JTIS
{
    public class Blocker
    {
        private Guid _id = Guid.NewGuid();
        public Guid tmpID 
        {
            get 
            {
                return _id;
            }
        }

        public bool Removed {get;set;}
        public DateTime StartDt {get;set ;}
        public DateTime? EndDt {get; set;}
        public bool CurrentlyBlocked {get;set;}
        public string IssueKey {get;private set;}
        public bool Adjusted {get;set;}
        public List<string> AdjustmentNotes = new List<string>();

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
        public Blocker(string issKey, bool currentlyBlocked,  DateTime start, ChangeLogTypeEnum blockType, string? blockerField = null, DateTime? end = null): this()
        {
            IssueKey = issKey;
            CurrentlyBlocked = currentlyBlocked;
            StartDt = start;
            BlockedType = blockType;
            BlockerFieldName = blockerField;
            EndDt = end;
        }


    }
}