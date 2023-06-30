
using Atlassian.Jira;
using JTIS.Console;
using JTIS.Extensions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;
using Spectre.Console;

namespace JTIS.Config
{
    public class JTISConfig
    {
        public Guid? Key {get;set;}
        public bool IsDirty {get; set;}

        [JsonIgnore]
        private JiraRepo? _jiraRepo = null;
        [JsonIgnore]
        public JiraRepo? jiraRepo 
        {
            get{
                return _jiraRepo;
            }
        }
        [JsonIgnore]
        public Jira? jira 
        {
            get{
                return _jiraRepo  != null ? _jiraRepo.jira : null;
            }
        }
        private int _configId;
        [JsonProperty("cfgId")]
        public int configId 
        {
            get
            {
                return _configId;
            }
        }
        private string? _timeZoneId = null;
        private List<JQLConfig> _savedJQL = new List<JQLConfig>();
        private List<JiraStatus> _statusConfigs = new List<JiraStatus>();
        private List<JiraStatus> _defaultStatusConfigs = new List<JiraStatus>();
        private SortedList<string,string> _CustomFields = new SortedList<string, string>();

        private JTISConfig()
        {
            //need for serialization
        }
        private JTISConfig(string login, string token, string jiraRootUrl, string defPrj, int cfgId)
        {
            userName = login;
            apiToken = token;
            baseUrl = jiraRootUrl;
            defaultProject = defPrj;
            _configId = cfgId;
            configName = string.Format("CFG{0:00} - {1} - {2}",configId,baseUrl,defaultProject);
        }

        public static JTISConfig? Create(string login, string apiToken, string baseUrl, string defPrj, int cfgId)
        {
            var tmpCfg = new JTISConfig(login,apiToken,baseUrl,defPrj, cfgId);
            if (tmpCfg.Connected)
            {
                return tmpCfg;
            }
            return null;
        }
        public static JTISConfig? Create(JTISConfig deserializedCfg)
        {
            if (deserializedCfg.Connected)
            {
                return deserializedCfg;
            }
            else 
            {
                return null;
            }
        }

        [JsonIgnore]
        public bool Connected
        {

            get{
                bool tConnected = false;
                if (jira != null){
                    tConnected = true;
                }
                else {
                    try{
                        if (JiraUtil.CreateRestClient(userName,apiToken,baseUrl))
                        {
                            _jiraRepo = JiraUtil.JiraRepo;
                        }
                        if (jira != null)
                        {
                            Initialize(defaultProject);
                            tConnected = true;
                        }
                    }
                    catch (System.Exception excep){
                        ConsoleUtil.WriteError("Error connecting to Jira",ex:excep);
                        tConnected = false;
                    }
                }
                return tConnected;
            }

        }

        private void Initialize(string defProject)
        {
            bool isNewFile = Key == null;
            if (isNewFile){Key = Guid.NewGuid();}
            UpdateDefaultStatusConfigs(defProject, isNewFile);
        }

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



        [JsonProperty("configName")]
        public string? configName {get; private set;}

        [JsonProperty("userName")]
        public string? userName {get; set;}

        [JsonProperty("apiToken")]
        public string? apiToken {get; set;}

        [JsonProperty("baseUrl")]
        public string? baseUrl {get; set;}

        [JsonProperty("defaultProject")]
        public string? defaultProject {get;set;}
        public DateTime? ServerInfoUpdated {get;set;}

        public bool DefaultTimeZoneDisplay()
        {
            return JTISTimeZone.DefaultTimeZone;
        }


        [JsonProperty("SavedJQL")]
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
        [JsonIgnore]
        public bool HasSavedJQLQuery
        {
            get{
                return SavedJQL.Any(x=>x.JQLSyntax);
            }
        }
        [JsonIgnore]
        public bool HasSavedJQLList
        {
            get{
                return SavedJQL.Any(x=>x.JQLSyntax==false);
            }
        }

        public void AddJQL(JQLConfig jc, bool saveFile = false)
        {
            jc.jqlId = SavedJQLCount + 1;
            _savedJQL.Add(jc);

            _savedJQL = _savedJQL.OrderBy(x=>x.jqlName).ToList();
            for (int i = 0; i < _savedJQL.Count; i ++)
            {
                _savedJQL[i].jqlId = i + 1;
            }

            if (saveFile)
            {
                CfgManager.SaveConfigList();
            }
            else 
            {
                IsDirty = true;
            }            
        }
        public void AddJQL(string shortName, string saveJql , bool saveFile = false)
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
            AddJQL(cfg, saveFile);

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
                CfgManager.SaveConfigList();
            }
        }
        private void UpdateStatusCfgLocal(JiraStatus jStatus)
        {
            if (_statusConfigs.Exists(x=>x.StatusId == jStatus.StatusId))
            {
                _statusConfigs.RemoveAll(x=>x.StatusId == jStatus.StatusId);
            }
            _statusConfigs.Add(jStatus);
            IsDirty = true;
        }
        private void ResetLocalIssueStatusCfg()
        {
            _statusConfigs.Clear();
            IsDirty = true;
        }
        private void ResetOnlineIssueStatusCfg()
        {
            _defaultStatusConfigs.Clear();
            IsDirty = true;
        }
        private void UpdateStatusCfgOnline(JiraStatus jStatus)
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

        public void UpdateDefaultStatusConfigs(string defProject,  bool clearLocal = false)
        {
            List<JiraStatus>? statusAll ; 
            List<JiraStatus>? statusProj;

            AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(), 
                    new PercentageColumn(),
                    new ElapsedTimeColumn(), 
                    new SpinnerColumn()
                })
                .Start(ctx => 
                {
                    var task1 = ctx.AddTask("[blue on white]Get Jira Statuses[/]");
                    var task2 = ctx.AddTask("[blue on white]Check Default Project Statuses[/]");        
                    var task3 = ctx.AddTask("[blue on white]Check Missing Status Configs[/]");        

                    task1.MaxValue = 2;
                    statusAll = GetJiraStatuses(defProject, false);
                    task1.Increment(1);
                    statusProj = GetJiraStatuses(defProject, true);
                    task1.Increment(1);

                    task2.MaxValue = statusAll.Count;
                    for (int i = 0; i < statusAll.Count; i ++)
                    {
                        if (statusProj.Exists(x=>x.StatusId == statusAll[i].StatusId))
                        {
                            statusAll[i].DefaultInUse = true;
                        }
                        task2.Increment(1);                        
                    }

                    task3.MaxValue = 3;

                    ResetOnlineIssueStatusCfg();
                    foreach (var stCfg in statusAll)
                    {
                        UpdateStatusCfgOnline(stCfg);
                    }
                    task3.Increment(1);
                    if (clearLocal || StatusConfigs.Count == 0)
                    {
ResetLocalIssueStatusCfg();
                        foreach (var tmpStCfg in statusAll)
                        {
                            UpdateStatusCfgLocal(tmpStCfg);
                        }
                        task3.Increment(1);
                    }
                    else 
                    {
                        FillMissingStatusConfigs();
                        task3.Increment(1);
                    }
                    CfgManager.SaveConfigList();
                    task3.Increment(1);
                });
        }

        private void FillMissingStatusConfigs()
        {
            if (DefaultStatusConfigs.Count > 0 )
            {
                for (int i = 0; i < DefaultStatusConfigs.Count; i ++)
                {
                    var dflt = DefaultStatusConfigs[i];
                    if (StatusConfigs.SingleOrDefault(x=>x.StatusId  == dflt.StatusId )==null)
                    {
                        UpdateStatusCfgLocal(dflt);
                    }
                }
                if (IsDirty)
                {
                    CfgManager.SaveConfigList();
                }
            }
        }

        private List<JiraStatus> GetJiraStatuses(string defProject,  bool defaultProjectOnly = true)
        {

            List<JiraStatus> ret = new List<JiraStatus>();
            string data = string.Empty;
            if (defaultProjectOnly)
            {
                data = jiraRepo.GetProjectItemStatusesAsync(defProject).GetAwaiter().GetResult(); 
                JArray json = JArray.Parse(data);
                for (int i = 0; i < json.Count; i++)
                {
                    JToken jt = json[i]["statuses"];                      
                    for (int k = 0; k < jt.Count(); k ++)
                    {
                        JToken j = jt[k].Value<JToken>();
                        JiraStatus checkStatus = new JiraStatus(j);
                        if (ret.Exists(x=>x.StatusId == checkStatus.StatusId)==false)          
                        {
                            ret.Add(checkStatus);
                        }
                    }
                }
            }
            else 
            {
                data = jiraRepo.GetItemStatusesAsync().GetAwaiter().GetResult(); 
                JArray json = JArray.Parse(data);
                for (int i = 0; i < json.Count; i++)
                {
                    JToken j = json[i].Value<JToken>();
                    JiraStatus checkStatus = new JiraStatus(j);
                    if (ret.Exists(x=>x.StatusId == checkStatus.StatusId)==false)          
                    {
                        ret.Add(checkStatus);
                    }
                }

            }
            return ret;
        }

 

        public override string ToString()
        {
            return string.Format($"{configId:00} | {configName}");
        }

        internal void UpdateConfigId(int newCfgId)
        {
            if (newCfgId.Equals(configId)==false)
            {
                _configId = newCfgId;
                configName = string.Format("CFG{0:00} - {1} - {2}",configId,baseUrl,defaultProject);
                IsDirty = true;
            }
        }

        internal static JTISConfig? ManualCreate()
        {
            var tLogin = ConsoleUtil.GetInput<string>("Enter Jira Login email address");
            var tAPIToken = ConsoleUtil.GetInput<string>("Enter Jira API Token");
            AnsiConsole.MarkupLine($"[dim](Example of Url: https://yourcompany.Atlassian.net/)[/]");
            var tURL = ConsoleUtil.GetInput<string>("Enter Jira base URL {}");
            AnsiConsole.MarkupLine($"[dim](A Jira Project is usually the character that appear [italic]before[/] the number in a Jira Issue, such as 'WWT' in Jira Issues 'WWT-100')[/]");
            var tProj = ConsoleUtil.GetInput<string>("Enter Default Project Key");

            AnsiConsole.Write(new Rule($"[blue on white][/]").Centered());
            AnsiConsole.Write(new Rule($"[darkred on white][bold]Login: [/]{tLogin}[/]").LeftJustified());
            AnsiConsole.Write(new Rule($"[darkred on white][bold]APIToken: [/]{tAPIToken}[/]").LeftJustified());
            AnsiConsole.Write(new Rule($"[darkred on white][bold]Jira Url: [/]{tURL}[/]").LeftJustified());
            AnsiConsole.Write(new Rule($"[darkred on white][bold]Default Project: [/]{tProj}[/]").LeftJustified());

            if (ConsoleUtil.Confirm($"Attempt to Create a new Jira Connection Profile using entered information above?",true))
            {
                var testCFG = JTISConfig.Create(tLogin,tAPIToken,tURL,tProj,CfgManager.ConfigCount + 1);
                if (testCFG != null) 
                {
                    var prj = testCFG.jira.Projects.GetProjectAsync(tProj).GetAwaiter().GetResult();
                    if (prj.Key.StringsMatch(tProj)==false)
                    {
                        if (ConsoleUtil.Confirm($"Project Key '{tProj}' is not a valid project key. Do you want to select a valid key from Jira?",true))
                        {
                            IEnumerable<Project> prjList = testCFG.jira.Projects.GetProjectsAsync().GetAwaiter().GetResult();
                            var p = new SelectionPrompt<Project>();
                            p.Title("Select Project");
                            p.AddChoices(prjList);
                            var selProj = p.Show(AnsiConsole.Console);
                            testCFG.defaultProject = selProj.Key;
                            ConsoleUtil.PressAnyKeyToContinue($"Project Key Changed to: {selProj.Key} ");
                            return testCFG;                            
                        }
                    else 
                        return testCFG;
                    }
                }
            }
            return null;

        }
    }
}
