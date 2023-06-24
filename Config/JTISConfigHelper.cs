using System.Security.Cryptography;
using System.Reflection.Metadata.Ecma335;
using System.Linq;
using System.Xml.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Runtime.Versioning;
using System.Diagnostics;
using System.Threading;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JConsole.ConsoleHelpers.ConsoleTables;
using static JiraCon.ConsoleUtil;
using Spectre.Console;

namespace JiraCon
{
    
    public static class JTISConfigHelper
    {
        public static bool IsInitialized {get;set;}
        public static bool IsConsoleRecording {get;set;}
        private static JTISConfig? _config ;

        public static JTISConfig? config
        {
            get
            {


                return _config;
            }
            set
            {
                AnsiConsole.Status()
                    .Start($"Connecting to: {value.baseUrl}", ctx=>
                    {
                        ctx.Status("[bold]Please wait while a connection is established ...[/]");
                        ctx.Spinner(Spinner.Known.Dots);
                        ctx.SpinnerStyle(new Style(AnsiConsole.Foreground,AnsiConsole.Background));
                        Thread.Sleep(1000);

                        _config = value;
                        JiraUtil.CreateRestClient();
                        if (_config.DefaultStatusConfigs.Count == 0 || _config.DefaultStatusConfigs.Count != _config.StatusConfigs.Count )
                        {
                            // ConsoleUtil.WriteStdLine("PLEASE WAIT -- COMPARING STATUS CONFIGS WITH DEFAULT LIST FROM JIRA ...",StdLine.slResponse,false);
                            ctx.Status("[italic]Checking Jira Issue Status Configuration[/]");
                            Thread.Sleep(1000);
                            UpdateDefaultStatusConfigs();
                        }
                        else 
                        {
                            ctx.Status("[italic]Checking Local Issue Status Configuration[/]");
                            Thread.Sleep(1000);
                            FillMissingStatusConfigs();
                        }
                    }
                    );
            }
        }

        private static List<JTISConfig> cfgList = new List<JTISConfig>();
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
                return cfgList.Count;
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
                        tmpRet.Add(string.Format("{0:00} | {1}",cfgList[i].configId,cfgList[i].configName));
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

            string? tmpUserName = ConsoleUtil.GetConsoleInput<string>("Missing config -- please enter username (email address) for Jira login:");
            string? tmpApiToken = ConsoleUtil.GetConsoleInput<string>("Missing config -- please enter API token for Jira login:");
            string? tmpUrl = ConsoleUtil.GetConsoleInput<string>("Missing config -- please enter base url for Jira instance:");
            string? tmpProject = ConsoleUtil.GetConsoleInput<string>("Missing Project Key -- please enter ProjectKey for current Jira instance:");

            if (tmpUserName != null && tmpApiToken != null && tmpUrl != null & tmpProject != null)
            {
                retCfg = new JTISConfig();
                retCfg.configId = cfgList.Count + 1;
                retCfg.userName = tmpUserName;
                retCfg.apiToken = tmpApiToken;
                retCfg.baseUrl = tmpUrl;
                retCfg.defaultProject = tmpProject;
                retCfg.configName = MakeConfigName(retCfg);
                cfgList.Add(retCfg);
                SaveConfigList();

                //retCfg.SaveToFile(ConfigFilePath,1);
                return retCfg;                
            }
            else 
            {
                return null;
            }
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
                    Console.ReadKey(true);
                }
            }
        }

        public static void SaveConfigList()
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

                    config.DefaultStatusConfigs.Clear();
                    config.DefaultStatusConfigs = statusAll;
                    task3.Increment(1);
                    if (clearLocal)
                    {
                        config.StatusConfigs.Clear();
                        config.StatusConfigs = statusAll;
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
            bool isChanged = false;

            if (cfg.DefaultStatusConfigs.Count > 0 )
            {
                for (int i = 0; i < cfg.DefaultStatusConfigs.Count; i ++)
                {
                    var dflt = cfg.DefaultStatusConfigs[i];
                    if (cfg.StatusConfigs.SingleOrDefault(x=>x.StatusId  == dflt.StatusId )==null)
                    {
                        isChanged = true;
                        cfg.StatusConfigs.Add(new JiraStatus(dflt.StatusId,dflt.StatusName,dflt.CategoryKey,dflt.CategoryName, dflt.DefaultInUse));
                    }
                }
                if (isChanged)
                {
                    JTISConfigHelper.SaveConfigList();
                }
            }
        }
        public static void ReadConfigList()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            string data = string.Empty;
            using (StreamReader reader = new StreamReader(ConfigFilePath))
            {
                data = reader.ReadToEnd();
            }
            var desObj = JsonConvert.DeserializeObject<List<JTISConfig>>(data);
            cfgList = desObj;
        }

        internal static JTISConfig? ChangeCurrentConfig(string? msg)
        {

            if (IsInitialized == false)
            {
                IsInitialized = true;
                if (ConsoleUtil.Confirm("Record console session?",false))
                {
                    IsConsoleRecording = true ;
                    AnsiConsole.Record();
                    ConsoleUtil.PressAnyKeyToContinue("Recording started. You'll have the option to save the recording when you quit using the EXIT Menu Option. You can also save the recording using the Configuration menu.");
                }
            }

            JTISConfig? chCfg = null; 
            Console.Clear();
            if (msg==null || msg.Length == 0)
            {
                msg = "[bold black on lightyellow3]SELECT CONFIGURATION BY [underline]USING ARROW KEYS[/], THEN PRESS 'ENTER'[/]";
            }
            WriteAppTitle();
            var panel = new Panel(msg);
            panel.Border = BoxBorder.Rounded;
            panel.Width = 20;
            panel.HeaderAlignment(Justify.Center );
            AnsiConsole.Write(panel);

            var cfgNames = JTISConfigHelper.ConfigNameList;
            cfgNames.Add("[dim]00 | EXIT[/]");
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



        internal static void ViewAll()
        {

            var tbl = new Table();
            tbl.AddColumns("loginName", "apiKey", "Jira Base Url", "Default Project");
            tbl.AddRow(JTISConfigHelper.config.userName, "**********", JTISConfigHelper.config.baseUrl, JTISConfigHelper.config.defaultProject);
            for (int i = 0; i < JTISConfigHelper.cfgList.Count; i++)
            {
                if (JTISConfigHelper.cfgList[i].configId != JTISConfigHelper.config.configId)
                {
                    tbl.AddRow(JTISConfigHelper.cfgList[i].userName, "**********", JTISConfigHelper.cfgList[i].baseUrl, JTISConfigHelper.cfgList[i].defaultProject);
                }
            }
            AnsiConsole.Write(tbl);
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
                ConsoleUtil.WriteStdLine("PRESS ANY KEY TO CONTINUE",StdLine.slResponse,false);
                Console.ReadKey(true);
            }
            return retJql;
        }
    }
}
