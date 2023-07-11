using JTIS.Analysis;

namespace JTIS
{
    public class IssueCalcs
    {
        public JIssue? IssueObj {get;set;}
        public List<StateCalc> StateCalcs {get;set;}
        public List<Blocker> Blockers {get;set;}
        public double CalendarDays {get; private set;}
        public double BusinessDays {get; private set;}
        public double UnblockedActiveDays {get; private set;}
        public double BlockedActiveDays {get; private set;}
        public StateCalc? FirstActiveStateCalc {get;set;}

        public void SetCalendarDays(double days)
        {
            if (CalendarDays != 0 && days != CalendarDays) 
            {
                throw new InvalidDataException("CalendarDays can only be set once");
            }
            CalendarDays = days;
        }
        public void SetBusinessDays(double days)
        {
            if (BusinessDays != 0 && days != BusinessDays) 
            {
                throw new InvalidDataException("BusinessDays can only be set once");
            }
            BusinessDays= days;
        }
        public void SetUnblockedActiveDays(double days)
        {
            
            if (UnblockedActiveDays != 0 && days != UnblockedActiveDays) 
            {
                throw new InvalidDataException("UnblockedActiveDays can only be set once");
            }
            UnblockedActiveDays = days;
        }
        public void SetBlockedActiveDays(double days)
        {
            if (BlockedActiveDays != 0 && days != BlockedActiveDays) 
            {
                throw new InvalidDataException("BlockedActiveDays can only be set once");
            }
            BlockedActiveDays = days;
        }
        public void ResetTotalDaysFields()
        {
            CalendarDays = 0;
            BlockedActiveDays = 0;
            UnblockedActiveDays = 0;
            BusinessDays = 0;
        }

        public IssueCalcs()
        {
            StateCalcs = new List<StateCalc>();
            Blockers = new List<Blocker>();
        }
        public IssueCalcs(JIssue jIss):this()
        {
            IssueObj = jIss;
            Populate();
            PopulateBlockers();
        }

        public List<string> StateCalcStringList(bool addHeader = false)
        {
            List<string> ret = new List<string>();
            bool addedHeader = false;
            if (StateCalcs.Count > 0)
            {
                for (int i = 0; i < StateCalcs.Count; i ++)
                {
                    if (addHeader == true &&  addedHeader == false)
                    {
                        ret.Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}","Key","CurrentStatus","ChangeLogId", "ChangeLogType", "StartDt","EndDt","CalDays","BusDays","BlockedBusDays","TotBlockedDays", "FromId","FromValue","ToId","ToValue", "TrackType" ));                    
                        addedHeader = true;
                    }
                    StateCalc c = StateCalcs[i];
                    ret.Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",IssueObj.Key,IssueObj.StatusName,c.LogItem.ChangeLog.Id, c.LogItem.ChangeLogType, c.StartDt,c.EndDt,c.LogItem.TotalCalendarTime.Days, c.LogItem.TotalBusinessTime.Days, Math.Round(c.LogItem.TotalCalendarBlockedTime.TotalDays,2),Math.Round(c.LogItem.TotalBlockedBusinessTime.TotalDays,2),c.FromId,c.FromValue,c.ToId,c.ToValue,  c.LogItem.TrackType   ));                    
                }
            }

            return ret;
        }

        private void Populate()
        {
            foreach (var cl in IssueObj.ChangeLogs)
            {
                foreach (var cli in cl.Items)
                {
                    if (cli.ChangeLogType == ChangeLogTypeEnum.clStatus || cli.ChangeLogType == ChangeLogTypeEnum.clBlockedFlag || cli.ChangeLogType == ChangeLogTypeEnum.clBlockedField)
                    {
                        StateCalc sc = new StateCalc(cli, cl.CreatedDate );
                        StateCalcs.Add(sc);
                    }
                }
                //figure out StatusType here
                
            }
        }

        private void PopulateBlockers()
        {

        }
    }
}