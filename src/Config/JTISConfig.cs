using System.Linq;
using Atlassian.Jira;
using JTIS.Console;
using JTIS.Extensions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Spectre.Console;

namespace JTIS.Config
{
    public class JTISConfig
    {        
        private IssueNotes? _issNotes;
        private CfgOptions? _cfgOptions;
        
        private string? _timeZoneId;

        //A LIST OF STRINGS WHICH WILL GET SCRUBBED WHEN WRITING TO SCREEN
        //MAKES IT EASIER TO TAKE SCREENSHOTS AND SHARE ON GITHUB!
        public List<string>? ScrubData {get;set;}

        public IReadOnlyList<string> ScrubList()
        {
            if (ScrubData == null) { ScrubData = new List<string>();}
            return ScrubData;
        }
        public void DeleteScrubTerms(params string[] items)
        {
            foreach (var item in items)
            {
                if (ScrubData.Exists(x=>x.StringsMatch(item)))
                {
                    ScrubData.Remove(item);
                }
            }
        }
        public void AddScrubTerms(params string[] items)
        {
            if (ScrubData == null) {ScrubData = new List<string>();} 
            foreach (var item in items)
            {
                if (!ScrubData.Exists(x=>x.StringsMatch(item)))
                {
                    ScrubData.Add(item);
                }
            }
        }

        [JsonProperty]
        public CfgOptions? cfgOptions
        {
            get 
            {
                if (_cfgOptions == null) { _cfgOptions = new CfgOptions();}
                return _cfgOptions;                
            }
            set 
            {
                _cfgOptions = value;
            }
        }

        [JsonProperty]
        public IssueNotes? issueNotes 
        {
            get{
                if (_issNotes == null) {_issNotes = new IssueNotes();}
                return _issNotes;
            }       
            set{
                _issNotes = value;
            }
        }

        [JsonProperty("Key")]
        public Guid? Key {get;set;}
        
        [JsonIgnore]
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

        public JArray? Fields {get;set;}
        private int _configId;

        [JsonProperty("cfgId")]
        public int configId 
        {
            get
            {
                return _configId;
            }
            set 
            {
                _configId = value;
            }
        }
        private List<JQLConfig> _savedJQL = new List<JQLConfig>();
        private List<JiraStatus> _statusConfigs = new List<JiraStatus>();
        private List<JiraStatus> _defaultStatusConfigs = new List<JiraStatus>();
        private SortedList<string,string> _CustomFields = new SortedList<string, string>();

        private JTISConfig()
        {
            //need for serialization
        }


        private JTISConfig(string login, string token, string jiraRootUrl, string defPrj, int cfgId):this()
        {
            userName = login;
            apiToken = token;
            baseUrl = jiraRootUrl;
            defaultProject = defPrj;
            _configId = cfgId;
            configName = string.Format("CFG{0:00} - {1} - {2}",configId,baseUrl,defaultProject);
            issueNotes = new IssueNotes();
            ScrubData = new List<string>();
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
                            if (Fields == null)
                            {
                                Fields = _jiraRepo.GetFieldsAsJson();
                            }
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

        [JsonProperty("TimeZoneId")]
        public string? TimeZoneId
        {
            get{
                return _timeZoneId;
            }
            set{
                _timeZoneId = value;
                JTISTimeZone.SetJTISTimeZone(this);
            }
        }

        [JsonProperty("configName"),JsonRequired]
        public string? configName {get; private set;}

        [JsonProperty("userName"),JsonRequired]
        public string? userName {get; set;}

        [JsonProperty("apiToken"),JsonRequired]
        public string? apiToken {get; set;}

        [JsonProperty("baseUrl"),JsonRequired]
        public string? baseUrl {get; set;}

        [JsonProperty("defaultProject"),JsonRequired]
        public string? defaultProject {get;set;}

        [JsonIgnore]
        public DateTime? ServerInfoUpdated {get;set;}
        
        public bool DefaultTimeZoneDisplay()
        {
            return JTISTimeZone.DefaultTimeZone;
        }


        [JsonProperty("SavedJQL")]
        public List<JQLConfig> SavedJQL 
        {
            get 
            {
                return _savedJQL;
            }
        }

        [JsonProperty("StatusConfigs")]
        public IReadOnlyList<JiraStatus> StatusConfigs 
        {
            get
            {
                return _statusConfigs;
            }
        }
        [JsonProperty("DefaultStatusConfigs")]
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
        public void AddJQL(string shortName, string saveJql)
        {
            if (JQLUtil.JQLSyntax(saveJql)==false)
            {
                while(saveJql.Contains("  "))
                {
                    saveJql = saveJql.Replace("  "," ");
                }
                while(saveJql.Contains(", "))
                {
                    saveJql = saveJql.Replace(", ",",");
                }
                int commaCount = saveJql.Split(',',StringSplitOptions.RemoveEmptyEntries).Length;
                int spaceCount = saveJql.Split(' ',StringSplitOptions.RemoveEmptyEntries).Length;
                Char delimChar = ' ';
                if (commaCount > spaceCount) {delimChar=',';}
                saveJql = JQLBuilder.BuildInList("Key",saveJql, delimChar,string.Format($"{defaultProject.Trim()}-"));
            }

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

        [JsonIgnore]
        public bool ValidIssueStatusSequence 
        {
            get {
                var invalidCount = this.StatusConfigs.Where(x=>x.ProgressOrder == 0 && x.DefaultInUse == true).Count();
                return invalidCount == 0;
            }
        }

        [JsonIgnore]
        public int StatusesCount
        {
            get {
                return StatusConfigs.Where(x=>x.DefaultInUse).Count();
            }
        }

        [JsonIgnore]
        public List<JiraStatus> LocalProjectDefaultStatuses 
        {
            get 
            {
                return StatusConfigs.Where(x=>x.DefaultInUse==true).OrderBy(y=>y.ProgressOrder).ToList();
            }
        }

        public void UpdateStatusProgressOrder(int statusId, int progressOrder)
        {
            JiraStatus? existStat = StatusConfigs.SingleOrDefault(x=>x.ProgressOrder == progressOrder);
            if (existStat != null) {
                existStat.ProgressOrder = 0;
            }
            JiraStatus updateStat = StatusConfigs.Single(x=>x.StatusId == statusId);
            updateStat.ProgressOrder = progressOrder;
            IsDirty=true;
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
            List<JiraStatus> origDefault = LocalProjectDefaultStatuses;
            List<JiraStatus>? statusAll ; 
            List<JiraStatus>? statusProj;

            AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(), 
                    new ProgressBarColumn(),
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
                            JiraStatus? origSeq = origDefault.SingleOrDefault(x=>x.StatusId==tmpStCfg.StatusId);
                            if (origSeq!= null) {
                                tmpStCfg.ProgressOrder = origSeq.ProgressOrder;
                            }
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

        public List<JiraStatus> GetJiraStatuses(string defProject,  bool defaultProjectOnly = true)
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
            return string.Format($"{configId:00} | {configName.Scrub()}");
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


#region MANUAL JTISCONFIG
        private static void RenderManualConfigStatus(string login, string apiToken, string baseUrl, string proj)
        {
            ConsoleUtil.WriteAppTitle();
            var p = new Panel($" :llama: [bold underline]CREATE NEW JIRA CONFIGURATION[/]  ");
            p.Expand();
            p.Border(BoxBorder.None);
            p.HeaderAlignment(Justify.Left);
            var tbl = new Table().Border(TableBorder.Horizontal);
            tbl.AddColumn(new TableColumn($"[dim]LOGIN[/]").Alignment(Justify.Center)).HorizontalBorder();
            tbl.AddColumn(new TableColumn($"[dim]API TOKEN[/]").Alignment(Justify.Center)).HorizontalBorder();
            tbl.AddColumn(new TableColumn($"[dim]JIRA ROOT URL[/]").Alignment(Justify.Center)).HorizontalBorder();
            tbl.AddColumn(new TableColumn($"[dim]DEFAULT PROJECT KEY[/]").Alignment(Justify.Center)).HorizontalBorder();
            tbl.AddRow(new Markup[]
            {
                new Markup($"[bold]{login}[/]"){Justification=Justify.Center}, 
                new Markup($"[dim]* concealed *[/]"){Justification=Justify.Left}, 
                new Markup($"[bold]{baseUrl}[/]"){Justification=Justify.Center}, 
                new Markup($"[bold]{proj}[/]"){Justification=Justify.Center}
            });


            AnsiConsole.Write(p);
            AnsiConsole.Write(tbl);
        }

        internal static JTISConfig? ManualCreate()
        {
            JTISConfig? result = null;

            try{        
                string tLogin = string.Empty;
                string tAPIToken = string.Empty;
                string tURL = string.Empty;
                string tProj = string.Empty;

                RenderManualConfigStatus(tLogin,tAPIToken,tURL,tProj);
                tLogin = ConsoleUtil.GetInput<string>("Enter Jira Login").Trim();

                RenderManualConfigStatus(tLogin,tAPIToken,tURL,tProj);
                AnsiConsole.Write(new Rule());
                AnsiConsole.MarkupLine($"[dim](Atlassian requires the use of tokens when authenticating to their API. Active Jira users can create a new token if needed by following these instructions)[/]{Environment.NewLine}[bold]https://support.atlassian.com/atlassian-account/docs/manage-api-tokens-for-your-atlassian-account/[/]");
                
                tAPIToken = ConsoleUtil.GetInput<string>("Enter Jira API Token",concealed:true).Trim();

                RenderManualConfigStatus(tLogin,tAPIToken,tURL,tProj);
                AnsiConsole.Write(new Rule());                
                AnsiConsole.MarkupLine($"[dim](Example of Url: https://yourcompany.Atlassian.net/)[/]");
                tURL = ConsoleUtil.GetInput<string>("Enter Jira base URL").Trim();

                RenderManualConfigStatus(tLogin,tAPIToken,tURL,tProj);
                AnsiConsole.Write(new Rule());
                AnsiConsole.MarkupLine($"[dim](A Jira Project is usually the characters that appear [italic]before[/] the number in a Jira Issue, such as 'WWT' in Jira Issues 'WWT-100')[/]");
                tProj = ConsoleUtil.GetInput<string>("Enter Default Project Key").Trim().ToUpper();
                
                RenderManualConfigStatus(tLogin,tAPIToken,tURL,tProj);

                if (ConsoleUtil.Confirm($"A successful connection is needed to verify the information you provided.{Environment.NewLine}[bold]Attempt to authenticate to Jira now?[/]",true))
                {
                    int configNumber = 1;
                    if (CfgManager.config != null && CfgManager.Configs.Count > 0)
                    {
                        configNumber = CfgManager.Configs.Count + 1;
                    }
                    List<string> validProjects = JiraUtil.ValidProjectKeys(tLogin,tAPIToken,tURL);
                    if (validProjects.Count==0)
                    {
                        ConsoleUtil.PressAnyKeyToContinue("Failed to connect.");
                        return null;
                    }
                    if (!validProjects.Contains(tProj))
                    {
                        ConsoleUtil.WriteError($"The Default Project '{tProj}' is not valid. Please choose a valid project key",pause:false);
                        var sp = new SelectionPrompt<string>();
                        sp.HighlightStyle(new Style(decoration:Decoration.Bold));
                        sp.AddChoices(validProjects);
                        sp.Title("Select Valid Project");
                        tProj = AnsiConsole.Prompt<string>(sp);
                    }
                    var testCfg = JTISConfig.Create(tLogin,tAPIToken,tURL,tProj, configNumber);
                    if (testCfg != null)
                    {
                        result = testCfg;
                    }
                    
                }
            }
            catch (Exception e)
            {
                ConsoleUtil.WriteError("Unable to connect with the information provided",false,e,false);
                result = null;
            }
            ConsoleUtil.PressAnyKeyToContinue();
            return result;
        }

        internal static JTISConfig? ManualCreateccc()
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
                            p.HighlightStyle(new Style(decoration:Decoration.Bold));
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
#endregion 

    }
}
