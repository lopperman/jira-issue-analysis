using System.Data;
using JTIS.Console;
using JTIS.Data;
using JTIS.Extensions;
using Newtonsoft.Json;
using Spectre.Console;

namespace JTIS.Config
{
    public static class CfgManager
    {
        private static jtisRefData? _refData;
        private static JTISConfig? _config ;

        public static jtisRefData RefData{
            get{
                if (config == null || JiraUtil.JiraRepo == null)
                {
                    throw new InvalidOperationException("JiraRepo not initialized");
                }
                else if (_refData == null)
                { 
                    _refData = jtisRefData.Create(config);
                }
                return _refData;
            }
        }

        public static JTISConfig? config
        {
            get
            {   
                if (_config == null)
                {
                    return null;
                }
                if (_config.Connected)
                {
                    if (_config.SavedJQLCount == 0)
                    {
                        JQLUtil.CheckDefaultJQL(_config);
                    }


                    return _config;
                }
                return null;
            }
            set
            {
                if (value.Connected)
                {
                    if (!cfgList.Exists(x=>x.Key==value.Key))
                    {
                        cfgList.Add(value);
                    }
                    _config = null;
                    _refData = null;

                    _config = value;
                }
                else 
                {
                    ConsoleUtil.WriteError($"Could not connect with Jira Connection Profile: {value.ToString()}",false,pause:true);                                            
                    if (_config == null)
                    {
                        ConsoleUtil.ByeByeForced();
                    }      
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
        public static string defaultConfigFileName = "JiraTISCfg.json";
        

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
                string fPath = string.Empty;
                if (JTISConfigFilePath == null)
                {
                    fPath = Path.Combine(JTISRootPath,"Config");
                }
                else 
                {
                    fPath = Path.GetDirectoryName(JTISConfigFilePath);
                }
                if (Directory.Exists(fPath)==false)
                {
                    Directory.CreateDirectory(fPath);
                }
                return fPath;
            }
        }
        public static string ConfigFilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(JTISConfigFilePath))
                {
                    return Path.Combine(ConfigFolderPath,defaultConfigFileName);
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
                bool isDirty = false;
                for (int i = 0; i < cfgList.Count; i++)
                {
                    var tCfg = cfgList[i];
                    if (tCfg.configId != (i+1))
                    {
                        isDirty = true;
                        tCfg.UpdateConfigId(i + 1);
                    }
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


        public static JTISConfig?  CreateConfig(bool addToConfigList = true, bool connectTo = true )
        {
            
            JTISConfig? retCfg = JTISConfig.ManualCreate();            
            if (retCfg!= null)
            {
                if (addToConfigList)
                {
                    cfgList.Add(retCfg);
                    if (connectTo)
                    {
                        config = retCfg;    
                    }
                    SaveConfigList();
                }
            }
            return retCfg;
        }

        public static void  DeleteConfigFile(string? filePath)
        {
            if (filePath == null && CfgManager.config != null)
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

        public static void SaveConfigList()
        {
            if (cfgList.Count > 0)
            {
                for (int i = 0 ; i < cfgList.Count; i ++)
                {
                    cfgList[i].UpdateConfigId( i + 1);
                    if (cfgList[i].Key == null)
                    {
                        cfgList[i].Key = Guid.NewGuid();
                    }
                }

                if (File.Exists(ConfigFilePath))
                {
                    File.Delete(ConfigFilePath);
                }
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;                
                settings.MissingMemberHandling = MissingMemberHandling.Ignore;
                
                string data = JsonConvert.SerializeObject(cfgList,Formatting.None,settings);
                using (StreamWriter writer = new StreamWriter(ConfigFilePath, false))
                {
                    writer.Write(data);
                }                                
                foreach (JTISConfig tcfg in cfgList)
                {
                    tcfg.IsDirty = false;
                }
            }
        }





        public static List<JTISConfig>? ReadConfigFile(string filePath)
        {
            if (ConfigFilePath.StringsMatch(filePath)==false)
            {
                JTISConfigFilePath = filePath;
            }
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
                var desObj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JTISConfig>>(data);            
                // var desObj = JsonConvert.DeserializeObject<List<JTISConfig>>(data);            
                if (desObj != null)
                {
                    retList = desObj;
                }

            }
            catch 
            {
                AnsiConsole.Write(new Spectre.Console.Rule());
                AnsiConsole.MarkupLine($"[bold]The file[/] [italic] '{filePath}'[/] [bold] is invalid or needs to be created[/]");
                if (Path.Exists(filePath)==false)
                {
                    AnsiConsole.MarkupLine($"[bold red on white]*** Missing file *** '{filePath}'[/]");
                }
                AnsiConsole.Write(new Spectre.Console.Rule());

                // ConsoleUtil.WriteError($"Error Reading '{filePath}'",false,ex);
            }

            return retList;
        }

        internal static JTISConfig? ChangeCurrentConfig(List<JTISConfig>? choices = null)
        {
            JTISConfig? resp = null;
            if (choices == null) 
            {
                choices = cfgList;
            }
            if (choices.Count == 1) 
            {
                resp = choices.First();
            }
            else if (choices.Count == 0)
            {
                resp = JTISConfig.ManualCreate();
            }
            else 
            {
                if (config == null)
                {
                    ConsoleUtil.WriteAppTitle();
                }

                resp = SelectJTISConfig("Select Jira Config",false,list:choices);
            }
            if (resp != null)
            {
                if (resp.Connected)
                {
                    return resp;
                }
            }
            return null;
        }



        internal static void ViewAll(bool showAPIKey = false)
        {
            ConsoleUtil.WriteAppTitle();
            var cfg = CfgManager.config;

            foreach (var c in CfgManager.cfgList)
            {
                var grid = new Grid();
                var gc1 = new GridColumn();
                gc1.Alignment(Justify.Center);
                gc1.Padding(0,0,1,1);
                gc1.Width(4);
                var gc2 = new GridColumn();
                gc2.Alignment(Justify.Left);
                gc1.Padding(0,0,1,1);
                // gc2.Width(20);
                var gc3 = new GridColumn();
                gc3.Alignment(Justify.Left);
                gc1.Padding(0,0,1,1);
                // gc3.Width(25);
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
                    new Markup("[bold]ID[/]"), 
                    new Markup("[bold]USER NAME[/]"), 
                    new Markup("[bold]BASE URL[/]"), 
                    new Markup("[bold]DEF PRJ[/]"), 
                    new Markup("[bold]API KEY[/]") 
                    }); 
                grid.AddRow(new Markup[]{
                    new Markup(String.Format("[italic]{0:00}[/]",c.configId)), 
                    new Markup(String.Format($"[italic]{c.userName}{Environment.NewLine}[dim](Key: {c.Key})[/][/]",c.userName)), 
                    new Markup(String.Format("[italic]{0}[/]",c.baseUrl)), 
                    new Markup(String.Format("[italic]{0}[/]",c.defaultProject)),                 
                    new Markup(String.Format("[italic]{0}[/]",showAPIKey ? c.apiToken : "*| concealed |*")) 
                    }); 
                var p = new Panel(grid);
                p.Header($"config file: {CfgManager.ConfigFilePath}");
                p.HeaderAlignment(Justify.Left);
                p.Border(BoxBorder.Rounded);
                p.BorderColor(Style.Parse("dim").Foreground);
                p.Expand();
                p.PadTop(1);
                p.SafeBorder();

                AnsiConsole.Write(p);
            }
        }

        internal static void DeleteConfig()
        {
            JTISConfig? delCfg = null; 

            if (cfgList.Count == 1)
            {
                ConsoleUtil.PressAnyKeyToContinue("OPERATION CANCELLED - YOU CANNOT DELETE ACTIVE CONNECTION AND YOU ONLY HAVE 1 JIRA CONFIGURATION!");
                return;
            }

            ConsoleUtil.WriteAppTitle();

            // var sp  = new SelectionPrompt<JTISConfig>();
            // sp.AddChoices<JTISConfig>(CfgManager.cfgList);
            // sp.Title("SELECT ITEM TO DELETE");
            // delCfg = AnsiConsole.Prompt<JTISConfig>(sp);

            delCfg = SelectJTISConfig("[bold]Select Jira Config to [underline]DELETE[/][/]. [dim](You'll be asked to confirm before deleting[/])",canSelectCurrent:false);
            if (delCfg == null)
            {
                return;
            }

            if (delCfg.Key==config.Key)
            {
                ConsoleUtil.WriteError("you cannot delete a configuration that is currently connected");
                ConsoleUtil.PressAnyKeyToContinue();
                return;
            }
            if (ConsoleUtil.Confirm($"Delete Jira Config: {delCfg.ToString()}?",false))
            {
                cfgList.Remove(delCfg);
                SaveConfigList();
                ConsoleUtil.PressAnyKeyToContinue("SUCCESSFULLY DELETED JIRA CONFIGURATION");
            }
        }



        public static string? GetSavedJQL(string title = "SELECT SAVED JQL")
        {
            string? retJql = string.Empty;
            ConsoleUtil.Lines.AddConsoleLine(title,ConsoleUtil.StdStyle(StdLine.slResponse));
            if (CfgManager.config.SavedJQLCount > 0)
            {
                for (int i = 0; i < CfgManager.config.SavedJQLCount; i ++)
                {
                    JQLConfig tJql = CfgManager.config.SavedJQL[i];
                    ConsoleUtil.Lines.AddConsoleLine(string.Format("NAME: {0:00} - {1}",tJql.jqlId,tJql.jqlName) ,ConsoleUtil.StdStyle(StdLine.slOutput));
                    ConsoleUtil.Lines.AddConsoleLine(string.Format("JQL: {0}",tJql.jql) ,ConsoleUtil.StdStyle(StdLine.slOutput));
                }
            }
            else 
            {
                ConsoleUtil.Lines.AddConsoleLine("Saved JQL does not exist for current config",ConsoleUtil.StdStyle(StdLine.slOutput));
            }
            ConsoleUtil.Lines.WriteQueuedLines(false);
            if (CfgManager.config.SavedJQLCount > 0)
            {
                var jqlId = ConsoleUtil.GetConsoleInput<int>("Enter JQL item number (number after 'NAME') to select. Enter zero ('0') to cancel",false);
                if (jqlId > 0 && jqlId <= CfgManager.config.SavedJQLCount)
                {
                    JQLConfig? tmpJQL = CfgManager.config.SavedJQL.Single(x=>x.jqlId == jqlId);

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




        internal static void ChangeTimeZone()
        {
            string? searchTerm = string.Empty;
            string curTZ = JTISTimeZone.DisplayTimeZone.DisplayName;

            // string curTZ = CfgManager.config.TimeZoneDisplay.DisplayName;
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
                    sp.HighlightStyle(new Style(decoration:Decoration.Bold));

                    sp.Title("Select Time Zone - (Required - can cancel after selection)");
                    sp.PageSize = 20;
                    sp.AddChoices(results.ToArray());
                    var selTZ = AnsiConsole.Prompt(sp);
                    if (ConsoleUtil.Confirm($"Change Time-Zone displayed for Jira data to: {selTZ.DisplayName}?",false))
                    {
                        CfgManager.config.TimeZoneId =  selTZ.Id;
                        CfgManager.SaveConfigList();
                        ConsoleUtil.PressAnyKeyToContinue($"Jira Configuration '{CfgManager.config.configName}' has been updated to display time zone ** {selTZ.DisplayName} ** ");
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
            //         CfgManager.SaveConfigList();
            //         task3.Increment(1);
            //     });

        }

        internal static string? CheckManualFilePath(string checkPath)
        {            
            bool failed = false;
            if (Path.IsPathFullyQualified(checkPath)==false)
            {
                failed = true;
            }
            else 
            {
                if (Path.GetExtension(checkPath).StringsMatch(".json")==false)
                {
                    failed = true;
                }
            }
            if (failed)
            {
                ConsoleUtil.WriteError($"{checkPath} is not a valid config fully qualified file path{Environment.NewLine}(If you wish to create a new config file, pass in the full path to the file name, which should have a '.json' extension)",pause:true);
                ConsoleUtil.ByeByeForced();
            }
            if (!failed)
            {
                return checkPath;
            }
            else 
            {
                return null;
            }
        }

        internal static void SetConfigList(List<JTISConfig> tmpConfigs)
        {
            cfgList = tmpConfigs;
            var setCfg = ChangeCurrentConfig(cfgList);
            if (setCfg != null)
            {
                config = setCfg;
            }
        }

        public static  JTISConfig? SelectJTISConfig(string markupTitle, bool confirm = true, bool canSelectCurrent = true,  List<JTISConfig>? list = null)
        {            

            JTISConfig? selectedCfg = null;
            if (list == null)
            {
                list = cfgList;;
            }

            if (canSelectCurrent == false && list.Count == 1)
            {
                ConsoleUtil.PressAnyKeyToContinue("THIS FUNCTION DOES NOT ALLOW YOU TO SELECT YOUR CURRENT CONFIG");
                return null;
            }
            if (markupTitle.StringsMatch(Markup.Remove(markupTitle)))
            {
                markupTitle = $"[bold]{markupTitle}[/]";
            }
            AnsiConsole.Write(new Spectre.Console.Rule());
            AnsiConsole.MarkupLine($"\t{markupTitle}");
            AnsiConsole.Write(new Spectre.Console.Rule());

            var sp  = new SelectionPrompt<JTISConfig>();
            sp.HighlightStyle(new Style(decoration:Decoration.Bold));

            sp.AddChoices<JTISConfig>(list);
            //sp.Title("SELECT CONFIG ITEM");
            selectedCfg = AnsiConsole.Prompt<JTISConfig>(sp);

            if (canSelectCurrent == false && config.Key == selectedCfg.Key)
            {
                ConsoleUtil.PressAnyKeyToContinue("THIS FUNCTION DOES NOT ALLOW YOU TO SELECT YOUR CURRENT CONFIG");
                return null;
            }


            if (confirm)
            {
                if (!ConsoleUtil.Confirm($"Proceed with '{selectedCfg.ToString()}'",true))
                {
                    return null;
                }
            }
            return selectedCfg;

        }

        internal static void AddNewConfig()
        {
            ConsoleUtil.WriteAppTitle();
            JTISConfig? newConfig = null;
            AnsiConsole.Write(new Spectre.Console.Rule());
            var onlyProject = ConsoleUtil.Confirm("Would you like to copy an existing Jira Configuration and just change the Default Project?",false);
            if (onlyProject)
            {
                var anyConfig = SelectJTISConfig("Select any config to duplicate");                
                var newDefProject = ConsoleUtil.GetInput<string>("Enter Jira Project Key to set as Default Project for the new Config",allowEmpty:true);
                if (newDefProject!=null && newDefProject.Length > 0)
                {
                    newDefProject = newDefProject.Trim().ToUpper();
                    foreach (var existCfg in cfgList)                    
                    {
                        if (existCfg.userName.StringsMatch(anyConfig.userName) && 
                            existCfg.baseUrl.StringsMatch(anyConfig.baseUrl) && 
                            existCfg.defaultProject.StringsMatch(newDefProject)) 
                        {
                            ConsoleUtil.PressAnyKeyToContinue($"OPERATION CANCELLED - A MATCHING CONFIG ALREADY EXISTS WITH DEFAULT PROJECT '{newDefProject}'");
                            return;
                        }
                        else 
                        {
                            newConfig = JTISConfig.Create(anyConfig.userName,anyConfig.apiToken,anyConfig.baseUrl,newDefProject,cfgList.Count +  1);                            
                        }
                    }
                }
                else 
                {
                    ConsoleUtil.PressAnyKeyToContinue("OPERATION CANCELLED");
                    return;
                }
            }
            else
            {
                newConfig = JTISConfig.ManualCreate();
            }
            if (newConfig != null)
            {
                cfgList.Add(newConfig);
                SaveConfigList();
                if (ConsoleUtil.Confirm($"New Jira config ('{newConfig.ToString()}') has been created and saved. Would you like to connect to it now?",true,false))
                {
                    config = newConfig;
                }
            }


        }
    }
}
