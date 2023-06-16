using System.Runtime.CompilerServices;


namespace  JiraCon
{
    public class IssueCalcs
    {
        public JIssue? IssueObj {get;set;}
        public List<StateCalc> StateCalcs {get;set;}
        public List<Blocker> Blockers {get;set;}

        public IssueCalcs()
        {
            StateCalcs = new List<StateCalc>();
            Blockers = new List<Blocker>();
        }
        public IssueCalcs(JIssue jIss):this()
        {
            IssueObj = jIss;
            Populate();
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
                        ret.Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}","Key","Summary","CurrentStatus","ChangeLogType", "StartDt","EndDt","FromId","FromValue","ToId","ToValue" ));                    
                        addedHeader = true;
                    }
                    StateCalc c = StateCalcs[i];
                    ret.Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",IssueObj.Key,IssueObj.Summary.Replace(","," "),IssueObj.StatusName,c.LogItem.ChangeLogType, c.StartDt,c.EndDt,c.FromId,c.FromValue,c.ToId,c.ToValue ));                    
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
                    // if (cli.FieldName.ToLower() == "status")
                    // {
                    //     StateCalc sc = new StateCalc(cli, cl.CreatedDate );
                    //     StateCalcs.Add(sc);
                    // }
                    if (cli.ChangeLogType == ChangeLogTypeEnum.clStatus || cli.ChangeLogType == ChangeLogTypeEnum.clBlockedFlag || cli.ChangeLogType == ChangeLogTypeEnum.clBlockedField)
                    {
                        StateCalc sc = new StateCalc(cli, cl.CreatedDate );
                        StateCalcs.Add(sc);
                    }
                }
            }
        }
    }
}