using System;
using System.Collections.Generic;
using System.Linq;
using JiraCon;

namespace JConsole.Utilities
{

    public class WorkMetrics
    {
        private SortedList<JIssue, List<WorkMetric>> _workMetricList = new SortedList<JIssue, List<WorkMetric>>();
        private JiraRepo _repo = null;
        private int _startHour = 0;
        private int _endHour = 0;
        private SortedDictionary<string, string> _forceIgnoreKeyAndReason = new SortedDictionary<string, string>();
           

        public WorkMetrics(JiraRepo repo, int startHour, int endHour)
        {

            if (repo == null)
            {
                throw new NullReferenceException("class 'WorkMetrics' cannot be instantiated with a null JiraRepo object");
            }
            _repo = repo;

            _startHour = startHour;
            _endHour = endHour;
        }

        public SortedList<JIssue,List<WorkMetric>> GetWorkMetrics()
        {
            return _workMetricList;
        }

        public List<WorkMetric> AddIssue(JIssue issue, List<JIssue> jIssues)
        {
            var ret = BuildIssueMetrics(issue, jIssues);
            _workMetricList.Add(issue, ret);
            return ret;
        }

        public void AddIssue(List<JIssue> issues)
        {
            foreach (var iss in issues)
            {
                AddIssue(iss,issues);
            }
        }

        private List<WorkMetric> BuildIssueMetrics(JIssue issue, List<JIssue> jIssues)
        {
            var ret = new List<WorkMetric>();

            SortedDictionary<DateTime, JItemStatus> statusStartList = new SortedDictionary<DateTime, JItemStatus>();

            foreach (var changeLog in issue.ChangeLogs)
            {
                var items = changeLog.Items.Where(item => item.FieldName == "status");
                foreach (JIssueChangeLogItem item in items)
                {
                    var itemStatus = _repo.JItemStatuses.SingleOrDefault(y=>y.StatusName.ToLower() == item.ToValue.ToLower());
                    if (itemStatus == null && issue.IssueType.ToLower() != "epic")
                    {
                        var err = string.Format("Error getting JItemStatus for Issue {0} ({1}).  Cannot determine calendar/active work time for state: '{2}'.  (** If this item SHOULD be included in calendarWork or activeWork calculations then a value will need to be added to the JiraConIssueStatus.txt for status: {2})", issue.Key, issue.IssueType, item.ToValue);
                        ConsoleUtil.WriteLine(err,ConsoleColor.DarkRed,ConsoleColor.Gray,false);
                        ConsoleUtil.WriteLine("PRESS ANY KEY TO CONTINUE", ConsoleColor.DarkRed, ConsoleColor.Gray, false);
                        var ok = Console.ReadKey(true);
                    }
                    if (itemStatus == null)
                    {
                        itemStatus = new JItemStatus(item.ToValue.ToLower(), "ERROR", "ERROR", "ERROR");
                    }
                    statusStartList.Add(changeLog.CreatedDate, itemStatus);
                }
            }

            if (statusStartList.Count == 0)
            {
                return ret;
            }

            var keys = statusStartList.Keys.ToList().OrderBy(x => x).ToList();

            string forceIgnoreReason = null;
            if (_forceIgnoreKeyAndReason.ContainsKey(issue.Key))
            {
                forceIgnoreReason = _forceIgnoreKeyAndReason[issue.Key];
            }

            for (int i = 0; i < keys.Count; i ++)
            {
                //takes care of entries for first (if more than one item exists) and last
                if (keys.Count == 1)
                {
                    ret.Add(new WorkMetric(statusStartList[keys[i]], keys[i], DateTime.Now,_startHour,_endHour, issue,jIssues, forceIgnoreReason));
                }
                else if (i == keys.Count -1)
                {
                    ret.Add(new WorkMetric(statusStartList[keys[i - 1]], keys[i - 1], keys[i], _startHour, _endHour, issue, jIssues, forceIgnoreReason));
                    ret.Add(new WorkMetric(statusStartList[keys[i]], keys[i], DateTime.Now, _startHour, _endHour, issue, jIssues, forceIgnoreReason));
                }
                else if (i > 0)
                {
                    ret.Add(new WorkMetric(statusStartList[keys[i - 1]], keys[i - 1], keys[i], _startHour, _endHour, issue, jIssues, forceIgnoreReason));
                }
            }

            return ret;
        }

        internal void AddForceIgnore(string key, string reason)
        {
            _forceIgnoreKeyAndReason.Add(key, reason);
        }
    }

    public class WorkMetric
    {
        public JItemStatus ItemStatus { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        private string _forceIgnoreReason = null;

        public bool Exclude
        {
            get
            {
                return _excludeReasons != null && _excludeReasons.Length > 0;
            }
        }
        private string _excludeReasons = string.Empty;

        public string ExcludeReasons
        {
            get
            {
                return _excludeReasons;
            }
        }

        private void AddExclusion(string reason)
        {
            if (_excludeReasons.Length == 0)
            {
                _excludeReasons = reason;
            }
            else
            {
                _excludeReasons = string.Format("{0}; {1}", _excludeReasons, reason);
            }
        }

        public WorkMetric(JItemStatus itemStatus, DateTime start, DateTime end, int startHour, int endHour, JIssue issue, List<JIssue> issues, string forceIgnoreReason)
        {
            ItemStatus = itemStatus;
            Start = start;
            End = end;
            StartHour = startHour;
            EndHour = endHour;
            if (!string.IsNullOrWhiteSpace(forceIgnoreReason))
            {
                _forceIgnoreReason = forceIgnoreReason;
            }

            CalculateExclusions(issue, issues);
        }



        public void CalculateExclusions(JIssue issue, List<JIssue> issues)
        {
            if (!string.IsNullOrWhiteSpace(_forceIgnoreReason))
            {
                AddExclusion(_forceIgnoreReason);
                return;
            }

            if (!IncludeForTimeCalc)
            {
                AddExclusion("Non-working Status");
            }
            //TODO:  make these labels configurable
            if (issue.Labels != null && issue.Labels.Count > 0)
            {
                foreach (string label in issue.Labels)
                {
                    if (label.ToLower().Contains("jm") || label.ToLower().Contains("jm-work") || label.ToLower().Contains("jm_work") || label.ToLower().Contains("jmwork"))
                    {
                        AddExclusion("JM Work");
                        break;
                    }
                }
            }
            //check if parent of sub-tasks
            if (issue.IssueType.ToLower() == "epic")
            {
                AddExclusion("Issue Type = Epic");
            }


        }

        public double TotalDays
        {
            get
            {
                return Math.Round(End.Subtract(Start).TotalDays, 2);
            }
        }
        public double TotalHours
        {
            get
            {
                return Math.Round(End.Subtract(Start).TotalHours, 1);
            }
        }


        public double TotalWeekdays
        {
            get
            {
                return Math.Round(RemoveWeekends(Start, End).TotalDays,2);
            }
        }

        public double TotalWeekdayHours
        {
            get
            {
                return Math.Round(RemoveWeekends(Start, End).TotalHours, 2);
            }
        }

        public double TotalBusinessDays
        {
            get
            {
                return Math.Round((RemoveWeekendsAndAfterHours(Start, End).TotalHours/ (EndHour-StartHour)), 2);
            }
        }

        public double TotalBusinessHours
        {
            get
            {
                return Math.Round(RemoveWeekendsAndAfterHours(Start, End).TotalHours, 2);
            }
        }

        public double Total8HourAdjBusinessHours
        {
            get
            {
                //cannnot have more than 8 hours * #business days
                double avgBusHOurs = TotalBusinessHours / TotalBusinessDays;
                if (avgBusHOurs > 8 && (EndHour - StartHour) > 8)
                {
                    var over8Hours = (EndHour - StartHour) - 8;
                    double adjPerc = 1 - (over8Hours / avgBusHOurs);
                    return Math.Round(TotalBusinessHours * adjPerc);
                }
                return TotalBusinessHours;
            }
        }

        public bool TransitionAfterHours
        {
            get
            {
                return (Start.Hour < StartHour || Start.Hour > EndHour || End.Hour > EndHour || End.Hour < StartHour);
            }
        }

        public bool IncludeForTimeCalc
        {
            get
            {
                return (ItemStatus.ActiveWork || ItemStatus.CalendarWork);
            }
        }

        public TimeSpan RemoveWeekends(DateTime start, DateTime end)
        {
            TimeSpan ts = end.Subtract(start);

            for (DateTime d = start; d < end; d = d.AddMinutes(5))
            {
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    if (ts.TotalMinutes > 5)
                    {
                        ts -= TimeSpan.FromMinutes(5);
                    }
                }
            }

            return ts;
        }

        public TimeSpan RemoveWeekendsAndAfterHours(DateTime start, DateTime end)
        {
            TimeSpan ts = end.Subtract(start);

            for (DateTime d = start; d < end; d = d.AddMinutes(5))
            {
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    if (ts.TotalMinutes > 5)
                    {
                        ts -= TimeSpan.FromMinutes(5);
                    }
                }
                else if (d.Hour < StartHour || d.Hour > EndHour)
                {
                    ts -= TimeSpan.FromMinutes(5);
                }
            }

            return ts;
        }


        //        public // what day/hour combination do we use?
    }



}
