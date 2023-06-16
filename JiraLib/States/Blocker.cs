

namespace JiraCon
{
    public enum BlockerType
    {
        btUnknown = 0, 
        btFlagImpediment = 1, 
        btPriorityField = 2
    }
    public class Blocker
    {
        public DateTime StartDt {get;private set ;}
        public DateTime EndDt {get;private set;}
        public string IssueKey {get;private set;}
        public BlockerType BlockType {get;private set;}
        public bool IsBlocked {get; private set;}

        public Blocker()
        {
            StartDt = DateTime.MinValue;
            EndDt = DateTime.MinValue;
            IssueKey = string.Empty;
            BlockType = BlockerType.btUnknown;
        }
        public Blocker(string issKey, DateTime start, DateTime end, BlockerType type, bool isBlockedNow): this()
        {
            IssueKey = issKey;
            StartDt = start;
            EndDt = end;
            BlockType = type;
            IsBlocked = isBlockedNow;
        }

    }
}