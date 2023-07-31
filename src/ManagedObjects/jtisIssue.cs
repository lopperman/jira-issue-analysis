using Atlassian.Jira;
using JTIS.Analysis;
using JTIS.Config;
using JTIS.Extensions;

namespace JTIS.Data;

public class jtisIssue: ITimeSummary
{
    private List<IssueChangeLog> _changeLogs = new List<IssueChangeLog>();
    private jtisBlockers? blockers = null;
    private jtisStatuses? _statuses = null;
    public Issue issue {get;private set;}

    public StatusType StatusCategory
    {
        get {                
            var statId = issue.Status.Id;
            var jStatus = CfgManager.config.StatusConfigs.Single(x=>x.StatusId.ToString()==statId);
            switch (jStatus.CategoryName.ToLower())
            {
                case "done":
                    return StatusType.stEnd;;
                case "in progress":
                    return StatusType.stActiveState;
                case "to do":
                    return StatusType.stPassiveState;       
                case "ignore":
                    return StatusType.stIgnoreState;                 
                default:
                    return StatusType.stUnknown;;
            }
        }
    }
    public void BuildBlockers()
    {
        blockers = jtisBlockers.Create(this);
    }

    public jtisStatuses? StatusItems
    {
        get 
        {
            if (_statuses == null && _changeLogs.Count() > 0)
            {
                _statuses = jtisStatuses.Create(this);
            }
            return _statuses;
        }
    }
    public int BlockerCount 
    {
        get{
            return Blockers.Blockers.Count();
        }
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

    public jtisStatus? EnteredOnOrAfter(JiraStatus jstat)
    {
        if (StatusItems == null || StatusItems.Statuses == null || CfgManager.config.ValidIssueStatusSequence==false)
        {
            return null;
        }
        var stats = CfgManager.config.LocalProjectDefaultStatuses.Where(x=>x.ProgressOrder >= jstat.ProgressOrder).ToList();
        jtisStatus? retVal = null;
        int nextOrder = jstat.ProgressOrder;
        while (true)
        {
            var searchLocalStatus = stats.FirstOrDefault(x=>x.ProgressOrder==nextOrder);
            retVal = StatusItems.Statuses.SingleOrDefault(x=>x.IssueStatus.StringsMatch(searchLocalStatus.StatusName));
            if (retVal != null) 
            {
                return retVal;
            }
            nextOrder +=1;
            if (nextOrder > stats.Max(x=>x.ProgressOrder))
            {
                return null;
            }
        }

    }

    public TimeSpan tsCalendarTime {
        get {
            return StatusItems.tsCalendarTime;
        }
    }
    public TimeSpan tsBusinessTime {
        get {
            return StatusItems.tsBusinessTime;
        }
    }

    public TimeSpan tsBlockedCalendarTime {
        get {
            return StatusItems.tsBlockedCalendarTime;
        }
    }

    public TimeSpan tsBlockedBusinessTime {
        get {
            return StatusItems.tsBlockedBusinessTime;
        }
    }

    public TimeSpan tsUnblockedCalendarTime {
        get {
            return StatusItems.tsUnblockedCalendarTime;
        }
    }

    public TimeSpan tsUnblockedBusinessTime {
        get {
            return StatusItems.tsUnblockedBusinessTime;;
        }
    }

    public TimeSpan tsBlockedActiveBusTime {
        get {
            return StatusItems.tsBlockedActiveBusTime;
        }
    }
    public TimeSpan tsUnblockedActiveBusTime {
        get {
            return StatusItems.tsUnblockedActiveBusTime;
        }
    }
    public TimeSpan tsBlockedActiveCalTime {
        get {
            return StatusItems.tsBlockedActiveCalTime;
        }
    }
    public TimeSpan tsUnblockedActiveCalTime {
        get {
            return StatusItems.tsUnblockedActiveCalTime;
        }
    }

    public TimeSpan tsBlockedPassiveBusTime {
        get {
            return StatusItems.tsBlockedPassiveBusTime;
        }
    }
    public TimeSpan tsUnblockedPassiveBusTime {
        get {
            return StatusItems.tsUnblockedPassiveBusTime;
        }
    }
    public TimeSpan tsBlockedPassiveCalTime {
        get {
            return StatusItems.tsBlockedPassiveCalTime;
        }
    }
    public TimeSpan tsUnblockedPassiveCalTime {
        get {
            return StatusItems.tsUnblockedPassiveCalTime;
        }
    }

    public TimeSpan tsActiveCalTime {
        get {
            return StatusItems.tsActiveCalTime;
        }
    }
    public TimeSpan tsActiveBusTime {
        get {
            return StatusItems.tsActiveBusTime;
        }
    }
    public TimeSpan tsPassiveCalTime {
        get {
            return StatusItems.tsPassiveCalTime;
        }
    }
    public TimeSpan tsPassiveBusTime {
        get {
            return StatusItems.tsPassiveBusTime;
        }
    }    

}

