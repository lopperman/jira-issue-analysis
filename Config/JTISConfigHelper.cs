using System.Xml.Linq;
using System.Data;
using JTIS.Console;
using JTIS.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;

namespace JTIS.Config
{
    public static class JTISConfigHelper
    {
        public static bool IsInitialized {get;set;}
        
        private static JTISConfig? _config ;

        public static JTISConfig? config
        {
            get
            {

                return _config;
            }
            set
            {
                AnsiConsole.Status().Start($"Connecting to: {value.baseUrl}", ctx=>
                {
                    ctx.Status("[bold]Please wait while a connection is established ...[/]");
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(new Style(AnsiConsole.Foreground,AnsiConsole.Background));
                    Thread.Sleep(500);
                    _config = value;
                    JiraUtil.CreateRestClient();
                });

                    if (_config.DefaultStatusConfigs.Count == 0 || _config.DefaultStatusConfigs.Count != _config.StatusConfigs.Count )
                    {
                        // ConsoleUtil.WriteStdLine("PLEASE WAIT -- COMPARING STATUS CONFIGS WITH DEFAULT LIST FROM JIRA ...",StdLine.slResponse,false);
                        //ctx.Status("[italic]Checking Jira Issue Status Configuration[/]");
                        Thread.Sleep(1000);
                        UpdateDefaultStatusConfigs();
                    }
                    else 
                    {
                        //ctx.Status("[italic]Checking Local Issue Status Configuration[/]");
                        Thread.Sleep(1000);
                        FillMissingStatusConfigs();
                    }
                    
            }
        }

        private static List<JTISConfig> cfgList = new List<JTISConfig>();
        public static IReadOnlyList<JTISConfig> Configs 
        {
            get
            {
                return cfgList;
            }
        }
        public static string? JTISConfigFilePath {get;set;}
        public static string configFileName = "JiraTISCfg.json";
        public static string JTISRootPath
        {
            get
            {
                string tmpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"JiraJTIS");
                if (Directory.Exists(tmpPath)==false)
                {
                    Directory.CreateDirectory(tmpPath);
                }
                return tmpPath;
            }
        }
        public static string ConfigFolderPath
        {
            get
            {
                string tmpPath = Path.Combine(JTISRootPath,"Config");
                if (Directory.Exists(tmpPath)==false)
                {
                    Directory.CreateDirectory(tmpPath);
                }
                return tmpPath;
            }
        }
        public static string ConfigFilePath
        {
            get
            {
                if (JTISConfigFilePath == null || JTISConfigFilePath.Length ==0)
                {
                    return Path.Combine(ConfigFolderPath,configFileName);
                }
                else 
                {
                    return JTISConfigFilePath;
                }
            }
        }

        public static int ConfigCount
        {
            get
            {
                return  cfgList.Count;
            }
        }

        private static void ResequenceConfigIds()
        {
            if (cfgList.Count > 0)
            {
                bool foundDefault = false;
                bool isDirty = false;
                for (int i = 0; i < cfgList.Count; i++)
                {
                    var tCfg = cfgList[i];
                    if (tCfg.configId == 0)
                    {
                        foundDefault = true;
                    }
                    else
                    {
                        if (tCfg.configId != (i+1))
                        {
                            isDirty = true;
                            tCfg.configId = i + 1;
                        }
                    }
                }
                if (foundDefault == false)
                {
                    cfgList.Add(CreateDefaultConfig());
                    isDirty = true;
                }
                if (isDirty)
                {
                    SaveConfigList();
                }
            }
        }

        public static List<string> ConfigNameList
        {
            get
            {
                List<string> tmpRet = new List<string>();

                if (ConfigCount > 0)
                {
                    for (int i = 0; i < cfgList.Count; i++)
                    {
                        if (cfgList[i].configId != 0)
                        {
                            tmpRet.Add(string.Format("{0:00} | {1}",cfgList[i].configId,cfgList[i].configName));
                        }
                    }
                    var cfgDef = cfgList.SingleOrDefault(x=>x.configId == 0);
                    if (cfgDef != null)
                    {
                        tmpRet.Add(string.Format("{0:00} | {1}",cfgDef.configId,cfgDef.configName));
                    }
                }
                return tmpRet;
            }
        }

        public static JTISConfig? GetConfigFromList(int cfgID)
        {
            return cfgList.Find(x=>x.configId==cfgID);
        }

        public static string MakeConfigName(JTISConfig cfg)
        {
            return string.Format("CFG{0:00} - {1} - {2}",cfg.configId,cfg.baseUrl,cfg.defaultProject);
        }

        public static JTISConfig? CreateConfig()
        {
            
            JTISConfig? retCfg = null;

            string? tmpUserName = ConsoleUtil.GetInput<string>("Missing config -- please enter username (email address) for Jira login:",allowEmpty:true);
            string? tmpApiToken = ConsoleUtil.GetInput<string>("Missing config -- please enter API token for Jira login:",allowEmpty:true);
            string? tmpUrl = ConsoleUtil.GetInput<string>("Missing config -- please enter base url for Jira instance:",allowEmpty:true);
            string? tmpProject = ConsoleUtil.GetInput<string>("Missing Project Key -- please enter ProjectKey for current Jira instance:",allowEmpty:true);

            // if (string.IsNullOrEmpty(tmpUserName) == false && tmpApiToken != null && tmpUrl != null & tmpProject != null)
            if (string.IsNullOrEmpty(tmpUserName) == false && string.IsNullOrEmpty(tmpApiToken) == false && string.IsNullOrEmpty(tmpUrl) == false && string.IsNullOrEmpty(tmpProject) == false)
            {
                retCfg = new JTISConfig();
                retCfg.configId = cfgList.Count + 1;
                retCfg.userName = tmpUserName;
                retCfg.apiToken = tmpApiToken;
                retCfg.baseUrl = tmpUrl;
                retCfg.defaultProject = tmpProject;
                retCfg.configName = MakeConfigName(retCfg);
                if (retCfg.ValidConfig)
                {
                    cfgList.Add(retCfg);
                    SaveConfigList();
                    return retCfg;
                }
            }
            return null;

        }

        public static void  DeleteConfigFile(string? filePath)
        {
            if (filePath == null && JTISConfigHelper.config != null)
            {
                filePath = ConfigFilePath  ;
            }
            if (filePath != null && filePath.Length > 0 )
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    ConsoleUtil.WriteStdLine("Config file has been deleted. Run program again to create new config file. Press any key to exit.", StdLine.slResponse, true);
                    ConsoleUtil.PressAnyKeyToContinue();
                }
            }
        }

        public static void SaveConfigList(JTISConfig jtisCfg)
        {
            if (cfgList.Count > 0)
            {
                if (File.Exists(ConfigFilePath))
                {
                    File.Delete(ConfigFilePath);
                }
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;                
                string data = JsonConvert.SerializeObject(cfgList,Formatting.None,settings);
                using (StreamWriter writer = new StreamWriter(ConfigFilePath, false))
                {
                    writer.Write(data);
                }                                
                foreach (var tcfg in cfgList)
                {
                    tcfg.IsDirty = false;
                }
            }
        }
        public static void SaveConfigList()
        {
            if (config != null)
            {
                SaveConfigList(config);
            }
        }

        public static List<JiraStatus> GetJiraStatuses(bool defaultProjectOnly = true)
        {
            List<JiraStatus> ret = new List<JiraStatus>();
            string data = string.Empty;
            if (defaultProjectOnly)
            {
                data = JiraUtil.JiraRepo.GetProjectItemStatusesAsync().GetAwaiter().GetResult(); 
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
                data = JiraUtil.JiraRepo.GetItemStatusesAsync().GetAwaiter().GetResult(); 
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

        public static void UpdateDefaultStatusConfigs(bool clearLocal = false)
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
                    statusAll = GetJiraStatuses(false);
                    task1.Increment(1);
                    statusProj = GetJiraStatuses(true);
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

                    config.ResetOnlineIssueStatusCfg();
                    foreach (var stCfg in statusAll)
                    {
                        config.UpdateStatusCfgOnline(stCfg);
                    }
                    task3.Increment(1);
                    if (clearLocal || config.StatusConfigs.Count == 0)
                    {
                        config.ResetLocalIssueStatusCfg();
                        foreach (var tmpStCfg in statusAll)
                        {
                            config.UpdateStatusCfgLocal(tmpStCfg);
                        }
                        task3.Increment(1);
                    }
                    else 
                    {
                        FillMissingStatusConfigs();
                        task3.Increment(1);
                    }
                    JTISConfigHelper.SaveConfigList();
                    task3.Increment(1);
                });



        }

        private static void FillMissingStatusConfigs()
        {
            JTISConfig cfg = JTISConfigHelper.config;

            if (cfg.DefaultStatusConfigs.Count > 0 )
            {
                for (int i = 0; i < cfg.DefaultStatusConfigs.Count; i ++)
                {
                    var dflt = cfg.DefaultStatusConfigs[i];
                    if (cfg.StatusConfigs.SingleOrDefault(x=>x.StatusId  == dflt.StatusId )==null)
                    {
                        cfg.UpdateStatusCfgLocal(dflt);
                    }
                }
                if (cfg.IsDirty)
                {
                    JTISConfigHelper.SaveConfigList();
                }
            }
        }

        public static List<JTISConfig>? ReadConfigFile(string filePath)
        {
            List<JTISConfig>? retList = null;

            try 
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                string data = string.Empty;
                using (StreamReader reader = new StreamReader(ConfigFilePath))
                {
                    data = reader.ReadToEnd();
                }
                var desObj = JsonConvert.DeserializeObject<List<JTISConfig>>(data);            
                if (desObj != null)
                {
                    retList = desObj;
                }

            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteError($"Error Reading '{filePath}'",false,ex);
            }

            return retList;
        }
        public static bool ReadConfigList(string? cfgFilePath = null)
        {
            var retVal = false;
            List<JTISConfig>? desObj = ReadConfigFile(cfgFilePath ?? ConfigFilePath);
            if (desObj != null)
            {
                cfgList = desObj;
                if (cfgList.Exists(x=>x.configId == 0)==false)
                {
                    cfgList.Add(CreateDefaultConfig());
                    SaveConfigList();
                }
                retVal = true;
            }
            return retVal;
            // JsonSerializerSettings settings = new JsonSerializerSettings();
            // settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            // string data = string.Empty;
            // using (StreamReader reader = new StreamReader(ConfigFilePath))
            // {
            //     data = reader.ReadToEnd();
            // }
            // var desObj = JsonConvert.DeserializeObject<List<JTISConfig>>(data);
        }

        internal static JTISConfig? ChangeCurrentConfig(string? msg)
        {
            if (config != null && config.IsDirty)
            {
                SaveConfigList();
            }
            if (IsInitialized == false)
            {
                IsInitialized = true;
                //AnsiConsole.Clear();
                ConsoleUtil.WriteAppTitle();
                if (ConsoleUtil.Confirm("Record console session?",false))
                {
                    ConsoleUtil.IsConsoleRecording = true ;
                    AnsiConsole.Record();
                    ConsoleUtil.PressAnyKeyToContinue("Recording started. You'll have the option to save the recording when you quit using the EXIT Menu Option. You can also save the recording using the Configuration menu.");
                }
            }

            JTISConfig? chCfg = null; 
            AnsiConsole.Clear();
            if (msg==null || msg.Length == 0)
            {
                msg = "[bold black on lightyellow3]SELECT CONFIGURATION BY [underline]USING ARROW KEYS[/], THEN PRESS 'ENTER'[/]";
            }
            ConsoleUtil.WriteAppTitle();
            var panel = new Panel(msg);
            panel.Border = BoxBorder.Rounded;
            panel.Width = 20;
            panel.HeaderAlignment(Justify.Center );
            AnsiConsole.Write(panel);

            var cfgNames = JTISConfigHelper.ConfigNameList;
            string[] choices = cfgNames.Select(x=>x.ToString()).ToArray();

            var sg = new AnsiConsoleSettings();
            sg.ColorSystem = ColorSystemSupport.Detect ;
            var cs = AnsiConsole.Create(sg);
            
            string cfgResp = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    // .Title(msg)
                    // .HighlightStyle.Decoration(Decoration.Bold)
                    .PageSize(10)
                    .MoreChoicesText("(Move up and down to reveal more choices)")
                    .AddChoices(choices));

            if (string.IsNullOrEmpty(cfgResp))
            {
                ConsoleUtil.ByeByeForced();
            }
            chCfg = JTISConfigHelper.cfgList.SingleOrDefault(x=>x.ToString()==cfgResp);
            if (chCfg == null)
            {
                ConsoleUtil.ByeByeForced();
            }
            return chCfg;

        }



        internal static void ViewAll(bool showAPIKey = false)
        {
            var cfg = JTISConfigHelper.config;

            foreach (var c in JTISConfigHelper.cfgList)
            {
                var grid = new Grid();
                var gc1 = new GridColumn();
                gc1.Alignment(Justify.Center);
                gc1.Padding(0,0,1,1);
                gc1.Width(4);
                var gc2 = new GridColumn();
                gc2.Alignment(Justify.Left);
                gc1.Padding(0,0,1,1);
                gc2.Width(20);
                var gc3 = new GridColumn();
                gc3.Alignment(Justify.Left);
                gc1.Padding(0,0,1,1);
                gc3.Width(25);
                var gc4 = new GridColumn();
                gc4.Alignment(Justify.Left);
                gc1.Padding(0,0,1,1);
                var gc5 = new GridColumn();
                gc5.Alignment(Justify.Left);
                gc1.Padding(0,0,0,1);
                grid.Expand();
                grid.AddColumns(gc1,gc2,gc3,gc4,gc5);
                grid.AddEmptyRow();
                grid.AddRow(new Markup[]{
                    new Markup("[bold blue]ID[/]"), 
                    new Markup("[bold blue]USER NAME[/]"), 
                    new Markup("[bold blue]BASE URL[/]"), 
                    new Markup("[bold blue]DEF PRJ[/]"), 
                    new Markup("[bold blue]API KEY[/]") 
                    }); 
                grid.AddRow(new Markup[]{
                    new Markup(String.Format("[blue on lightskyblue1]{0:00}[/]",c.configId)), 
                    new Markup(String.Format("[blue on lightskyblue1]{0}[/]",c.userName)), 
                    new Markup(String.Format("[blue on lightskyblue1]{0}[/]",c.baseUrl)), 
                    new Markup(String.Format("[blue on lightskyblue1]{0}[/]",c.defaultProject)),                 
                    new Markup(String.Format("[blue on lightskyblue1]{0}[/]",showAPIKey ? c.apiToken : "*| concealed |*")) 
                    }); 
                var p = new Panel(grid);
                p.Header($"config file: {JTISConfigHelper.ConfigFilePath}");
                p.HeaderAlignment(Justify.Left);
                p.Border(BoxBorder.Rounded);
                p.BorderColor(Style.Parse("dim blue").Foreground);
                p.Expand();
                p.PadTop(1);
                p.SafeBorder();

                AnsiConsole.Write(p);
            }
        }

        internal static void DeleteConfig()
        {
            JTISConfig? delCfg = null; 
            JTISConfig? changeToCfg = null;

            ConsoleUtil.WriteStdLine("** DELETE JIRA CONFIG ** ", StdLine.slError, true);
            var cfgNames = JTISConfigHelper.ConfigNameList;
            for (int i = 0; i < cfgNames.Count; i ++)
            {
                ConsoleUtil.WriteStdLine(cfgNames[i],StdLine.slCode);
            }
            var cfgResp = ConsoleUtil.GetConsoleInput<int>("Enter the config number you want to DELETE");
            delCfg = JTISConfigHelper.GetConfigFromList(cfgResp);
            if (delCfg !=null )
            {
                if (ConfigCount > 1 && delCfg.configName == config.configName)
                {
                    changeToCfg = ChangeCurrentConfig(string.Format("Enter config id number to use after '{0}' is deleted",delCfg.configName));
                    if (changeToCfg != null)
                    {
                        if (changeToCfg.configId == delCfg.configId)
                        {
                            ConsoleUtil.WriteStdLine("You cannot use the config you are trying to delete!",StdLine.slError);
                            ConsoleUtil.PressAnyKeyToContinue();
                            return;
                        }
                        else 
                        {
                            config = changeToCfg;
                        }
                    }
                }
                else 
                {
                    config = null;
                }
                cfgList.Remove(delCfg);
                if (cfgList.Count > 0)
                {
                    for (int i = 0; i < cfgList.Count ; i ++)
                    {
                        JTISConfig modCfg = cfgList[i];
                        modCfg.configId = i + 1;
                        modCfg.configName = MakeConfigName(modCfg);
                    }
                    SaveConfigList();
                }
            }
        }

        public static void CheckDefaultJQL()
        {            
            var blockedName = "(def) Blocked Work";
            var editedName = "(def) Recent Updates";
            var blockedJql = $"project={config.defaultProject} and status not in (backlog, done) and (priority = Blocked OR Flagged in (Impediment))";
            var editedJql = $"project={config.defaultProject} and updated >= -7d";

            var list = new SortedList<string,string>();
            list.Add(blockedName,blockedJql);
            list.Add(editedName,editedJql);

            foreach (var kvp in list)
            {
                var existJql = config.SavedJQL.FirstOrDefault(x=>x.jqlName == kvp.Key);
                if (existJql == null || existJql.jql.StringsMatch(kvp.Value)==false)
                {
                    if (existJql != null)
                    {
                        config.DeleteJQL(existJql);
                    }
                    config.AddJQL(kvp.Key,kvp.Value);
                }
            }


            if (config.IsDirty)
            {
                SaveConfigList(config);
            }
        }

        public static string? GetSavedJQL(string title = "SELECT SAVED JQL")
        {
            string? retJql = string.Empty;
            ConsoleUtil.Lines.AddConsoleLine(title,ConsoleUtil.StdStyle(StdLine.slResponse));
            if (JTISConfigHelper.config.SavedJQLCount > 0)
            {
                for (int i = 0; i < JTISConfigHelper.config.SavedJQLCount; i ++)
                {
                    JQLConfig tJql = JTISConfigHelper.config.SavedJQL[i];
                    ConsoleUtil.Lines.AddConsoleLine(string.Format("NAME: {0:00} - {1}",tJql.jqlId,tJql.jqlName) ,ConsoleUtil.StdStyle(StdLine.slOutput));
                    ConsoleUtil.Lines.AddConsoleLine(string.Format("JQL: {0}",tJql.jql) ,ConsoleUtil.StdStyle(StdLine.slOutput));
                }
            }
            else 
            {
                ConsoleUtil.Lines.AddConsoleLine("Saved JQL does not exist for current config",ConsoleUtil.StdStyle(StdLine.slOutput));
            }
            ConsoleUtil.Lines.WriteQueuedLines(false);
            if (JTISConfigHelper.config.SavedJQLCount > 0)
            {
                var jqlId = ConsoleUtil.GetConsoleInput<int>("Enter JQL item number (number after 'NAME') to select. Enter zero ('0') to cancel",false);
                if (jqlId > 0 && jqlId <= JTISConfigHelper.config.SavedJQLCount)
                {
                    JQLConfig? tmpJQL = JTISConfigHelper.config.SavedJQL.Single(x=>x.jqlId == jqlId);

                    if (tmpJQL != null) 
                    {
                        retJql = tmpJQL.jql;
                    }
                }
            }
            else 
            {
                ConsoleUtil.PressAnyKeyToContinue();
            }
            return retJql;
        }

        private static JTISConfig CreateDefaultConfig()
        {
            var newCfg = new JTISConfig();
            newCfg.apiToken = "***";
            newCfg.baseUrl = "https://github.com/lopperman/jira-issue-analysis";
            newCfg.defaultProject = "***";
            newCfg.userName = "***";
            newCfg.configName = "CFG00 - ADD NEW CONFIG";
            newCfg.configId = 0;
            return newCfg;
        }

        internal static bool ValidateConfigFileArg(string argVal)
        {
            //IF FILE EXIST, CONFIRM FORMAT
            //IF FILE DOES NOT EXIST, CREATE IT WITH A 'FAKE MUST ADD' CONFIG AND RETURN TRUE
            bool isValid = false;
            try 
            {
                if (Path.GetExtension(argVal) != null && String.Compare(Path.GetExtension(argVal),".json")==0)
                {
                    if (File.Exists(argVal) == false)
                    {
                        //var newCFG = CreateDefaultConfig();

                        cfgList.Add(CreateDefaultConfig());
                        if (string.Compare(argVal,ConfigFilePath,false)!=0)
                        {
                            JTISConfigFilePath = argVal;
                        }
                        SaveConfigList();
                        ConsoleUtil.PressAnyKeyToContinue($"ADDED NEW CONFIG FILE: {ConfigFilePath}");
                        isValid = true;
                    }
                    else 
                    {
                        if (ReadConfigList(argVal))
                        {
                        if (string.Compare(argVal,ConfigFilePath,false)!=0)
                        {
                            JTISConfigFilePath = argVal;
                        }
                            isValid = true;
                        }
                    }
                }
            }
            catch(Exception ex) 
            {
                ConsoleUtil.WriteError($"Error validating '{argVal}' as a filePath",false,ex);
                isValid = false;
            }
            return isValid;
        }

        internal static void ChangeTimeZone()
        {
            string? searchTerm = string.Empty;
            string curTZ = JTISTimeZone.DisplayTimeZone.DisplayName;

            // string curTZ = JTISConfigHelper.config.TimeZoneDisplay.DisplayName;
            searchTerm = ConsoleUtil.GetInput<string>($"Date/Time data from Jira is currently displaying as TimeZone: {curTZ}{Environment.NewLine}To change displayed times to another time zone, enter search text (e.g. 'Pacific') and you will be able to select from the filtered results, or press 'ENTER' to cancel",allowEmpty:true);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                List<TimeZoneInfo> zones = TimeZoneInfo.GetSystemTimeZones().ToList();
                if (zones.Exists(x=>x.DisplayName.Contains(searchTerm,StringComparison.InvariantCultureIgnoreCase)))
                {
                    List<TimeZoneInfo> results = zones.Where(x=>x.DisplayName.Contains(searchTerm,StringComparison.InvariantCultureIgnoreCase)).ToList();
                    if (results.Count > 50)
                    {
                        ConsoleUtil.WriteError($"{results.Count} results were returned. Try entering a difference search expression.");
                        ChangeTimeZone();
                        return;
                    }
                    var sp = new SelectionPrompt<TimeZoneInfo>();
                    sp.Title("Select Time Zone - (Required - can cancel after selection)");
                    sp.PageSize = 20;
                    sp.AddChoices(results.ToArray());
                    var selTZ = AnsiConsole.Prompt(sp);
                    if (ConsoleUtil.Confirm($"Change Time-Zone displayed for Jira data to: {selTZ.DisplayName}?",false))
                    {
                        JTISConfigHelper.config.TimeZoneId =  selTZ.Id;
                        JTISConfigHelper.SaveConfigList();
                        ConsoleUtil.PressAnyKeyToContinue($"Jira Configuration '{JTISConfigHelper.config.configName}' has been updated to display time zone ** {selTZ.DisplayName} ** ");
                    }


                // var sp = new SelectionPrompt<MenuFunction>();
                
                // sp.PageSize = 16;
                // sp.AddChoices(menuItems);
                // if (menu == MenuEnum.meMain)
                // {
                //     sp.AddChoiceGroup(
                //             menuSeparator, 
                //             new MenuFunction(MenuItemEnum.miChangeConnection,"Connect to other Jira Site","[dim]Connect to other Jira Site[/]"),
                //             new MenuFunction(MenuItemEnum.miExit,"Exit App","[dim bold]Exit App[/]",true,Emoji.Known.SmallOrangeDiamond));
                // }
                // else 
                // {
                //     sp.AddChoiceGroup(
                //             menuSeparator, 
                //             new MenuFunction(MenuItemEnum.miMenu_Main,"Back to Main Menu","Back to [bold]Main Menu[/]"),
                //             new MenuFunction(MenuItemEnum.miChangeConnection,"Connect to different Jira","[dim]Connect to other Jira Site[/]"),
                //             new MenuFunction(MenuItemEnum.miExit,"Exit App","[dim bold]Exit App[/]",true,Emoji.Known.SmallOrangeDiamond));

                // }                    
                // var mnu = AnsiConsole.Prompt(sp);

                }

            }


            //             List<JiraStatus>? statusAll ; 
            // List<JiraStatus>? statusProj;

            // AnsiConsole.Progress()
            //     .Columns(new ProgressColumn[]
            //     {
            //         new TaskDescriptionColumn(), 
            //         new PercentageColumn(),
            //         new ElapsedTimeColumn(), 
            //         new SpinnerColumn()
            //     })
            //     .Start(ctx => 
            //     {
            //         var task1 = ctx.AddTask("[blue on white]Get Jira Statuses[/]");
            //         var task2 = ctx.AddTask("[blue on white]Check Default Project Statuses[/]");        
            //         var task3 = ctx.AddTask("[blue on white]Check Missing Status Configs[/]");        


            //         task1.MaxValue = 2;
            //         statusAll = GetJiraStatuses(false);
            //         task1.Increment(1);
            //         statusProj = GetJiraStatuses(true);
            //         task1.Increment(1);

            //         task2.MaxValue = statusAll.Count;
            //         for (int i = 0; i < statusAll.Count; i ++)
            //         {
            //             if (statusProj.Exists(x=>x.StatusId == statusAll[i].StatusId))
            //             {
            //                 statusAll[i].DefaultInUse = true;
            //             }
            //             task2.Increment(1);                        
            //         }

            //         task3.MaxValue = 3;

            //         config.ResetOnlineIssueStatusCfg();
            //         foreach (var stCfg in statusAll)
            //         {
            //             config.UpdateStatusCfgOnline(stCfg);
            //         }
            //         task3.Increment(1);
            //         if (clearLocal || config.StatusConfigs.Count == 0)
            //         {
            //             config.ResetLocalIssueStatusCfg();
            //             foreach (var tmpStCfg in statusAll)
            //             {
            //                 config.UpdateStatusCfgLocal(tmpStCfg);
            //             }
            //             task3.Increment(1);
            //         }
            //         else 
            //         {
            //             FillMissingStatusConfigs();
            //             task3.Increment(1);
            //         }
            //         JTISConfigHelper.SaveConfigList();
            //         task3.Increment(1);
            //     });

        }
    }
}
