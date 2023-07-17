using Atlassian.Jira;
using JTIS.Extensions;
using Newtonsoft.Json;

namespace JTIS
{
    public class JIssue:IComparable, IComparer<JIssue>
    {
        private Issue? _issue ;
        private List<string> _components = new List<string>();
        private List<JCustomField> _customFields = new List<JCustomField>();
        private List<string> _labels = new List<string>();
        // private List<JIssue> _subTasks = new List<JIssue>();
        private List<JIssueChangeLog> _changeLogs = new List<JIssueChangeLog>();

        //TODO:  what to do about initializing subTasks and changeLogs

        public JIssue()
        {
            Key = string.Empty ;
            Project = string.Empty ;
            SecurityLevel = string.Empty;
            StatusName = string.Empty;
            StatusDescription = string.Empty;
            StatusCategoryKey = string.Empty;
            ParentIssueKey = string.Empty;
            IssueType = string.Empty;
            Summary = string.Empty;
            ParentKey = string.Empty;
            StatusCategoryName = string.Empty ;
        }

        public JIssue(Issue issue): this()
        {
            this._issue = issue;
            Initialize();
        }


        #region Properties

        public string Key { get; set; }
        public string Project { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ResolutionDate { get; set; }
        public string SecurityLevel { get; set; }
        public string StatusName { get; set; }
        public string StatusDescription { get; set; } 
        public string StatusCategoryKey { get; set; }
        public string StatusCategoryName { get; set; }
        public string ParentIssueKey { get; set; }
        public string IssueType { get; set; }
        public string Summary { get; set; }
        public string ParentKey { get; set; }

        private void Initialize()
        {
            if (_issue == null) return;

            Key = _issue.Key.Value;
            Summary = _issue.Summary.Replace(",", " ");
            Project = _issue.Project;
            CreateDate = _issue.Created ?? null;
            UpdateDate = _issue.Updated ?? null;
            DueDate = _issue.DueDate ?? null;
            ResolutionDate = _issue.ResolutionDate ?? null;
            SecurityLevel = _issue.SecurityLevel == null ? null : _issue.SecurityLevel.Description;
            if (_issue.Status != null)
            {
                StatusName = _issue.Status.Name;
                StatusDescription = _issue.Status.Description;
                StatusCategoryKey = _issue.Status.StatusCategory.Key;
                StatusCategoryName = _issue.Status.StatusCategory.Name;
            }
            ParentIssueKey = _issue.ParentIssueKey;
            IssueType = _issue.Type.Name;

            if (_issue.CustomFields != null)
            {
                foreach (var cf in _issue.CustomFields)
                {
                    _customFields.Add(new JCustomField(cf.Id, cf.Name, cf.Values));
                }
            }

            if (_issue.Labels != null && _issue.Labels.Count > 0)
            {
                foreach (string child in _issue.Labels)
                {
                    _labels.Add(child);
                }
            }
        }
        public string EpicLinkKey
        {
            get
            {
                string ret = string.Empty;

                if (!string.IsNullOrWhiteSpace(JiraUtil.JiraRepo.EpicLinkFieldName))
                {
                    var epicLinkCustomField = _customFields.Where(x => x.Id == JiraUtil.JiraRepo.EpicLinkFieldName).SingleOrDefault();
                    if (epicLinkCustomField != null)
                    {
                        if (epicLinkCustomField.Values.Length > 0)
                        {
                            return epicLinkCustomField.Values[0];
                        }
                    }
                }
                return ret;
            }
        }

        [JsonIgnore]
        public Issue JiraIssue
        {
            get
            {
                return _issue;
            }
            set
            {
                _issue = value;
                Initialize();

            }
        }
        public void AddChangeLogs(IEnumerable<IssueChangeLog> logs, bool clearExisting = false)
        {
            if (clearExisting)
            {
                _changeLogs.Clear();
            }
            foreach (var l in logs)
            {
                AddChangeLog(l);
            }
        }

        public void AddChangeLog(IssueChangeLog changeLog)
        {
            _changeLogs.Add(new JIssueChangeLog(this, changeLog));
        }

        public List<JIssueChangeLog> ChangeLogs
        {
            get
            {
                if (_changeLogs == null) _changeLogs = new List<JIssueChangeLog>();
                return _changeLogs.OrderBy(x=>x.CreatedDate).ToList();
            }
            set
            {
                _changeLogs = value;
            }

        }

        public bool IsBlocked {
            get{
                var blocked = false;
                if (_issue.Priority != null && _issue.Priority.Name.StringsMatch("block",StringCompareType.scContains))
                {
                    blocked = true;
                }
                else if (_issue.Status != null && _issue.Status.Name.StringsMatch("block",StringCompareType.scContains))
                {
                    blocked = true;
                }
                else 
                {
                    var flaggedField = _issue.CustomFields.Where(x=>x.Name.StringsMatch("flagged")).FirstOrDefault();
                    if (flaggedField != null && flaggedField.Values != null)
                    {
                        var flagVals = flaggedField.Values.ToList();
                        if (flagVals.Any(x=>x.StringsMatch("impediment")))
                        {
                            blocked = true ;
                        }
                        else if (flagVals.Any(x=>x.StringsMatch("block",StringCompareType.scContains)))
                        {
                            blocked = true;
                        }
                    }
                }
                return blocked;
            }

        }

        public List<T>? GetCustomFieldValues<T>(string customFieldName)
        {
            List<T>? ret = null;

            var find = _customFields.Where(x => x.Id == customFieldName || x.Name == customFieldName).FirstOrDefault();

            if (find != null)
            {
                ret = new List<T>();
                foreach (string s in find.Values)
                {
                    ret.Add(JHelper.GetValue<T>(s));
                }
            }

            return ret ;
        }

        public int CompareTo(object obj)
        {
            JIssue other = (obj as JIssue);

            return Key.CompareTo(other.Key);
        }

        public int Compare(JIssue x, JIssue y)
        {
            return x.Key.CompareTo(y.Key);
        }

        #endregion


    }
}
