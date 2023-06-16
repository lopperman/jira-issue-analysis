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
                if (_config.DefaultStatusConfigs.Count == 0 || _config.DefaultStatusConfigs.Count != _config.StatusConfigs.Count )
                {
                    ConsoleUtil.WriteStdLine("PLEASE WAIT -- COMPARING STATUS CONFIGS WITH DEFAULT LIST FROM JIRA ...",StdLine.slResponse,false);
                    UpdateDefaultStatusConfigs();
                }
                else 
                {
                    FillMissingStatusConfigs();
                }


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
            List<JiraStatus> statusAll = GetJiraStatuses(false);
            List<JiraStatus> statusProj = GetJiraStatuses(true);

            for (int i = 0; i < statusAll.Count; i ++)
            {
                if (statusProj.Exists(x=>x.StatusId == statusAll[i].StatusId))
                {
                    statusAll[i].DefaultInUse = true;
                }
            }

            config.DefaultStatusConfigs.Clear();
            config.DefaultStatusConfigs = statusAll;
            if (clearLocal)
            {
                config.StatusConfigs.Clear();
                config.StatusConfigs = statusAll;
            }
            else 
            {
                FillMissingStatusConfigs();
            }
            JTISConfigHelper.SaveConfigList();
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
            JTISConfig? chCfg = null; 
            if (msg==null || msg.Length == 0)
            {
                msg = "ENTER CONFIG ID:";
            }
            ConsoleUtil.WriteLine("** CHOOSE CONFIG **",StdForecolor(StdLine.slOutputTitle), StdBackcolor(StdLine.slOutputTitle));
            var cfgNames = JTISConfigHelper.ConfigNameList;
            for (int i = 0; i < cfgNames.Count; i ++)
            {
                ConsoleUtil.WriteLine(cfgNames[i],ConsoleColor.DarkBlue,ConsoleColor.Yellow,false);
            }
            int cfgResp = 0;
            if (JTISConfigHelper.config == null)
            {
                cfgResp = ConsoleUtil.GetConsoleInput<int>(msg,false);
            }
            else 
            {
                cfgResp = ConsoleUtil.GetConsoleInput<int>(msg,true);
            }            
            
            if (cfgResp == 0)
            {
                ConsoleUtil.ByeByeForced();
            }

            chCfg = JTISConfigHelper.GetConfigFromList(cfgResp);
            if (chCfg !=null )
            {
                if (JTISConfigHelper.config != null)
                {
                    JTISConfigHelper.config = null;
                }
                return chCfg;
            }
            else 
            {
                return null;
            }

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
            Console.ReadKey(true);
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

        public static string? GetSavedJQL(string title = "SELECT SAVED JQL")
        {
            string? retJql = string.Empty;
            ConsoleUtil.Lines.AddConsoleLine(title,StdLine.slResponse,false );
            if (JTISConfigHelper.config.SavedJQLCount > 0)
            {
                for (int i = 0; i < JTISConfigHelper.config.SavedJQLCount; i ++)
                {
                    JQLConfig tJql = JTISConfigHelper.config.SavedJQL[i];
                    ConsoleUtil.Lines.AddConsoleLine(string.Format("NAME: {0:00} - {1}",tJql.jqlId,tJql.jqlName) ,StdLine.slOutputTitle);
                    ConsoleUtil.Lines.AddConsoleLine(string.Format("JQL: {0}",tJql.jql) ,StdLine.slOutput);
                }
            }
            else 
            {
                ConsoleUtil.Lines.AddConsoleLine("Saved JQL does not exist for current config",StdLine.slOutput);
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
