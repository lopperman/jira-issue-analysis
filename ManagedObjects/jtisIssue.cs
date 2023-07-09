using Atlassian.Jira;

namespace JTIS.Data
{
    public class jtisIssue
    {
        public Issue issue {get;private set;}

        public List<IssueChangeLog> ChangeLogs {get; private set;}

        private JIssue? _jIssue = null;
        public JIssue jIssue 
        {
            get{
                if (_jIssue == null){
                    _jIssue = new JIssue(issue);
                }
                if (_jIssue.ChangeLogs.Count() != ChangeLogs.Count())
                {
                    _jIssue.ChangeLogs.Clear();
                    _jIssue.AddChangeLogs(ChangeLogs);
                }
                return _jIssue;
            }
        }

        public jtisIssue(Issue iss)
        {
            issue = iss;
            ChangeLogs = new List<IssueChangeLog>();
        }

        public jtisIssue AddChangeLogs(IEnumerable<IssueChangeLog>? changeLogs)
        {
            if (changeLogs != null && changeLogs.Count() > 0)
            {
                ChangeLogs.AddRange(changeLogs);
            }
            return this;
        }
    }
}