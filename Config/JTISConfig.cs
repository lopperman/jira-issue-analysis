using System.Text.Json.Serialization;
using JTIS.Extensions;

namespace JTIS.Config
{
    public enum StatusType
    {
        stActiveState = 1, 
        stPassiveState = 2, 
        stIgnoreState = 3, 
        stStart = 4, 
        stEnd = 5, 
        stUnknown = 6
        // for any issue, 'Start' is the first active state that occurred
        // stStart = 5
    }    
    public class JTISConfig: IDisposable
    {
        private bool _validConn = false;
        private bool disposedValue;        
        private string? _timeZoneId = null;
        private List<JQLConfig> _savedJQL = new List<JQLConfig>();
        private List<JiraStatus> _statusConfigs = new List<JiraStatus>();
        private List<JiraStatus> _defaultStatusConfigs = new List<JiraStatus>();
        private SortedList<string,string> _CustomFields = new SortedList<string, string>();

        [JsonIgnore]
        public bool IsDirty {get;set;}
        public string? TimeZoneId 
        {
            get
            {
                return _timeZoneId;
            }
            set 
            {
                JTISTimeZone.Reset();
                _timeZoneId = value;
                JTISTimeZone.SetJTISTimeZone(this);
            }
        }            


        public JTISConfig()
        {
        }

        [JsonPropertyName("cfgId")]
        public int? configId {get; set;}

        [JsonPropertyName("cfgName")]
        public string? configName {get; set;}

        [JsonPropertyName("loginName")]
        public string? userName {get; set;}

        [JsonPropertyName("securityToken")]
        public string? apiToken {get; set;}

        [JsonPropertyName("jiraBaseUrl")]
        public string? baseUrl {get; set;}

        [JsonPropertyName("projectKey")]
        public string? defaultProject {get;set;}
        public DateTime? ServerInfoUpdated {get;set;}

        public bool DefaultTimeZoneDisplay()
        {
            return JTISTimeZone.DefaultTimeZone;
        }


        [JsonPropertyName("savedJQL")]
        public IReadOnlyList<JQLConfig> SavedJQL 
        {
            get 
            {
                return _savedJQL;
            }
        }
        public IReadOnlyList<JiraStatus> StatusConfigs 
        {
            get
            {
                return _statusConfigs;
            }
        }
        public IReadOnlyList<JiraStatus> DefaultStatusConfigs 
        {
            get
            {
                return _defaultStatusConfigs;
            }
        }
        

        [JsonIgnore]
        public int SavedJQLCount
        {
            get
            {
                return SavedJQL.Count;
            }
        }
        public bool HasSavedJQLQuery
        {
            get{
                return SavedJQL.Any(x=>x.JQLSyntax);
            }
        }
        public bool HasSavedJQLList
        {
            get{
                return SavedJQL.Any(x=>x.JQLSyntax==false);
            }
        }

        [JsonIgnore]
        public bool ValidConfig 
        {
            get
            {

                bool tmpValid = true;
                if (configId == 0)
                {
                    return tmpValid;
                }

                if (userName == null || userName.Length == 0)
                {
                    tmpValid = false;
                }
                if (apiToken==null || apiToken.Length==0)
                {
                    tmpValid = false;
                }
                if (baseUrl == null || baseUrl.Length==0)
                {
                    tmpValid = false;
                }
                if (defaultProject==null || defaultProject.Length==0)
                {
                    tmpValid = false;
                }
                if (configName == null || configName.Length==0)
                {
                    tmpValid = false;
                }
                if (configId == null || configId.Value <=0)
                {
                    tmpValid = false;
                }
                if (tmpValid == true && _validConn == false )
                {                    
                    if (JiraUtil.CreateRestClient(this)==true)
                    {
                        tmpValid = true;
                        _validConn = true ;
                    }
                    else 
                    {
                        tmpValid = false;
                    }
                }
                return tmpValid;
            }
        }


        public void AddJQL(JQLConfig jc)
        {
            jc.jqlId = SavedJQLCount + 1;
            _savedJQL.Add(jc);

            _savedJQL = _savedJQL.OrderBy(x=>x.jqlName).ToList();
            for (int i = 0; i < _savedJQL.Count; i ++)
            {
                _savedJQL[i].jqlId = i + 1;
            }

            IsDirty = true;
            
        }
        public void AddJQL(string shortName, string saveJql )
        {
            if (_savedJQL.Exists(x=>x.jqlName.StringsMatch(shortName)))
            {
                int iCounter = 1;
                string newName = string.Empty;
                while(true)
                {
                    iCounter +=1;
                    newName = $"{shortName} - {iCounter}";
                    if (!_savedJQL.Exists(x=>x.jqlName.StringsMatch(newName)))
                    {
                        shortName = newName;
                        break;
                    }

                }
            }
            JQLConfig cfg = new JQLConfig(shortName,saveJql);
            AddJQL(cfg);
        }
        public void DeleteJQL(JQLConfig cfg)
        {
            IsDirty = true;
            var delCfg = _savedJQL.FirstOrDefault(x=>x.jqlName.StringsMatch(cfg.jqlName));
            if (delCfg != null)
            {
                _savedJQL.Remove(delCfg);
                if (SavedJQLCount > 0)
                {
                    for (int i = 0; i < SavedJQLCount; i ++)
                    {
                        SavedJQL[i].jqlId = i + 1;
                    }
                }
                JTISConfigHelper.SaveConfigList(this);
            }
        }
        public void UpdateStatusCfgLocal(JiraStatus jStatus)
        {
            if (_statusConfigs.Exists(x=>x.StatusId == jStatus.StatusId))
            {
                _statusConfigs.RemoveAll(x=>x.StatusId == jStatus.StatusId);
            }
            _statusConfigs.Add(jStatus);
            IsDirty = true;
        }
        public void ResetLocalIssueStatusCfg()
        {
            _statusConfigs.Clear();
            IsDirty = true;
        }
        public void ResetOnlineIssueStatusCfg()
        {
            _defaultStatusConfigs.Clear();
            IsDirty = true;
        }
        public void UpdateStatusCfgOnline(JiraStatus jStatus)
        {
            if (_defaultStatusConfigs.Exists(x=>x.StatusId == jStatus.StatusId))
            {
                _defaultStatusConfigs.RemoveAll(x=>x.StatusId == jStatus.StatusId);
            }
            _defaultStatusConfigs.Add(jStatus);
            IsDirty = true;
        }
        public JQLConfig? GetJQLConfig(int jqlId)
        {
            return SavedJQL.FirstOrDefault<JQLConfig>(x=>x.jqlId == jqlId);
        }
        public JQLConfig? GetJQLConfig(string cfgName)
        {
            return SavedJQL.FirstOrDefault<JQLConfig>(x=>x.jqlName == cfgName);
        }

        public List<string> JQLNames()
        {
            List<string> retNames = new List<string>();
            if (SavedJQLCount > 0)
            {
                for (int i = 0; i < SavedJQLCount; i ++)
                {
                    retNames.Add(string.Format("{0:00} - {1}",SavedJQL[i].jqlId,SavedJQL[i].jqlName));
                }
            }
            return retNames;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public override string ToString()
        {
            return string.Format("{0:00} | {1}",this.configId,this.configName);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
