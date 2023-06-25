using System.Reflection.Metadata.Ecma335;
using System.Dynamic;
using System.IO;
using System;
using System.Text.Json.Serialization;

namespace JiraCon
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
        // private const string CFG_FIELD_ID = "configId";
        // private const string CFG_FIELD_NAME = "configName";
        // private const string CFG_FIELD_USERNAME = "username";
        // private const string CFG_FIELD_APITOKEN = "apitoken";
        // private const string CFG_FIELD_BASEURL = "jiraurl";
        // private const string CFG_FIELD_PROJECT = "project";
        private bool _validConn = false;
        private bool disposedValue;
        [JsonIgnore]
        public bool IsDirty {get;set;}

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
        
        private List<JQLConfig> _savedJQL = new List<JQLConfig>();
        private List<JiraStatus> _statusConfigs = new List<JiraStatus>();
        private List<JiraStatus> _defaultStatusConfigs = new List<JiraStatus>();


        [JsonIgnore]
        public int SavedJQLCount
        {
            get
            {
                return SavedJQL.Count;
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
            IsDirty = true;
        }
        public void AddJQL(string shortName, string saveJql )
        {
            JQLConfig cfg = new JQLConfig(shortName,saveJql);
            AddJQL(cfg);
        }
        public void DeleteJQL(JQLConfig cfg)
        {
            IsDirty = true;
            _savedJQL.Remove(cfg);
            if (SavedJQLCount > 0)
            {
                for (int i = 0; i < SavedJQLCount; i ++)
                {
                    SavedJQL[i].jqlId = i + 1;
                }
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
