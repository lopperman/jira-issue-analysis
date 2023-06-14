using System.Data;
using System.Reflection;
using System.Runtime.Versioning;
using System.Diagnostics;
using System.Threading;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using JConsole.ConsoleHelpers.ConsoleTables;
using static JiraCon.ConsoleUtil;

namespace JiraCon
{
    
    public static class JTISConfigHelper
    {
        private static JTISConfig? _config ;

        public static JTISConfig? config
        {
            get
            {
                return _config;
            }
            set
            {
                _config = value;
                JiraUtil.CreateRestClient();
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
                    ConsoleUtil.WriteLine("Config file has been deleted. Run program again to create new config file. Press any key to exit.", ConsoleColor.White, ConsoleColor.DarkMagenta, true);
                    Console.ReadKey(true);
                }
            }
        }

        public static void SaveConfigList()
        {
            if (cfgList.Count > 0)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                string data = JsonConvert.SerializeObject(cfgList,Formatting.None,settings);
                using (StreamWriter writer = new StreamWriter(ConfigFilePath, false))
                {
                    writer.Write(data);
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
            JTISConfig? chCfg = null; 
            if (msg==null || msg.Length == 0)
            {
                msg = "Enter the config id number to use";
            }
            ConsoleUtil.WriteLine("Choose config or press '0' to exit:", ConsoleColor.DarkMagenta, ConsoleColor.White);
            var cfgNames = JTISConfigHelper.ConfigNameList;
            for (int i = 0; i < cfgNames.Count; i ++)
            {
                ConsoleUtil.WriteLine(cfgNames[i],ConsoleColor.DarkBlue,ConsoleColor.Yellow,false);
            }
            var cfgResp = ConsoleUtil.GetConsoleInput<int>(msg);
            if (cfgResp == 0)
            {
                ConsoleUtil.ByeByeForced();
            }
            chCfg = JTISConfigHelper.GetConfigFromList(cfgResp);
            if (chCfg !=null && chCfg.ValidConfig==true)
            {
                return chCfg;
            }
            else 
            {
                return null;
            }

        }

        public static List<JItemStatus> GetItemStatusConfig()
        {

            var ret = new List<JItemStatus>();
            string fName = string.Format("CONFIG_ISSUE_STATUS_{0:00}.txt",JTISConfigHelper.config.configId);
            string path = Path.Combine(JTISRootPath, fName);

            if (!File.Exists(path))
            {
                ConsoleUtil.WriteLine("Missing config file: " + path, ConsoleColor.Red, ConsoleColor.Gray, false);
                ConsoleUtil.WriteLine("File will be generated with defaults but you will need to verify 'CalendarWork' and 'ActiveWork' in file: " + path,ConsoleColor.Red, ConsoleColor.Gray, false);
                ConsoleUtil.WriteLine("Press any key to continue");
                var ok = Console.ReadKey(true);

                var jItemStatuses = JiraUtil.JiraRepo.GetJItemStatusDefaults();
                ret = jItemStatuses;
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine("##statusName, statusId, categoryKey, categoryName, calendarWork, activeWork");
                    foreach (JItemStatus jis in jItemStatuses)
                    {
                        writer.WriteLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}", jis.StatusName, jis.StatusId, jis.CategoryKey, jis.CategoryName, jis.CalendarWork, jis.ActiveWork));
                    }
                }
            }
            else
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith(value: "#"))
                        {
                            continue;
                        }
                        string[] arr = line.Split(separator: '|');
                        JItemStatus newJIS = new JItemStatus();
                        newJIS.StatusName = arr[0];
                        newJIS.StatusId = arr[1];
                        newJIS.CategoryKey = arr[2];
                        newJIS.CategoryName = arr[3];
                        newJIS.CalendarWork = (arr[4].ToLower() == "true") ? true : false;
                        newJIS.ActiveWork = (arr[5].ToLower() == "true") ? true : false;
                        ret.Add(newJIS);
                    }
                }
            }
            return ret;
        }        

        internal static void ViewAll()
        {

            ConsoleTable table = null;

            // ConsoleUtil.WriteLine(text: "");
            // ConsoleUtil.WriteLine(text: "********** LOGIN CONFIG **********", ConsoleColor.Yellow, ConsoleColor.Black, false);
            // ConsoleUtil.WriteLine(text: "");
            // ConsoleUtil.WriteLine(string.Format("Path = {0}",Path.Combine(personalFolder,configFileName)));

            table = new ConsoleTable("loginName", "apiKey", "Jira Base Url", "Default Project");
            table.AddRow(JTISConfigHelper.config.userName, JTISConfigHelper.config.apiToken, JTISConfigHelper.config.baseUrl, JTISConfigHelper.config.defaultProject);
            table.Write();
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("********** END LOGIN CONFIG ******",ConsoleUtil.StdForecolor(StdLine.slOutputTitle), ConsoleUtil.StdBackcolor(StdLine.slOutputTitle), false);
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("PRESS ANY KEY TO CONTINUE");
            var ok = Console.ReadKey(true);


            // ConsoleUtil.WriteLine(text: "********** ISSUE STATUS TIME METRICS CONFIG *********", ConsoleColor.Yellow, ConsoleColor.Black, clearScreen: false);
            // ConsoleUtil.WriteLine(text: "");
            // ConsoleUtil.WriteLine(text: "All issue statuses. These are determined automatically via the Issue Status Cagetory from Jira, unless otherwise annotated. To add or remove overrides, use the Config menu option for 'Override Issue Status Time Metrics'.");
            // ConsoleUtil.WriteLine(text: "");
            // ConsoleUtil.WriteLine(string.Format("Path = {0}", Path.Combine(personalFolder, configIssueStatus)));

            table = new ConsoleTable("Name","Category Key/Name","CalendarWork","ActiveWork");
            foreach (var jis in JiraUtil.JiraRepo.JItemStatuses)
            {
                table.AddRow(jis.StatusName, string.Format("{0}/{1}",jis.CategoryKey, jis.CategoryName), jis.CalendarWork, jis.ActiveWork);
            }
            table.Write();
            ConsoleUtil.WriteLine(text: "");
            ConsoleUtil.WriteLine(text: "********** END ISSUE STATUS TIME METRICS CONFIG *****", ConsoleColor.Yellow, ConsoleColor.Black, false);
            ConsoleUtil.WriteLine(text: "");
            
            ConsoleUtil.WriteLine("PRESS ANY KEY TO CONTINUE");
            ok = Console.ReadKey(true);
        }

        internal static void DeleteConfig()
        {
            JTISConfig? delCfg = null; 
            JTISConfig? changeToCfg = null;

            ConsoleUtil.WriteLine("** DELETE JIRA CONFIG ** ", ConsoleColor.DarkRed,ConsoleColor.Yellow, true);
            var cfgNames = JTISConfigHelper.ConfigNameList;
            for (int i = 0; i < cfgNames.Count; i ++)
            {
                ConsoleUtil.WriteLine(cfgNames[i]);
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
                            ConsoleUtil.WriteLine("You cannot use the config you are trying to delete! Press any key to continue");
                            Console.ReadKey(true);
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
    }
}
