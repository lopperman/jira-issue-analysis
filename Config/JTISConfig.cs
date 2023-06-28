using System.Text.Json.Serialization;
using JTIS.Console;

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

#region JTIS TIME ZONE
    public static class JTISTimeZone
    {
        private static TimeZoneInfo? _timeZoneInfo = null;
        private static int? cfgId;

        public static TimeZoneInfo DisplayTimeZone
        {
            get
            {
                if (_timeZoneInfo != null)
                {
                    return _timeZoneInfo ;
                }
                else 
                {
                    return TimeZoneInfo.Local;
                }
            }
        }

        public static bool DefaultTimeZone
        {
            get{
                return DisplayTimeZone.Id.Equals(TimeZoneInfo.Local.Id,StringComparison.OrdinalIgnoreCase);
            }
        }
        public static void Reset()
        {
            _timeZoneInfo = null;
            cfgId = 0;
        }
        public static void SetJTISTimeZone(JTISConfig cfg)
        {
            cfgId = cfg.configId;
            _timeZoneInfo = null;
            var lookupTZ = FindTimeZone(cfg.TimeZoneId);
            if (lookupTZ != null)
            {
                _timeZoneInfo = lookupTZ;
            }
        }
        private static TimeZoneInfo? FindTimeZone(string? id)
        {
            TimeZoneInfo? retTZ = null;

            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }
            else 
            {
                try 
                {
                    var findTZ = TimeZoneInfo.FindSystemTimeZoneById(id);
                    if (findTZ != null)
                    {
                        retTZ = findTZ;
                    }
                }
                catch (Exception exObj)
                {
                    ConsoleUtil.WriteError(string.Format("Error parsing TimeZone Id '{0}'",id),false,ex:exObj,pause:true);
                }
            }
            return retTZ;
        }
    }
#endregion
    public class JTISConfig: IDisposable
    {
        private bool _validConn = false;
        private bool disposedValue;
        
        private string? _timeZoneId = null;
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

        public bool DefaultTimeZoneDisplay()
        {
            return JTISTimeZone.DefaultTimeZone;
        }

        // public bool DefaultTimeZoneDisplay
        // {
        //     get
        //     {
        //         if (string.IsNullOrWhiteSpace(TimeZoneId))
        //         {
        //             return true;
        //         }
        //         else 
        //         {
        //             var tz = JTISTimeZone.SetJTISTimeZone
        //         }
        //         if (TimeZoneDisplay == null) {TimeZoneDisplay = TimeZoneInfo.Local;}
        //         return TimeZoneDisplay.Equals(TimeZoneInfo.Local);
        //     }
        // }
        // public string TimeZoneDisplayInfo()
        // {
        //     if (_timeZoneInfo == null)
        //     {
        //         if (!string.IsNullOrEmpty(TimeZoneId ))
        //         {
        //             var tzAttach = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x=>x.Id == TimeZoneId);
        //             if (tzAttach != null)
        //             {
        //                 _timeZoneInfo = tzAttach;
        //             }
        //         }
        //         if (_timeZoneInfo == null) 
        //         {
        //             UpdateDisplayTimeZone(TimeZoneInfo.Local);
        //         }
        //     }
        //     if (DefaultTimeZoneDisplay)
        //     {
        //         return $"Showing Times as your local zone: ({TimeZoneDisplay.DisplayName})";
        //     }
        //     else 
        //     {
        //         return $"Showing Times as ** CUSTOMIZED **: ({TimeZoneDisplay.DisplayName})";
        //     }
        // }
        // public void UpdateDisplayTimeZone(TimeZoneInfo tzInfo)
        // {
        //     this.TimeZoneDisplay

        //     bool tzDirty = false;
        //     if (_timeZoneInfo == null)
        //     {
        //         tzDirty = true;
        //     }
        //     else 
        //     {
        //         if (_timeZoneInfo.Equals(tzInfo)==false)               
        //         {
        //             tzDirty = true;
        //         }
        //     }
        //     _timeZoneInfo = tzInfo;
        //     TimeZoneId = tzInfo.Id;
        //     if (tzDirty)
        //     {
        //         JTISConfigHelper.SaveConfigList();
        //     }
        // }

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
