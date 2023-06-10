using System.Reflection;
using System.Runtime.Versioning;
using System.Diagnostics;
using System.Threading;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace JiraCon
{
    
    public static class JTISConfigHelper
    {
        private static List<JTISConfig> cfgList = new List<JTISConfig>();
        public static string? JTISConfigFilePath {get;set;}
        public static string configFileName = "JiraTISConfig.txt";
        public static string configFolderName = "JiraTIS";        
        public static string ConfigFolderPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"Library","Application Support",configFolderName );
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

        public static JTISConfig? GetConfigFromList(int cfgID)
        {
            return cfgList.Find(x=>x.configId==cfgID);
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
                retCfg.configName = string.Format("CFG{0:00} - {1} - {2}",retCfg.configId,retCfg.baseUrl,retCfg.defaultProject);
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
            if (filePath == null && MainClass.config != null)
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

        // public static List<JTISConfig> GetJTISConfigs(string jsonFilePath)
        // {
        //     JTISConfig cfg1 = new JTISConfig(true);
        //     cfg1.userName="paulbrower97@gmail.com";
        //     cfg1.apiToken="abcde12345";
        //     cfg1.baseUrl="www.jira.com";
        //     cfg1.defaultProject="WWT1";
        //     JTISConfig cfg2 = new JTISConfig(true);
        //     cfg2.userName="paulbrower97@outlook.com";
        //     cfg2.apiToken="abcde12345";
        //     cfg2.baseUrl="www.jira.com";
        //     cfg2.defaultProject="WWT1";

        //     var retList = new List<JTISConfig>();
        //     retList.Add(cfg1);
        //     retList.Add(cfg2);

        //     JsonSerializerSettings settings = new JsonSerializerSettings();
        //     settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        //     string data = JsonConvert.SerializeObject(retList,Formatting.None,settings);

        //     using (StreamWriter writer = new StreamWriter(jsonFilePath, false))
        //     {
        //         writer.Write(data);
        //     }            

        //     return retList;
        // }

        // public static JTISConfig GetConfig()
        // {            


        //     if (ret == null)
        //     {
        //         string userName = "";
        //         string apiToken = "";
        //         string jiraBaseUrl = "";
        //         string projKey = "";

        //         userName = GetConsoleInput("Missing config -- please enter username (email address) for Jira login:");
        //         apiToken = GetConsoleInput("Missing config -- please enter API token for Jira login:");
        //         jiraBaseUrl = GetConsoleInput("Missing config -- please enter base url for Jira instance:");
        //         projKey = GetConsoleInput("Missing Project Key -- please enter ProjectKey for current Jira instance:");

        //         bool validCredentials = false;
        //         //test connection
        //         try
        //         {
        //             ConsoleUtil.WriteLine("testing Jira connection ...");
        //             var testConn = new JiraRepo(jiraBaseUrl, userName, apiToken);

        //             if (testConn != null)
        //             {
        //                 var test = testConn.GetJira().IssueTypes.GetIssueTypesAsync().Result.ToList();
        //                 if (test != null && test.Count > 0)
        //                 {
        //                     validCredentials = true;
        //                     ConsoleUtil.WriteLine("testing Jira connection ... successful");
        //                 }
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             ConsoleUtil.WriteLine("testing Jira connection ... failed");
        //             ConsoleUtil.WriteLine(ex.Message);
        //         }

        //         if (!validCredentials)
        //         {
        //             return GetConfig();
        //         }
        //         else
        //         {
        //             using (StreamWriter writer = new StreamWriter(configFile))
        //             {
        //                 writer.WriteLine(string.Format("{0} {1} {2} {3}", userName, apiToken, jiraBaseUrl, projKey));
        //             }
        //             return GetConfig();
        //         }
        //     }

        //     return ret;

        // }

    }
}
