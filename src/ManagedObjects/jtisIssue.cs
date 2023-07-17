using Atlassian.Jira;

namespace JTIS.Data
{
    public class jtisIssue
    {
        private List<IssueChangeLog> _changeLogs = new List<IssueChangeLog>();
        private jtisBlockers? blockers = null;
        public Issue issue {get;private set;}

        public void BuildBlockers()
        {
            blockers = jtisBlockers.Create(this);
        }

        public jtisBlockers Blockers
        {
            get
            {
                if (blockers == null)
                {
                    BuildBlockers();
                }
                return blockers;                
            }
        }

        public List<IssueChangeLog> ChangeLogs 
        {
            get
            {
                return _changeLogs.OrderBy(x=>x.CreatedDate).ToList();
            }
        }

        private JIssue? _jIssue = null;
        
        public JIssue jIssue 
        {
            get{
                if (_jIssue == null){
                    _jIssue = new JIssue(issue);
                }
                if (_jIssue.ChangeLogs.Count() != _changeLogs.Count())
                {
                    _jIssue.ChangeLogs.Clear();
                    _jIssue.AddChangeLogs(_changeLogs);
                }
                return _jIssue;
            }
        }

        public jtisIssue(Issue iss)
        {
            issue = iss;
        }

        public jtisIssue AddChangeLogs(IEnumerable<IssueChangeLog>? changeLogs)
        {
            _changeLogs.AddRange(changeLogs);
            return this;
        }
    }
}