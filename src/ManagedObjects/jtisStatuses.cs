using Atlassian.Jira;
using JTIS.Analysis;
using JTIS.Config;
using JTIS.Extensions;

namespace JTIS.Data;

public class jtisStatuses: ITimeSummary
{
    private List<jtisStatus> _statuses = new List<jtisStatus>();

    public IReadOnlyList<jtisStatus> Statuses
    {
        get{
            return _statuses.OrderBy(x=>x.StatusSequence).ThenBy(y=>y.FirstEntryDate).ToList();
        }
    }

    public jtisStatus? FirstActive 
    {
        get {
            return _statuses.SingleOrDefault(x=>x.StatusCategory==StatusType.stStart);
        }
    }
    public static jtisStatuses Create(jtisIssue issue, string? stopAtStatus = null)
    {
        jtisStatuses newObj = new jtisStatuses();

        newObj.Populate(issue, stopAtStatus);

        return newObj;
    }

    private void Populate(jtisIssue jtisIss, string? stopAtStatus)
    {
        if (jtisIss.ChangeLogs.Count() == 0) {return;}

        
        List<IssueChangeLog> _statusChanges = jtisIss.ChangeLogs.Where(x=>x.Items.Any(y=>y.FieldName.StringsMatch("status"))).OrderBy(z=>z.CreatedDate).ToList();

        if (_statusChanges.Count() == 0) {return;}

        bool foundFirstActive = false;
        for (int i = 0 ; i < _statusChanges.Count ; i ++)
        {
            bool lastStatus = (i == _statusChanges.Count -1);
            var cli = ChangeLogItem(_statusChanges[i]);
            jtisStatus? item = _statuses.SingleOrDefault(x=>x.IssueStatus.StringsMatch(cli.ToValue));            
            var statCat = StatusCategory(cli.ToId);
            // var statCat = jtisIss.StatusCategory;
            if (statCat == StatusType.stActiveState && foundFirstActive == false)
            {
                foundFirstActive = true;
                statCat = StatusType.stStart;
            }
            if (item == null) {
                JiraStatus? localStatus = CfgManager.config.StatusConfigs.FirstOrDefault(x=>x.StatusName.StringsMatch(cli.ToValue) && x.DefaultInUse==true);
                int seqNum = 0;
                if (localStatus != null) {seqNum=localStatus.ProgressOrder;}
                item = new jtisStatus(jtisIss.jIssue.Key,jtisIss.jIssue.IssueType,cli.ToValue ?? "ERROR", statCat, cli.ToId,seqNum);
                _statuses.Add(item);
            }
            item.AddEntry(cli.ToValue ?? "ERROR",_statusChanges[i].CreatedDate, lastStatus ? null : _statusChanges[i+1].CreatedDate, jtisIss);
        }

    }

    private StatusType StatusCategory(string toStatusId)
    {
        var jStatus = CfgManager.config.StatusConfigs.Single(x=>x.StatusId.ToString().StringsMatch(toStatusId));
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


    public TimeSpan tsCalendarTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                ts = ts.Add(s.tsCalendarTime);
            });
            return ts;
        }
    }
    public TimeSpan tsBusinessTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                ts = ts.Add(s.tsBusinessTime);
            });
            return ts;
        }
    }

    public TimeSpan tsBlockedCalendarTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                ts = ts.Add(s.tsBlockedCalendarTime);
            });
            return ts;
        }
    }

    public TimeSpan tsBlockedBusinessTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                ts = ts.Add(s.tsBlockedBusinessTime);
            });
            return ts;
        }
    }

    public TimeSpan tsUnblockedCalendarTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                ts = ts.Add(s.tsUnblockedCalendarTime);
            });
            return ts;
        }
    }

    public TimeSpan tsUnblockedBusinessTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                ts = ts.Add(s.tsUnblockedBusinessTime);
            });
            return ts;
        }
    }

    public TimeSpan tsBlockedActiveBusTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stActiveState || s.StatusCategory==StatusType.stStart)
                {
                    ts = ts.Add(s.tsBlockedBusinessTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan tsUnblockedActiveBusTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stActiveState || s.StatusCategory==StatusType.stStart)
                {
                    ts = ts.Add(s.tsUnblockedBusinessTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan tsBlockedActiveCalTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stActiveState || s.StatusCategory==StatusType.stStart)
                {
                    ts = ts.Add(s.tsBlockedCalendarTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan tsUnblockedActiveCalTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stActiveState || s.StatusCategory==StatusType.stStart)
                {
                    ts = ts.Add(s.tsUnblockedCalendarTime);
                }
            });
            return ts;
        }
    }

    public TimeSpan tsBlockedPassiveBusTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stPassiveState)
                {
                    ts = ts.Add(s.tsBlockedBusinessTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan tsUnblockedPassiveBusTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stPassiveState)
                {
                    ts = ts.Add(s.tsUnblockedBusinessTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan tsBlockedPassiveCalTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stPassiveState)
                {
                    ts = ts.Add(s.tsBlockedCalendarTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan tsUnblockedPassiveCalTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stPassiveState)
                {
                    ts = ts.Add(s.tsUnblockedCalendarTime);
                }
            });
            return ts;
        }
    }

    public TimeSpan tsActiveCalTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stActiveState || s.StatusCategory==StatusType.stStart)
                {
                    ts = ts.Add(s.tsCalendarTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan tsActiveBusTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stActiveState || s.StatusCategory==StatusType.stStart)
                {
                    ts = ts.Add(s.tsBusinessTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan tsPassiveCalTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stPassiveState)
                {
                    ts = ts.Add(s.tsCalendarTime);
                }
            });
            return ts;
        }
    }
    public TimeSpan tsPassiveBusTime {
        get {
            TimeSpan ts = new TimeSpan();
            _statuses.ForEach(s=> {
                if (s.StatusCategory==StatusType.stPassiveState)
                {
                    ts = ts.Add(s.tsBusinessTime);
                }
            });
            return ts;
        }
    }

    private IssueChangeLogItem ChangeLogItem(IssueChangeLog changeLog)
    {
        return changeLog.Items.Single(x=>x.FieldName.StringsMatch("status"));
    }
}
